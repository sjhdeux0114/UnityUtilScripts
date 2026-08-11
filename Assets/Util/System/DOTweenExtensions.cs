using DG.Tweening;
using UnityEngine;

public static class DOTweenExtensions
{
    /// <summary>
    /// CurveX, CurveY, CurveScale 기반의 곡선 오프셋 이동 트윈
    /// </summary>
    /// <param name="target">이동할 Transform (RectTransform 포함)</param>
    /// <param name="targetPos">목적지 위치</param>
    /// <param name="duration">이동 시간</param>
    /// <param name="curveX">X축 오프셋 AnimationCurve (-1 ~ +1)</param>
    /// <param name="curveY">Y축 오프셋 AnimationCurve (-1 ~ +1)</param>
    /// <param name="curveScale">커브 오프셋 강도/스케일 (기본값: 200)</param>
    /// <returns>Tweener (SetEase, OnComplete 등 체이닝 가능)</returns>
    public static Tweener DOCurveMove(
        this Transform target,
        Vector3 targetPos,
        float duration,
        AnimationCurve curveX,
        AnimationCurve curveY,
        float curveScale = 200f)
    {
        if (target == null) return null;

        Vector3 startPos = target.position;
        float dir = (startPos.x > targetPos.x) ? -1f : 1f;

        return DOVirtual.Float(0f, 1f, duration, t =>
        {
            if (target == null) return;

            // 1. 기본 선형 위치 (DOMove 기준)
            float posX = Mathf.Lerp(startPos.x, targetPos.x, t);
            float posY = Mathf.Lerp(startPos.y, targetPos.y, t);

            // 2. CurveX 오프셋 적용
            if (curveX != null && curveX.length > 0)
            {
                posX += curveX.Evaluate(t) * curveScale * dir;
            }

            // 3. CurveY 오프셋 적용
            if (curveY != null && curveY.length > 0)
            {
                posY += curveY.Evaluate(t) * curveScale;
            }

            target.position = new Vector3(posX, posY, startPos.z);
        })
        .SetEase(Ease.Linear)
        .SetTarget(target);
    }
}
