using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteGroupAnimator))]
public class SpriteGroupAnimatorEditor : Editor
{
    private SpriteGroupAnimator _target => target as SpriteGroupAnimator;
    private InspectorParser.LayoutData _layout;
    private int _selectedTabIndex = 0;

    // 에디터(Edit Mode) 프리뷰용 상태 변수들
    private SpriteGroupState previewState;
    private double lastUpdateTime;
    private float previewFrameTimer;
    private int previewCurrentFrame;
    private bool isPreviewing;
    private bool previewPingPongForward = true;

    private void OnEnable()
    {
        if (target == null) return;
        _layout = InspectorParser.GetLayout(target.GetType());
        _selectedTabIndex = PlayerPrefs.GetInt($"Tab_{target.GetInstanceID()}", 0);
    }

    private void OnDisable()
    {
        if (target != null) PlayerPrefs.SetInt($"Tab_{target.GetInstanceID()}", _selectedTabIndex);
        StopPreview(); // 에디터 창 포커스 잃거나 닫힐 때 프리뷰 중단
    }

    private void OnDestroy()
    {
        StopPreview();
    }

    public override void OnInspectorGUI()
    {
        if (_layout == null) OnEnable();

        serializedObject.Update();

        // 1. Script 필드 표시 (비활성화)
        SerializedProperty script = serializedObject.FindProperty("m_Script");
        if (script != null)
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(script);
            GUI.enabled = true;
        }

        // 2. 인스펙터 커스텀 레이아웃 그리기 (ShowIf 속성 등 유지)
        if (_layout != null && _layout.HasCustomLayout)
        {
            InspectorDrawer.DrawLayout(serializedObject, _layout, ref _selectedTabIndex);
        }
        else
        {
            DrawPropertiesExcluding(serializedObject, "m_Script");
        }

        // [InspectorButton] 어트리뷰트 버튼들 그리기
        if (_layout != null)
        {
            InspectorDrawer.DrawButtons(targets, _layout.Buttons);
        }

        serializedObject.ApplyModifiedProperties();

