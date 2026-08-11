using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// -----------------------------------------------------------------------------
// 1. 메인 에디터 (TabbedEditor) - GUILayout 사용
// -----------------------------------------------------------------------------
[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class TabbedEditor : Editor
{
    private InspectorParser.LayoutData _layout;
    private int _selectedTabIndex = 0;

    private void OnEnable()
    {
        if (target == null) return;
        _layout = InspectorParser.GetLayout(target.GetType());
        _selectedTabIndex = PlayerPrefs.GetInt($"Tab_{target.GetInstanceID()}", 0);
    }

    private void OnDisable()
    {
        if (target != null) PlayerPrefs.SetInt($"Tab_{target.GetInstanceID()}", _selectedTabIndex);
    }

    public override void OnInspectorGUI()
    {
        if (_layout == null) OnEnable();
        if (_layout == null) 
        {
            base.OnInspectorGUI();
            return;
        }

        serializedObject.Update();

        SerializedProperty script = serializedObject.FindProperty("m_Script");
        if (script != null)
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(script);
            GUI.enabled = true;
        }

        if (_layout.HasCustomLayout)
        {
            InspectorDrawer.DrawLayout(serializedObject, _layout, ref _selectedTabIndex);
        }
        else
        {
            DrawPropertiesExcluding(serializedObject, "m_Script");
        }

        InspectorDrawer.DrawButtons(targets, _layout.Buttons);
        serializedObject.ApplyModifiedProperties();
    }
}

// -----------------------------------------------------------------------------
// 2. 유니버설 드로어 (UniversalDrawer) - Rect 기반 (근본적 해결)
// -----------------------------------------------------------------------------
[CustomPropertyDrawer(typeof(ExtendedInspectorAttribute))]
[CustomPropertyDrawer(typeof(InspectorBase), true)]
public class UniversalDrawer : PropertyDrawer
{
    private Dictionary<string, int> _tabIndexCache = new Dictionary<string, int>();

    // 1. 높이 계산: GUILayout 추측이 아닌, 실제 필드들의 합을 정확히 계산
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

        var type = GetFieldType();
        var layout = InspectorParser.GetLayout(type);
        string key = property.propertyPath;

        if (!_tabIndexCache.ContainsKey(key)) _tabIndexCache[key] = 0;

        // Rect 방식의 높이 계산 함수 호출
        return EditorGUIUtility.singleLineHeight + InspectorDrawer.CalculateHeight(property, layout, _tabIndexCache[key]);
    }

    // 2. 그리기: GUILayout.BeginArea 등을 일절 사용하지 않음
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string key = property.propertyPath;

        // Foldout
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (!property.isExpanded) return;

        var type = GetFieldType();
        var layout = InspectorParser.GetLayout(type);

        if (!_tabIndexCache.ContainsKey(key)) _tabIndexCache[key] = 0;
        int tabIndex = _tabIndexCache[key];

        // Foldout 다음 줄부터 시작
        Rect contentRect = new Rect(position.x + 10f, position.y + EditorGUIUtility.singleLineHeight + 2f, position.width - 10f, position.height);

        EditorGUI.BeginChangeCheck();

        // Rect 기반 그리기 호출
        InspectorDrawer.DrawRect(contentRect, property, layout, ref tabIndex);

        if (EditorGUI.EndChangeCheck())
        {
            _tabIndexCache[key] = tabIndex;
            // 탭 변경 시 포커스 해제 (입력 버그 방지)
            GUI.FocusControl(null);
        }
    }

    private System.Type GetFieldType()
    {
        var type = fieldInfo.FieldType;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return type.GetGenericArguments()[0];
        if (type.IsArray) return type.GetElementType();
        return type;
    }
}