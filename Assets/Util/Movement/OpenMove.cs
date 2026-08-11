using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenMove : MonoBehaviour
{
    public Transform StartPos;

    float times;
    float delaytimes;

    public bool isPlay = false;

    public Vector3 Result_Pos;
    public Vector3 Result_Rot;
    public Vector3 Result_Scale;

    public Vector3 Dest_Pos;


    public OpenMoveScritable[] MoveType;
    int moveTypeIndex = 0;

    public bool bAwake = true;
    public bool bRandomType = false;
    public int RandomMax = 5;
    public int ReserveIndex = -1;
    public int MoveTypeIndex
    {
        get
        {
            return moveTypeIndex;

        }
        set
        {
            moveTypeIndex = Mathf.Clamp(value, 0, MoveType.Length - 1);
        }
    }
    public OpenMoveScritable GetMode => MoveType[MoveTypeIndex];


    private void OnEnable()
    {
        if (bAwake)
        {
            Init();
            Reset();

            if (ReserveIndex >= 0)
            {
                MoveTypeIndex = ReserveIndex;
                ReserveIndex = -1;
            }
            else if (bRandomType)
            {
                MoveTypeIndex = Random.Range(0, RandomMax);
            }
            Play();
        }
    }

    public void Init()
    {
    }
    public void Def_Pos()
    {
        Result_Pos = transform.localPosition;
        Result_Rot = transform.localEulerAngles;
        Result_Scale = transform.localScale;
    }
    public void Def_Curve()
    {
        Keyframe[] keys = new Keyframe[2];
        keys[0] = new Keyframe(0f, 0f); // �ð� 0, �� 0
        keys[1] = new Keyframe(1f, 1f); // �ð� 1, �� 1

        // AnimationCurve�� Keyframe �迭�� �ʱ�ȭ
        GetMode.Curve_PosX = new AnimationCurve(keys);
        GetMode.Curve_PosY = new AnimationCurve(keys);
        GetMode.Curve_PosZ = new AnimationCurve(keys);
        GetMode.Curve_Rot = new AnimationCurve(keys);
        GetMode.Curve_Scale = new AnimationCurve(keys);
    }

    public void Reset()
    {
        Dest_Pos = StartPos.localPosition;
        transform.localPosition = Dest_Pos;
        transform.localEulerAngles = Result_Rot - GetMode.Dest_Rot;
        transform.localScale = GetMode.Dest_Scale;

        delaytimes = GetMode.DelayTime;
        times = 0;

    }

    public void ViewPer(float p)
    {
        Vector3 pos = Vector3.one;

        float start = Dest_Pos.x;
        float end = Result_Pos.x;
        float p_value = GetMode.Curve_PosX.Evaluate(p);


        pos.x = start + (end - start) * p_value;

        start = Dest_Pos.y;
        end = Result_Pos.y;

        // p ���� GetMode.Curve_PosY.Evaluate(p)�� ����ϴ�.
        p_value = GetMode.Curve_PosY.Evaluate(p);
        pos.y = start + (end - start) * p_value;

        start = Dest_Pos.z;
        end = Result_Pos.z;

        // p ���� GetMode.Curve_PosY.Evaluate(p)�� ����ϴ�.
        p_value = GetMode.Curve_PosZ.Evaluate(p);
        pos.z = start + (end - start) * p_value;


        transform.localPosition = pos;

        float curveP = GetMode.Curve_Rot.Evaluate(p);
        Vector3 totalRot = GetMode.Dest_Rot * curveP;
        transform.localEulerAngles = Result_Rot - GetMode.Dest_Rot + totalRot;


        Vector3 startV = GetMode.Dest_Scale;
        Vector3 endV = Result_Scale;

        p_value = GetMode.Curve_Scale.Evaluate(p);

        transform.localScale = startV + (endV - startV) * p_value;
    }

    public void UpdateDelta(float delta)
    {
        if (delaytimes > 0)
        {
            delaytimes -= delta;
            return;
        }

        times += delta;

        float p = Mathf.Clamp01(times / GetMode.MoveTime);

        ViewPer(p);

        if (p >= 1.0f)
        {
            isPlay = false;
            if (Application.isPlaying)
            {
                if (GetMode.snd_End != null)
                    SoundEventBus.PlayEffect(GetMode.snd_End);
            }
        }
    }

    public void Play()
    {
        Reset();
        isPlay = true;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isPlay)
        {
            UpdateDelta(Time.deltaTime);

        }
    }
}
