using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimpleSpriteInfo))]
public class SimpleSpriteInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 먼저 그리기
        base.OnInspectorGUI();

        GUILayout.Space(20);
        EditorGUILayout.LabelField("빠른 애니메이션 추가", EditorStyles.boldLabel);

        if (GUILayout.Button("새 애니메이션 일괄 추가 창 열기", GUILayout.Height(40)))
        {
            SimpleSpriteDropWindow.ShowWindow((SimpleSpriteInfo)target);
        }

        SimpleSpriteInfo info = target as SimpleSpriteInfo;
        for (int i = 0; i < info.animations.Count; i++)
        {
            if (GUILayout.Button($"{info.animations[i].animationName} sprite 삭제", GUILayout.Height(40)))
            {
                info.animations[i].sprites = new List<Sprite>();
                EditorUtility.SetDirty(info);
            }


        }
    }
}

public class SimpleSpriteDropWindow : EditorWindow
{
    public SimpleSpriteInfo targetInfo;

    public static void ShowWindow(SimpleSpriteInfo info)
    {
        var window = GetWindow<SimpleSpriteDropWindow>("스프라이트 드롭 창");
        window.targetInfo = info;
        window.minSize = new Vector2(300, 200);
        window.Show();
    }

    private Vector2 scrollPos;

