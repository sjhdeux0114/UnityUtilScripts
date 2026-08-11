#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

// -----------------------------------------------------------------------------
// [분석기] 스크립트 구조 분석
// -----------------------------------------------------------------------------
public static class InspectorParser
{
    private static Dictionary<Type, LayoutData> _cache = new Dictionary<Type, LayoutData>();

    public class LayoutData
    {
        public List<string> TabNames = new List<string>();
        public Dictionary<string, List<FieldData>> TabContents = new Dictionary<string, List<FieldData>>();
        public List<MethodInfo> Buttons = new List<MethodInfo>();
        public bool HasCustomLayout = false;
    }

    public class FieldData
    {
        public string FieldName;
        public string TabName;
        public string BoxName;
        public bool IsHorizontal;
        public ViewerAttribute Viewer;

        // 속성들
        public InlineAttribute Inline;
        public RequiredAttribute Required;
        public ValueDropdownAttribute Dropdown;
        public ShowIfAttribute ShowIf;
        public ReadOnlyAttribute ReadOnly;
        public MinMaxSliderAttribute MinMax;
        public ProgressBarAttribute ProgressBar;
        public SceneNameAttribute SceneName;
        public TagAttribute Tag;
        public LayerAttribute Layer;
        public SortingLayerAttribute SortingLayer;
        public SuffixAttribute Suffix;
        public TitleAttribute Title;
        public AssetsOnlyAttribute AssetsOnly;
        public SceneObjectsOnlyAttribute SceneOnly;
        public AnimatorParamAttribute AnimParam;
        public InputAxisAttribute InputAxis;
        public FolderPathAttribute FolderPath;
        public FindChildAttribute FindChild;
        public ColorPresetAttribute ColorPreset;
        public OnValueChangedAttribute OnValueChanged;
        public AssetListAttribute AssetList;

        public FieldInfo Info;
    }

    public static LayoutData GetLayout(Type type)
    {
        if (_cache.TryGetValue(type, out var data)) return data;

        data = new LayoutData();
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var fi in fields)
        {
            if (fi.Name == "m_Script") continue;
            if (!fi.IsPublic && fi.GetCustomAttribute<SerializeField>() == null) continue;

            Type checkType = fi.FieldType;
            if (checkType.IsArray) checkType = checkType.GetElementType();
            else if (checkType.IsGenericType && checkType.GetGenericTypeDefinition() == typeof(List<>)) checkType = checkType.GetGenericArguments()[0];

            string tab = "Main";
            string box = null;
            bool horiz = false;

            var tabAttr = fi.GetCustomAttribute<TabGroupAttribute>();
            if (tabAttr != null) { tab = tabAttr.tabName; data.HasCustomLayout = true; }
            var boxAttr = fi.GetCustomAttribute<BoxGroupAttribute>();
            if (boxAttr != null) { box = boxAttr.groupName; data.HasCustomLayout = true; }
            var horizAttr = fi.GetCustomAttribute<HorizontalGroupAttribute>();
            if (horizAttr != null) { horiz = true; data.HasCustomLayout = true; }

            // 기존 속성 파싱
            var inlineAttr = fi.GetCustomAttribute<InlineAttribute>();
            var requiredAttr = fi.GetCustomAttribute<RequiredAttribute>();
            var dropdownAttr = fi.GetCustomAttribute<ValueDropdownAttribute>();
            var optionalAttr = fi.GetCustomAttribute<OptionalAttribute>();
            var showIfAttr = fi.GetCustomAttribute<ShowIfAttribute>();
            var readOnlyAttr = fi.GetCustomAttribute<ReadOnlyAttribute>();
            var minMaxAttr = fi.GetCustomAttribute<MinMaxSliderAttribute>();
            var progressBarAttr = fi.GetCustomAttribute<ProgressBarAttribute>();
            var sceneNameAttr = fi.GetCustomAttribute<SceneNameAttribute>();
            var tagAttr = fi.GetCustomAttribute<TagAttribute>();
            var layerAttr = fi.GetCustomAttribute<LayerAttribute>();
            var sortingLayerAttr = fi.GetCustomAttribute<SortingLayerAttribute>();
            var suffixAttr = fi.GetCustomAttribute<SuffixAttribute>();
            var titleAttr = fi.GetCustomAttribute<TitleAttribute>();
            var assetsOnlyAttr = fi.GetCustomAttribute<AssetsOnlyAttribute>();
            var sceneOnlyAttr = fi.GetCustomAttribute<SceneObjectsOnlyAttribute>();
            var animParamAttr = fi.GetCustomAttribute<AnimatorParamAttribute>();
            var inputAxisAttr = fi.GetCustomAttribute<InputAxisAttribute>();
            var folderPathAttr = fi.GetCustomAttribute<FolderPathAttribute>();
            var findChildAttr = fi.GetCustomAttribute<FindChildAttribute>();
            var colorPresetAttr = fi.GetCustomAttribute<ColorPresetAttribute>();
            var onValueChangedAttr = fi.GetCustomAttribute<OnValueChangedAttribute>();

            // AssetList (자동 인식 포함)
            var assetListAttr = fi.GetCustomAttribute<AssetListAttribute>();


            if (inlineAttr != null || requiredAttr != null || dropdownAttr != null ||
                showIfAttr != null || readOnlyAttr != null || minMaxAttr != null ||
                progressBarAttr != null || sceneNameAttr != null ||
                tagAttr != null || layerAttr != null || sortingLayerAttr != null ||
                suffixAttr != null || titleAttr != null || assetsOnlyAttr != null || sceneOnlyAttr != null ||
                animParamAttr != null || inputAxisAttr != null || folderPathAttr != null ||
                findChildAttr != null || colorPresetAttr != null || onValueChangedAttr != null ||
                assetListAttr != null)
                data.HasCustomLayout = true;


            // GameObject 자동 Required 처리
            if (requiredAttr == null &&
                (checkType == typeof(GameObject) || checkType == typeof(Text) || checkType == typeof(Image)) &&
                optionalAttr == null)
            {
                requiredAttr = new RequiredAttribute("GameObject must be assigned!");
                data.HasCustomLayout = true;
            }

            var viewer = checkType.GetCustomAttribute<ViewerAttribute>();
            var noViewAttr = checkType.GetCustomAttribute<NoViewAttribute>();
            bool isMediaType = checkType == typeof(Sprite) ||
                               checkType == typeof(GameObject) || checkType == typeof(TextAsset);

            if (viewer != null || (isMediaType && noViewAttr == null))
            {
                if (viewer == null) viewer = new ViewerAttribute(100, 100);
                data.HasCustomLayout = true;
            }

            if (!data.TabContents.ContainsKey(tab))
            {
                data.TabContents[tab] = new List<FieldData>();
                data.TabNames.Add(tab);
            }

            data.TabContents[tab].Add(new FieldData
            {
                FieldName = fi.Name,
                TabName = tab,
                BoxName = box,
                IsHorizontal = horiz,
                Viewer = viewer,
                Inline = inlineAttr,
                Required = requiredAttr,
                Dropdown = dropdownAttr,
                ShowIf = showIfAttr,
                ReadOnly = readOnlyAttr,
                MinMax = minMaxAttr,
                ProgressBar = progressBarAttr,
                SceneName = sceneNameAttr,
                Tag = tagAttr,
                Layer = layerAttr,
                SortingLayer = sortingLayerAttr,
                Suffix = suffixAttr,
                Title = titleAttr,
                AssetsOnly = assetsOnlyAttr,
                SceneOnly = sceneOnlyAttr,
                AnimParam = animParamAttr,
                InputAxis = inputAxisAttr,
                FolderPath = folderPathAttr,
                FindChild = findChildAttr,
                ColorPreset = colorPresetAttr,
                OnValueChanged = onValueChangedAttr,
                AssetList = assetListAttr,
                Info = fi
            });
        }

