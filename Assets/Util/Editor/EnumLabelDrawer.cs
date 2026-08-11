#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// EnumLabelAttribute 속성이 지정된 필드의 라벨을 커스텀 라벨로 변경하여 그려주는 드로어입니다.
/// </summary>
[CustomPropertyDrawer(typeof(EnumLabelAttribute))]
public class EnumLabelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumLabelAttribute enumLabelAttribute = (EnumLabelAttribute)attribute;
        label.text = enumLabelAttribute.label;
        EditorGUI.PropertyField(position, property, label);
    }
}
#endif
