using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(_RND_COUNT))]
public class _RND_COUNTDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 라벨을 그리고, 값 필드의 위치를 계산합니다.
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // 자식 필드들이 인덴트를 갖지 않도록 잠시 0으로 변경
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // 영역 3등분
        float width = position.width / 3f;
        Rect minRect = new Rect(position.x, position.y, width - 2, position.height);
        Rect maxRect = new Rect(position.x + width, position.y, width - 2, position.height);
        Rect cntRect = new Rect(position.x + width * 2, position.y, width - 2, position.height);

        float oldLabelWidth = EditorGUIUtility.labelWidth;

        // 각 필드 그리기 (라벨 길이를 짧게 고정)
        EditorGUIUtility.labelWidth = 30f;
        EditorGUI.PropertyField(minRect, property.FindPropertyRelative("min"), new GUIContent("min"));
        
        EditorGUIUtility.labelWidth = 35f;
        EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("max"), new GUIContent("max"));
        
        EditorGUIUtility.labelWidth = 30f;
        EditorGUI.PropertyField(cntRect, property.FindPropertyRelative("TargetCnt"), new GUIContent("Cnt"));

        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(_RND_COUNT_F))]
public class _RND_COUNT_FDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        float width = position.width / 3f;
        Rect minRect = new Rect(position.x, position.y, width - 2, position.height);
        Rect maxRect = new Rect(position.x + width, position.y, width - 2, position.height);
        Rect cntRect = new Rect(position.x + width * 2, position.y, width - 2, position.height);

        float oldLabelWidth = EditorGUIUtility.labelWidth;

        EditorGUIUtility.labelWidth = 30f;
        EditorGUI.PropertyField(minRect, property.FindPropertyRelative("min"), new GUIContent("min"));
        
        EditorGUIUtility.labelWidth = 35f;
        EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("max"), new GUIContent("max"));
        
        EditorGUIUtility.labelWidth = 30f;
        EditorGUI.PropertyField(cntRect, property.FindPropertyRelative("TargetCnt"), new GUIContent("Cnt"));

        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
