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

    // 배열별 Foldout 상태 저장 (Key: InstanceID + PropertyPath)
    private readonly Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

    [MenuItem("Tools/Texture Batch Dropper (텍스처 일괄 드롭)")]
    public static void ShowWindow()
    {
        var window = GetWindow<TextureDropTool>("Texture Dropper");
        window.minSize = new Vector2(350, 420);
        window.Show();
    }

    private void OnEnable()
    {
        if (Selection.activeGameObject != null)
        {
            _targetObject = Selection.activeGameObject;
        }
    }

    private void OnSelectionChange()
    {
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
            "1. 대상 오브젝트를 지정합니다. (🔒 고정 가능)\n" +
            "2. 작업할 배열의 ▶ 화살표를 눌러 펼칩니다.\n" +
            "3. 프로젝트 창에서 파일들을 드래그하여 파란 드롭 박스에 넣으세요!\n" +
            "   (Sprite 배열에 Texture를 넣어도 자동으로 Sprite를 찾아 등록합니다)",
            MessageType.Info);

        EditorGUILayout.Space(5);

        // 옵션 툴바
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            _sortByNaturalName = EditorGUILayout.ToggleLeft("01, 02... 자동 정렬", _sortByNaturalName, GUILayout.Width(130));
            _appendMode = EditorGUILayout.ToggleLeft("뒤에 추가(Append)", _appendMode, GUILayout.Width(120));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("모두 펼치기", EditorStyles.toolbarButton))
            {
                SetAllFoldouts(true);
            }
            if (GUILayout.Button("모두 접기", EditorStyles.toolbarButton))
            {
                SetAllFoldouts(false);
            }
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
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6);
        }

        if (totalFoundSlots == 0)
        {
            EditorGUILayout.HelpBox("현재 오브젝트에 Texture 또는 Sprite 배열/리스트 필드를 가진 컴포넌트가 없습니다.", MessageType.None);
        }

        EditorGUILayout.EndScrollView();
    }

    private void SetAllFoldouts(bool state)
    {
        var keys = _foldoutStates.Keys.ToList();
        foreach (var key in keys)
        {
            _foldoutStates[key] = state;
        }
    }

    private bool GetFoldoutState(string key, bool defaultState = false)
    {
        if (!_foldoutStates.TryGetValue(key, out bool state))
        {
            _foldoutStates[key] = defaultState;
            return defaultState;
        }
        return state;
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
        string elemType = GetArrayElementTypeName(arrayProp);
        return elemType.Contains("Sprite") || elemType.Contains("Texture");
    }

    private string GetArrayElementTypeName(SerializedProperty arrayProp)
    {
        if (arrayProp.arraySize > 0)
        {
            return arrayProp.GetArrayElementAtIndex(0).type;
        }
        else
        {
            int oldSize = arrayProp.arraySize;
            arrayProp.arraySize = 1;
            string type = arrayProp.GetArrayElementAtIndex(0).type;
            arrayProp.arraySize = oldSize;
            return type;
        }
    }

    private void DrawDropZone(MonoBehaviour comp, SerializedObject so, SerializedProperty arrayProp)
    {
        string propPath = arrayProp.propertyPath.Replace(".Array.data[", "[").Replace("]", "]");
        string elemTypeName = GetArrayElementTypeName(arrayProp);
        bool isSpriteArray = elemTypeName.Contains("Sprite");
        string typeLabel = isSpriteArray ? "Sprite" : "Texture";

        string foldoutKey = $"{comp.GetInstanceID()}_{arrayProp.propertyPath}";
        bool isExpanded = GetFoldoutState(foldoutKey, false);

        // Foldout 헤더 디자인
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            bool nextState = EditorGUILayout.Foldout(
                isExpanded,
                $"{propPath}  [{typeLabel}]  (현재 {arrayProp.arraySize}개)",
                true,
                foldoutStyle);

            if (nextState != isExpanded)
            {
                _foldoutStates[foldoutKey] = nextState;
                isExpanded = nextState;
            }
        }

        // 펼쳐져 있을 때만 드롭존 표시
        if (isExpanded)
        {
            EditorGUILayout.Space(3);

            Rect dropRect = GUILayoutUtility.GetRect(0, 44, GUILayout.ExpandWidth(true));

            Event evt = Event.current;
            bool isHovering = dropRect.Contains(evt.mousePosition);

            Color originalColor = GUI.backgroundColor;
            if (isHovering)
            {
                GUI.backgroundColor = new Color(0.4f, 0.8f, 1f, 1f);
            }

            GUI.Box(dropRect, $"▼ 여기에 [{typeLabel}] 에셋들을 드래그 & 드롭하세요 ▼", EditorStyles.helpBox);
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
                            Undo.RecordObject(comp, "Batch Assign Textures/Sprites");
                            so.Update();

                            int startIndex = _appendMode ? arrayProp.arraySize : 0;
                            int newSize = _appendMode ? arrayProp.arraySize + droppedObjects.Count : droppedObjects.Count;

                            arrayProp.arraySize = newSize;

                            int successCount = 0;
                            for (int i = 0; i < droppedObjects.Count; i++)
                            {
                                var elem = arrayProp.GetArrayElementAtIndex(startIndex + i);
                                Object targetAsset = ConvertToAppropriateAsset(droppedObjects[i], isSpriteArray);
                                elem.objectReferenceValue = targetAsset;
                                if (targetAsset != null) successCount++;
                            }

                            so.ApplyModifiedProperties();
                            EditorUtility.SetDirty(comp);

                            Debug.Log($"<color=cyan>[TextureDropTool]</color> '{comp.name}'의 <b>{propPath}</b>에 <b>{droppedObjects.Count}개</b> 중 <b>{successCount}개</b>의 에셋이 성공적으로 등록되었습니다!");
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

        EditorGUILayout.EndVertical();
    }

    private Object ConvertToAppropriateAsset(Object source, bool isTargetSprite)
    {
        if (source == null) return null;

        if (isTargetSprite)
        {
            if (source is Sprite spr) return spr;
            if (source is Texture2D tex) return GetSpriteFromTexture(tex);
        }
        else
        {
            if (source is Texture tex) return tex;
            if (source is Sprite spr) return spr.texture;
        }

        return source;
    }

    private Sprite GetSpriteFromTexture(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(path)) return null;

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;

        var allSprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if (allSprites.Length > 0)
        {
            var matched = allSprites.FirstOrDefault(s => s.name == texture.name);
            return matched != null ? matched : allSprites[0];
        }

        Debug.LogWarning($"[TextureDropTool] '{texture.name}' 텍스처에서 Sprite를 찾을 수 없습니다. Texture Type이 'Sprite (2D and UI)'인지 확인해주세요.");
        return null;
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
