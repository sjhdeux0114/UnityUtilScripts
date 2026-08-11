#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace OniMusha.Utils
{
    [CustomEditor(typeof(SmoothGridLayout), true)]
    [CanEditMultipleObjects]
    public class SmoothGridLayoutEditor : Editor
    {
        private SerializedProperty m_UseSmooth;
        private SerializedProperty m_SmoothSpeed;
        private SerializedProperty m_SmoothType;
        private SerializedProperty m_SmoothTime;
        private SerializedProperty m_SnapNewChildren;

        private SerializedProperty m_Padding;
        private SerializedProperty m_CellSize;
        private SerializedProperty m_Spacing;
        private SerializedProperty m_StartCorner;
        private SerializedProperty m_StartAxis;
        private SerializedProperty m_ChildAlignment;
        private SerializedProperty m_Constraint;
        private SerializedProperty m_ConstraintCount;

        protected virtual void OnEnable()
        {
            m_UseSmooth = serializedObject.FindProperty("m_UseSmooth");
            m_SmoothSpeed = serializedObject.FindProperty("m_SmoothSpeed");
            m_SmoothType = serializedObject.FindProperty("m_SmoothType");
            m_SmoothTime = serializedObject.FindProperty("m_SmoothTime");
            m_SnapNewChildren = serializedObject.FindProperty("m_SnapNewChildren");

            m_Padding = serializedObject.FindProperty("m_Padding");
            m_CellSize = serializedObject.FindProperty("m_CellSize");
            m_Spacing = serializedObject.FindProperty("m_Spacing");
            m_StartCorner = serializedObject.FindProperty("m_StartCorner");
            m_StartAxis = serializedObject.FindProperty("m_StartAxis");
            m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
            m_Constraint = serializedObject.FindProperty("m_Constraint");
            m_ConstraintCount = serializedObject.FindProperty("m_ConstraintCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (m_Padding != null) EditorGUILayout.PropertyField(m_Padding, true);
            if (m_CellSize != null) EditorGUILayout.PropertyField(m_CellSize, true);
            if (m_Spacing != null) EditorGUILayout.PropertyField(m_Spacing, true);
            if (m_StartCorner != null) EditorGUILayout.PropertyField(m_StartCorner, true);
            if (m_StartAxis != null) EditorGUILayout.PropertyField(m_StartAxis, true);
            if (m_ChildAlignment != null) EditorGUILayout.PropertyField(m_ChildAlignment, true);
            if (m_Constraint != null) EditorGUILayout.PropertyField(m_Constraint, true);
            if (m_Constraint != null && m_Constraint.enumValueIndex != 0 && m_ConstraintCount != null)
            {
                EditorGUILayout.PropertyField(m_ConstraintCount, true);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Smooth Animation Settings", EditorStyles.boldLabel);
            if (m_UseSmooth != null) EditorGUILayout.PropertyField(m_UseSmooth);
            if (m_UseSmooth != null && m_UseSmooth.boolValue)
            {
                if (m_SmoothType != null) EditorGUILayout.PropertyField(m_SmoothType);
                if (m_SmoothType != null && m_SmoothType.enumValueIndex == (int)SmoothGridLayout.SmoothType.Lerp)
                {
                    if (m_SmoothSpeed != null) EditorGUILayout.PropertyField(m_SmoothSpeed);
                }
                else if (m_SmoothTime != null)
                {
                    EditorGUILayout.PropertyField(m_SmoothTime);
                }
                if (m_SnapNewChildren != null) EditorGUILayout.PropertyField(m_SnapNewChildren);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
