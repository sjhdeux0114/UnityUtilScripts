using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public enum MOVE_LOOP_TYPE
{
    _ONCE,
    _LOOP,
    _PINGPONG
}

public class MoveLooping : MonoBehaviour
{
    public Transform _Target;
    public Vector3 Min_Pos;
    public Vector3 Max_Pos;
    [Header("시작부터 끝까지 이동 시간")]
    public float OnTime;
    public MOVE_LOOP_TYPE _LoopType = MOVE_LOOP_TYPE._ONCE;
    public bool bStart;
    public bool bReset;
    bool bAct;
    Vector3 Move_Delta;
    Vector3 Start_Pos;
    Vector3 Target_pos;
    float PlayTimes;
    public bool bAction = false;
    public bool bBackward;

    public UnityEvent Act_Event;
    public UnityEvent Reset_Event;

    // Start is called before the first frame update
    void Start()
    {
        if (!_Target)
            _Target = transform;
    }
    private void OnEnable()
    {
        bAct = false;
        if (bStart)
        {
            _Play();
        }
    }
    public void _Stop()
    {
        bAct = false;
    }
    public void _Play(bool bResetPos=true,bool backward=false)
	{
        bBackward = backward;
        bAct = true;
	    PlayTimes = 0;
        if (bReset)
        {
            if (bResetPos)
            {
                if (bBackward)
                    _Target.localPosition = Max_Pos;
                else
                    _Target.localPosition = Min_Pos;


                Start_Pos = Min_Pos;
                Target_pos = Max_Pos;

                if (bBackward)
                {
                    Start_Pos = Max_Pos;
                    Target_pos = Min_Pos;

                    Reset_Event.Invoke();
                }
                else
                {
                    Act_Event.Invoke();
                }

            }
            else
            {
                Start_Pos = _Target.localPosition;
                Target_pos = Max_Pos;

                if (bBackward)
                {
                    Target_pos = Min_Pos;
                    Reset_Event.Invoke();
                }
                else
                {
                    Act_Event.Invoke();
                }

                
            }
        }
		
    }

    [ContextMenu("Get Min")]
    public void Get_MinPos()
    {
        Min_Pos = _Target.localPosition;
    }
    [ContextMenu("Get Max")]
    public void Get_MaxPos()
    {
        Max_Pos = _Target.localPosition;
    }

    [ContextMenu("Set Min")]
    public void Set_MinPos()
    {
        _Target.localPosition = Min_Pos;
    }
    [ContextMenu("Set Max")]
    public void Set_MaxPos()
    {
        _Target.localPosition = Max_Pos;
    }

    // Update is called once per frame
    void Update()
    {
        if(bAct)
        {
            PlayTimes += Time.deltaTime;
            float t = PlayTimes / OnTime;
            if (t >= 1) t = 1;
            _Target.localPosition = Vector3.Lerp(Start_Pos,Target_pos,t);

            if(t >= 1)
            {
                if (_LoopType == MOVE_LOOP_TYPE._LOOP)
                {
                    PlayTimes = 0;
                }
                else if (_LoopType == MOVE_LOOP_TYPE._PINGPONG)
                {
                    PlayTimes = 0;
                    Vector3 tmp = Start_Pos;
                    Start_Pos = Target_pos;
                    Target_pos = tmp;
                }
            }

        }

        if(bAction)
        {
            bAction = false;
	        _Play();
        }
    }
}
