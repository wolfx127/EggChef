using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;

namespace Framework
{
    public static class YooAssetMgr
    {
        // 你的 Windows 宝塔站点
        // 每个包上传到 EggChef/<PackageName>/
        private const string ServerRoot = "http://49.234.199.29/EggChef/";

        // YooAsset 默认缓存目录名
        private const string CacheFolderName = "yoo";

        private class PackageRecord
        {
            public string PackageName;
            public ResourcePackage Package;
            public string CurrentVersion;
            public bool IsReady;
            public bool IsLoading;
        }

        private static readonly Dictionary<string, PackageRecord> _records = new();

        // ==================== API 1：加载/更新包 ====================
        public static IEnumerator LoadPackageAsync(
            string packageName,
            Action<ResourcePackage> onComplete,
            Action<float, string> onProgress = null)
        {
            if (!YooAssets.IsInitialized)
                YooAssets.Initialize();

            if (!YooAssets.TryGetPackage(packageName, out var package))
                package = YooAssets.CreatePackage(packageName);

            if (!_records.TryGetValue(packageName, out var record))
            {
                record = new PackageRecord { PackageName = packageName };
                _records[packageName] = record;
            }
            record.Package = package;

            if (record.IsLoading)
            {
                while (record.IsLoading)
                    yield return null;

                onComplete?.Invoke(record.IsReady ? record.Package : null);
                yield break;
            }

            record.IsLoading = true;
            onProgress?.Invoke(0f, "初始化资源包...");

            // 初始化
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
                yield return initOp;

                if (initOp.Status != EOperationStatus.Succeeded)
                {
                    Debug.LogError($"[AutoLoader] 初始化失败: {initOp.Error}");
                    Finish(record, onComplete, null);
                    yield break;
                }
            }

            // 先读旧包版本
            string localVersion = ReadLocalVersion(packageName);

            // 再请求远端版本
            onProgress?.Invoke(5f, "请求版本...");
            string remoteVersion = null;
            var versionOp = package.RequestPackageVersionAsync(
                new RequestPackageVersionOptions(true, 3));
            yield return versionOp;

            if (versionOp.Status == EOperationStatus.Succeeded)
                remoteVersion = versionOp.PackageVersion;
            else
                Debug.LogWarning($"[AutoLoader] 请求版本失败，尝试本地: {versionOp.Error}");

            string targetVersion = remoteVersion;
            bool needDownload = false;

            if (!string.IsNullOrEmpty(remoteVersion))
            {
                needDownload = remoteVersion != localVersion;
            }
            else
            {
                targetVersion = localVersion;
            }

            // 远端失败、本地也没有 -> 用内置首包版本
            if (string.IsNullOrEmpty(targetVersion))
                targetVersion = ReadBuiltinVersion(packageName);

            if (string.IsNullOrEmpty(targetVersion))
            {
                Debug.LogError($"[AutoLoader] {packageName} 没有可用版本");
                Finish(record, onComplete, null);
                yield break;
            }

#if UNITY_EDITOR
            // 模拟模式没有 Bundle 下载
            needDownload = false;
#endif

            // 换版本前，先卸掉旧资源
            if (needDownload && record.IsReady)
            {
                onProgress?.Invoke(8f, "卸载旧资源...");
                yield return package.UnloadAllAssetsAsync();
                record.IsReady = false;
            }

            // 关键：不管版本是否相同，都要加载 Manifest
            onProgress?.Invoke(10f, "加载清单...");
            var manifestOp = package.LoadPackageManifestAsync(
                new LoadPackageManifestOptions(targetVersion, 60));
            yield return manifestOp;

            // 远端 Manifest 失败，回退旧版本
            if (manifestOp.Status != EOperationStatus.Succeeded
                && needDownload
                && !string.IsNullOrEmpty(localVersion)
                && localVersion != targetVersion)
            {
                targetVersion = localVersion;
                needDownload = false;
                manifestOp = package.LoadPackageManifestAsync(
                    new LoadPackageManifestOptions(targetVersion, 60));
                yield return manifestOp;
            }

            if (manifestOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"[AutoLoader] 加载清单失败: {manifestOp.Error}");
                Finish(record, onComplete, null);
                yield break;
            }