    private void OnGUI()
    {
        if (targetInfo == null)
        {
            EditorGUILayout.HelpBox("대상 SimpleSpriteInfo가 지정되지 않았습니다. 인스펙터에서 창을 다시 열어주세요.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField($"대상: {targetInfo.name}", EditorStyles.boldLabel);
        GUILayout.Space(10);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.LabelField("SpriteInfo 복사", EditorStyles.boldLabel);
        Rect copyDropArea = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));
        GUI.Box(copyDropArea, "\n여기에 드롭하면 SpriteInfo가 복사 됩니다.", EditorStyles.helpBox);
        HandleDrop(copyDropArea, -2);

        GUILayout.Space(20);


        // --- 새 애니메이션으로 추가 ---
        EditorGUILayout.LabelField("새 애니메이션으로 추가", EditorStyles.boldLabel);
        Rect newDropArea = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));
        GUI.Box(newDropArea, "\n여기에 드롭하면 새로운 애니메이션이 생성됩니다.", EditorStyles.helpBox);
        HandleDrop(newDropArea, -1);

        GUILayout.Space(20);

        // --- 기존 애니메이션에 추가 ---
        EditorGUILayout.LabelField("기존 애니메이션에 스프라이트 덮어쓰기", EditorStyles.boldLabel);

        if (targetInfo.animations == null || targetInfo.animations.Count == 0)
        {
            EditorGUILayout.HelpBox("현재 등록된 애니메이션이 없습니다.", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < targetInfo.animations.Count; i++)
            {
                var anim = targetInfo.animations[i];
                string animName = string.IsNullOrEmpty(anim.animationName) ? $"[이름 없음 {i}]" : anim.animationName;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"{i + 1}. {animName} (현재 {anim.sprites?.Count ?? 0}장)");

                Rect existingDropArea = GUILayoutUtility.GetRect(0.0f, 40.0f, GUILayout.ExpandWidth(true));
                GUI.Box(existingDropArea, "이 애니메이션에 스프라이트 덮어쓰기", EditorStyles.helpBox);
                HandleDrop(existingDropArea, i);

                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void HandleDrop(Rect dropArea, int targetIndex)
    {
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    // --- 복사 대상(SimpleSpriteInfo) 처리 할 경우 ---
                    if (targetIndex == -2)
                    {
                        List<SimpleSpriteInfo> droppedInfos = new List<SimpleSpriteInfo>();
                        foreach (Object obj in DragAndDrop.objectReferences)
                        {
                            if (obj is SimpleSpriteInfo ssi)
                            {
                                droppedInfos.Add(ssi);
                            }
                            else
                            {
                                string path = AssetDatabase.GetAssetPath(obj);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    var loaded = AssetDatabase.LoadAssetAtPath<SimpleSpriteInfo>(path);
                                    if (loaded != null)
                                        droppedInfos.Add(loaded);
                                }
                            }
                        }

                        // 단일 파일만 허용
                        if (droppedInfos.Count == 0)
                        {
                            EditorUtility.DisplayDialog("복사 오류", "선택된 항목에서 SimpleSpriteInfo 에셋을 찾을 수 없습니다.", "확인");
                            evt.Use();
                            return;
                        }

                        if (droppedInfos.Count > 1)
                        {
                            EditorUtility.DisplayDialog("복사 오류", "단일 SimpleSpriteInfo 에셋만 드롭하세요.", "확인");
                            evt.Use();
                            return;
                        }

                        var srcInfo = droppedInfos[0];
                        if (srcInfo == null)
                        {
                            EditorUtility.DisplayDialog("복사 오류", "유효한 SimpleSpriteInfo가 아닙니다.", "확인");
                            evt.Use();
                            return;
                        }

                        // Undo 등록
                        Undo.RegisterCompleteObjectUndo(targetInfo, "Copy SimpleSpriteInfo Animations and Names");
                        SerializedObject so = new SerializedObject(targetInfo);
                        so.Update();

                        SerializedProperty animsProp = so.FindProperty("animations");
                        SerializedProperty namesProp = so.FindProperty("names");

                        // names 복제
                        if (namesProp != null)
                        {
                            namesProp.stringValue = srcInfo.names;
                        }
                        else
                        {
                            targetInfo.names = srcInfo.names;
                        }

                        if (animsProp == null)
                        {
                            Debug.LogWarning("Target SimpleSpriteInfo에 'animations' 프로퍼티가 없습니다.");
                        }
                        else
                        {
                            // 기존 애니메이션 완전 삭제 후 새로 복사 (덮어쓰기)
                            animsProp.ClearArray();
                            // ClearArray가 완전히 비우지 못하는 경우 대비
                            animsProp.arraySize = 0;

                            if (srcInfo.animations != null)
                            {
                                // Deep-copy animations by operating on the target object's list to copy all fields
                                if (targetInfo.animations == null)
                                    targetInfo.animations = new List<AnimationData>();

                                targetInfo.animations.Clear();

                                for (int a = 0; a < srcInfo.animations.Count; a++)
                                {
                                    var srcAnim = srcInfo.animations[a];
                                    // Deep copy serializable fields using JsonUtility
                                    AnimationData newAnim = new AnimationData();
                                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(srcAnim), newAnim);
                                    targetInfo.animations.Add(newAnim);
                                }

                                // Ensure serialized representation picks up direct list changes
                                // (we still apply modified properties below for other changes)
                            }
                        }

                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(targetInfo);
                        // Ensure changes are written to disk for ScriptableObject asset
                        AssetDatabase.SaveAssets();
                        Debug.Log($"[복사] '{srcInfo.name}'의 names와 {srcInfo.animations?.Count ?? 0}개의 애니메이션으로 덮어쓰기 했습니다.");

                        evt.Use();
                        return;
                    }

                    // --- 기존 동작: Sprite 또는 Texture2D에서 스프라이트 추출 ---
                    List<Sprite> droppedSprites = new List<Sprite>();
                    foreach (Object obj in DragAndDrop.objectReferences)
                    {
                        if (obj is Sprite sprite)
                        {
                            droppedSprites.Add(sprite);
                        }
                        else if (obj is Texture2D)
                        {
                            string path = AssetDatabase.GetAssetPath(obj);
                            var spritesInTexture = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>();
                            droppedSprites.AddRange(spritesInTexture);
                        }
                    }

                    droppedSprites = droppedSprites.Distinct().OrderBy(s => s.name).ToList();

                    if (droppedSprites.Count > 0)
                    {
                        SerializedObject so = new SerializedObject(targetInfo);
                        so.Update();

                        SerializedProperty animsProp = so.FindProperty("animations");

                        int indexToModify = targetIndex;
                        string targetName = "";

                        if (targetIndex == -1)
                        {
                            // 새 애니메이션 추가
                            indexToModify = animsProp.arraySize;
                            animsProp.InsertArrayElementAtIndex(indexToModify);

                            string baseName = droppedSprites[0].name;
                            int underscoreIndex = baseName.LastIndexOf('_');
                            if (underscoreIndex > 0) baseName = baseName.Substring(0, underscoreIndex);
                            targetName = baseName.ToLower();

                            SerializedProperty newAnimProp = animsProp.GetArrayElementAtIndex(indexToModify);
                            newAnimProp.FindPropertyRelative("animationName").stringValue = targetName;
                        }
                        else
                        {
                            // 기존 애니메이션에 덮어쓰기
                            targetName = targetInfo.animations[targetIndex].animationName;
                        }

                        // 스프라이트 데이터 덮어쓰기
                        SerializedProperty targetAnimProp = animsProp.GetArrayElementAtIndex(indexToModify);
                        SerializedProperty spritesProp = targetAnimProp.FindPropertyRelative("sprites");
                        spritesProp.ClearArray();
                        for (int i = 0; i < droppedSprites.Count; i++)
                        {
                            spritesProp.InsertArrayElementAtIndex(i);
                            spritesProp.GetArrayElementAtIndex(i).objectReferenceValue = droppedSprites[i];
                        }

                        so.ApplyModifiedProperties();

                        if (targetIndex == -1)
                            Debug.Log($"[새 애니메이션] '{targetName}'이(가) {droppedSprites.Count}장의 스프라이트와 함께 생성되었습니다.");
                        else
                            Debug.Log($"[기존 애니메이션] '{targetName}'에 {droppedSprites.Count}장의 스프라이트가 덮어씌워졌습니다.");
                    }
                    evt.Use();
                }
                break;
        }
    }
}
