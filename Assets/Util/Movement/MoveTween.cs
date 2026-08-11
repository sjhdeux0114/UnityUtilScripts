using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Waypoint
{
    public Vector3 position;
    [Range(-180f, 180f)] public float angle;    // 나가는 방향 각도
    [Range(0f, 10f)] public float strength = 2f; // 곡률 강도

    public Vector3 GetTangent()
    {
        // 2D 기준으로 Z축 회전을 통해 방향 벡터 계산 (기본 방향: 오른쪽)
        return Quaternion.Euler(0, 0, angle) * Vector3.right;
    }
}
public enum FOLLOW_DIRECTION
{
    NONE,
    UP,
    DOWN,
    LEFT,
    RIGHT,
}

public class MoveTween : MonoBehaviour
{
    public enum MoveMode
    {
        Once,
        Loop,
        PingPong
    }

    [Header("Waypoints")]
    public List<Waypoint> waypoints = new List<Waypoint>();

    [Header("Settings")]
    public float duration = 2.0f;
    public Ease easeType = Ease.Linear;
    public MoveMode moveMode = MoveMode.Once;
    public PathType pathType = PathType.CatmullRom;
    public bool playOnEnable = true;
    public bool AutoHide = false;
    public UnityEvent OnCompleteEvent;

    private DG.Tweening.Tween moveTween;
    [Range(0.0f, 1.0f)]
    public float PreView = 0;
    public FOLLOW_DIRECTION FollowDir = FOLLOW_DIRECTION.NONE;
    public bool bLocal;
    bool _isPlaying;
    public bool _IsPlaying()
    {
        return _isPlaying;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        if (waypoints != null && waypoints.Count > 0)
        {
            bool isUI = GetComponentInParent<Canvas>() != null;
            float fixedLocalZ = transform.localPosition.z;

            // PreView 값(0~1)에 따라 경로상의 위치 및 회전 적용
            UpdateTransform(PreView);

            // UI의 경우 인스펙터에서 Z값이 변경되는 것을 막기 위해 이전 Z값 유지
            if (isUI)
            {
                Vector3 lp = transform.localPosition;
                lp.z = fixedLocalZ;
                transform.localPosition = lp;
            }
        }
    }
#endif

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    public void RePlay()
    {
        if (!_IsPlaying())
            Play();
    }

    [ContextMenu("Play")]
    public void Play()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        moveTween?.Kill();

        // 캔버스 하위 UI 요소인지 확인
        bool isUI = GetComponentInParent<Canvas>() != null;
        float fixedLocalZ = transform.localPosition.z;

        // 오브젝트를 시작 위치 및 회전으로 초기화
        UpdateTransform(0f);
        if (isUI)
        {
            Vector3 lp = transform.localPosition;
            lp.z = fixedLocalZ;
            transform.localPosition = lp;
        }

        int loops = (moveMode == MoveMode.Once) ? 1 : -1;
        LoopType loopType = (moveMode == MoveMode.PingPong) ? LoopType.Yoyo : LoopType.Restart;

