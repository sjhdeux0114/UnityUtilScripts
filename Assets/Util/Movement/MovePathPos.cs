using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public enum CMOVE_TYPE
{
    TOTAL = 0,
    ONE_PATH,
}
public enum CLINE_TYPE
{
    LINE = 0,
    BEZIER,
}



[System.Serializable]
public class CMovePath
{
    public Vector3 Pos;
    public Vector3 Scale;
    public Vector3 Rotation;
    public float Speed = 1.0f;
    public AnimationCurve Curve;
    [Range(-180f, 180f)] public float angle = 0f;
    public float strength = 2f;
    public bool bCustomBezier = false;
    public Vector3 Tangent = Vector3.zero;

    public Vector3 GetTangent()
    {
        return Quaternion.Euler(0, 0, angle) * Vector3.right;
    }

    public CMovePath(CMovePath src)
    {
        if (src == null)
        {
            Pos = Vector3.zero;
            Scale = Vector3.one;
            Rotation = Vector3.zero;
            Curve = new AnimationCurve();
            Curve.AddKey(new Keyframe(0, 0));
            Curve.AddKey(new Keyframe(1, 1));
            angle = 0f;
            strength = 2f;
            bCustomBezier = false;
            Tangent = Vector3.zero;
        }
        else
        {
            Pos = src.Pos;
            Scale = src.Scale;
            Rotation = src.Rotation;
            Speed = src.Speed;
            Curve = src.Curve != null ? new AnimationCurve(src.Curve.keys) : new AnimationCurve();
            angle = src.angle;
            strength = src.strength;
            bCustomBezier = src.bCustomBezier;
            Tangent = src.Tangent;
        }
    }

}
public class MovePathPos : MonoBehaviour
{
    public CMOVE_TYPE PathMode = CMOVE_TYPE.TOTAL;
    public CLINE_TYPE LineMode = CLINE_TYPE.LINE;
    public List<CMovePath> TargetPoints = new List<CMovePath>();
    public bool bLocal;
    float fSpeed;
    // 최소/최대 속도를 관리할 변수 추가
    public float minSpeed = 1f;
    public float maxSpeed = 5f;
    float Speed_X = 1;
    private int currentPointIndex = 0;

    public bool bAwake = false;
    public bool bAutoHide = false;
    public bool bLoop = false;
    public Vector3 Rnd_Pos_Min;
    public Vector3 Rnd_Pos_Max;
    Vector3 Rnd_Pos;
    public float MaxTime = -1;
    public Color Color_Gizmo = Color.green;
    public float GizmoRadius = 0.05f;

    public float MaxPer = 1;

    public UnityAction Act_End;

    public UnityAction<int> Act_Start;
    public UnityAction<int> Act_Next;



    float OnTimes = 0;
    // ✅ 추가된 Lock 옵션
    public bool Lock_X;
    public bool Lock_Y;
    public bool Lock_Z;

    private void OnEnable()
    {
        OnTimes = 0;
        if (bAwake)
            Play();
    }

    int NowSegment = 0;
    public int NextCall = -1;
    public int StartCall = -1;

    public void Set_Speed(float val = 1.0f)
    {
        Speed_X = val;
    }

    private void ApplyFinalState(int endIndex)
    {
        // endIndex: 마지막으로 맞추고 싶은 TargetPoints의 인덱스
        Vector3 pos = TargetPoints[endIndex].Pos + Rnd_Pos;
        Vector3 scale = TargetPoints[endIndex].Scale;
        Quaternion rot = Quaternion.Euler(TargetPoints[endIndex].Rotation);

        // 🔒 Lock 적용
        if (Lock_X) pos.x = bLocal ? transform.localPosition.x : transform.position.x;
        if (Lock_Y) pos.y = bLocal ? transform.localPosition.y : transform.position.y;
        if (Lock_Z) pos.z = bLocal ? transform.localPosition.z : transform.position.z;

        if (bLocal) transform.localPosition = pos;
        else transform.position = pos;

        transform.localScale = scale;
        transform.rotation = rot;
    }