        // 3. 테스트 재생용 버튼 그리기 (에디트 모드/플레이 모드 모두 작동)
        DrawAnimationPlayButtons();
    }

    private void DrawAnimationPlayButtons()
    {
        if (_target == null || _target.animations == null || _target.animations.Count == 0) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Play Test Animations (테스트 재생)", EditorStyles.boldLabel);

        bool isPlayMode = Application.isPlaying;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // 에디트 모드이고 프리뷰 중일 때 프리뷰 정지 버튼 노출
        if (!isPlayMode && isPreviewing)
        {
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f); // 연빨강
            if (GUILayout.Button("■ Stop Preview", GUILayout.Height(25)))
            {
                StopPreview();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();
        }

        foreach (var anim in _target.animations)
        {
            if (anim == null || anim.spriteGroup == null) continue;

            string animName = string.IsNullOrEmpty(anim.stateName) ? anim.spriteGroup._Name : anim.stateName;

            // 현재 재생 또는 에디터 프리뷰 중인 상태이면 하이라이트
            bool isCurrent = false;
            if (isPlayMode)
            {
                isCurrent = _target.isPlaying && _target.currentStateName == animName;
            }
            else
            {
                isCurrent = isPreviewing && previewState == anim;
            }

            if (isCurrent)
            {
                GUI.backgroundColor = new Color(0.6f, 1f, 0.6f); // 연초록
            }

            string btnLabel = isPlayMode ? $"▶ Play: {animName}" : $"▶ Preview: {animName}";
            if (GUILayout.Button(btnLabel, GUILayout.Height(25)))
            {
                if (isPlayMode)
                {
                    _target.Play(animName);
                }
                else
                {
                    StartPreview(anim);
                }
            }

            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.EndVertical();
    }

    #region Editor Preview Logic (Edit Mode)

    private void StartPreview(SpriteGroupState anim)
    {
        StopPreview(); // 돌고 있던 이전 프리뷰 정지

        previewState = anim;
        previewCurrentFrame = (anim.playMode == SpriteGroupPlayMode.Once || anim.playMode == SpriteGroupPlayMode.Loop || anim.playMode == SpriteGroupPlayMode.PingPong) ? 0 : anim.spriteGroup.Sprites.Length - 1;
        previewFrameTimer = 0f;
        lastUpdateTime = EditorApplication.timeSinceStartup;
        isPreviewing = true;
        previewPingPongForward = true;

        if (previewState.spriteGroup != null)
        {
            SPRITE_DIR dir = previewState.spriteGroup._dir;
            bool flipX = (dir == SPRITE_DIR.FLIP_X || dir == SPRITE_DIR.FLIP_XY);
            bool flipY = (dir == SPRITE_DIR.FLIP_Y || dir == SPRITE_DIR.FLIP_XY);

            Undo.RecordObject(_target, "Preview Sprite Group Frame");
            _target.flipX = flipX;
            _target.flipY = flipY;

            if (_target.spriteRenderer != null)
            {
                Undo.RecordObject(_target.spriteRenderer, "Preview Sprite Group Frame");
                _target.spriteRenderer.flipX = flipX;
                _target.spriteRenderer.flipY = flipY;
                EditorUtility.SetDirty(_target.spriteRenderer);
            }
            if (_target.uiImage != null)
            {
                Undo.RecordObject(_target.uiImage, "Preview Sprite Group Frame");
                _target.uiImage.SetVerticesDirty();
                EditorUtility.SetDirty(_target.uiImage);
            }
        }

        EditorApplication.update += EditorUpdate;
        ApplyPreviewFrame();
    }

    private void StopPreview()
    {
        if (isPreviewing)
        {
            EditorApplication.update -= EditorUpdate;
            isPreviewing = false;
            previewState = null;
            Repaint();
        }
    }

    private void EditorUpdate()
    {
        if (!isPreviewing || previewState == null || previewState.spriteGroup == null || _target == null)
        {
            StopPreview();
            return;
        }

        double currentTime = EditorApplication.timeSinceStartup;
        double dt = currentTime - lastUpdateTime;
        lastUpdateTime = currentTime;

        Sprite[] sprites = previewState.spriteGroup.Sprites;
        if (sprites == null || sprites.Length == 0)
        {
            StopPreview();
            return;
        }

        previewFrameTimer += (float)dt;
        float fps = previewState.spriteGroup._fps > 0 ? previewState.spriteGroup._fps : 30f;
        float frameDuration = 1f / fps;

        if (previewFrameTimer >= frameDuration)
        {
            int framesToAdvance = Mathf.FloorToInt(previewFrameTimer / frameDuration);
            previewFrameTimer %= frameDuration;

            AdvancePreviewFrames(framesToAdvance, sprites.Length);
        }
    }

    private void AdvancePreviewFrames(int count, int totalFrames)
    {
        for (int i = 0; i < count; i++)
        {
            switch (previewState.playMode)
            {
                case SpriteGroupPlayMode.Once:
                    if (previewCurrentFrame < totalFrames - 1)
                    {
                        previewCurrentFrame++;
                    }
                    else
                    {
                        StopPreview();
                        return;
                    }
                    break;

                case SpriteGroupPlayMode.Backward:
                    if (previewCurrentFrame > 0)
                    {
                        previewCurrentFrame--;
                    }
                    else
                    {
                        StopPreview();
                        return;
                    }
                    break;

                case SpriteGroupPlayMode.Loop:
                    previewCurrentFrame = (previewCurrentFrame + 1) % totalFrames;
                    break;

                case SpriteGroupPlayMode.PingPong:
                    if (previewPingPongForward)
                    {
                        if (previewCurrentFrame < totalFrames - 1)
                        {
                            previewCurrentFrame++;
                        }
                        else
                        {
                            previewPingPongForward = false;
                            previewCurrentFrame = Mathf.Max(0, totalFrames - 2);
                        }
                    }
                    else
                    {
                        if (previewCurrentFrame > 0)
                        {
                            previewCurrentFrame--;
                        }
                        else
                        {
                            previewPingPongForward = true;
                            previewCurrentFrame = Mathf.Min(totalFrames - 1, 1);
                        }
                    }
                    break;
            }
        }

        ApplyPreviewFrame();
    }

    private void ApplyPreviewFrame()
    {
        if (previewState == null || previewState.spriteGroup == null || _target == null) return;
        Sprite[] sprites = previewState.spriteGroup.Sprites;
        if (sprites == null || previewCurrentFrame < 0 || previewCurrentFrame >= sprites.Length) return;

        Sprite sprite = sprites[previewCurrentFrame];

        if (_target.spriteRenderer != null)
        {
            Undo.RecordObject(_target.spriteRenderer, "Preview Sprite Group Frame");
            _target.spriteRenderer.sprite = sprite;
            EditorUtility.SetDirty(_target.spriteRenderer);
        }
        if (_target.uiImage != null)
        {
            Undo.RecordObject(_target.uiImage, "Preview Sprite Group Frame");
            _target.uiImage.sprite = sprite;
            EditorUtility.SetDirty(_target.uiImage);
        }

        // 씬 뷰와 인스펙터 화면 즉시 갱신
        SceneView.RepaintAll();
        Repaint();
    }

    #endregion
}

