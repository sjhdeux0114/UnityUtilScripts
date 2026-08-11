using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// This is an editor script for SimpleSpriteAnimator.
// It creates buttons in the inspector to easily test animations.

[CustomEditor(typeof(SimpleSpriteAnimator))]
public class SimpleSpriteAnimatorEditor : Editor
{
    private int selectedAnimationIndex = -1;
    private readonly int currentFrameIndex = 0;
    SimpleSpriteAnimator animator = null;
    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector GUI first.
        base.OnInspectorGUI();

        // Get the target object (SimpleSpriteAnimator script).
        animator = (SimpleSpriteAnimator)target;

        // --- Play Test Section ---
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Play Test (Editor)", EditorStyles.boldLabel);


        if (animator.infoData != null)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < animator.infoData.Length; i++)
            {
                if (animator.infoData[i] != null)
                {
                    if (GUILayout.Button($"[ {animator.infoData[i].names} ]"))
                    {
                        animator.ChangeInfoData(i);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.Space(20);
        EditorGUILayout.LabelField("빠른 애니메이션 추가", EditorStyles.boldLabel);

        if (GUILayout.Button("새 애니메이션 일괄 추가 창 열기", GUILayout.Height(40)))
        {
            SimpleSpriteDropWindow2.ShowWindow((SimpleSpriteAnimator)target);
        }


        EditorGUILayout.HelpBox("Press a button below to preview the animation in the Scene view.", MessageType.Info);
        if (!Application.isPlaying)
        {
            if (GUILayout.Button($"Init - Dic:"))
            {
                animator._Init_Dic();
            }
        }

        // Animation selection buttons
        foreach (var animation in animator.animations)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button($"Preview: {animation.animationName}"))
            {

                if (Application.isPlaying)
                {
                    animator.Play(animation.animationName);
                }
                else
                {
                    animator.Play(animation.animationName);

                    bPlay = true;

                }

            }
            if (Application.isPlaying)
            {
                if (GUILayout.Button($"ADD: {animation.animationName}"))
                {
                    animator.AddNext(animation.animationName);
                }
            }

            EditorGUILayout.EndHorizontal();

        }

        // Add a Stop button
        if (bPlay)
        {
            if (GUILayout.Button("Stop"))
            {
                selectedAnimationIndex = -1;
                // You may want to revert to the initial sprite here if needed
                // For now, it just stops updating.
                bPlay = false;
            }
        }
        EditorGUILayout.HelpBox("Delete Sprite", MessageType.Info);
        EditorGUILayout.BeginHorizontal();

        foreach (var animation in animator.animations)
        {

            if (GUILayout.Button($"{animation.animationName}"))
            {

                if (Application.isPlaying)
                {
                }
                else
                {
                    animation.sprites.Clear();
                }

            }


        }
        EditorGUILayout.EndHorizontal();

        // Display current frame info
        if (selectedAnimationIndex != -1 && selectedAnimationIndex < animator.animations.Count)
        {
            var currentAnimation = animator.animations[selectedAnimationIndex];
            if (currentAnimation.sprites.Count > 0)
            {
                EditorGUILayout.LabelField($"Playing: {currentAnimation.animationName} (Frame: {currentFrameIndex + 1}/{currentAnimation.sprites.Count})");
            }
        }


    }

    private void EditorUpdate()
    {
        if (EditorApplication.isPlaying)
        {
            bPlay = false;
            return;
        }
        if (animator == null)
            return;

        // In here you can check the current realtime, see if a certain
        // amount of time has elapsed, and perform some task.
        float delta = Time.realtimeSinceStartup - deltatimes;

        deltatimes = Time.realtimeSinceStartup;
        if (bPlay)
        {
            animator._Update(delta);
            EditorUtility.SetDirty(animator);
        }
    }


    bool bPlay = false;
    float deltatimes = 0;

}


public class SimpleSpriteDropWindow2 : EditorWindow
{
    public SimpleSpriteAnimator targetInfo;

    public static void ShowWindow(SimpleSpriteAnimator info)
    {
        var window = GetWindow<SimpleSpriteDropWindow2>("스프라이트 드롭 창");
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
