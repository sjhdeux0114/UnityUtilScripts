#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TextureDropTool : EditorWindow
{
    private GameObject _targetObject;
    private bool _isLocked = false;
    private Vector2 _scrollPos;
    private bool _sortByNaturalName = true;
    private bool _appendMode = false;

    [MenuItem("Tools/Texture Batch Dropper (텍스처 일괄 드롭)")]
    public static void ShowWindow()
    {
        var window = GetWindow<TextureDropTool>("Texture Dropper");
        window.minSize = new Vector2(350, 420);
        window.Show();
    }

    private void OnEnable()
    {
        // 창이 열릴 때 현재 선택된 게임오브젝트가 있으면 기본 타깃으로 설정
        if (Selection.activeGameObject != null)
        {
            _targetObject = Selection.activeGameObject;
        }
    }

    private void OnSelectionChange()
    {
        // 고정(Lock) 상태가 아니고, 새로 선택한 것이 '게임오브젝트'일 때만 대상 변경
        // (프로젝트 창에서 텍스처나 에셋을 선택했을 때는 기존 오브젝트 유지를 위해 풀리지 않음!)
        if (!_isLocked && Selection.activeGameObject != null)
        {
            _targetObject = Selection.activeGameObject;
            Repaint();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "【사용 방법】\n" +
            "1. 대상 오브젝트를 지정하거나 하이어라키에서 선택합니다. (🔒 고정 가능)\n" +
            "2. 프로젝트 창에서 텍스처 파일들을 여러 개 선택합니다.\n" +
            "3. 아래 원하는 배열 슬롯의 파란 박스로 마우스 드래그 & 드롭하세요!",
            MessageType.Info);

        EditorGUILayout.Space(5);

        // 옵션 툴바
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            _sortByNaturalName = EditorGUILayout.ToggleLeft("이름순(01, 02...) 자동 정렬", _sortByNaturalName, GUILayout.Width(175));
            _appendMode = EditorGUILayout.ToggleLeft("기존 배열 뒤에 추가 (Append)", _appendMode);
        }

        EditorGUILayout.Space(5);

        // 대상 오브젝트 선택 및 고정(Lock) 바
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("대상 오브젝트", GUILayout.Width(80));

            EditorGUI.BeginChangeCheck();
            _targetObject = (GameObject)EditorGUILayout.ObjectField(_targetObject, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }

            // 고정 토글 버튼 (자물쇠 모양)
            GUIContent lockContent = new GUIContent(_isLocked ? "🔒 고정됨" : "🔓 고정하기", "활성화하면 다른 게임오브젝트를 클릭해도 대상이 유지됩니다.");
            Color prevColor = GUI.backgroundColor;
            if (_isLocked) GUI.backgroundColor = new Color(1f, 0.7f, 0.7f, 1f);

            if (GUILayout.Button(lockContent, EditorStyles.miniButton, GUILayout.Width(75)))
            {
                _isLocked = !_isLocked;
            }
            GUI.backgroundColor = prevColor;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        if (_targetObject == null)
        {
            EditorGUILayout.HelpBox("대상 게임오브젝트가 없습니다.\n하이어라키에서 오브젝트를 클릭하거나 위 [대상 오브젝트] 슬롯에 넣어주세요.", MessageType.Warning);
            return;
        }

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        MonoBehaviour[] components = _targetObject.GetComponents<MonoBehaviour>();
        int totalFoundSlots = 0;

        foreach (var comp in components)
        {
            if (comp == null) continue;

            SerializedObject so = new SerializedObject(comp);
            List<SerializedProperty> arrayProperties = FindArrayProperties(so);

            if (arrayProperties.Count == 0) continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"컴포넌트: {comp.GetType().Name}", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            foreach (var prop in arrayProperties)
            {
                DrawDropZone(comp, so, prop);
                totalFoundSlots++;
                EditorGUILayout.Space(4);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(8);
        }

        if (totalFoundSlots == 0)
        {
            EditorGUILayout.HelpBox("현재 오브젝트에 Texture 또는 Sprite 배열/리스트 필드를 가진 컴포넌트가 없습니다.", MessageType.None);
        }

        EditorGUILayout.EndScrollView();
    }

    private List<SerializedProperty> FindArrayProperties(SerializedObject so)
    {
        var list = new List<SerializedProperty>();
        var iterator = so.GetIterator();

        if (iterator.NextVisible(true))
        {
            do
            {
                if (iterator.isArray && iterator.propertyType != SerializedPropertyType.String)
                {
                    if (IsTextureOrSpriteArray(iterator))
                    {
                        list.Add(iterator.Copy());
                    }
                }
            } while (iterator.NextVisible(true));
        }

        return list;
    }

    private bool IsTextureOrSpriteArray(SerializedProperty arrayProp)
    {
        if (arrayProp.arraySize > 0)
        {
            var firstElem = arrayProp.GetArrayElementAtIndex(0);
            if (firstElem.propertyType == SerializedPropertyType.ObjectReference)
            {
                return true;
            }
        }
        else
        {
            int oldSize = arrayProp.arraySize;
            arrayProp.arraySize = 1;
            var elem = arrayProp.GetArrayElementAtIndex(0);
            bool isObj = elem.propertyType == SerializedPropertyType.ObjectReference;
            arrayProp.arraySize = oldSize;
            return isObj;
        }

        return false;
    }

    private void DrawDropZone(MonoBehaviour comp, SerializedObject so, SerializedProperty arrayProp)
    {
        string propPath = arrayProp.propertyPath.Replace(".Array.data[", "[").Replace("]", "]");
        string labelText = $"{propPath} (현재 {arrayProp.arraySize}개)";

        Rect dropRect = GUILayoutUtility.GetRect(0, 48, GUILayout.ExpandWidth(true));

        Event evt = Event.current;
        bool isHovering = dropRect.Contains(evt.mousePosition);

        Color originalColor = GUI.backgroundColor;
        if (isHovering)
        {
            GUI.backgroundColor = new Color(0.4f, 0.8f, 1f, 1f);
        }

        GUI.Box(dropRect, $"{labelText}\n▼ 여기에 텍스처들을 드래그 & 드롭하세요 ▼", EditorStyles.helpBox);
        GUI.backgroundColor = originalColor;

        if (isHovering)
        {
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    var droppedObjects = DragAndDrop.objectReferences
                        .Where(obj => obj is Texture || obj is Sprite)
                        .ToList();

                    if (_sortByNaturalName)
                    {
                        droppedObjects = droppedObjects.OrderBy(x => x.name, new NaturalStringComparer()).ToList();
                    }

                    if (droppedObjects.Count > 0)
                    {
                        Undo.RecordObject(comp, "Batch Assign Textures");
                        so.Update();

                        int startIndex = _appendMode ? arrayProp.arraySize : 0;
                        int newSize = _appendMode ? arrayProp.arraySize + droppedObjects.Count : droppedObjects.Count;

                        arrayProp.arraySize = newSize;

                        for (int i = 0; i < droppedObjects.Count; i++)
                        {
                            var elem = arrayProp.GetArrayElementAtIndex(startIndex + i);
                            elem.objectReferenceValue = droppedObjects[i];
                        }

                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(comp);

                        Debug.Log($"<color=cyan>[TextureDropTool]</color> '{comp.name}'의 <b>{propPath}</b>에 <b>{droppedObjects.Count}개</b>의 에셋이 등록되었습니다!");
                    }
                    else
                    {
                        Debug.LogWarning("[TextureDropTool] 드래그된 에셋 중에 Texture 또는 Sprite 파일이 없습니다.");
                    }
                }
                evt.Use();
            }
        }
    }

    private class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return EditorUtility.NaturalCompare(x, y);
        }
    }
}
#endif
