#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SndClass))]
public class SndClassDrawer : PropertyDrawer
{
    private const float Gap = 6f;
    private const float LabelWidthVol = 30f;
    private const float LabelWidthType = 36f;
    private const float LabelWidthChannel = 55f;
    private const float LabelWidthSType = 50f;

    private static string GetDetailKey(SerializedProperty property)
    {
        int id = property.serializedObject.targetObject != null ? property.serializedObject.targetObject.GetInstanceID() : 0;
        return $"SndClass_Detail_{id}_{property.propertyPath}";
    }

    private static bool IsDetailExpanded(SerializedProperty property)
    {
        return SessionState.GetBool(GetDetailKey(property), false);
    }

    private static void SetDetailExpanded(SerializedProperty property, bool expanded)
    {
        SessionState.SetBool(GetDetailKey(property), expanded);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        bool hasHeader = label != null && !string.IsNullOrEmpty(label.text);

        if (hasHeader && !property.isExpanded)
        {
            return lineHeight;
        }

        float height = hasHeader ? (lineHeight + spacing) : 0f;

        // 1. Clip 필드 높이
        SerializedProperty clipProp = property.FindPropertyRelative("Clip");
        if (clipProp != null)
        {
            height += EditorGUI.GetPropertyHeight(clipProp, true) + spacing;
        }
        else
        {
            height += lineHeight + spacing;
        }

        // 2. Detail 폴드아웃
        bool showDetail = IsDetailExpanded(property);
        if (showDetail)
        {
            height += lineHeight + spacing; // Detail 폴드아웃 헤더
            height += lineHeight + spacing; // vol & Type
            height += lineHeight;           // Channel & S_Type
        }
        else
        {
            height += lineHeight;           // Detail 폴드아웃 헤더만
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float currentY = position.y;

        SerializedProperty clipProp = property.FindPropertyRelative("Clip");
        SerializedProperty volProp = property.FindPropertyRelative("vol");
        SerializedProperty typeProp = property.FindPropertyRelative("type") ?? property.FindPropertyRelative("Type");
        SerializedProperty channelProp = property.FindPropertyRelative("Channel");
        SerializedProperty sTypeProp = property.FindPropertyRelative("S_TYPE") ?? property.FindPropertyRelative("S_Type") ?? property.FindPropertyRelative("sType");

        bool hasHeader = label != null && !string.IsNullOrEmpty(label.text);

        if (hasHeader)
        {
            AudioClip clip = clipProp?.objectReferenceValue as AudioClip;
            string foldoutTitle = label.text;
            if (clip != null)
            {
                foldoutTitle += $"  ({clip.name})";
            }

            Rect foldoutRect = new Rect(position.x, currentY, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, new GUIContent(foldoutTitle), true);
            currentY += lineHeight + spacing;

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }
        }

        int indent = EditorGUI.indentLevel;
        if (hasHeader)
        {
            EditorGUI.indentLevel++;
        }

        // 1. Clip 필드
        if (clipProp != null)
        {
            float clipHeight = EditorGUI.GetPropertyHeight(clipProp, true);
            Rect clipRect = new Rect(position.x, currentY, position.width, clipHeight);
            EditorGUI.PropertyField(clipRect, clipProp, new GUIContent("Clip"), true);
            currentY += clipHeight + spacing;
        }

        // 2. Detail 폴드아웃 (나머지 4개 필드 접기/펼치기)
        bool showDetail = IsDetailExpanded(property);
        Rect detailRect = new Rect(position.x, currentY, position.width, lineHeight);
        bool newShowDetail = EditorGUI.Foldout(detailRect, showDetail, "Detail", true);
        if (newShowDetail != showDetail)
        {
            SetDetailExpanded(property, newShowDetail);
            showDetail = newShowDetail;
        }
        currentY += lineHeight + spacing;

        if (showDetail)
        {
            EditorGUI.indentLevel++;

            // vol & Type (같은 가로줄)
            Rect row1 = new Rect(position.x, currentY, position.width, lineHeight);
            DrawVolAndType(row1, volProp, typeProp);
            currentY += lineHeight + spacing;

            // Channel & S_Type (같은 가로줄)
            Rect row2 = new Rect(position.x, currentY, position.width, lineHeight);
            DrawChannelAndSType(row2, channelProp, sTypeProp);

            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    private static void DrawVolAndType(Rect rowRect, SerializedProperty volProp, SerializedProperty typeProp)
    {
        Rect contentRect = EditorGUI.IndentedRect(rowRect);
        int savedIndent = EditorGUI.indentLevel;
        float savedLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUI.indentLevel = 0;

        float halfWidth = (contentRect.width - Gap) * 0.5f;
        Rect leftRect = new Rect(contentRect.x, contentRect.y, halfWidth, contentRect.height);
        Rect rightRect = new Rect(contentRect.x + halfWidth + Gap, contentRect.y, halfWidth, contentRect.height);

        if (volProp != null)
        {
            EditorGUIUtility.labelWidth = LabelWidthVol;
            EditorGUI.PropertyField(leftRect, volProp, new GUIContent("Vol"));
        }

        if (typeProp != null)
        {
            EditorGUIUtility.labelWidth = LabelWidthType;
            EditorGUI.PropertyField(rightRect, typeProp, new GUIContent("Type"));
        }

        EditorGUIUtility.labelWidth = savedLabelWidth;
        EditorGUI.indentLevel = savedIndent;
    }

    private static void DrawChannelAndSType(Rect rowRect, SerializedProperty channelProp, SerializedProperty sTypeProp)
    {
        Rect contentRect = EditorGUI.IndentedRect(rowRect);
        int savedIndent = EditorGUI.indentLevel;
        float savedLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUI.indentLevel = 0;

        float halfWidth = (contentRect.width - Gap) * 0.5f;
        Rect leftRect = new Rect(contentRect.x, contentRect.y, halfWidth, contentRect.height);
        Rect rightRect = new Rect(contentRect.x + halfWidth + Gap, contentRect.y, halfWidth, contentRect.height);

        if (channelProp != null)
        {
            EditorGUIUtility.labelWidth = LabelWidthChannel;
            EditorGUI.PropertyField(leftRect, channelProp, new GUIContent("Channel"));
        }

        if (sTypeProp != null)
        {
            EditorGUIUtility.labelWidth = LabelWidthSType;
            EditorGUI.PropertyField(rightRect, sTypeProp, new GUIContent("S_Type"));
        }

        EditorGUIUtility.labelWidth = savedLabelWidth;
        EditorGUI.indentLevel = savedIndent;
    }
}
#endif
