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

    // 접기/펼치기 상태 저장 (Key: InstanceID + Prefix + PropertyPath)
    private readonly Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

    [MenuItem("Tools/Texture Batch Dropper (텍스처 일괄 드롭)")]
    public static void ShowWindow()
    {
        var window = GetWindow<TextureDropTool>("Texture Dropper");
        window.minSize = new Vector2(360, 450);
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
            "2. 작업할 최상위 배열(예: _BaseTexture)을 펼친 후 대상 슬롯에 드롭하세요!\n" +
            "   (Sprite 슬롯에 Texture를 넣어도 자동으로 대응되는 Sprite를 찾아 등록합니다)",
            MessageType.Info);

        EditorGUILayout.Space(5);

        // 옵션 툴바
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            _sortByNaturalName = EditorGUILayout.ToggleLeft("01, 02... 자동 정렬", _sortByNaturalName, GUILayout.Width(125));
            _appendMode = EditorGUILayout.ToggleLeft("추가(Append)", _appendMode, GUILayout.Width(90));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("대분류 펼치기", EditorStyles.toolbarButton))
            {
                SetFoldoutsByPrefix("_root_", true);
            }
            if (GUILayout.Button("대분류 접기", EditorStyles.toolbarButton))
            {
                SetFoldoutsByPrefix("_root_", false);
            }
            if (GUILayout.Button("슬롯 모두 접기", EditorStyles.toolbarButton))
            {
                SetFoldoutsByPrefix("_slot_", false);
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
            EditorGUILayout.Space(4);

            // 최상위 배열 필드명(예: _BaseTexture, _ShapeTexture 등)으로 그룹화
            var rootGroups = arrayProperties
                .GroupBy(p => GetRootPropertyName(p.propertyPath))
                .ToList();

            foreach (var group in rootGroups)
            {
                string rootName = group.Key;
                string rootKey = $"{comp.GetInstanceID()}_root_{rootName}";
                bool isRootExpanded = GetFoldoutState(rootKey, false);

                // 최상위 프로퍼티(배열) 정보 조회
                var rootProp = so.FindProperty(rootName);
                string arrayCountInfo = (rootProp != null && rootProp.isArray) ? $" (크기: {rootProp.arraySize})" : "";

                // 대분류 Foldout 헤더
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                GUIStyle rootFoldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = 11
                };

                bool nextRootState = EditorGUILayout.Foldout(
                    isRootExpanded,
                    $"📂 {rootName}{arrayCountInfo}  [하위 슬롯 {group.Count()}개]",
                    true,
                    rootFoldoutStyle);

                if (nextRootState != isRootExpanded)
                {
                    _foldoutStates[rootKey] = nextRootState;
                    isRootExpanded = nextRootState;
                }

                // 대분류가 펼쳐져 있을 때만 하위 슬롯들 표시
                if (isRootExpanded)
                {
                    EditorGUILayout.Space(3);
                    EditorGUI.indentLevel++;

                    foreach (var prop in group)
                    {
                        DrawDropZone(comp, so, prop, rootName);
                        totalFoundSlots++;
                        EditorGUILayout.Space(2);
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space(2);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
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

    private string GetRootPropertyName(string propertyPath)
    {
        int dotIdx = propertyPath.IndexOf('.');
        int bracketIdx = propertyPath.IndexOf('[');

        int splitIdx = -1;
        if (dotIdx >= 0 && bracketIdx >= 0) splitIdx = Mathf.Min(dotIdx, bracketIdx);
        else if (dotIdx >= 0) splitIdx = dotIdx;
        else if (bracketIdx >= 0) splitIdx = bracketIdx;

        return splitIdx >= 0 ? propertyPath.Substring(0, splitIdx) : propertyPath;
    }

    private void SetFoldoutsByPrefix(string prefix, bool state)
    {
        var keys = _foldoutStates.Keys.Where(k => k.Contains(prefix)).ToList();
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

    private void DrawDropZone(MonoBehaviour comp, SerializedObject so, SerializedProperty arrayProp, string rootName)
    {
        string fullPath = arrayProp.propertyPath.Replace(".Array.data[", "[").Replace("]", "]");

        // 대분류 이름 뒤의 상대 경로만 깔끔하게 추출 (예: _BaseTexture[0]._Tex -> [0]._Tex)
        string displayPath = fullPath.StartsWith(rootName) ? fullPath.Substring(rootName.Length).TrimStart('.') : fullPath;
        if (string.IsNullOrEmpty(displayPath)) displayPath = fullPath;

        string elemTypeName = GetArrayElementTypeName(arrayProp);
        bool isSpriteArray = elemTypeName.Contains("Sprite");
        string typeLabel = isSpriteArray ? "Sprite" : "Texture";

        string slotKey = $"{comp.GetInstanceID()}_slot_{arrayProp.propertyPath}";
        bool isSlotExpanded = GetFoldoutState(slotKey, false);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUIStyle slotFoldoutStyle = new GUIStyle(EditorStyles.foldout);

            bool nextSlotState = EditorGUILayout.Foldout(
                isSlotExpanded,
                $"{displayPath}  [{typeLabel}]  (현재 {arrayProp.arraySize}개)",
                true,
                slotFoldoutStyle);

            if (nextSlotState != isSlotExpanded)
            {
                _foldoutStates[slotKey] = nextSlotState;
                isSlotExpanded = nextSlotState;
            }
        }

        // 슬롯이 펼쳐졌을 때만 드롭존 박스 표시
        if (isSlotExpanded)
        {
            EditorGUILayout.Space(2);

            Rect dropRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));

            Event evt = Event.current;
            bool isHovering = dropRect.Contains(evt.mousePosition);

            Color originalColor = GUI.backgroundColor;
            if (isHovering)
            {
                GUI.backgroundColor = new Color(0.4f, 0.8f, 1f, 1f);
            }

            GUI.Box(dropRect, $"▼ 여기에 [{typeLabel}] 드래그 & 드롭 ▼", EditorStyles.helpBox);
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

                            Debug.Log($"<color=cyan>[TextureDropTool]</color> '{comp.name}'의 <b>{fullPath}</b>에 <b>{droppedObjects.Count}개</b> 중 <b>{successCount}개</b>의 에셋이 성공적으로 등록되었습니다!");
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
