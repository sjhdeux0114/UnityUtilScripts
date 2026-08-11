using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

// This script can be used with both SpriteRenderer and UI.Image components.
// It allows you to manage and play different animations based on a list of sprites.

public class SimpleSpriteAnimator : MonoBehaviour
{
    // The component to control. Automatically detects either SpriteRenderer or Image.
    private SpriteRenderer spriteRenderer;
    private Image uiImage;

    public string StartAnimation;
    public string NowAnimation;

    public SimpleSpriteInfo[] infoData;
    Dictionary<string, SimpleSpriteInfo> infoDict = new Dictionary<string, SimpleSpriteInfo>();
    public SimpleSpriteInfo GetInfo(string names) { 
        if (infoDict.ContainsKey(names))
            return infoDict[names];
        return null;
    }

    [Tooltip("List of all animations.")]
    public List<AnimationData> animations = new List<AnimationData>();

    // Current animation information
    private AnimationData currentAnimation;

    private int currentFrameIndex;
    private float frameTimer;
    private int pingpongDirection = 1;
    private bool isPlaying = false;
    public bool IsPlaying { get { return isPlaying; } }
    public Queue<string> playQueue = new Queue<string>();
    public UnityAction<string> Act_Start;
    public UnityAction<string> Act_End;

    public UnityEvent EV_On;
    public Vector3 Org_Pos;
    public Vector3 Target_Pos;
    public Vector2 Fixed_Pos;
    public bool FixPositionX;
    public bool FixPositionY;
    public bool bMove = false;
    RectTransform _Rect;


    public Dictionary<string, AnimationData> animationDict = new Dictionary<string, AnimationData>();

    bool bInit = false;
    public void _Init_Dic()
    {
        if (bInit) return;
        Org_Pos = transform.localPosition;
        _Rect = gameObject.GetComponent<RectTransform>();
        if (FixPositionX)
        {
            Fixed_Pos.x = transform.localPosition.x;
        }
        if (FixPositionY)
        {
            Fixed_Pos.y = transform.localPosition.y;
        }
        bInit = true;
        // Get the appropriate component
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<Image>();

        if (spriteRenderer == null && uiImage == null)
        {
            Debug.LogError("SimpleSpriteAnimator requires either a SpriteRenderer or an Image component.");
        }

        for (int i = 0; i < infoData.Length; i++)
        {
            infoData[i].names = infoData[i].names.ToLower().Trim();
            Debug.Log("이름들 : " + infoData[i].names);
            if (!infoDict.ContainsKey(infoData[i].names))
                infoDict.Add(infoData[i].names.ToLower(), infoData[i]);
        }

        RefreshDictionary();
    }

    public void ChangeInfoData(int index)
    {
        if (!bInit) _Init_Dic();
        if (index < 0 || index >= infoData.Length)
        {
            Debug.Log($"잘못된 index 번호  : {index} , {infoData.Length}");
            return;
        }
        if (infoData[index] != null)
        {
            animations = infoData[index].animations;
            RefreshDictionary();
        }

    }
    public void ChangeInfoData(string names)
    {
        if (!bInit) _Init_Dic();
        if (infoDict.ContainsKey(names))
        {
            animations = infoDict[names].animations;
            RefreshDictionary();
        }

    }

    public void RefreshDictionary()
    {
        animationDict.Clear();
        if (animations == null) return;

        for (int i = 0; i < animations.Count; i++)
        {
            if (animations[i] == null) continue;

            animations[i].animationName = animations[i].animationName?.ToLower().Trim() ?? "";
            animations[i].NextAnimation = animations[i].NextAnimation?.ToLower().Trim() ?? "";

            if (!animationDict.ContainsKey(animations[i].animationName))
                animationDict.Add(animations[i].animationName, animations[i]);
        }
    }

    private void Awake()
    {
        _Init_Dic();
    }

    private void OnEnable()
    {
        CoroutineUtil.NextFrame(() =>
        {
            EV_On.Invoke();

        });

        StartAnimation = StartAnimation.ToLower().Trim();
        if (animationDict.ContainsKey(StartAnimation))
        {
            Play(StartAnimation);

        }
    }