    public void _Update(float dt)
    {
        if (isPlaying)
        {
            Vector3 finalPos = Vector3.zero;
            switch (PathMode)
            {
                case CMOVE_TYPE.TOTAL:
                    {
                        int OldSegment = NowSegment;
                        times += dt;
                        float normalizedTime = times / fSpeed * Speed_X;

                        if (normalizedTime >= 1.0f)
                        {
                            NextCall = OldSegment;
                            if (Act_Next != null) Act_Next.Invoke(OldSegment);

                            normalizedTime = 1.0f;
                            ApplyFinalState(currentPointIndex + 1);

                            isPlaying = false;
                            if (bAutoHide) gameObject.SetActive(false);

                            if (bLoop)
                            {
                                if (OnTimes >= MaxTime && MaxTime > 0)
                                {
                                    isPlaying = false;
                                    Act_End?.Invoke();
                                }
                                else
                                    Play();
                            }
                            else
                                Act_End?.Invoke();

                            return;
                        }

                        float startTimeRatio = 0f;
                        for (int i = 0; i < TargetPoints.Count - 1; i++)
                        {
                            float endTimeRatio = segmentTimes[i];
                            if (normalizedTime <= endTimeRatio)
                            {
                                NowSegment = i;
                                if (OldSegment != NowSegment)
                                {
                                    NextCall = OldSegment;
                                    StartCall = NowSegment;
                                    Act_Next?.Invoke(OldSegment);
                                    Act_Start?.Invoke(NowSegment);
                                }

                                float segmentRatioLength = endTimeRatio - startTimeRatio;
                                float t = (normalizedTime - startTimeRatio) / segmentRatioLength;

                                Vector3 startSegment = TargetPoints[i].Pos + Rnd_Pos;
                                Vector3 endSegment = TargetPoints[i + 1].Pos + Rnd_Pos;

                                Vector3 startSegmentScale = TargetPoints[i].Scale;
                                Vector3 endSegmentScale = TargetPoints[i + 1].Scale;

                                Quaternion startSegmentRot = Quaternion.Euler(TargetPoints[i].Rotation);
                                Quaternion endSegmentRot = Quaternion.Euler(TargetPoints[i + 1].Rotation);

                                float ret_t = TargetPoints[i].Curve.Evaluate(t);
                                if (LineMode == CLINE_TYPE.BEZIER)
                                {
                                    GetBezierControlPoints(i, out Vector3 c1, out Vector3 c2);
                                    finalPos = GetBezierPoint(startSegment, c1, c2, endSegment, ret_t);
                                }
                                else
                                {
                                    finalPos = Vector3.LerpUnclamped(startSegment, endSegment, ret_t);
                                }

                                // 🔒 Lock 적용
                                if (Lock_X) finalPos.x = bLocal ? transform.localPosition.x : transform.position.x;
                                if (Lock_Y) finalPos.y = bLocal ? transform.localPosition.y : transform.position.y;
                                if (Lock_Z) finalPos.z = bLocal ? transform.localPosition.z : transform.position.z;

                                if (bLocal) transform.localPosition = finalPos;
                                else transform.position = finalPos;

                                transform.localScale = Vector3.LerpUnclamped(startSegmentScale, endSegmentScale, ret_t);
                                transform.rotation = Quaternion.Slerp(startSegmentRot, endSegmentRot, ret_t);
                                return;
                            }
                            startTimeRatio = endTimeRatio;
                        }
                    }
                    break;

                case CMOVE_TYPE.ONE_PATH:
                    {
                        times += dt * TargetPoints[currentPointIndex].Speed;
                        float normalizedTime = Mathf.Clamp01(times / fSpeed * Speed_X);

                        if (normalizedTime >= MaxPer)
                        {
                            normalizedTime = MaxPer;

                            if (MaxPer >= 1)
                            {
                                ApplyFinalState(currentPointIndex + 1);

                                if (bAutoHide) gameObject.SetActive(false);
                                isPlaying = false;
                                Act_Next?.Invoke(currentPointIndex);
                                Act_End?.Invoke();
                                return;
                            }
                            else
                            {
                                times = fSpeed * MaxPer * Speed_X;
                                Act_End?.Invoke();
                            }
                        }

                        float currentDistance = normalizedTime * totalPathDistance;

                        Vector3 startSegment = TargetPoints[currentPointIndex].Pos + Rnd_Pos;
                        Vector3 endSegment = TargetPoints[currentPointIndex + 1].Pos + Rnd_Pos;

                        Vector3 startSegmentScale = TargetPoints[currentPointIndex].Scale;
                        Vector3 endSegmentScale = TargetPoints[currentPointIndex + 1].Scale;

                        Quaternion startSegmentRot = Quaternion.Euler(TargetPoints[currentPointIndex].Rotation);
                        Quaternion endSegmentRot = Quaternion.Euler(TargetPoints[currentPointIndex + 1].Rotation);

                        float segmentDistance = (LineMode == CLINE_TYPE.BEZIER)
                            ? GetBezierSegmentLength(currentPointIndex)
                            : Vector3.Distance(startSegment, endSegment);
                        if (segmentDistance <= 0.0001f) segmentDistance = 0.0001f;

                        float t = currentDistance / segmentDistance;
                        float ret_t = TargetPoints[currentPointIndex].Curve.Evaluate(t);

                        if (LineMode == CLINE_TYPE.BEZIER)
                        {
                            GetBezierControlPoints(currentPointIndex, out Vector3 c1, out Vector3 c2);
                            finalPos = GetBezierPoint(startSegment, c1, c2, endSegment, ret_t);
                        }
                        else
                        {
                            finalPos = Vector3.LerpUnclamped(startSegment, endSegment, ret_t);
                        }

                        // 🔒 Lock 적용
                        if (Lock_X) finalPos.x = bLocal ? transform.localPosition.x : transform.position.x;
                        if (Lock_Y) finalPos.y = bLocal ? transform.localPosition.y : transform.position.y;
                        if (Lock_Z) finalPos.z = bLocal ? transform.localPosition.z : transform.position.z;

                        if (ret_t != float.NaN)
                        {



                            if (bLocal) transform.localPosition = finalPos;
                            else transform.position = finalPos;

                            transform.localScale = Vector3.LerpUnclamped(startSegmentScale, endSegmentScale, ret_t);
                            transform.rotation = Quaternion.Slerp(startSegmentRot, endSegmentRot, ret_t);
                        }
                    }
                    break;
            }
        }
    }