        if (data.TabNames.Contains("Main"))
        {
            data.TabNames.Remove("Main");
            data.TabNames.Insert(0, "Main");
        }

        data.Buttons = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes(typeof(InspectorButtonAttribute), true).Length > 0 && m.GetParameters().Length == 0)
            .ToList();

        _cache[type] = data;
        return data;
    }
}

// -----------------------------------------------------------------------------
// [그리기 엔진]
// -----------------------------------------------------------------------------
public static class InspectorDrawer
{
    private static bool ShouldShow(SerializedProperty prop, ShowIfAttribute attr)
    {
        if (attr == null) return true;
        SerializedProperty conditionProp = prop.serializedObject.FindProperty(attr.ConditionName);
        if (conditionProp == null && prop.propertyPath.Contains("."))
        {
            string path = prop.propertyPath;
            string parentPath = path.Substring(0, path.LastIndexOf('.'));
            conditionProp = prop.serializedObject.FindProperty(parentPath + "." + attr.ConditionName);
        }
        if (conditionProp != null && conditionProp.propertyType == SerializedPropertyType.Boolean) return conditionProp.boolValue;
        return true;
    }

    private static string ValidateObject(SerializedProperty prop, AssetsOnlyAttribute assetsOnly, SceneObjectsOnlyAttribute sceneOnly)
    {
        if (prop.propertyType != SerializedPropertyType.ObjectReference || prop.objectReferenceValue == null) return null;
        bool isAsset = AssetDatabase.Contains(prop.objectReferenceValue);
        if (assetsOnly != null && !isAsset) return "Only assets (prefabs) allowed!";
        if (sceneOnly != null && isAsset) return "Only scene objects allowed!";
        return null;
    }

    // ==================================================================================
    // 1. 메인 에디터용 (GUILayout)
    // ==================================================================================
    public static void DrawLayout(SerializedObject serializedObject, InspectorParser.LayoutData layout, ref int selectedTabIndex)
    {
        if (layout.TabNames.Count > 1)
        {
            EditorGUILayout.Space();
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, layout.TabNames.ToArray());
            EditorGUILayout.Space();
        }

