#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

// 핵심: typeof(Enum)을 타겟으로 하고, 두 번째 인자로 true(상속된 타입 포함)를 줍니다.
[CustomPropertyDrawer(typeof(Enum), true)]
public class GlobalEnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 1. [Flags] 속성이 있는 Enum(중복 선택)은 기본 Unity 방식으로 그리기
        // (검색 팝업은 기본적으로 단일 선택이라, Flags는 예외 처리하는 게 좋습니다)
        if (fieldInfo != null && fieldInfo.FieldType.IsDefined(typeof(FlagsAttribute), false))
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // 2. 기본 라벨 그리기
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // 3. 현재 선택된 값 이름 가져오기
        // (리스트 배열 내의 Enum 등에서 인덱스 오류 방지)
        string currentName = "Unknown";
        string[] displayNames = GetFriendlyNames(property);
        if (property.enumValueIndex >= 0 && property.enumValueIndex < displayNames.Length)
        {
            currentName = displayNames[property.enumValueIndex];
        }

        // 4. 버튼 그리기
        if (GUI.Button(position, currentName, EditorStyles.popup))
        {
            var dropdown = new GlobalEnumDropdown(property, displayNames);
            dropdown.Show(position);
        }
    }

    private Type GetEnumType()
    {
        if (fieldInfo == null) return null;
        Type type = fieldInfo.FieldType;
        if (type.IsArray)
        {
            return type.GetElementType();
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
        {
            return type.GetGenericArguments()[0];
        }
        return type;
    }

    private string[] GetFriendlyNames(SerializedProperty property)
    {
        string[] enumNames = property.enumNames;
        if (enumNames == null) return property.enumDisplayNames;

        string[] displayNames = new string[enumNames.Length];
        Type enumType = GetEnumType();

        for (int i = 0; i < enumNames.Length; i++)
        {
            displayNames[i] = property.enumDisplayNames[i]; // default fallback
            if (enumType != null)
            {
                try
                {
                    var memberInfo = enumType.GetMember(enumNames[i]);
                    if (memberInfo != null && memberInfo.Length > 0)
                    {
                        var attr = memberInfo[0].GetCustomAttribute<EnumLabelAttribute>();
                        if (attr != null && !string.IsNullOrEmpty(attr.label))
                        {
                            displayNames[i] = attr.label;
                        }
                    }
                }
                catch
                {
                    // Fallback to default enum display name
                }
            }
        }
        return displayNames;
    }
}

// 드롭다운 로직 (이전과 동일하지만 클래스 이름만 구분을 위해 변경)
public class GlobalEnumDropdown : AdvancedDropdown
{
    private SerializedProperty _property;
    private string[] _names;

    public GlobalEnumDropdown(SerializedProperty property, string[] displayNames) : base(new AdvancedDropdownState())
    {
        _property = property;
        _names = displayNames;

        var currentSize = minimumSize;
        currentSize.y = 250f;
        minimumSize = currentSize;
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem("Select Option");

        for (int i = 0; i < _names.Length; i++)
        {
            var item = new AdvancedDropdownItem(_names[i])
            {
                id = i
            };
            root.AddChild(item);
        }

        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        _property.serializedObject.Update();
        _property.enumValueIndex = item.id;
        _property.serializedObject.ApplyModifiedProperties();
    }
}
#endif