    void Update()
    {
        if (bPause) return;
        _Update(Time.deltaTime);
    }
    public void Pause(bool b)
    {
        if (bPause == b) return;
        bPause = b;
        if (!bPause)
        {
            // --- Logic to resume from the new position starts here ---
            if (isPlaying && PathMode == CMOVE_TYPE.TOTAL)
            {
                // 1. Get the current position (with offset)
                Vector3 currentPos = bLocal ? transform.localPosition : transform.position;
                currentPos -= Rnd_Pos; // Remove the random offset to compare with TargetPoints

                float accumulatedNormalizedTime = 0f;
                float closestDistance = float.MaxValue;
                int closestSegment = -1;
                float closestTimeInSegment = 0f; // t value (0 to 1) within the segment

                // Find the closest point/segment on the path to the current position
                for (int i = 0; i < TargetPoints.Count - 1; i++)
                {
                    Vector3 startSegment = TargetPoints[i].Pos;
                    Vector3 endSegment = TargetPoints[i + 1].Pos;

                    // Project the current position onto the line segment
                    Vector3 segmentVector = endSegment - startSegment;
                    float segmentLengthSq = segmentVector.sqrMagnitude;

                    // If segmentLengthSq is zero (points are the same), skip
                    if (segmentLengthSq < 0.0001f)
                    {
                        accumulatedNormalizedTime = segmentTimes[i];
                        continue;
                    }

                    // Calculate t (0 to 1) for the projection on the segment
                    float t = Vector3.Dot(currentPos - startSegment, segmentVector) / segmentLengthSq;
                    t = Mathf.Clamp01(t); // Clamp t to be between 0 and 1

                    // Find the closest point on the segment
                    Vector3 closestPointOnSegment = Vector3.Lerp(startSegment, endSegment, t);
                    float distanceToPath = Vector3.Distance(currentPos, closestPointOnSegment);

                    if (distanceToPath < closestDistance)
                    {
                        closestDistance = distanceToPath;
                        closestSegment = i;
                        closestTimeInSegment = t;
                    }

                    // Update accumulated time for the start of the next segment
                    if (i < segmentTimes.Count)
                    {
                        // Use the accumulated time ratio for the start of the next segment
                        accumulatedNormalizedTime = segmentTimes[i];
                    }
                }

                // 2. Calculate the corresponding total normalized time
                if (closestSegment != -1)
                {
                    float startTimeRatio = closestSegment == 0 ? 0f : segmentTimes[closestSegment - 1];
                    float endTimeRatio = segmentTimes[closestSegment];
                    float segmentRatioLength = endTimeRatio - startTimeRatio;

                    // Calculate the normalized time (0 to 1) for the entire path
                    float totalNormalizedTime = startTimeRatio + (segmentRatioLength * closestTimeInSegment);

                    // 3. Reverse-calculate 'times' and set the current segment
                    times = totalNormalizedTime * fSpeed * Speed_X;
                    NowSegment = closestSegment;

                    // Re-calculate the actual position/scale/rotation to ensure consistency before resuming
                    _Update(0);

                    //Debug.Log($"Resuming from manual position. Calculated total normalized time: {totalNormalizedTime:F3}. Resuming segment: {NowSegment}");
                }
            }
            // --- Logic to resume from the new position ends here ---
        }
    }
    public bool bPause = false;

