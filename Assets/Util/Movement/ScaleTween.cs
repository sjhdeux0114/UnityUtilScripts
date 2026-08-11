using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ScaleInfo
{
    public Vector3 Start_Scale = Vector3.one;
    public Vector3 End_Scale = Vector3.one;
}

public class ScaleTween : MonoBehaviour
{
    public enum ScaleMode
    {
        Once,
        Loop,
        PingPong,
        AllPlay,
        AllPlayLoop,
        AllPlayPingPong
    }

    [Header("스케일points")]
    public List<ScaleInfo> scalepoints = new List<ScaleInfo>();

    [Header("Settings")]
    public float duration = 2.0f;
    public Ease easeType = Ease.Linear;
    public ScaleMode scaleMode = ScaleMode.Once;
    public int playIndex = 0;
    public bool playOnEnable = true;
    public bool AutoHide = false;
    public UnityEvent OnCompleteEvent;

    private DG.Tweening.Tween scaleTween;
    [Range(0.0f, 1.0f)]
    public float PreView = 0;

    public bool bLocal = true;
    bool _isPlaying;
    private Vector3 dynamicStartScale;
    private bool useDynamicStart = false;

    public bool _IsPlaying()
    {
        return _isPlaying;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        if (scalepoints != null && scalepoints.Count > 0)
        {
            UpdateTransform(PreView);
        }
    }
#endif

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play(playIndex);
        }
    }

    public void RePlay()
    {
        if (!_IsPlaying())
            Play(playIndex);
    }



    public void Play(int index = 0)
    {
        if (scalepoints == null || scalepoints.Count == 0) return;

        playIndex = index;

        // Record the current scale as dynamic starting scale before any modification
        dynamicStartScale = (bLocal || transform.parent == null) ? transform.localScale : transform.lossyScale;
        useDynamicStart = Application.isPlaying;

        scaleTween?.Kill();
        _isPlaying = true;

        UpdateTransform(0f);

        int loops = 1;
        LoopType loopType = LoopType.Restart;

        if (scaleMode == ScaleMode.Loop || scaleMode == ScaleMode.AllPlayLoop)
        {
            loops = -1;
            loopType = LoopType.Restart;
        }
        else if (scaleMode == ScaleMode.PingPong || scaleMode == ScaleMode.AllPlayPingPong)
        {
            loops = -1;
            loopType = LoopType.Yoyo;
        }

        float progress = 0f;
        scaleTween = DOTween.To(() => progress, x =>
        {
            progress = x;
            UpdateTransform(progress);
        }, 1f, duration)
        .SetEase(easeType)
        .SetLoops(loops, loopType)
        .SetAutoKill(false)
        .OnStepComplete(() =>
        {
            useDynamicStart = false;
        })
        .OnComplete(() =>
        {
            if (scaleMode == ScaleMode.Once || scaleMode == ScaleMode.AllPlay)
            {
                if (AutoHide)
                    gameObject.SetActive(false);

                OnCompleteEvent?.Invoke();
                _isPlaying = false;
            }
        });
    }

    private Vector3 GetScaleAtTime(float t)
    {
        if (scalepoints == null || scalepoints.Count == 0) return transform.localScale;

        bool isAllPlay = (scaleMode == ScaleMode.AllPlay || scaleMode == ScaleMode.AllPlayLoop || scaleMode == ScaleMode.AllPlayPingPong);

        if (!isAllPlay)
        {
            int idx = Mathf.Clamp(playIndex, 0, scalepoints.Count - 1);
            Vector3 start = useDynamicStart ? dynamicStartScale : scalepoints[idx].Start_Scale;
            return Vector3.Lerp(start, scalepoints[idx].End_Scale, t);
        }
        else
        {
            float totalSegments = scalepoints.Count;
            float currentPos = t * totalSegments;
            int index = Mathf.FloorToInt(currentPos);
            float segmentT = currentPos - index;

            if (index >= scalepoints.Count)
            {
                return scalepoints[scalepoints.Count - 1].End_Scale;
            }

            Vector3 start = (index == 0 && useDynamicStart) ? dynamicStartScale : scalepoints[index].Start_Scale;
            return Vector3.Lerp(start, scalepoints[index].End_Scale, segmentT);
        }
    }

    private void UpdateTransform(float t)
    {
        Vector3 targetScale = GetScaleAtTime(t);
        if (bLocal || transform.parent == null)
        {
            transform.localScale = targetScale;
        }
        else
        {
            Vector3 parentScale = transform.parent.lossyScale;
            transform.localScale = new Vector3(
                parentScale.x != 0 ? targetScale.x / parentScale.x : targetScale.x,
                parentScale.y != 0 ? targetScale.y / parentScale.y : targetScale.y,
                parentScale.z != 0 ? targetScale.z / parentScale.z : targetScale.z
            );
        }
    }

    public void Stop()
    {
        scaleTween?.Kill();
        _isPlaying = false;
    }

    public void HideObj()
    {
        gameObject.SetActive(false);
    }

    public void DestroyObj()
    {
        Destroy(gameObject);
    }
}
