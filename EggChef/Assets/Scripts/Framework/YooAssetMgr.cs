using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Framework
{
    // YooAsset 底层操作，只有 4 个公开 API：
    //   1. LoadPackageAsync         按名字加载（并更新）资源包
    //   2. LoadAssetAsync           从包里加载资源
    //   3. ReleasePackageAsync      释放单个包
    //   4. ReleaseAllPackagesAsync  释放所有包
    // 不含 UI、引用计数、并发去重：这些都由 YooAssetResourceHub 负责。
    // 调用者不要直接用它，统一走 YooAssetResourceHub。
    public static class YooAssetMgr
    {
        // 每个包上传到 EggChef/<PackageName>/
        private const string ServerRoot = "http://49.234.199.29/EggChef/";

        // YooAsset 默认缓存目录名
        private const string CacheFolderName = "yoo";

        // 已加载成功的包
        private static readonly Dictionary<string, ResourcePackage> Packages = new();

        // ==================== API 1：加载/更新包 ====================
        // onProgress: 0~1。失败返回 null（失败后不会留下半初始化的包）。
        public static async UniTask<ResourcePackage> LoadPackageAsync(
            string packageName,
            Action<float> onProgress = null)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                Debug.LogError("[YooAssetMgr] 包名为空");
                return null;
            }

            // 已经加载好的包直接返回
            if (Packages.TryGetValue(packageName, out var ready))
            {
                onProgress?.Invoke(1f);
                return ready;
            }

            if (!YooAssets.IsInitialized)
                YooAssets.Initialize();

            if (!YooAssets.TryGetPackage(packageName, out var package))
                package = YooAssets.CreatePackage(packageName);

            try
            {
                if (await InitAndUpdateAsync(package, packageName, onProgress))
                {
                    Packages[packageName] = package;
                    onProgress?.Invoke(1f);
                    return package;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[YooAssetMgr] {packageName} 加载异常: {e}");
            }

            // 失败：把半成品销毁，下次重试从干净状态开始
            await TryDiscardAsync(package);
            return null;
        }

        // ==================== API 2：从包里加载资源 ====================
        // 成功返回已完成的 handle（用完调用 handle.Release()），失败返回 null。
        public static async UniTask<AssetHandle> LoadAssetAsync<T>(
            ResourcePackage package, string location)
            where T : UnityEngine.Object
        {
            if (package == null)
            {
                Debug.LogError("[YooAssetMgr] package 为 null");
                return null;
            }

            if (package.InitializeStatus != EOperationStatus.Succeeded)
            {
                Debug.LogError($"[YooAssetMgr] 包还没初始化: {package.PackageName}");
                return null;
            }

            var handle = package.LoadAssetAsync<T>(location);
            await handle;

            if (handle.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"[YooAssetMgr] 加载资源失败 {location}");
                handle.Release();
                return null;
            }

            return handle;
        }

        // ==================== API 3：释放单个包 ====================
        // DestroyPackageAsync 内部会卸载资源并释放 Handle
        public static async UniTask<bool> ReleasePackageAsync(ResourcePackage package)
        {
            if (package == null)
                return true;

            string name = package.PackageName;

            var destroyOp = package.DestroyPackageAsync();
            await destroyOp;

            if (destroyOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"[YooAssetMgr] 释放失败 {name}: {destroyOp.Error}");
                return false;
            }

            YooAssets.RemovePackage(name);
            Packages.Remove(name);
            return true;
        }

        // ==================== API 4：释放所有包 ====================
        public static async UniTask ReleaseAllPackagesAsync()
        {
            var list = new List<ResourcePackage>(Packages.Values);

            foreach (var package in list)
                await ReleasePackageAsync(package);
        }

        // ==================== 内部：初始化 + 更新 ====================
        private static async UniTask<bool> InitAndUpdateAsync(
            ResourcePackage package,
            string packageName,
            Action<float> onProgress)
        {
            onProgress?.Invoke(0f);

            // ---------- 初始化 ----------
            if (package.InitializeStatus != EOperationStatus.Succeeded)
            {
                InitializePackageOptions options;

#if UNITY_EDITOR
                // 编辑器模拟模式，不走服务器
                var buildResult = EditorSimulateBuildInvoker.Build(
                    packageName, (int)EBundleType.VirtualAssetBundle);
                var editorOptions = new EditorSimulateModeOptions();
                editorOptions.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(
                        buildResult.PackageRootDirectory);
                options = editorOptions;
#else
                // 打包后：内置首包（如果有）+ 沙盒缓存 + 远端
                var hostOptions = new HostPlayModeOptions();

                if (ReadBuiltinVersion(packageName) != null)
                {
                    var builtinParams =
                        FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();
                    builtinParams.AddParameter(
                        EFileSystemParameter.CopyBuiltinPackageManifest, true);
                    hostOptions.BuiltinFileSystemParameters = builtinParams;
                }

                // 每个包一个远程目录
                hostOptions.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultSandboxFileSystemParameters(
                        new RemoteService($"{ServerRoot}{packageName}/"));
                options = hostOptions;
#endif

                var initOp = package.InitializePackageAsync(options);
                await initOp;

                if (initOp.Status != EOperationStatus.Succeeded)
                {
                    Debug.LogError($"[YooAssetMgr] 初始化失败: {initOp.Error}");
                    return false;
                }
            }

            // ---------- 版本 ----------
            string localVersion = ReadLocalVersion(packageName);

            onProgress?.Invoke(0.05f);
            string remoteVersion = null;
            var versionOp = package.RequestPackageVersionAsync(
                new RequestPackageVersionOptions(true, 3));
            await versionOp;

            if (versionOp.Status == EOperationStatus.Succeeded)
                remoteVersion = versionOp.PackageVersion;
            else
                Debug.LogWarning($"[YooAssetMgr] 请求版本失败，尝试本地: {versionOp.Error}");

            string targetVersion = remoteVersion;
            bool needDownload = false;

            if (!string.IsNullOrEmpty(remoteVersion))
                needDownload = remoteVersion != localVersion;
            else
                targetVersion = localVersion;

            // 远端失败、本地也没有 -> 用内置首包版本
            if (string.IsNullOrEmpty(targetVersion))
                targetVersion = ReadBuiltinVersion(packageName);

            if (string.IsNullOrEmpty(targetVersion))
            {
                Debug.LogError($"[YooAssetMgr] {packageName} 没有可用版本");
                return false;
            }

#if UNITY_EDITOR
            // 模拟模式没有 Bundle 下载
            needDownload = false;
#endif

            // ---------- 清单（不管版本是否相同，都要加载） ----------
            onProgress?.Invoke(0.10f);
            var manifestOp = package.LoadPackageManifestAsync(
                new LoadPackageManifestOptions(targetVersion, 60));
            await manifestOp;

            // 远端清单失败，回退旧版本
            if (manifestOp.Status != EOperationStatus.Succeeded
                && needDownload
                && !string.IsNullOrEmpty(localVersion)
                && localVersion != targetVersion)
            {
                targetVersion = localVersion;
                needDownload = false;
                manifestOp = package.LoadPackageManifestAsync(
                    new LoadPackageManifestOptions(targetVersion, 60));
                await manifestOp;
            }

            if (manifestOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"[YooAssetMgr] 加载清单失败: {manifestOp.Error}");
                return false;
            }

            // ---------- 下载缺的 Bundle ----------
            if (needDownload)
            {
                onProgress?.Invoke(0.15f);
                var downloader = package.CreateResourceDownloader(
                    new ResourceDownloaderOptions(10, 3));

                if (downloader.TotalDownloadCount > 0)
                {
                    downloader.DownloadProgressChanged += data =>
                        onProgress?.Invoke(0.15f + 0.80f * data.Progress);

                    downloader.StartDownload();
                    await downloader;

                    if (downloader.Status != EOperationStatus.Succeeded)
                    {
                        Debug.LogWarning(
                            $"[YooAssetMgr] 下载失败: {downloader.Error}，回退本地");

                        if (!string.IsNullOrEmpty(localVersion)
                            && localVersion != targetVersion)
                        {
                            var fallbackOp = package.LoadPackageManifestAsync(
                                new LoadPackageManifestOptions(localVersion, 60));
                            await fallbackOp;

                            if (fallbackOp.Status == EOperationStatus.Succeeded)
                            {
                                Debug.LogWarning($"[YooAssetMgr] {packageName} 使用本地版本");
                                return true;
                            }
                        }

                        return false;
                    }
                }

                // 新包下载完，删旧 Manifest / 旧 Bundle
                onProgress?.Invoke(0.95f);
                await ClearOldCacheAsync(package);
            }

            return true;
        }

        private static async UniTask TryDiscardAsync(ResourcePackage package)
        {
            try
            {
                await ReleasePackageAsync(package);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[YooAssetMgr] 清理失败的包时出错: {e.Message}");
            }
        }

        // 从旧包的 Manifest 文件名里取版本：<Package>_<Version>.bytes
        private static string ReadLocalVersion(string packageName)
        {
#if UNITY_EDITOR
            return null;
#else
            try
            {
                string root = Path.Combine(
                    Application.dataPath, CacheFolderName, packageName, "ManifestFiles");

                if (!Directory.Exists(root))
                    return null;

                string prefix = packageName + "_";
                string[] files = Directory.GetFiles(root, prefix + "*.bytes");

                if (files.Length == 0)
                    return null;

                string newest = files[0];
                DateTime newestTime = File.GetLastWriteTimeUtc(newest);

                for (int i = 1; i < files.Length; i++)
                {
                    DateTime time = File.GetLastWriteTimeUtc(files[i]);
                    if (time > newestTime)
                    {
                        newest = files[i];
                        newestTime = time;
                    }
                }

                string fileName = Path.GetFileNameWithoutExtension(newest);
                if (!fileName.StartsWith(prefix, StringComparison.Ordinal))
                    return null;

                return fileName.Substring(prefix.Length);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[YooAssetMgr] 读本地版本失败: {e.Message}");
                return null;
            }
#endif
        }

        // 内置首包版本：StreamingAssets/yoo/<Package>/<Package>.version
        private static string ReadBuiltinVersion(string packageName)
        {
            string root = Path.Combine(
                Application.streamingAssetsPath, "yoo", packageName);
            string versionFile = Path.Combine(root, packageName + ".version");

            if (!File.Exists(versionFile))
                return null;

            string version = File.ReadAllText(versionFile).Trim();
            if (version.Length == 0)
                return null;

            if (!File.Exists(Path.Combine(root, $"{packageName}_{version}.hash")))
                return null;

            if (!File.Exists(Path.Combine(root, $"{packageName}_{version}.bytes")))
                return null;

            return version;
        }

        // 新包生效后，清掉旧 Manifest 和旧 Bundle
        private static async UniTask ClearOldCacheAsync(ResourcePackage package)
        {
            var clearManifest = package.ClearCacheAsync(
                new ClearCacheOptions(ClearCacheMethods.ClearUnusedManifestFiles));
            await clearManifest;

            if (clearManifest.Status != EOperationStatus.Succeeded)
                Debug.LogWarning($"[YooAssetMgr] 清理旧清单失败: {clearManifest.Error}");

            var clearBundle = package.ClearCacheAsync(
                new ClearCacheOptions(ClearCacheMethods.ClearUnusedBundleFiles));
            await clearBundle;

            if (clearBundle.Status != EOperationStatus.Succeeded)
                Debug.LogWarning($"[YooAssetMgr] 清理旧 Bundle 失败: {clearBundle.Error}");
        }

        private class RemoteService : IRemoteService
        {
            private readonly string _root;

            public RemoteService(string root)
            {
                _root = root.TrimEnd('/');
            }

            public IReadOnlyList<string> GetRemoteUrls(string fileName)
            {
                return new[] { $"{_root}/{fileName}" };
            }
        }

        // 关闭 Domain Reload 时，静态状态要手动重置
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Packages.Clear();
        }
    }
}