    float totalPathDistance = 0f;
    float times;
    public bool isPlaying = false;

    // MovePathPos 클래스 내부에 추가
    float totalSpeedRatio = 0f;
    List<float> segmentTimes = new List<float>(); // 각 세그먼트가 전체 시간 중 차지하는 비율 (0~1)

    public void PlayIndex(int n)
    {
        if (TargetPoints.Count - 1 <= n || n < 0) return;

        Play(n);


    }

    public void Set_Pos_Index(int n)
    {
        if (TargetPoints.Count <= n || n < 0) return;
        if (bLocal)
            transform.localPosition = TargetPoints[n].Pos;
        else
            transform.position = TargetPoints[n].Pos;
        transform.localScale = TargetPoints[n].Scale;
        transform.rotation = Quaternion.Euler(TargetPoints[n].Rotation);
        isPlaying = false;
    }
    public void Play(int n = 0)
    {
        NextCall = -1;
        StartCall = -1;
        if (TargetPoints.Count < 2)
        {
            Debug.LogWarning("Path needs at least two points to simulate.");
            return;
        }
        Rnd_Pos.x = Random.Range(Rnd_Pos_Min.x, Rnd_Pos_Max.x);
        Rnd_Pos.y = Random.Range(Rnd_Pos_Min.y, Rnd_Pos_Max.y);
        Rnd_Pos.z = Random.Range(Rnd_Pos_Min.z, Rnd_Pos_Max.z);

        fSpeed = Random.Range(minSpeed, maxSpeed);

        // 1. 전체 Speed 비율 합계 계산
        totalSpeedRatio = 0f;
        for (int i = 0; i < TargetPoints.Count - 1; i++)
        {
            // 세그먼트 i -> i+1 의 속도는 TargetPoints[i]의 Speed를 사용
            if (TargetPoints[i].Speed <= 0) TargetPoints[i].Speed = 1;
            totalSpeedRatio += TargetPoints[i].Speed;
        }

        // 2. 각 세그먼트의 시간 비율 (정규화된 시간) 계산
        segmentTimes.Clear();
        float accumulatedRatio = 0f;
        for (int i = 0; i < TargetPoints.Count - 1; i++)
        {
            float ratio = TargetPoints[i].Speed / totalSpeedRatio; // 이 세그먼트가 차지하는 시간 비율
            accumulatedRatio += ratio;
            // segmentTimes에는 해당 세그먼트가 끝나는 시점의 '전체 시간 대비 누적 비율'을 저장
            segmentTimes.Add(accumulatedRatio);
        }



        totalPathDistance = 0f;

        currentPointIndex = n;
        times = 0;

        if (PathMode == CMOVE_TYPE.TOTAL)
        {
            for (int i = 0; i < TargetPoints.Count - 1; i++)
            {
                if (LineMode == CLINE_TYPE.BEZIER)
                {
                    totalPathDistance += GetBezierSegmentLength(i);
                }
                else
                {
                    Vector3 currentPos = TargetPoints[i].Pos + Rnd_Pos;
                    Vector3 nextPos = TargetPoints[i + 1].Pos + Rnd_Pos;
                    totalPathDistance += Vector3.Distance(currentPos, nextPos);
                }
            }
        }
        else
        {
            if (LineMode == CLINE_TYPE.BEZIER)
            {
                totalPathDistance = GetBezierSegmentLength(n);
            }
            else
            {
                Vector3 currentPos = TargetPoints[n].Pos + Rnd_Pos;
                Vector3 nextPos = TargetPoints[n + 1].Pos + Rnd_Pos;
                totalPathDistance = Vector3.Distance(currentPos, nextPos);
            }
        }

        isPlaying = true;
        NowSegment = n;
        StartCall = NowSegment;
        if (Act_Start != null)
        {
            Act_Start.Invoke(NowSegment);
        }

        _Update(0);
    }

    #region Bezier Helpers