        if (selectedTabIndex >= layout.TabNames.Count || selectedTabIndex < 0) selectedTabIndex = 0;
        string currentTab = layout.TabNames.Count > 0 ? layout.TabNames[selectedTabIndex] : "Main";

        if (layout.TabContents.TryGetValue(currentTab, out var fields))
        {
            DrawFieldsGUILayout(serializedObject, fields);
        }
    }

    private static void DrawFieldsGUILayout(SerializedObject serObj, List<InspectorParser.FieldData> fields)
    {
        string currentBox = null;
        bool inHorizontal = false;

        foreach (var field in fields)
        {
            SerializedProperty prop = serObj.FindProperty(field.FieldName);
            if (prop == null) continue;

            if (!ShouldShow(prop, field.ShowIf)) continue;

            if (field.OnValueChanged != null) EditorGUI.BeginChangeCheck();

            if (field.BoxName != currentBox)
            {
                if (inHorizontal) { EditorGUILayout.EndHorizontal(); inHorizontal = false; }
                if (currentBox != null) EditorGUILayout.EndVertical();

                if (field.BoxName != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(field.BoxName, EditorStyles.boldLabel);
                }
                currentBox = field.BoxName;
            }

            if (field.Title != null)
            {
                if (inHorizontal) { EditorGUILayout.EndHorizontal(); inHorizontal = false; }
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(field.Title.Title, EditorStyles.boldLabel);
                if (field.Title.ShowLine)
                {
                    Rect lineRect = EditorGUILayout.GetControlRect(false, 1f);
                    EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                    EditorGUILayout.Space();
                }
            }

            if (field.IsHorizontal)
            {
                if (!inHorizontal) { EditorGUILayout.BeginHorizontal(); inHorizontal = true; }
            }
            else
            {
                if (inHorizontal) { EditorGUILayout.EndHorizontal(); inHorizontal = false; }
            }

            bool guiEnabled = GUI.enabled;
            if (field.ReadOnly != null) GUI.enabled = false;

            Color originalColor = GUI.backgroundColor;
            bool isMissing = false;
            if (field.Required != null && prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == null)
            {
                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
                isMissing = true;
            }

            string validationError = ValidateObject(prop, field.AssetsOnly, field.SceneOnly);
            if (validationError != null) GUI.backgroundColor = new Color(1f, 0.8f, 0.4f);

            // --- 그리기 분기 ---

            if (field.AssetList != null && prop.isArray) DrawAssetList(prop);
            else if (field.Dropdown != null) DrawDropdown(prop, field.Dropdown, field.Info);
            else if (field.SceneName != null && prop.propertyType == SerializedPropertyType.String) DrawSceneName(prop);
            else if (field.Tag != null && prop.propertyType == SerializedPropertyType.String) prop.stringValue = EditorGUILayout.TagField(new GUIContent(prop.displayName), prop.stringValue);
            else if (field.Layer != null && prop.propertyType == SerializedPropertyType.Integer) prop.intValue = EditorGUILayout.LayerField(new GUIContent(prop.displayName), prop.intValue);
            else if (field.SortingLayer != null && prop.propertyType == SerializedPropertyType.Integer) DrawSortingLayer(prop);
            else if (field.AnimParam != null && prop.propertyType == SerializedPropertyType.String) DrawAnimatorParam(prop, field.AnimParam);
            else if (field.InputAxis != null && prop.propertyType == SerializedPropertyType.String) DrawInputAxis(prop); // [FIX] 호출 연결
            else if (field.FolderPath != null && prop.propertyType == SerializedPropertyType.String) DrawFolderPath(prop);
            else if (field.FindChild != null && prop.propertyType == SerializedPropertyType.ObjectReference) DrawFindChild(prop, field.FindChild);
            else if (field.ColorPreset != null && prop.propertyType == SerializedPropertyType.Color) DrawColorPreset(prop, field.ColorPreset);
            else if (field.MinMax != null && prop.propertyType == SerializedPropertyType.Vector2) DrawMinMaxSlider(prop, field.MinMax);
            else if (field.ProgressBar != null && (prop.propertyType == SerializedPropertyType.Float || prop.propertyType == SerializedPropertyType.Integer)) DrawProgressBar(prop, field.ProgressBar); // [FIX] 호출 연결
            else if (field.Inline != null && prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                EditorGUILayout.PropertyField(prop, true);
                if (prop.objectReferenceValue != null && prop.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    SerializedObject targetObj = new SerializedObject(prop.objectReferenceValue);
                    targetObj.Update();
                    SerializedProperty child = targetObj.GetIterator();
                    bool enter = true;
                    while (child.NextVisible(enter))
                    {
                        enter = false;
                        if (child.name == "m_Script") continue;
                        EditorGUILayout.PropertyField(child, true);
                    }
                    targetObj.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                if (field.Suffix != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(prop, true);
                    GUILayout.Label(field.Suffix.Label, GUILayout.Width(30));
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    bool isTarget = prop.propertyType == SerializedPropertyType.ObjectReference || (prop.isArray && prop.propertyType != SerializedPropertyType.String);
                    if (field.Viewer != null && isTarget)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                        EditorGUILayout.PropertyField(prop, true);
                        EditorGUILayout.EndVertical();
                        DrawViewerGUI(prop, field.Viewer);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(prop, true);
                    }
                }
            }

            GUI.backgroundColor = originalColor;
            if (field.ReadOnly != null) GUI.enabled = guiEnabled;

            if (isMissing) EditorGUILayout.HelpBox(field.Required.Message, MessageType.Error);
            if (validationError != null) EditorGUILayout.HelpBox(validationError, MessageType.Warning);

            if (field.OnValueChanged != null && EditorGUI.EndChangeCheck())
            {
                prop.serializedObject.ApplyModifiedProperties();
                InvokeMethod(prop.serializedObject.targetObject, field.OnValueChanged.MethodName);
            }
        }
        if (inHorizontal) EditorGUILayout.EndHorizontal();
        if (currentBox != null) EditorGUILayout.EndVertical();
    }

    // ==================================================================================
    // 2. 유니버설 드로어용 (Rect 기반)
    // ==================================================================================
    public static float CalculateHeight(SerializedProperty rootProp, InspectorParser.LayoutData layout, int selectedTabIndex)
    {
        float totalHeight = 0f;
        if (layout.TabNames.Count > 1) totalHeight += EditorGUIUtility.singleLineHeight + 4f;

        if (selectedTabIndex >= layout.TabNames.Count || selectedTabIndex < 0) selectedTabIndex = 0;
        string currentTab = layout.TabNames.Count > 0 ? layout.TabNames[selectedTabIndex] : "Main";

        if (layout.TabContents.TryGetValue(currentTab, out var fields))
        {
            string currentBox = null;
            bool inHorizontal = false;
            float maxHorizontalHeight = 0f;

            foreach (var field in fields)
            {
                SerializedProperty prop = rootProp.FindPropertyRelative(field.FieldName);
                if (prop == null) continue;
                if (!ShouldShow(prop, field.ShowIf)) continue;

                float fieldHeight = EditorGUI.GetPropertyHeight(prop, true);

                // ... (기존 로직 유지) ...

                bool isRef = prop.propertyType == SerializedPropertyType.ObjectReference;
                if (field.Viewer != null && isRef && prop.objectReferenceValue != null)
                {
                    fieldHeight += field.Viewer.Height + 2f; // 이미지 등
                }

                // ... (기존 박스/Horizontal 로직 유지) ...
                if (field.BoxName != currentBox)
                {
                    if (inHorizontal) { totalHeight += maxHorizontalHeight + 2f; inHorizontal = false; maxHorizontalHeight = 0f; }
                    if (field.BoxName != null) totalHeight += EditorGUIUtility.singleLineHeight + 4f;
                    currentBox = field.BoxName;
                }

                if (field.IsHorizontal)
                {
                    inHorizontal = true;
                    maxHorizontalHeight = Mathf.Max(maxHorizontalHeight, fieldHeight);
                }
                else
                {
                    if (inHorizontal) { totalHeight += maxHorizontalHeight + 2f; inHorizontal = false; maxHorizontalHeight = 0f; }
                    totalHeight += fieldHeight + 2f;
                }
            }
            if (inHorizontal) totalHeight += maxHorizontalHeight + 2f;
        }
        return totalHeight + 10f;
    }

    public static void DrawRect(Rect position, SerializedProperty rootProp, InspectorParser.LayoutData layout, ref int selectedTabIndex)
    {
        Rect curRect = position;
        if (layout.TabNames.Count > 1)
        {
            curRect.height = EditorGUIUtility.singleLineHeight;
            selectedTabIndex = GUI.Toolbar(curRect, selectedTabIndex, layout.TabNames.ToArray());
            curRect.y += curRect.height + 4f;
        }

        if (selectedTabIndex >= layout.TabNames.Count || selectedTabIndex < 0) selectedTabIndex = 0;
        string currentTab = layout.TabNames.Count > 0 ? layout.TabNames[selectedTabIndex] : "Main";

        if (layout.TabContents.TryGetValue(currentTab, out var fields))
        {
            // ... (Horizontal 관련 변수들 기존 유지) ...
            string currentBox = null;
            bool inHorizontal = false;
            float horizontalStartY = curRect.y;
            float maxHorizontalHeight = 0f;
            List<InspectorParser.FieldData> horizontalFields = new List<InspectorParser.FieldData>();

            // ... (FlushHorizontal 함수 기존 유지) ...
            void FlushHorizontal(float totalWidth)
            {
                // (기존 코드와 동일) ...
                if (horizontalFields.Count == 0) return;
                // ... 생략 ...
                curRect.y = horizontalStartY + maxHorizontalHeight + 2f;
                horizontalFields.Clear();
                inHorizontal = false;
                maxHorizontalHeight = 0f;
            }

            foreach (var field in fields)
            {
                SerializedProperty prop = rootProp.FindPropertyRelative(field.FieldName);
                if (prop == null) continue;
                if (!ShouldShow(prop, field.ShowIf)) continue;

                if (field.BoxName != currentBox)
                {
                    if (inHorizontal) FlushHorizontal(position.width);
                    if (field.BoxName != null)
                    {
                        curRect.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.LabelField(curRect, field.BoxName, EditorStyles.boldLabel);
                        curRect.y += curRect.height + 2f;
                    }
                    currentBox = field.BoxName;
                }

                float propHeight = EditorGUI.GetPropertyHeight(prop, true);

                if (field.IsHorizontal)
                {
                    if (!inHorizontal) { inHorizontal = true; horizontalStartY = curRect.y; }
                    horizontalFields.Add(field);
                    maxHorizontalHeight = Mathf.Max(maxHorizontalHeight, propHeight);
                }
                else
                {
                    if (inHorizontal) FlushHorizontal(position.width);

                    // ... (기존 그리기 로직 유지) ...
                    // ... (MinMax, ProgressBar 등) ...

                    bool guiEnabled = GUI.enabled;
                    if (field.ReadOnly != null) GUI.enabled = false;

                    curRect.height = propHeight;
                    if (field.AssetList != null && prop.isArray) DrawAssetListRect(curRect, prop);
                    else if (field.Dropdown != null) DrawDropdownRect(curRect, prop, field.Dropdown, field.Info);
                    else if (field.SceneName != null && prop.propertyType == SerializedPropertyType.String) DrawSceneNameRect(curRect, prop);
                    else if (field.Tag != null && prop.propertyType == SerializedPropertyType.String) prop.stringValue = EditorGUI.TagField(curRect, new GUIContent(prop.displayName), prop.stringValue);
                    else if (field.Layer != null && prop.propertyType == SerializedPropertyType.Integer) prop.intValue = EditorGUI.LayerField(curRect, new GUIContent(prop.displayName), prop.intValue);
                    else if (field.SortingLayer != null && prop.propertyType == SerializedPropertyType.Integer) DrawSortingLayerRect(curRect, prop);
                    else if (field.InputAxis != null && prop.propertyType == SerializedPropertyType.String) DrawInputAxisRect(curRect, prop);
                    else if (field.MinMax != null && prop.propertyType == SerializedPropertyType.Vector2) DrawMinMaxSliderRect(curRect, prop, field.MinMax);
                    else if (field.ProgressBar != null && (prop.propertyType == SerializedPropertyType.Float || prop.propertyType == SerializedPropertyType.Integer)) DrawProgressBarRect(curRect, prop, field.ProgressBar);
                    else
                    {
                        EditorGUI.PropertyField(curRect, prop, true);
                    }

                    if (field.ReadOnly != null) GUI.enabled = guiEnabled;

                    // 뷰어 처리
                    bool isRef = prop.propertyType == SerializedPropertyType.ObjectReference;
                    if (field.Viewer != null && isRef && prop.objectReferenceValue != null)
                    {
                        var obj = prop.objectReferenceValue;

                        // 기존 이미지 뷰어
                        Rect viewRect = new Rect(curRect.x + EditorGUIUtility.labelWidth, curRect.y + propHeight + 2f, field.Viewer.Width, field.Viewer.Height);

                        Texture2D tex = null;
                        if (obj is Sprite s) tex = AssetPreview.GetAssetPreview(s);
                        else if (obj is Texture2D t) tex = t;
                        else if (obj is GameObject g) tex = AssetPreview.GetAssetPreview(g);

                        if (tex) GUI.DrawTexture(viewRect, tex, ScaleMode.ScaleToFit);
                        curRect.y += field.Viewer.Height + 2f;
                    }
                    else
                    {
                        // 뷰어가 아닐 때는 그냥 높이만큼 이동
                        curRect.y += propHeight + 2f;
                    }
                }
            }
            if (inHorizontal) FlushHorizontal(position.width);
        }
    }

    // ==================================================================================
    // 3. 헬퍼 함수들
    // ==================================================================================

    public static void DrawButtons(UnityEngine.Object[] targets, List<MethodInfo> methods)
    {
        if (methods == null || methods.Count == 0) return;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<InspectorButtonAttribute>();
            if (attr == null) continue;
            string label = string.IsNullOrEmpty(attr.Label) ? ObjectNames.NicifyVariableName(method.Name) : attr.Label;
            if (GUILayout.Button(label, GUILayout.Height(25))) foreach (var t in targets) method.Invoke(t, null);
        }
    }

    // [ProgressBar Fix] - 겹침 문제 해결 및 값 수정 가능
    private static void DrawProgressBar(SerializedProperty prop, ProgressBarAttribute attr)
    {
        DrawProgressBarImpl(EditorGUILayout.GetControlRect(), prop, attr);
    }
    private static void DrawProgressBarRect(Rect rect, SerializedProperty prop, ProgressBarAttribute attr)
    {
        DrawProgressBarImpl(rect, prop, attr);
    }
    private static void DrawProgressBarImpl(Rect rect, SerializedProperty prop, ProgressBarAttribute attr)
    {
        // 1. 라벨 그리기 (PrefixLabel은 클릭을 가로채지 않음)
        Rect labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
        EditorGUI.LabelField(labelRect, prop.displayName);

        // 2. 바 영역 계산
        Rect barRect = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y, rect.width - EditorGUIUtility.labelWidth, rect.height);

        float val = (prop.propertyType == SerializedPropertyType.Integer) ? prop.intValue : prop.floatValue;
        float max = attr.MaxValue;
        if (!string.IsNullOrEmpty(attr.MaxValueName))
        {
            SerializedProperty maxProp = prop.serializedObject.FindProperty(attr.MaxValueName);
            if (maxProp == null && prop.propertyPath.Contains("."))
            {
                string path = prop.propertyPath;
                string parentPath = path.Substring(0, path.LastIndexOf('.'));
                maxProp = prop.serializedObject.FindProperty(parentPath + "." + attr.MaxValueName);
            }
            if (maxProp != null)
            {
                if (maxProp.propertyType == SerializedPropertyType.Float) max = maxProp.floatValue;
                else if (maxProp.propertyType == SerializedPropertyType.Integer) max = maxProp.intValue;
            }
        }

        float pct = max > 0 ? val / max : 0;
        pct = Mathf.Clamp01(pct);

        // 3. ProgressBar 그리기 (배경)
        Color originalColor = GUI.color;
        if (attr.HasColor) GUI.color = new Color(attr.R, attr.G, attr.B);
        EditorGUI.ProgressBar(barRect, pct, $"{val}/{max} ({pct * 100:F0}%)");
        GUI.color = originalColor;

        // 4. 투명한 슬라이더 덮어씌우기 (입력 받기 위함)
        // 사용자가 ProgressBar 위에서 드래그하면 값이 바뀌도록 함
        Color prevColor = GUI.color;
        GUI.color = Color.clear; // 투명하게 만듦
        if (prop.propertyType == SerializedPropertyType.Integer)
        {
            prop.intValue = EditorGUI.IntSlider(barRect, prop.intValue, 0, (int)max);
        }
        else
        {
            prop.floatValue = EditorGUI.Slider(barRect, prop.floatValue, 0, max);
        }
        GUI.color = prevColor; // 색상 복구
    }

    // [InputAxis Fix]
    private static void DrawInputAxis(SerializedProperty prop)
    {
        DrawInputAxisImpl(EditorGUILayout.GetControlRect(), prop);
    }
    private static void DrawInputAxisRect(Rect rect, SerializedProperty prop)
    {
        DrawInputAxisImpl(rect, prop);
    }
    private static void DrawInputAxisImpl(Rect rect, SerializedProperty prop)
    {
        var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset").FirstOrDefault();
        if (inputManager != null)
        {
            SerializedObject so = new SerializedObject(inputManager);
            SerializedProperty axes = so.FindProperty("m_Axes");
            List<string> axisNames = new List<string>();
            for (int i = 0; i < axes.arraySize; i++)
            {
                string name = axes.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue;
                if (!axisNames.Contains(name)) axisNames.Add(name);
            }

            int index = axisNames.IndexOf(prop.stringValue);
            int newIndex = EditorGUI.Popup(rect, prop.displayName, index, axisNames.ToArray());
            if (newIndex >= 0) prop.stringValue = axisNames[newIndex];
            else if (index == -1) // 현재 값이 목록에 없을 때 (오타 등)
            {
                EditorGUI.PropertyField(rect, prop);
            }
        }
        else
        {
            EditorGUI.PropertyField(rect, prop);
        }
    }

    // [SceneName Fix]
    private static void DrawSceneName(SerializedProperty prop)
    {
        DrawSceneNameImpl(EditorGUILayout.GetControlRect(), prop);
    }
    private static void DrawSceneNameRect(Rect rect, SerializedProperty prop)
    {
        DrawSceneNameImpl(rect, prop);
    }
    private static void DrawSceneNameImpl(Rect rect, SerializedProperty prop)
    {
        var scenes = EditorBuildSettings.scenes.Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path)).ToList();
        if (scenes.Count == 0)
        {
            EditorGUI.PropertyField(rect, prop);
            return;
        }

        int index = scenes.IndexOf(prop.stringValue);
        int newIndex = EditorGUI.Popup(rect, prop.displayName, index, scenes.ToArray());
        if (newIndex >= 0) prop.stringValue = scenes[newIndex];
        else if (index == -1)
        {
            EditorGUI.Popup(rect, prop.displayName, -1, scenes.ToArray());
        }
    }

    // [AssetList & Rect Support]
    private static void DrawAssetList(SerializedProperty prop)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.PropertyField(prop, true);
        EditorGUILayout.EndVertical();
        if (GUILayout.Button("📂 Load", GUILayout.Width(60), GUILayout.Height(18))) LoadAssetsFromFolder(prop);
        EditorGUILayout.EndHorizontal();
    }
    private static void DrawAssetListRect(Rect rect, SerializedProperty prop)
    {
        EditorGUI.PropertyField(rect, prop, true);
        Rect btnRect = new Rect(rect.x, rect.yMax + 2f, 80f, 18f);
        if (GUI.Button(btnRect, "📂 Load Assets")) LoadAssetsFromFolder(prop);
    }
    private static void LoadAssetsFromFolder(SerializedProperty prop)
    {
        string path = EditorUtility.OpenFolderPanel("Select Folder to Load Assets", "Assets", "");
        if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
            Type elementType = GetArrayElementType(prop);
            string filter = "t:Object";
            if (typeof(UnityEngine.Object).IsAssignableFrom(elementType)) filter = $"t:{elementType.Name}";

            string[] guids = AssetDatabase.FindAssets(filter, new[] { path });
            prop.ClearArray();
            prop.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath(assetPath, elementType);
            }
        }
    }

    // [Helpers]
    private static Type GetArrayElementType(SerializedProperty prop)
    {
        var target = prop.serializedObject.targetObject;
        var field = target.GetType().GetField(prop.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null) return typeof(UnityEngine.Object);
        var type = field.FieldType;
        if (type.IsArray) return type.GetElementType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return type.GetGenericArguments()[0];
        return typeof(UnityEngine.Object);
    }

    private static void DrawFindChild(SerializedProperty prop, FindChildAttribute attr)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(prop, true);
        if (prop.objectReferenceValue == null && GUILayout.Button("🔍 Find", GUILayout.Width(60)))
        {
            string targetName = string.IsNullOrEmpty(attr.ChildName) ? prop.name : attr.ChildName;
            Component comp = prop.serializedObject.targetObject as Component;
            if (comp != null)
            {
                Transform[] allChildren = comp.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allChildren)
                {
                    if (t.name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        var field = comp.GetType().GetField(prop.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        var type = field != null ? field.FieldType : typeof(Component);
                        if (type == typeof(GameObject)) prop.objectReferenceValue = t.gameObject;
                        else prop.objectReferenceValue = t.GetComponent(type);
                        break;
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private static void DrawColorPreset(SerializedProperty prop, ColorPresetAttribute attr)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(prop.displayName);
        foreach (var i in Enumerable.Range(0, attr.Names.Length))
        {
            Color c = attr.Colors[i];
            GUI.backgroundColor = c;
            if (GUILayout.Button(attr.Names[i], GUILayout.Width(50))) prop.colorValue = c;
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(prop, new GUIContent("Custom"));
        EditorGUILayout.EndVertical();
    }

    private static void DrawMinMaxSlider(SerializedProperty prop, MinMaxSliderAttribute attr)
    {
        Vector2 v = prop.vector2Value; float min = v.x; float max = v.y;

        Rect rect = EditorGUILayout.GetControlRect();
        Rect labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
        Rect sliderRect = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y, rect.width - EditorGUIUtility.labelWidth, rect.height);

        EditorGUI.LabelField(labelRect, prop.displayName);

        float fieldWidth = 50f;
        Rect minField = new Rect(sliderRect.x, sliderRect.y, fieldWidth, sliderRect.height);
        Rect maxField = new Rect(sliderRect.xMax - fieldWidth, sliderRect.y, fieldWidth, sliderRect.height);
        Rect slider = new Rect(sliderRect.x + fieldWidth + 5f, sliderRect.y, sliderRect.width - (fieldWidth * 2) - 10f, sliderRect.height);

        min = EditorGUI.FloatField(minField, (float)Math.Round(min, 2));
        max = EditorGUI.FloatField(maxField, (float)Math.Round(max, 2));

        EditorGUI.MinMaxSlider(slider, ref min, ref max, attr.Min, attr.Max);

        prop.vector2Value = new Vector2(min, max);
    }
    private static void DrawMinMaxSliderRect(Rect rect, SerializedProperty prop, MinMaxSliderAttribute attr)
    {
        Vector2 v = prop.vector2Value; float min = v.x; float max = v.y;

        Rect labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
        Rect sliderRect = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y, rect.width - EditorGUIUtility.labelWidth, rect.height);

        EditorGUI.LabelField(labelRect, prop.displayName);

        float fieldWidth = 40f;
        Rect minField = new Rect(sliderRect.x, sliderRect.y, fieldWidth, sliderRect.height);
        Rect maxField = new Rect(sliderRect.xMax - fieldWidth, sliderRect.y, fieldWidth, sliderRect.height);
        Rect slider = new Rect(sliderRect.x + fieldWidth + 5f, sliderRect.y, sliderRect.width - (fieldWidth * 2) - 10f, sliderRect.height);

        min = EditorGUI.FloatField(minField, min);
        max = EditorGUI.FloatField(maxField, max);
        EditorGUI.MinMaxSlider(slider, ref min, ref max, attr.Min, attr.Max);
        prop.vector2Value = new Vector2(min, max);
    }

    private static void DrawFolderPath(SerializedProperty prop)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(prop);
        if (GUILayout.Button("📂", GUILayout.Width(30)))
        {
            string p = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (!string.IsNullOrEmpty(p) && p.StartsWith(Application.dataPath)) prop.stringValue = "Assets" + p.Substring(Application.dataPath.Length);
        }
        EditorGUILayout.EndHorizontal();
    }

    private static void DrawDropdown(SerializedProperty prop, ValueDropdownAttribute attr, FieldInfo info)
    {
        var list = GetDropdownList(prop, attr.MethodName);
        if (list == null) { EditorGUILayout.PropertyField(prop); return; }
        
        List<string> displayList = new List<string> { "(None)" };
        displayList.AddRange(list);
        
        int index = list.IndexOf(prop.stringValue) + 1; // if not found, -1 + 1 = 0 ((None))
        int newIndex = EditorGUILayout.Popup(prop.displayName, index, displayList.ToArray());
        if (newIndex > 0) prop.stringValue = list[newIndex - 1];
        else if (newIndex == 0) prop.stringValue = "";
    }
    private static void DrawDropdownRect(Rect rect, SerializedProperty prop, ValueDropdownAttribute attr, FieldInfo info)
    {
        var list = GetDropdownList(prop, attr.MethodName);
        if (list == null) { EditorGUI.PropertyField(rect, prop); return; }
        
        List<string> displayList = new List<string> { "(None)" };
        displayList.AddRange(list);
        
        int index = list.IndexOf(prop.stringValue) + 1; // if not found, -1 + 1 = 0 ((None))
        int newIndex = EditorGUI.Popup(rect, prop.displayName, index, displayList.ToArray());
        if (newIndex > 0) prop.stringValue = list[newIndex - 1];
        else if (newIndex == 0) prop.stringValue = "";
    }
    private static List<string> GetDropdownList(SerializedProperty prop, string methodName)
    {
        object target = prop.serializedObject.targetObject;
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method != null)
        {
            var result = method.Invoke(target, null);
            if (result is List<string> l) return l;
            if (result is string[] s) return s.ToList();
            if (result is IEnumerable<string> e) return e.ToList();
        }
        return null;
    }

    private static void DrawViewerGUI(SerializedProperty prop, ViewerAttribute viewer)
    {
        UnityEngine.Object obj = null;

        // [수정됨] 배열인지 먼저 확인 후 값을 가져오도록 순서를 변경했습니다.
        if (prop.isArray)
        {
            // 배열인 경우 첫 번째 요소의 값을 가져옵니다 (배열 자체의 값이 아님)
            if (prop.arraySize > 0)
            {
                var element = prop.GetArrayElementAtIndex(0);
                if (element.propertyType == SerializedPropertyType.ObjectReference)
                    obj = element.objectReferenceValue;
            }
        }
        else
        {
            // 배열이 아닌 경우에만 직접 접근
            if (prop.propertyType == SerializedPropertyType.ObjectReference)
                obj = prop.objectReferenceValue;
        }

        if (obj == null) return;

        if (obj is Sprite)
        {
            if (obj is Sprite s)
            {
                Texture2D tex = AssetPreview.GetAssetPreview(s);
                if (tex) GUI.DrawTexture(GUILayoutUtility.GetRect(viewer.Width, viewer.Height), tex, ScaleMode.ScaleToFit);
            }
        }
    }

    private static void DrawSortingLayer(SerializedProperty prop)
    {
        var layers = SortingLayer.layers;
        var names = layers.Select(l => l.name).ToArray();
        var ids = layers.Select(l => l.id).ToArray();
        int idx = Array.IndexOf(ids, prop.intValue);
        if (idx == -1) idx = 0;
        int newIdx = EditorGUILayout.Popup(prop.displayName, idx, names);
        if (newIdx >= 0) prop.intValue = ids[newIdx];
    }
    private static void DrawSortingLayerRect(Rect rect, SerializedProperty prop)
    {
        var layers = SortingLayer.layers;
        var names = layers.Select(l => l.name).ToArray();
        var ids = layers.Select(l => l.id).ToArray();
        int idx = Array.IndexOf(ids, prop.intValue);
        if (idx == -1) idx = 0;
        int newIdx = EditorGUI.Popup(rect, prop.displayName, idx, names);
        if (newIdx >= 0) prop.intValue = ids[newIdx];
    }

    private static void DrawAnimatorParam(SerializedProperty prop, AnimatorParamAttribute attr)
    {
        var target = prop.serializedObject.targetObject as Component;
        if (target == null) { EditorGUILayout.PropertyField(prop); return; }
        var field = target.GetType().GetField(attr.AnimatorName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Animator anim = field?.GetValue(target) as Animator;
        if (anim == null) anim = target.GetComponent<Animator>();

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            var prms = anim.parameters.Select(p => p.name).ToArray();
            int idx = Array.IndexOf(prms, prop.stringValue);
            int newIdx = EditorGUILayout.Popup(prop.displayName, idx, prms);
            if (newIdx >= 0) prop.stringValue = prms[newIdx];
        }
        else EditorGUILayout.PropertyField(prop);
    }

    private static void InvokeMethod(object target, string methodName)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method != null) method.Invoke(target, null);
    }
}
#endif