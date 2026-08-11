#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ExtendedTooltipAttribute))]
public class ExtendedTooltipDrawer : PropertyDrawer
{
    private static readonly Dictionary<string, bool> _states = new();
    private float Line = EditorGUIUtility.singleLineHeight;
    private const float Spacing = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var attr = (ExtendedTooltipAttribute)attribute;
        var key = GetKey(property);

        // 기본: 필드 한 줄 + 폴드 라인 한 줄
        float h = Line + Spacing + Line;

        // 펼쳐졌으면 HelpBox + (선택) 버튼 높이 추가
        if (IsExpanded(key, attr))
        {
            string content = BuildHelpContent(attr);
            float helpHeight = CalcHelpHeight(content);
            h += Spacing + helpHeight;

            if (!string.IsNullOrEmpty(attr.Url))
                h += Spacing + Line; // "문서 열기" 버튼 한 줄
        }

        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (ExtendedTooltipAttribute)attribute;
        var key = GetKey(property);

        if (!_states.ContainsKey(key) && attr.StartExpanded)
            _states[key] = true;

        // 1) 첫 줄: 원래 필드
        var fieldRect = new Rect(position.x, position.y, position.width, Line);
        EditorGUI.PropertyField(fieldRect, property, label, true);

        // 2) 둘째 줄: 폴드아웃 헤더
        var foldRect = new Rect(position.x, fieldRect.yMax + Spacing, position.width, Line);
        string foldLabel = string.IsNullOrEmpty(attr.Title) ? attr.FoldLabel : attr.Title;
        bool expanded = EditorGUI.Foldout(foldRect, IsExpanded(key, attr), foldLabel, true);
        _states[key] = expanded;

        // 3) 펼친 콘텐츠
        if (expanded)
        {
            float y = foldRect.yMax + Spacing;

            string content = BuildHelpContent(attr);
            float helpHeight = CalcHelpHeight(content);
            var helpRect = new Rect(position.x, y, position.width, helpHeight);
            EditorGUI.HelpBox(helpRect, content, Convert(attr.Type));

            y += helpHeight + Spacing;

            if (!string.IsNullOrEmpty(attr.Url))
            {
                var btnRect = new Rect(position.x, y, position.width, Line);
                if (GUI.Button(btnRect, "문서 열기"))
                    Application.OpenURL(attr.Url);
            }
        }
    }

    // ===== Helpers =====
    private static string GetKey(SerializedProperty p)
        => p.serializedObject.targetObject.GetInstanceID() + "_" + p.propertyPath;

    private static bool IsExpanded(string key, ExtendedTooltipAttribute attr)
        => _states.TryGetValue(key, out var v) ? v : false;

    private static string BuildHelpContent(ExtendedTooltipAttribute attr)
    {
        string msg = attr.Message ?? string.Empty;

        // HTML 줄바꿈이 섞였을 수 있으니 안전하게 변환
        msg = msg.Replace("<br>", "\n").Replace("<br/>", "\n");

        return msg;
    }

    private static float CalcHelpHeight(string content)
    {
        var style = EditorStyles.helpBox;
        float w = EditorGUIUtility.currentViewWidth - 40f;
        float ht = style.CalcHeight(new GUIContent(content), w);
        return Mathf.Max(EditorGUIUtility.singleLineHeight * 2f, ht);
    }

    private static MessageType Convert(ExtendedTooltipAttribute.MessageType t)
    {
        return t switch
        {
            ExtendedTooltipAttribute.MessageType.Info => MessageType.Info,
            ExtendedTooltipAttribute.MessageType.Warning => MessageType.Warning,
            ExtendedTooltipAttribute.MessageType.Error => MessageType.Error,
            _ => MessageType.None
        };
    }
}
#endif