[CustomPropertyDrawer(typeof(SpriteGroupEvent))]
public class SpriteGroupEventDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty eventProp = property.FindPropertyRelative("onTriggerEvent");
        float eventHeight = EditorGUI.GetPropertyHeight(eventProp, true);

        // 기본 요소: Frame Index 슬라이더 (1줄) + 간격
        float height = EditorGUIUtility.singleLineHeight + 4f;

        // UnityEvent 높이 추가
        height += eventHeight + 4f;
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty frameIndexProp = property.FindPropertyRelative("frameIndex");
        SerializedProperty eventProp = property.FindPropertyRelative("onTriggerEvent");

        // 박스 배경 그리기 (영역 구분용)
        Rect boxRect = new Rect(position.x - 2f, position.y - 2f, position.width + 4f, position.height - 2f);
        GUI.Box(boxRect, "", EditorStyles.helpBox);

        Rect curRect = new Rect(position.x + 4f, position.y + 4f, position.width - 8f, EditorGUIUtility.singleLineHeight);

        SpriteGroup sg = GetSpriteGroup(property);
        if (sg != null && sg.Sprites != null && sg.Sprites.Length > 0)
        {
            int maxFrame = sg.Sprites.Length - 1;
            EditorGUI.BeginChangeCheck();
            int newFrame = EditorGUI.IntSlider(curRect, new GUIContent($"Frame Index (0~{maxFrame})"), frameIndexProp.intValue, 0, maxFrame);
            if (EditorGUI.EndChangeCheck())
            {
                frameIndexProp.intValue = newFrame;

                // 연결된 GameObject의 렌더러 Sprite를 해당 프레임 이미지로 즉시 교체
                SpriteGroupAnimator animator = property.serializedObject.targetObject as SpriteGroupAnimator;
                if (animator != null && newFrame >= 0 && newFrame < sg.Sprites.Length)
                {
                    Sprite sprite = sg.Sprites[newFrame];
                    if (sprite != null)
                    {
                        Undo.RecordObject(animator, "Update Event Frame Preview");
                        
                        SPRITE_DIR dir = sg._dir;
                        bool flipX = (dir == SPRITE_DIR.FLIP_X || dir == SPRITE_DIR.FLIP_XY);
                        bool flipY = (dir == SPRITE_DIR.FLIP_Y || dir == SPRITE_DIR.FLIP_XY);

                        // 렌더러 할당 확인 및 Sprite 적용
                        if (animator.spriteRenderer != null)
                        {
                            Undo.RecordObject(animator.spriteRenderer, "Update Event Frame Preview");
                            animator.spriteRenderer.sprite = sprite;
                            animator.spriteRenderer.flipX = flipX;
                            animator.spriteRenderer.flipY = flipY;
                            EditorUtility.SetDirty(animator.spriteRenderer);
                        }
                        animator.flipX = flipX;
                        animator.flipY = flipY;
                        if (animator.uiImage != null)
                        {
                            Undo.RecordObject(animator.uiImage, "Update Event Frame Preview");
                            animator.uiImage.sprite = sprite;
                            animator.uiImage.SetVerticesDirty();
                            EditorUtility.SetDirty(animator.uiImage);
                        }
                        
                        // 씬 뷰 즉시 갱신
                        SceneView.RepaintAll();
                    }
                }
            }
            curRect.y += EditorGUIUtility.singleLineHeight + 4f;
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            int newFrame = EditorGUI.IntField(curRect, new GUIContent("Frame Index"), frameIndexProp.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                frameIndexProp.intValue = Mathf.Max(0, newFrame);
            }
            curRect.y += EditorGUIUtility.singleLineHeight + 4f;
        }

        // UnityEvent 그리기
        float eventHeight = EditorGUI.GetPropertyHeight(eventProp, true);
        Rect eventRect = new Rect(curRect.x, curRect.y, curRect.width, eventHeight);
        EditorGUI.PropertyField(eventRect, eventProp, true);

        EditorGUI.EndProperty();
    }

    private SpriteGroup GetSpriteGroup(SerializedProperty property)
    {
        string path = property.propertyPath;
        // 경로는 보통 animations.Array.data[0].frameEvents.Array.data[1] 형태입니다.
        int lastEventIdx = path.LastIndexOf(".frameEvents.Array.data");
        if (lastEventIdx != -1)
        {
            string parentPath = path.Substring(0, lastEventIdx);
            SerializedProperty parentProp = property.serializedObject.FindProperty(parentPath);
            if (parentProp != null)
            {
                SerializedProperty spriteGroupProp = parentProp.FindPropertyRelative("spriteGroup");
                if (spriteGroupProp != null && spriteGroupProp.objectReferenceValue is SpriteGroup sg)
                {
                    return sg;
                }
            }
        }
        return null;
    }
}