    public void _Update(float t)
    {

        if (!isPlaying || currentAnimation == null || currentAnimation.sprites == null || currentAnimation.sprites.Count == 0)
        {
            return;
        }
        if (bMove)
        {
            if (Application.isPlaying)
                transform.localPosition = Vector3.Lerp(transform.localPosition, Target_Pos, Time.deltaTime * 5);
            else
                transform.localPosition = Target_Pos;
        }

        frameTimer += t;
        float frameDuration = 1f / currentAnimation.framesPerSecond;

        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            UpdateFrame();
        }


    }


    private void Update()
    {
        _Update(Time.deltaTime);

    }
    // --- Public Methods ---

    // Starts playing an animation.
    public void Play(string aniName)
    {

        aniName = aniName.ToLower().Trim();

        var nextAnimation = animations.FirstOrDefault(a => a.animationName == aniName);

        if (nextAnimation == null)
        {
            Debug.LogWarning($"Animation '{aniName}' not found.");
            return;
        }


        nextAnimation.EV_Start.Invoke();
        NowAnimation = aniName;
        currentAnimation = nextAnimation;

        if (currentAnimation.SizeDelta != Vector2.zero)
        {
            if (_Rect == null)
                _Rect = gameObject.GetComponent<RectTransform>();

            _Rect.sizeDelta = currentAnimation.SizeDelta;
        }
        isPlaying = true;
        currentFrameIndex = 0;
        if (currentAnimation.mode == ANIMATION_MODE.Backward)
        {
            currentFrameIndex = currentAnimation.sprites.Count - 1;
        }
        frameTimer = 0;
        if (Application.isPlaying)
        {
            if (currentAnimation.snd != null)
            {
                if (currentAnimation.SndDelay > 0)
                {
                    CoroutineUtil.Delay(currentAnimation.SndDelay, () =>
                    {
                        SoundEventBus.PlayEffect(currentAnimation.snd);
                    });
                }
                else
                    SoundEventBus.PlayEffect(currentAnimation.snd);

            }
            if (Act_Start != null)
                Act_Start.Invoke(aniName);
        }

        
        if (FixPositionX)
        {
            Org_Pos.x = Fixed_Pos.x;
        }
        if (FixPositionY)
        {
            Org_Pos.y = Fixed_Pos.y;
        }
        Target_Pos = Org_Pos;

        if (bMove)
        {
            UpdateSprite();
            transform.localPosition = Target_Pos;
        }

        ResetEvents();
        UpdateSprite();
    }

    // Queues an animation to play after the current one finishes.
    public void AddNext(string aniName)
    {
        aniName = aniName.ToLower().Trim();
        playQueue.Enqueue(aniName);
    }

    // Stops the current animation.
    public void Stop()
    {
        isPlaying = false;
        playQueue.Clear();
    }

    // --- Private Methods ---

    private void UpdateFrame()
    {
        switch (currentAnimation.mode)
        {
            case ANIMATION_MODE.Once:
                currentFrameIndex++;
                if (currentFrameIndex >= currentAnimation.sprites.Count)
                {
                    Target_Pos = Org_Pos;
                    transform.localPosition = Target_Pos;
                    isPlaying = false;
                    currentFrameIndex = currentAnimation.sprites.Count - 1; // Hold the last frame

                    if (Act_End != null)
                        Act_End.Invoke(currentAnimation.animationName);

                    if (currentAnimation.EV_End != null)
                    {
                        currentAnimation.EV_End.Invoke();
                    }

                    if (Application.isPlaying && currentAnimation.AutoDestroy)
                        Destroy(gameObject);
                    else if (Application.isPlaying && currentAnimation.AutoHide)
                        gameObject.SetActive(false);
                    else
                    {
                        if (animationDict.ContainsKey(currentAnimation.NextAnimation))
                        {
                            AddNext(currentAnimation.NextAnimation);
                        }
                        CheckPlayQueue();
                    }

                }
                break;
            case ANIMATION_MODE.Loop:

                if (currentFrameIndex + 1 >= currentAnimation.sprites.Count)
                {
                    ResetEvents();
                }
                currentFrameIndex = (currentFrameIndex + 1) % currentAnimation.sprites.Count;
                break;
            case ANIMATION_MODE.Pingpong:
                currentFrameIndex += pingpongDirection;
                if (currentFrameIndex >= currentAnimation.sprites.Count - 1)
                {
                    pingpongDirection = -1;
                    ResetEvents();
                }
                else if (currentFrameIndex <= 0)
                {
                    pingpongDirection = 1;
                    ResetEvents();
                }
                break;
            case ANIMATION_MODE.Backward:
                currentFrameIndex--;
                if (currentFrameIndex < 0)
                {
                    Target_Pos = Org_Pos;
                    transform.localPosition = Target_Pos;
                    isPlaying = false;
                    currentFrameIndex = 0; // Hold the first frame

                    if (Act_End != null)
                        Act_End.Invoke(currentAnimation.animationName);

                    if (currentAnimation.EV_End != null)
                    {
                        currentAnimation.EV_End.Invoke();
                    }


                    if (currentAnimation.AutoDestroy)
                        Destroy(gameObject);
                    else if (currentAnimation.AutoHide)
                        gameObject.SetActive(false);
                    else
                    {
                        if (animationDict.ContainsKey(currentAnimation.NextAnimation))
                        {
                            AddNext(currentAnimation.NextAnimation);
                        }
                        CheckPlayQueue();
                    }

                }
                break;
        }
        UpdateSprite();
    }

    private void ResetEvents()
    {
        if (currentAnimation != null && currentAnimation.EV_Actions != null)
        {
            foreach (var action in currentAnimation.EV_Actions)
            {
                if (action != null)
                    action.bEvent = false;
            }
        }
    }

    private void CheckEvents()
    {
        if (currentAnimation != null && currentAnimation.EV_Actions != null)
        {
            foreach (var action in currentAnimation.EV_Actions)
            {
                if (action != null && !action.bEvent)
                {
                    bool passed = false;
                    if (currentAnimation.mode == ANIMATION_MODE.Backward)
                    {
                        if (currentFrameIndex <= action.frame) passed = true;
                    }
                    else if (currentAnimation.mode == ANIMATION_MODE.Pingpong)
                    {
                        if (pingpongDirection > 0 && currentFrameIndex >= action.frame) passed = true;
                        if (pingpongDirection < 0 && currentFrameIndex <= action.frame) passed = true;
                    }
                    else
                    {
                        if (currentFrameIndex >= action.frame) passed = true;
                    }

                    if (passed)
                    {
                        action.bEvent = true;
                        if (action.EV != null)
                            action.EV.Invoke();
                    }
                }
            }
        }
    }

    private void UpdateSprite()
    {
        if (currentAnimation.sprites.Count > 0)
        {

            if (currentAnimation.MoveVector != Vector2.zero)
            {
                float per = (float)currentFrameIndex / currentAnimation.sprites.Count;
                Target_Pos = Org_Pos + new Vector3(currentAnimation.MoveVector.x * currentAnimation.curveX.Evaluate(per),
                    currentAnimation.MoveVector.y * currentAnimation.curveY.Evaluate(per), 0);

                if (currentAnimation.MoveVector.y == 0) Target_Pos.y = Org_Pos.y;
                if (currentAnimation.MoveVector.x == 0) Target_Pos.x = Org_Pos.x;

            }

            Sprite spriteToDisplay = currentAnimation.sprites[currentFrameIndex];
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = spriteToDisplay;
            }
            else if (uiImage != null)
            {
                uiImage.sprite = spriteToDisplay;
            }

            CheckEvents();
        }
    }

    private void CheckPlayQueue()
    {
        if (playQueue.Count > 0)
        {
            string nextAnimationName = playQueue.Dequeue();
            Play(nextAnimationName);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (animations == null) return;

        foreach (var anim in animations)
        {
            if (anim == null) continue;

            // FPS 기본값
            if (anim.framesPerSecond <= 0f)
                anim.framesPerSecond = 24f;

            // CurveX 기본값
            if (anim.curveX == null || anim.curveX.keys == null || anim.curveX.keys.Length == 0)
            {
                anim.curveX_Init();
            }

            // CurveY 기본값
            if (anim.curveY == null || anim.curveY.keys == null || anim.curveY.keys.Length == 0)
            {
                anim.curveY_Init();
            }
        }
    }
#endif
}