using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OniMusha.Utils
{
    /// <summary>
    /// Unity 표준 GridLayoutGroup의 모든 기능(Padding, CellSize, Spacing, StartCorner, StartAxis, ChildAlignment, Constraint)을 
    /// 상속받아 동일하게 제공하면서, 자식 오브젝트의 재배치 위치를 Vector2.Lerp 또는 SmoothDamp로 부드럽게 보간 이동시켜 주는 컴포넌트입니다.
    /// </summary>
    [AddComponentMenu("UI/Smooth Grid Layout Group", 153)]
    [DisallowMultipleComponent]
    public class SmoothGridLayout : GridLayoutGroup
    {
        public enum SmoothType
        {
            Lerp,
            SmoothDamp
        }

        [Header("Smooth Animation Settings")]
        [SerializeField] private bool m_UseSmooth = true;
        [SerializeField] private float m_SmoothSpeed = 15f;
        [SerializeField] private SmoothType m_SmoothType = SmoothType.Lerp;
        [SerializeField] private float m_SmoothTime = 0.15f;
        [SerializeField] private bool m_SnapNewChildren = true;

        public bool useSmooth { get => m_UseSmooth; set { m_UseSmooth = value; } }
        public float smoothSpeed { get => m_SmoothSpeed; set => m_SmoothSpeed = value; }
        public SmoothType smoothType { get => m_SmoothType; set => m_SmoothType = value; }
        public float smoothTime { get => m_SmoothTime; set => m_SmoothTime = value; }
        public bool snapNewChildren { get => m_SnapNewChildren; set => m_SnapNewChildren = value; }

        private readonly Dictionary<RectTransform, Vector2> m_TargetPositions = new Dictionary<RectTransform, Vector2>();
        private readonly Dictionary<RectTransform, Vector2> m_CurrentVelocities = new Dictionary<RectTransform, Vector2>();
        private readonly List<RectTransform> m_TrackedKeys = new List<RectTransform>();

        protected override void OnEnable()
        {
            base.OnEnable();
            m_TargetPositions.Clear();
            m_CurrentVelocities.Clear();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_TargetPositions.Clear();
            m_CurrentVelocities.Clear();
        }

        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }

        private void SetCellsAlongAxis(int axis)
        {
            int count = rectChildren.Count;
            if (count == 0)
            {
                m_TargetPositions.Clear();
                return;
            }

            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            int cellCountX = 1;
            int cellCountY = 1;

            if (constraint == Constraint.FixedColumnCount)
            {
                cellCountX = Mathf.Clamp(constraintCount, 1, count);
                cellCountY = Mathf.CeilToInt(count / (float)cellCountX);
            }
            else if (constraint == Constraint.FixedRowCount)
            {
                cellCountY = Mathf.Clamp(constraintCount, 1, count);
                cellCountX = Mathf.CeilToInt(count / (float)cellCountY);
            }
            else
            {
                if (cellSize.x + spacing.x <= 0)
                    cellCountX = int.MaxValue;
                else
                    cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

                if (cellSize.y + spacing.y <= 0)
                    cellCountY = int.MaxValue;
                else
                    cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
            }

            int cornerX = (int)startCorner % 2;
            int cornerY = (int)startCorner / 2;

            int actualCellCountX, actualCellCountY;
            if (startAxis == Axis.Horizontal)
            {
                actualCellCountX = Mathf.Clamp(cellCountX, 1, count);
                actualCellCountY = Mathf.CeilToInt(count / (float)actualCellCountX);
            }
            else
            {
                actualCellCountY = Mathf.Clamp(cellCountY, 1, count);
                actualCellCountX = Mathf.CeilToInt(count / (float)actualCellCountY);
            }

            Vector2 requiredSpace = new Vector2(
                actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
                actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
            );

            Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
            );

            for (int i = 0; i < count; i++)
            {
                int positionX;
                int positionY;

                if (startAxis == Axis.Horizontal)
                {
                    positionX = i % actualCellCountX;
                    positionY = i / actualCellCountX;
                }
                else
                {
                    positionX = i / actualCellCountY;
                    positionY = i % actualCellCountY;
                }

                if (cornerX == 1)
                    positionX = actualCellCountX - 1 - positionX;
                if (cornerY == 1)
                    positionY = actualCellCountY - 1 - positionY;

                RectTransform child = rectChildren[i];

                if (axis == 0)
                {
                    float posX = startOffset.x + (cellSize.x + spacing.x) * positionX;
                    Vector2 prevPos = child.anchoredPosition;

                    SetChildAlongAxis(child, 0, posX, cellSize.x);
                    float targetX = child.anchoredPosition.x;

                    if (Application.isPlaying && m_UseSmooth)
                    {
                        bool isNew = !m_TargetPositions.ContainsKey(child);
                        if (isNew && !m_SnapNewChildren)
                        {
                            Vector2 current = child.anchoredPosition;
                            current.x = prevPos.x;
                            child.anchoredPosition = current;
                        }
                        else if (!isNew)
                        {
                            Vector2 current = child.anchoredPosition;
                            current.x = prevPos.x;
                            child.anchoredPosition = current;
                        }

                        if (!m_TargetPositions.TryGetValue(child, out Vector2 target))
                            target = child.anchoredPosition;

                        target.x = targetX;
                        m_TargetPositions[child] = target;
                    }
                }
                else if (axis == 1)
                {
                    float posY = startOffset.y + (cellSize.y + spacing.y) * positionY;
                    Vector2 prevPos = child.anchoredPosition;

                    SetChildAlongAxis(child, 1, posY, cellSize.y);
                    float targetY = child.anchoredPosition.y;

                    if (Application.isPlaying && m_UseSmooth)
                    {
                        bool isNew = !m_TargetPositions.ContainsKey(child);
                        if (isNew && !m_SnapNewChildren)
                        {
                            Vector2 current = child.anchoredPosition;
                            current.y = prevPos.y;
                            child.anchoredPosition = current;
                        }
                        else if (!isNew)
                        {
                            Vector2 current = child.anchoredPosition;
                            current.y = prevPos.y;
                            child.anchoredPosition = current;
                        }

                        if (!m_TargetPositions.TryGetValue(child, out Vector2 target))
                            target = child.anchoredPosition;

                        target.y = targetY;
                        m_TargetPositions[child] = target;
                    }
                }
            }
        }

        protected virtual void Update()
        {
            if (!Application.isPlaying || !m_UseSmooth)
                return;

            CleanUpDestroyedChildren();
            AnimateChildren();
        }

        private void CleanUpDestroyedChildren()
        {
            m_TrackedKeys.Clear();
            foreach (var key in m_TargetPositions.Keys)
            {
                m_TrackedKeys.Add(key);
            }

            for (int i = m_TrackedKeys.Count - 1; i >= 0; i--)
            {
                var child = m_TrackedKeys[i];
                if (child == null || !child || !child.IsChildOf(transform) || !child.gameObject.activeInHierarchy)
                {
                    m_TargetPositions.Remove(child);
                    m_CurrentVelocities.Remove(child);
                }
            }
        }

        private void AnimateChildren()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            foreach (var kvp in m_TargetPositions)
            {
                RectTransform child = kvp.Key;
                if (child == null || !child || !child.gameObject.activeInHierarchy) continue;

                Vector2 target = kvp.Value;
                Vector2 current = child.anchoredPosition;

                if (Vector2.SqrMagnitude(current - target) > 0.01f)
                {
                    if (m_SmoothType == SmoothType.Lerp)
                    {
                        float t = 1f - Mathf.Exp(-m_SmoothSpeed * deltaTime);
                        child.anchoredPosition = Vector2.Lerp(current, target, t);
                    }
                    else if (m_SmoothType == SmoothType.SmoothDamp)
                    {
                        if (!m_CurrentVelocities.TryGetValue(child, out Vector2 vel))
                            vel = Vector2.zero;

                        Vector2 nextPos = Vector2.SmoothDamp(current, target, ref vel, m_SmoothTime, float.PositiveInfinity, deltaTime);
                        m_CurrentVelocities[child] = vel;
                        child.anchoredPosition = nextPos;
                    }
                }
                else
                {
                    child.anchoredPosition = target;
                }
            }
        }
    }
}
