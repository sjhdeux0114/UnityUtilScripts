using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// 전역 코루틴을 실행하기 위한 싱글톤 관리자 클래스
    /// </summary>
    internal sealed class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[CoroutineRunner]");
                    go.hideFlags = HideFlags.HideAndDontSave;
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<CoroutineRunner>();
                }
                return _instance;
            }
        }
    }

    /// <summary>
    /// 코루틴 유틸리티 (정적 호출 API 및 인스턴스 확장 메서드 제공)
    /// </summary>
    public static class CoroutineUtil
    {
        // =========================
        // 정적(static) 유틸 메서드
        // =========================

        /// <summary> 지정된 시간(delay) 이후에 action 실행 </summary>
        public static Coroutine Delay(float delay, Action action, bool unscaled = false, CancellationToken token = default)
            => CoroutineRunner.Instance.StartCoroutine(DelayEnumerator(delay, action, unscaled, token));

        /// <summary> 다음 프레임에 action 실행 </summary>
        public static Coroutine NextFrame(Action action, CancellationToken token = default)
            => CoroutineRunner.Instance.StartCoroutine(NextFrameEnumerator(action, token));

        /// <summary> 프레임 종료 시(EndOfFrame) action 실행 </summary>
        public static Coroutine EndOfFrame(Action action, CancellationToken token = default)
            => CoroutineRunner.Instance.StartCoroutine(EndOfFrameEnumerator(action, token));

        /// <summary> condition이 true가 될 때까지 대기 후 action 실행 </summary>
        public static Coroutine WaitUntil(Func<bool> condition, Action action, CancellationToken token = default)
            => CoroutineRunner.Instance.StartCoroutine(WaitUntilEnumerator(condition, action, token));

        /// <summary>
        /// interval 주기로 action을 반복 실행 (count < 0 이면 무한 반복)
        /// </summary>
        public static Coroutine Repeat(float interval, Action<int> action, int count = -1, bool unscaled = false, CancellationToken token = default)
            => CoroutineRunner.Instance.StartCoroutine(RepeatEnumerator(interval, action, count, unscaled, token));

        /// <summary> IEnumerator를 안전하게 실행 </summary>
        public static Coroutine Run(IEnumerator routine)
            => CoroutineRunner.Instance.StartCoroutine(WrapSafe(routine));

        /// <summary> 실행 중인 코루틴 중지 </summary>
        public static void Stop(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                CoroutineRunner.Instance.StopCoroutine(coroutine);
            }
        }

        /// <summary> routine이 timeout을 초과하면 중단하고 onTimeout 호출 </summary>
        public static Coroutine WithTimeout(IEnumerator routine, float timeout, Action onTimeout = null, bool unscaled = false, CancellationToken token = default)
            => CoroutineRunner.Instance.StartCoroutine(WithTimeoutEnumerator(routine, timeout, onTimeout, unscaled, token));

        // =========================
        // MonoBehaviour 확장 메서드
        // (호출한 컴포넌트가 파괴되면 자동으로 중단됨)
        // =========================

        public static Coroutine Delay(this MonoBehaviour mb, float delay, Action action, bool unscaled = false, CancellationToken token = default)
            => mb.StartCoroutine(DelayEnumerator(delay, action, unscaled, token));

        public static Coroutine NextFrame(this MonoBehaviour mb, Action action, CancellationToken token = default)
            => mb.StartCoroutine(NextFrameEnumerator(action, token));

        public static Coroutine EndOfFrame(this MonoBehaviour mb, Action action, CancellationToken token = default)
            => mb.StartCoroutine(EndOfFrameEnumerator(action, token));

        public static Coroutine WaitUntil(this MonoBehaviour mb, Func<bool> condition, Action action, CancellationToken token = default)
            => mb.StartCoroutine(WaitUntilEnumerator(condition, action, token));

        public static Coroutine Repeat(this MonoBehaviour mb, float interval, Action<int> action, int count = -1, bool unscaled = false, CancellationToken token = default)
            => mb.StartCoroutine(RepeatEnumerator(interval, action, count, unscaled, token));

        public static Coroutine Run(this MonoBehaviour mb, IEnumerator routine)
            => mb.StartCoroutine(WrapSafe(routine));

        public static Coroutine WithTimeout(this MonoBehaviour mb, IEnumerator routine, float timeout, Action onTimeout = null, bool unscaled = false, CancellationToken token = default)
            => mb.StartCoroutine(WithTimeoutEnumerator(routine, timeout, onTimeout, unscaled, token));

        // =========================
        // 가비지 할당 없는 커스텀 대기(Wait) IEnumerator
        // =========================

        /// <summary> 가비지 할당 없이 일정 시간 동안 대기하는 IEnumerator 반환 </summary>
        public static IEnumerator WaitFor(float time, bool unscaled = false)
        {
            bStop = false;
            while (time > 0f)
            {
                time -= unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
                if (bStop)
                    yield break;
            }
        }

        /// <summary> 가비지 할당 없이 랜덤한 시간 동안 대기하는 IEnumerator 반환 </summary>
        public static IEnumerator WaitForRandom(float minTime, float maxTime, bool unscaled = false)
        {
            bStop = false;
            float time = UnityEngine.Random.Range(minTime, maxTime);
            while (time > 0f)
            {
                time -= unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
                if (bStop)
                    yield break;

            }
        }

        static bool bStop = false;
        public static void WaitStop()
        {
            bStop = true;
        }

        // =========================
        // 내부 Enumerator 구현부
        // =========================

        public static IEnumerator DelayEnumerator(float delay, Action action, bool unscaled, CancellationToken token)
        {
            if (delay > 0f)
            {
                if (unscaled) yield return new WaitForSecondsRealtime(delay);
                else yield return new WaitForSeconds(delay);
            }
            if (token.IsCancellationRequested) yield break;
            SafeInvoke(action);
        }

        public static IEnumerator NextFrameEnumerator(Action action, CancellationToken token)
        {
            yield return null;
            if (token.IsCancellationRequested) yield break;
            SafeInvoke(action);
        }

        public static IEnumerator EndOfFrameEnumerator(Action action, CancellationToken token)
        {
            yield return new WaitForEndOfFrame();
            if (token.IsCancellationRequested) yield break;
            SafeInvoke(action);
        }

        public static IEnumerator WaitUntilEnumerator(Func<bool> condition, Action action, CancellationToken token)
        {
            if (condition == null) yield break;
            yield return new WaitUntil(() => token.IsCancellationRequested || condition());
            if (token.IsCancellationRequested) yield break;
            SafeInvoke(action);
        }

        public static IEnumerator RepeatEnumerator(float interval, Action<int> action, int count, bool unscaled, CancellationToken token)
        {
            if (action == null) yield break;
            int i = 0;
            var waitScaled = interval > 0f ? new WaitForSeconds(interval) : null;
            var waitUnscaled = interval > 0f ? new WaitForSecondsRealtime(interval) : null;

            while (!token.IsCancellationRequested && (count < 0 || i < count))
            {
                SafeInvoke(() => action(i));
                i++;

                if (interval > 0f)
                {
                    if (unscaled) yield return waitUnscaled;
                    else yield return waitScaled;
                }
                else
                {
                    // interval == 0 -> 다음 프레임까지 대기
                    yield return null;
                }
            }
        }

        public static IEnumerator WithTimeoutEnumerator(IEnumerator routine, float timeout, Action onTimeout, bool unscaled, CancellationToken token)
        {
            if (routine == null) yield break;

            float start = unscaled ? Time.unscaledTime : Time.time;
            while (true)
            {
                if (token.IsCancellationRequested) yield break;

                if (unscaled)
                {
                    if (Time.unscaledTime - start >= timeout)
                    {
                        SafeInvoke(onTimeout);
                        yield break;
                    }
                }
                else
                {
                    if (Time.time - start >= timeout)
                    {
                        SafeInvoke(onTimeout);
                        yield break;
                    }
                }

                // routine 진행 상태 확인
                bool hasNext;
                object current = null;
                try
                {
                    hasNext = routine.MoveNext();
                    if (hasNext) current = routine.Current;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    yield break;
                }

                if (!hasNext) yield break;

                // yield 값 반환
                yield return current;
            }
        }

        private static IEnumerator WrapSafe(IEnumerator routine)
        {
            while (true)
            {
                bool hasNext;
                object current = null;
                try
                {
                    hasNext = routine.MoveNext();
                    if (hasNext) current = routine.Current;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    yield break;
                }
                if (!hasNext) yield break;
                yield return current;
            }
        }

        private static void SafeInvoke(Action action)
        {
            try { action?.Invoke(); }
            catch (Exception e) { Debug.LogException(e); }
        }
    }
}