    public static Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        return uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
    }

    public Vector3 GetEffectiveTangent(int index)
    {
        if (TargetPoints == null || index < 0 || index >= TargetPoints.Count)
            return Vector3.zero;

        CMovePath pt = TargetPoints[index];
        if (pt.bCustomBezier)
        {
            return pt.GetTangent() * pt.strength;
        }

        if (pt.Tangent != Vector3.zero)
        {
            return pt.Tangent;
        }

        // Auto tangent
        Vector3 pStart = pt.Pos + Rnd_Pos;
        Vector3 pPrev;
        if (index > 0)
        {
            pPrev = TargetPoints[index - 1].Pos + Rnd_Pos;
        }
        else if (bLoop && TargetPoints.Count > 2)
        {
            pPrev = TargetPoints[TargetPoints.Count - 2].Pos + Rnd_Pos;
        }
        else
        {
            Vector3 pNextRef = (index + 1 < TargetPoints.Count) ? TargetPoints[index + 1].Pos + Rnd_Pos : pStart + Vector3.right;
            pPrev = pStart - (pNextRef - pStart);
        }

        Vector3 pNext;
        if (index + 1 < TargetPoints.Count)
        {
            pNext = TargetPoints[index + 1].Pos + Rnd_Pos;
        }
        else if (bLoop && TargetPoints.Count > 2)
        {
            pNext = TargetPoints[1].Pos + Rnd_Pos;
        }
        else
        {
            pNext = pStart + (pStart - pPrev);
        }

        Vector3 t = (pNext - pPrev) * 0.5f;
        return t / 3f;
    }

    public void GetBezierControlPoints(int index, out Vector3 c1, out Vector3 c2)
    {
        if (TargetPoints == null || index < 0 || index >= TargetPoints.Count - 1)
        {
            c1 = Vector3.zero;
            c2 = Vector3.zero;
            return;
        }

        Vector3 pStart = TargetPoints[index].Pos + Rnd_Pos;
        Vector3 pEnd = TargetPoints[index + 1].Pos + Rnd_Pos;

        c1 = pStart + GetEffectiveTangent(index);
        c2 = pEnd - GetEffectiveTangent(index + 1);
    }

    public float GetBezierSegmentLength(int index, int samples = 15)
    {
        if (TargetPoints == null || index < 0 || index >= TargetPoints.Count - 1) return 0f;

        GetBezierControlPoints(index, out Vector3 c1, out Vector3 c2);
        Vector3 pStart = TargetPoints[index].Pos + Rnd_Pos;
        Vector3 pEnd = TargetPoints[index + 1].Pos + Rnd_Pos;

        float length = 0f;
        Vector3 prev = pStart;
        for (int i = 1; i <= samples; i++)
        {
            float t = i / (float)samples;
            Vector3 pt = GetBezierPoint(pStart, c1, c2, pEnd, t);
            length += Vector3.Distance(prev, pt);
            prev = pt;
        }
        return length;
    }

    private Vector3 ToWorldGizmoPos(Vector3 pos)
    {
        if (bLocal)
        {
            if (transform.parent != null)
                return transform.parent.TransformPoint(pos);
            else
                return transform.TransformPoint(pos);
        }
        return pos;
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (TargetPoints == null || TargetPoints.Count == 0) return;

        Gizmos.color = Color_Gizmo;

        for (int i = 0; i < TargetPoints.Count; i++)
        {
            Vector3 worldPos = ToWorldGizmoPos(TargetPoints[i].Pos);
            float radius = GizmoRadius;
            Gizmos.DrawSphere(worldPos, radius);

#if UNITY_EDITOR
            // 오브젝트가 선택되지 않은 상태에서만 기즈모 라벨 표시 (선택 시에는 MovePathPosEditor의 인터랙티브 UI 표시)
            if (UnityEditor.Selection.activeGameObject != gameObject)
            {
                UnityEditor.Handles.Label(worldPos, $"  Point {i}");
            }
#endif
        }

        // 경로 선 그리기
        if (TargetPoints.Count > 1)
        {
            Gizmos.color = Color_Gizmo;
            if (LineMode == CLINE_TYPE.BEZIER)
            {
                for (int i = 0; i < TargetPoints.Count - 1; i++)
                {
                    GetBezierControlPoints(i, out Vector3 c1, out Vector3 c2);
                    Vector3 p0 = ToWorldGizmoPos(TargetPoints[i].Pos);
                    Vector3 p1 = ToWorldGizmoPos(c1);
                    Vector3 p2 = ToWorldGizmoPos(c2);
                    Vector3 p3 = ToWorldGizmoPos(TargetPoints[i + 1].Pos);

                    Vector3 prev = p0;
                    for (int s = 1; s <= 20; s++)
                    {
                        float t = s / 20f;
                        Vector3 current = GetBezierPoint(p0, p1, p2, p3, t);
                        Gizmos.DrawLine(prev, current);
                        prev = current;
                    }
                }
            }
            else
            {
                for (int i = 0; i < TargetPoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(ToWorldGizmoPos(TargetPoints[i].Pos), ToWorldGizmoPos(TargetPoints[i + 1].Pos));
                }
            }
        }
    }




}