        // 0에서 1까지의 진행률을 트윈
        float progress = 0f;
        moveTween = DOTween.To(() => progress, x =>
        {
            progress = x;
            // 월드 좌표 이동 및 회전 적용
            UpdateTransform(progress);

            // UI의 경우 인스펙터의 Pos Z값이 바뀌는 것을 방지하기 위해 로컬 Z 고정
            if (isUI)
            {
                Vector3 lp = transform.localPosition;
                lp.z = fixedLocalZ;
                transform.localPosition = lp;
            }
        }, 1f, duration)
        .SetEase(easeType)
        .SetLoops(loops, loopType)
        .SetAutoKill(false)
        .OnComplete(() =>
        {

            if (moveMode == MoveMode.Once)
            {
                if (AutoHide)
                    gameObject.SetActive(false);

                OnCompleteEvent?.Invoke();
                _isPlaying = false;
            }
        });
    }



    // 진행률(0~1)에 따른 전체 경로상의 위치 계산
    private Vector3 GetPointOnPath(float t)
    {
        if (waypoints == null || waypoints.Count == 0) return bLocal ? transform.localPosition : transform.position;
        if (waypoints.Count == 1) return waypoints[0].position;

        // 현재 어느 세그먼트(지점 사이)에 있는지 계산
        float totalSegments = waypoints.Count - 1;
        float currentPos = t * totalSegments;
        int index = Mathf.FloorToInt(currentPos);
        float segmentT = currentPos - index;

        // 마지막 지점 예외 처리
        if (index >= waypoints.Count - 1)
        {
            return waypoints[waypoints.Count - 1].position;
        }

        if (pathType == PathType.Linear)
        {
            return Vector3.Lerp(waypoints[index].position, waypoints[index + 1].position, segmentT);
        }
        else if (pathType == PathType.CubicBezier)
        {
            Waypoint start = waypoints[index];
            Waypoint end = waypoints[index + 1];
            Vector3 c1 = start.position + start.GetTangent() * start.strength;
            Vector3 c2 = end.position - end.GetTangent() * end.strength;
            return GetBezierPoint(start.position, c1, c2, end.position, segmentT);
        }
        else // CatmullRom
        {
            Vector3 p0 = (index == 0) ? waypoints[index].position : waypoints[index - 1].position;
            Vector3 p1 = waypoints[index].position;
            Vector3 p2 = waypoints[index + 1].position;
            Vector3 p3 = (index == waypoints.Count - 2) ? waypoints[index + 1].position : waypoints[index + 2].position;
            return GetCatmullRomPoint(p0, p1, p2, p3, segmentT);
        }
    }

    private void UpdateTransform(float t)
    {
        Vector3 currentPos = GetPointOnPath(t);
        if (bLocal)
        {
            transform.localPosition = currentPos;
        }
        else
        {
            transform.position = currentPos;
        }

        if (FollowDir != FOLLOW_DIRECTION.NONE)
        {
            float delta = 0.01f;
            Vector3 p1, p2;

            if (t <= 1f - delta)
            {
                p1 = currentPos;
                p2 = GetPointOnPath(t + delta);
            }
            else
            {
                p1 = GetPointOnPath(t - delta);
                p2 = currentPos;
            }

            Vector3 dir = (p2 - p1).normalized;
            if (dir != Vector3.zero)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                if (FollowDir == FOLLOW_DIRECTION.UP)
                {
                    angle += 90f;
                }
                else if (FollowDir == FOLLOW_DIRECTION.DOWN)
                {
                    angle -= 90f;
                }
                else if (FollowDir == FOLLOW_DIRECTION.LEFT)
                {
                    angle += 180f;
                }
                else if (FollowDir == FOLLOW_DIRECTION.RIGHT)
                {
                    angle += 0f;
                }

                if (bLocal)
                {
                    transform.localRotation = Quaternion.Euler(0, 0, angle);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }
    }

    public void Stop()
    {
        moveTween?.Kill();
        _isPlaying = false;

    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        System.Func<Vector3, Vector3> toWorldPos = (localPos) =>
        {
            if (bLocal && transform.parent != null)
                return transform.parent.TransformPoint(localPos);
            return localPos;
        };

        System.Func<Vector3, Vector3> toWorldDir = (localDir) =>
        {
            if (bLocal && transform.parent != null)
                return transform.parent.TransformDirection(localDir);
            return localDir;
        };

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 wPos = toWorldPos(waypoints[i].position);
            Gizmos.DrawSphere(wPos, 0.15f);

            // 방향 탄젠트 표시 (노란색 선)
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(wPos, wPos + toWorldDir(waypoints[i].GetTangent()) * (waypoints[i].strength * 0.5f));
            Gizmos.color = Color.cyan;
        }

        if (waypoints.Count < 2) return;

        if (pathType == PathType.Linear)
        {
            for (int i = 0; i < waypoints.Count - 1; i++)
                Gizmos.DrawLine(toWorldPos(waypoints[i].position), toWorldPos(waypoints[i + 1].position));
        }
        else if (pathType == PathType.CatmullRom)
        {
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Vector3 p0 = (i == 0) ? waypoints[i].position : waypoints[i - 1].position;
                Vector3 p1 = waypoints[i].position;
                Vector3 p2 = waypoints[i + 1].position;
                Vector3 p3 = (i == waypoints.Count - 2) ? waypoints[i + 1].position : waypoints[i + 2].position;

                Vector3 prevPos = toWorldPos(p1);
                for (int n = 1; n <= 20; n++)
                {
                    float t = n / 20f;
                    Vector3 currentPos = toWorldPos(GetCatmullRomPoint(p0, p1, p2, p3, t));
                    Gizmos.DrawLine(prevPos, currentPos);
                    prevPos = currentPos;
                }
            }
        }
        else if (pathType == PathType.CubicBezier)
        {
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Waypoint start = waypoints[i];
                Waypoint end = waypoints[i + 1];

                Vector3 c1 = start.position + start.GetTangent() * start.strength;
                Vector3 c2 = end.position - end.GetTangent() * end.strength;

                Vector3 prevPos = toWorldPos(start.position);
                for (int n = 1; n <= 20; n++)
                {
                    float t = n / 20f;
                    Vector3 currentPos = toWorldPos(GetBezierPoint(start.position, c1, c2, end.position, t));
                    Gizmos.DrawLine(prevPos, currentPos);
                    prevPos = currentPos;
                }
            }
        }
    }

    private Vector3 GetCatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t; float t3 = t2 * t;
        float f1 = -0.5f * t3 + t2 - 0.5f * t;
        float f2 = 1.5f * t3 - 2.5f * t2 + 1.0f;
        float f3 = -1.5f * t3 + 2.0f * t2 + 0.5f * t;
        float f4 = 0.5f * t3 - 0.5f * t2;
        return p0 * f1 + p1 * f2 + p2 * f3 + p3 * f4;
    }

    private Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
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



