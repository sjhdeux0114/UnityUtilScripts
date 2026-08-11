using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ScriptableObject), true)]
public class ScriptableObjectInlineDrawer : PropertyDrawer
{
    // Inspector에 그릴 때 필요한 전체 높이를 계산합니다.
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = EditorGUIUtility.singleLineHeight;

        // 접혀있지 않고, 값이 비어있지 않은 경우에만 내부 항목의 높이를 더합니다.
        if (property.objectReferenceValue != null && property.isExpanded)
        {
            SerializedObject obj = new SerializedObject(property.objectReferenceValue);
            SerializedProperty iterator = obj.GetIterator();
            bool enterChildren = true;

            totalHeight += EditorGUIUtility.standardVerticalSpacing * 2; // 상하 여백

            while (iterator.NextVisible(enterChildren))
            {
                // 불필요한 스크립트 참조 필드(m_Script)는 화면에서 숨깁니다.
                if (iterator.propertyPath != "m_Script")
                {
                    totalHeight += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                }
                enterChildren = false;
            }

            totalHeight += EditorGUIUtility.standardVerticalSpacing * 2;
        }

        return totalHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // 기본 필드 옆에 폴드아웃(접기/펴기) 아이콘을 커스텀하게 배치합니다.
        if (property.objectReferenceValue != null)
        {
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, 15f, EditorGUIUtility.singleLineHeight), property.isExpanded, GUIContent.none);
        }

        // ScriptableObject를 할당하는 기본 필드를 그립니다.
        EditorGUI.PropertyField(fieldRect, property, label, true);

        // 만약 할당된 오브젝트가 있고 사용자가 접기 버튼을 열었다면, 내부 데이터를 그려줍니다.
        if (property.objectReferenceValue != null && property.isExpanded)
        {
            // 그려질 위치를 계산합니다.
            float yOffset = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;

            // 시각적 구분을 위한 회색 박스 영역 계산
            float boxHeight = GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
            Rect boxRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, boxHeight);

            // 박스 그리기
            GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

            // ScriptableObject 내부의 변수들을 직렬화하여 가져옵니다.
            SerializedObject obj = new SerializedObject(property.objectReferenceValue);
            obj.Update();

            SerializedProperty iterator = obj.GetIterator();
            bool enterChildren = true;

            // 들여쓰기 증가
            EditorGUI.indentLevel++;

            while (iterator.NextVisible(enterChildren))
            {
                // 스크립트(m_Script) 변수는 사용자가 수정할 필요가 없으므로 제외
                if (iterator.propertyPath != "m_Script")
                {
                    float height = EditorGUI.GetPropertyHeight(iterator, true);

                    // 각 변수의 그릴 위치(Rect)를 지정
                    Rect propRect = new Rect(position.x + 15f, yOffset, position.width - 20f, height);

                    // 변수를 인스펙터에 그립니다.
                    EditorGUI.PropertyField(propRect, iterator, true);

                    yOffset += height + EditorGUIUtility.standardVerticalSpacing;
                }
                enterChildren = false;
            }

            // 들여쓰기 원상복구
            EditorGUI.indentLevel--;

            // 변경된 프로퍼티를 실제 데이터에 저장합니다.
            if (GUI.changed)
            {
                obj.ApplyModifiedProperties();
            }
        }

        EditorGUI.EndProperty();
    }
}