            // 下载缺的 Bundle
            if (needDownload)
            {
                onProgress?.Invoke(15f, "检查差异...");
                var downloader = package.CreateResourceDownloader(
                    new ResourceDownloaderOptions(10, 3));

                if (downloader.TotalDownloadCount > 0)
                {
                    downloader.DownloadProgressChanged += data =>
                    {
                        float progress = 15f + 80f * data.Progress;
                        onProgress?.Invoke(progress,
                            $"下载中 {data.CurrentDownloadCount}/{data.TotalDownloadCount}");
                    };

                    downloader.StartDownload();
                    yield return downloader;

                    if (downloader.Status != EOperationStatus.Succeeded)
                    {
                        Debug.LogWarning(
                            $"[AutoLoader] 下载失败: {downloader.Error}，回退本地");

                        if (!string.IsNullOrEmpty(localVersion)
                            && localVersion != targetVersion)
                        {
                            targetVersion = localVersion;
                            var fallbackOp = package.LoadPackageManifestAsync(
                                new LoadPackageManifestOptions(targetVersion, 60));
                            yield return fallbackOp;

                            if (fallbackOp.Status == EOperationStatus.Succeeded)
                            {
                                record.CurrentVersion = targetVersion;
                                onProgress?.Invoke(100f, "使用本地版本");
                                Finish(record, onComplete, package);
                                yield break;
                            }
                        }

                        Finish(record, onComplete, null);
                        yield break;
                    }
                }

                // 新包下载完，删旧 Manifest / 旧 Bundle
                onProgress?.Invoke(95f, "清理旧包...");
                yield return ClearOldCache(package);
            }

            record.CurrentVersion = targetVersion;
            onProgress?.Invoke(100f, "资源就绪");
            Finish(record, onComplete, package);
    }

    // ==================== API 2：加载资源 ====================
    public static AssetHandle LoadAssetAsync<T>(ResourcePackage package, string location)
        where T : UnityEngine.Object
    {
        if (package == null)
        {
            Debug.LogError("[AutoLoader] package 为 null");
            return null;
        }

        if (package.InitializeStatus != EOperationStatus.Succeeded)
        {
            Debug.LogError($"[AutoLoader] 包还没初始化: {package.PackageName}");
            return null;
        }

        return package.LoadAssetAsync<T>(location);
    }

    // ==================== API 3：释放单个包 ====================
    public static IEnumerator ReleasePackage(ResourcePackage package)
    {
        if (package == null)
            yield break;

        string name = package.PackageName;

        // DestroyPackageAsync 内部会卸载资源 + 释放 Handle
        var destroyOp = package.DestroyPackageAsync();
        yield return destroyOp;

        if (destroyOp.Status != EOperationStatus.Succeeded)
        {
            Debug.LogError($"[AutoLoader] 释放失败: {destroyOp.Error}");
            yield break;
        }

        YooAssets.RemovePackage(name);
        _records.Remove(name);
    }

    // ==================== API 4：释放所有包 ====================
    public static IEnumerator ReleaseAllPackage()
    {
        var names = new List<string>(_records.Keys);

        foreach (var name in names)
        {
            if (_records.TryGetValue(name, out var record)
                && record.Package != null)
            {
                yield return ReleasePackage(record.Package);
            }
        }

        _records.Clear();
    }

    // ==================== 内部 ====================
    private static void Finish(
        PackageRecord record,
        Action<ResourcePackage> onComplete,
        ResourcePackage package)
    {
        record.IsLoading = false;
        record.IsReady = package != null;
        record.Package = package;

        onComplete?.Invoke(package);
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
            Debug.LogWarning($"[AutoLoader] 读本地版本失败: {e.Message}");
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
    private static IEnumerator ClearOldCache(ResourcePackage package)
    {
        var clearManifest = package.ClearCacheAsync(
            new ClearCacheOptions(ClearCacheMethods.ClearUnusedManifestFiles));
        yield return clearManifest;

        if (clearManifest.Status != EOperationStatus.Succeeded)
            Debug.LogWarning($"[AutoLoader] 清理旧清单失败: {clearManifest.Error}");

        var clearBundle = package.ClearCacheAsync(
            new ClearCacheOptions(ClearCacheMethods.ClearUnusedBundleFiles));
        yield return clearBundle;

        if (clearBundle.Status != EOperationStatus.Succeeded)
            Debug.LogWarning($"[AutoLoader] 清理旧 Bundle 失败: {clearBundle.Error}");
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

    // ==================== 静态状态重置 ====================
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _records.Clear();
    }

    }
}
