using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ANIMATION_MODE
{
    Once,
    Loop,
    Pingpong,
    Backward
}

[System.Serializable]
public class UnityEventAction
{
    public UnityEvent EV;
    public bool bEvent;
    public int frame;
}

[System.Serializable]
public class AnimationData
{
    [Tooltip("Name of the animation. Used to call with the Play method.")]
    public string animationName;
    [Tooltip("Sprites for this animation.")]
    public List<Sprite> sprites;
    [Tooltip("Playback mode of the animation.")]
    public ANIMATION_MODE mode;
    [Tooltip("Frames per second (FPS).")]
    [Range(1, 60)]
    public float framesPerSecond = 24f;

    // 🔥 여기서 바로 기본값 주기
    [SerializeField]
    AnimationCurve CurveX = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public AnimationCurve curveX => CurveX;
    [SerializeField]
    AnimationCurve CurveY = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public AnimationCurve curveY => CurveY;
    public void curveX_Init() { CurveX = AnimationCurve.Linear(0f, 0f, 1f, 1f); }
    public void curveY_Init() { CurveY = AnimationCurve.Linear(0f, 0f, 1f, 1f); }
    public Vector2 MoveVector = Vector2.zero;
    public Vector2 SizeDelta = Vector2.zero;


    public string NextAnimation;
    public bool AutoHide;
    public bool AutoDestroy;
    public SndClass snd = new SndClass(null);
    public float SndDelay = 0.0f;
    public UnityEvent EV_Start;
    public UnityEvent EV_End;
    public List<UnityEventAction> EV_Actions;

    public AnimationData()
    {
        // 여기는 "코드로 new 할 때"만 유효 (Inspector 추가랑은 별개)
        if (string.IsNullOrEmpty(animationName))
            animationName = $"Ani{System.DateTime.Now.Minute}_{System.DateTime.Now.Second}";

        if (snd == null)
            snd = new SndClass(null);

        snd.vol = 100;
        curveX_Init();
        curveY_Init();
        framesPerSecond = 24;
    }
}

[CreateAssetMenu(fileName = "SimpleSpriteInfo", menuName = "ScriptableObjects/SimpleSpriteInfo", order = 1)]
public class SimpleSpriteInfo : ScriptableObject
{
    public string names;
    public List<AnimationData> animations = new List<AnimationData>();

    private void OnValidate()
    {
        if (animations != null)
        {
            foreach (var anim in animations)
            {
                if (anim.curveX == null || anim.curveX.keys == null || anim.curveX.keys.Length == 0)
                    anim.curveX_Init();


                if (anim.curveY == null || anim.curveY.keys == null || anim.curveY.keys.Length == 0)
                    anim.curveY_Init();


                if (anim.framesPerSecond <= 0)
                    anim.framesPerSecond = 24f;
            }
        }
    }
}