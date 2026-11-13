using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// UniTask 常用等待 / 延迟 / 循环工具。
    ///
    /// 全是静态方法，自身没有生命周期，取消由调用方传 CancellationToken：
    /// 组件里用 this.GetCancellationTokenOnDestroy()，或者自己 new 一个 CancellationTokenSource。
    ///
    /// 循环类方法（Repeat / RepeatFrames / RepeatEveryFrame）返回 UniTaskLoopHandle，
    /// 可以 Pause / Resume / Stop。
    /// </summary>
    public static class UniTaskMgr
    {
        #region 等待

        /// 等一帧（下一次 Update 阶段继续）
        public static UniTask NextFrame(CancellationToken token = default)
            => UniTask.Yield(PlayerLoopTiming.Update, token);

        /// 等 N 帧
        public static UniTask NextFrames(int frames, CancellationToken token = default)
            => UniTask.DelayFrame(frames, PlayerLoopTiming.Update, token);

        /// 等秒（受 timeScale 影响，暂停时不走）
        public static UniTask WaitSeconds(float seconds, CancellationToken token = default)
            => UniTask.Delay(TimeSpan.FromSeconds(seconds), false, PlayerLoopTiming.Update, token);

        /// 等秒（不受 timeScale 影响，暂停时照走）
        public static UniTask WaitSecondsRealtime(float seconds, CancellationToken token = default)
            => UniTask.Delay(TimeSpan.FromSeconds(seconds), true, PlayerLoopTiming.Update, token);

        /// 等下一个物理步（碰刚体用它，别用 NextFrame）
        public static UniTask NextFixedFrame(CancellationToken token = default)
            => UniTask.WaitForFixedUpdate(token);

        /// 等到条件成立
        public static UniTask WaitUntil(Func<bool> condition, CancellationToken token = default)
            => UniTask.WaitUntil(condition, PlayerLoopTiming.Update, token);

        /// 等到条件不成立
        public static UniTask WaitWhile(Func<bool> condition, CancellationToken token = default)
            => UniTask.WaitWhile(condition, PlayerLoopTiming.Update, token);

        #endregion

        #region 延迟执行

        /// N 秒后执行
        public static async UniTask DelayCall(float seconds, Action action, CancellationToken token = default)
        {
            await WaitSeconds(seconds, token);
            action?.Invoke();
        }

        /// N 秒后执行（忽略 timeScale）
        public static async UniTask DelayCallRealtime(float seconds, Action action, CancellationToken token = default)
        {
            await WaitSecondsRealtime(seconds, token);
            action?.Invoke();
        }

        /// N 帧后执行
        public static async UniTask DelayCallFrames(int frames, Action action, CancellationToken token = default)
        {
            await NextFrames(frames, token);
            action?.Invoke();
        }

        #endregion

        #region 计时器

        /// 计时器：每帧回调进度 0→1，结束时再回调一次 1
        public static async UniTask Timer(float duration, Action<float> onTick,
            bool ignoreTimeScale = false, CancellationToken token = default)
        {
            if (duration <= 0f)
            {
                onTick?.Invoke(1f);
                return;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                onTick?.Invoke(Mathf.Clamp01(elapsed / duration));
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                elapsed += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            }

            onTick?.Invoke(1f);
        }

        #endregion

        #region 循环（返回句柄，可暂停）

        /// 每 interval 秒执行一次
        public static UniTaskLoopHandle Repeat(float interval, Action action,
            CancellationToken token = default, bool startPaused = false)
        {
            UniTaskLoopHandle handle = new UniTaskLoopHandle(token, startPaused);
            RepeatLoop(handle, interval, action).Forget();
            return handle;
        }

        /// 每 frames 帧执行一次
        public static UniTaskLoopHandle RepeatFrames(int frames, Action action,
            CancellationToken token = default, bool startPaused = false)
        {
            UniTaskLoopHandle handle = new UniTaskLoopHandle(token, startPaused);
            RepeatFramesLoop(handle, frames, action).Forget();
            return handle;
        }

        /// 每帧执行一次
        public static UniTaskLoopHandle RepeatEveryFrame(Action action,
            CancellationToken token = default, bool startPaused = false)
        {
            UniTaskLoopHandle handle = new UniTaskLoopHandle(token, startPaused);
            RepeatEveryFrameLoop(handle, action).Forget();
            return handle;
        }

        /// 每物理步执行一次（默认 50Hz，跟 FixedUpdate 对齐）
        /// 注意：回调里算时间要用 Time.fixedDeltaTime，改刚体用 MovePosition
        public static UniTaskLoopHandle RepeatEveryFixedFrame(Action action,
            CancellationToken token = default, bool startPaused = false)
        {
            UniTaskLoopHandle handle = new UniTaskLoopHandle(token, startPaused);
            RepeatEveryFixedFrameLoop(handle, action).Forget();
            return handle;
        }

        private static async UniTaskVoid RepeatLoop(UniTaskLoopHandle handle, float interval, Action action)
        {
            CancellationToken token = handle.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (handle.IsPaused)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                        continue;
                    }

                    action?.Invoke();
                    await UniTask.Delay(TimeSpan.FromSeconds(interval), false, PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常退出路径，不用管
            }
            finally
            {
                handle.Release();
            }
        }

        private static async UniTaskVoid RepeatFramesLoop(UniTaskLoopHandle handle, int frames, Action action)
        {
            CancellationToken token = handle.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (handle.IsPaused)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                        continue;
                    }

                    action?.Invoke();
                    await UniTask.DelayFrame(frames, PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                handle.Release();
            }
        }

        private static async UniTaskVoid RepeatEveryFrameLoop(UniTaskLoopHandle handle, Action action)
        {
            CancellationToken token = handle.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (handle.IsPaused)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                        continue;
                    }

                    action?.Invoke();
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                handle.Release();
            }
        }

        private static async UniTaskVoid RepeatEveryFixedFrameLoop(UniTaskLoopHandle handle, Action action)
        {
            CancellationToken token = handle.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (handle.IsPaused)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                        continue;
                    }

                    action?.Invoke();
                    await UniTask.WaitForFixedUpdate(token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                handle.Release();
            }
        }

        #endregion
    }

    /// <summary>
    /// 循环的暂停键。由 UniTaskMgr.Repeat / RepeatFrames / RepeatEveryFrame / RepeatEveryFixedFrame 返回。
    ///
    /// Pause()  停止执行回调（循环本身还在，只是空转等 Resume）
    /// Resume() 继续
    /// Stop()   结束并释放，之后不可再用
    /// </summary>
    public sealed class UniTaskLoopHandle
    {
        private readonly CancellationTokenSource source;
        private bool paused;
        private bool released;

        internal UniTaskLoopHandle(CancellationToken token, bool startPaused)
        {
            source = CancellationTokenSource.CreateLinkedTokenSource(token);
            paused = startPaused;
        }

        internal CancellationToken Token => source.Token;

        public bool IsPaused => paused;
        public bool IsRunning => !source.IsCancellationRequested;

        public void Pause() => paused = true;

        public void Resume() => paused = false;

        public void Stop()
        {
            if (source.IsCancellationRequested) return;
            source.Cancel();
        }

        /// <summary>
        /// 循环退出时自己调用，释放内部 CTS。外部不用管。
        /// </summary>
        internal void Release()
        {
            if (released) return;
            released = true;
            source.Dispose();
        }
    }
}
