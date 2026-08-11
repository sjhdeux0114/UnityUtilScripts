using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public enum _ActType
{
    ROTATE_ON=0,
    ROTATE_OFF,
    LOOKTarget_ON,
    LOOKTarget_OFF,
}

[System.Serializable]
public class TargetEvent
{
    public float _Times;
    public _ActType Action;
    public bool _On;
}
[System.Serializable]
public class TargetPoint
{
    public Transform targets;
    public float Speed;
    public float Cam_fov;
}

[System.Serializable]
public enum _ACTION_TYPE
{
    DELAY_CAM,
    MOVE_NEXT_POS,
    DIRECT_LOOK,
    MOVE_STOP,
    EMPTY,
    MOVE_NEXT_LOOK,

}
[System.Serializable]
public class DelayAniClass
{
    public float _Times;
    public float delay_Time;
    public _ACTION_TYPE TYPE;
}

public class MoveTarget : MonoBehaviour {

    public TargetPoint[] targets;
    public DelayAniClass[] Delay_Action;

    public Color LineColor = Color.red;

    public bool isCam;

    public int target_Num;
    public float RotateDamp = 1;
    public float Rotate_Speed = 1;
    public float Rotate_Speed_Max = 1;
    public float MinDistance=1;

    public Transform Target_Root;
    public bool bRotate = true;
    public bool bLookTarget = false;
    public bool bGotoTarget = false;

    bool bRotate_Save = true;
    bool bLookTarget_Save = false;

    public Transform LookTarget;

    public TargetEvent[] _ActEvent;
    public float PlayTime;
    public Camera _cam;
    public float Delay_Time;
    public float Stop_Time;
    public bool isDelay=false;
    public bool isDelay2 = false;
    public float Gugan_Time = 0;
    public bool bPause = true;
    public bool bLate = false;

    // Use this for initialization
    void Awake () {


        foreach (TargetPoint _t in targets)
        {
            _t.targets.gameObject.SetActive(false);
        }



        Hard_Reset();




    }


    IEnumerator Delay_Action_Func()
    {
        int Count = 0;

        while (true)
        {
            yield return new WaitForSeconds(Delay_Action[Count]._Times);

            if(Delay_Action[Count].TYPE == _ACTION_TYPE.DELAY_CAM)
                Delay_Time = Delay_Action[Count].delay_Time;
            else if (Delay_Action[Count].TYPE == _ACTION_TYPE.MOVE_NEXT_POS)
            {
                transform.position = targets[target_Num].targets.position;

                target_Num++;
                if (target_Num >= targets.Length)
                    target_Num = 0;
                
            }
            else if (Delay_Action[Count].TYPE == _ACTION_TYPE.DIRECT_LOOK)
            {
                //transform.position = targets[target_Num].targets.position;
                //target_Num++;
                //if (target_Num >= targets.Length)
                //    target_Num = 0;

                transform.LookAt(targets[target_Num].targets.position);
            }
            else if (Delay_Action[Count].TYPE == _ACTION_TYPE.MOVE_STOP)
            {
                Stop_Time = Delay_Action[Count].delay_Time;
            }
            else if (Delay_Action[Count].TYPE == _ACTION_TYPE.MOVE_NEXT_LOOK)
            {
                transform.position = targets[target_Num].targets.position;

                target_Num++;
                if (target_Num >= targets.Length)
                    target_Num = 0;
                transform.LookAt(targets[target_Num].targets.position);
            }
            

            Count++;

            if (Count >= Delay_Action.Length)
                break;

            yield return new WaitForSeconds(Delay_Time);
        }
    }

    void Start()
    {
        if(isCam)
        {
            _cam = null;
            _cam = GetComponent<Camera>();
            if (_cam == null)
                isCam = false;
        }

        Hard_Reset();
    }

    // Update is called once per frame
    void Update () {

        if(!bLate)
            _Move();


        if(isDelay)
        {
            Delay_Time = 1;
            isDelay = false;

        }
        if (isDelay2)
        {
            Delay_Time = 2;
            isDelay2 = false;

        }
    }

    private void LateUpdate()
    {
        if(bLate)
            _Move();


    }

    public void Hard_Reset()
    {

        Delay_Time = 0;
        StopAllCoroutines();
        transform.position = targets[0].targets.position;
        transform.eulerAngles = targets[0].targets.eulerAngles;
        transform.LookAt(targets[1].targets.position);

        bRotate_Save = bRotate;
        bLookTarget_Save = bLookTarget;


        target_Num = 1;
        PlayTime = 0;

        for (int i = 0; i < _ActEvent.Length; i++)
        {
            _ActEvent[i]._On = true;
        }

        if (Delay_Action.Length > 0)
        {
            StartCoroutine(Delay_Action_Func());
        }
    }

    public void _Reset()
    {
        Delay_Time = 0;
        StopAllCoroutines();

        transform.position = targets[0].targets.position;
        transform.eulerAngles = targets[0].targets.eulerAngles;
        transform.LookAt(targets[1].targets.position);

        bRotate = bRotate_Save;
        bLookTarget = bLookTarget_Save;


        target_Num = 1;
        PlayTime = 0;

        for (int i=0;i< _ActEvent.Length;i++)
        {
            _ActEvent[i]._On = true;
        }

        if (Delay_Action.Length > 0)
        {
            StartCoroutine(Delay_Action_Func());
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void _Move(float delta_time = -1)
    {
        if(Delay_Time > 0)
        {
            Delay_Time -= Time.deltaTime;
            return;
        }

        Vector3 Old_pos = transform.position;
        if(Stop_Time > 0)
        {
            Stop_Time -= Time.deltaTime;
        }


        Gugan_Time += Time.deltaTime;
        if (delta_time < 0)
            delta_time = Time.deltaTime;

        if (delta_time <= 0) return;

        PlayTime += delta_time;

        int now = target_Num - 1;
        if (now < 0)
            now = targets.Length - 1;
        if (bGotoTarget)
        {
            Vector3 dir = Vector3.Normalize( targets[target_Num].targets.position - transform.position);
            transform.position += dir * targets[now].Speed * delta_time;
        }
        else
            transform.position += transform.forward * targets[now].Speed * delta_time;

        if (targets[now].Speed >= 1000)
        {
            transform.position = targets[target_Num].targets.position;
        }

        if (Stop_Time > 0)
            transform.position = Old_pos;

        int prev_num = target_Num - 1;
        if (prev_num < 0)
            prev_num = 0;

            //        Debug.Log(""+ target_Num + ","+targets[now].Speed);
        float max = Mathf.Abs(Vector3.Distance(targets[prev_num].targets.position, targets[target_Num].targets.position));

        float now_dis = Mathf.Abs(Vector3.Distance(transform.position, targets[target_Num].targets.position));
        


        float dis_pers = 1;
        if (max > 0)
            dis_pers = 1 - (now_dis / max);
        else
            dis_pers = 1;

        float Time_Ret = Gugan_Time;
        if (Time_Ret >= 1)
            Time_Ret = 1;
        float rotate_ret = Rotate_Speed + (Rotate_Speed_Max - Rotate_Speed) * Time_Ret;

        if (bRotate)
        {
            if (targets[now].Speed >= 1000)
            {
                
                transform.rotation = targets[target_Num].targets.rotation;
            }
            else
            {
                float Rotate_Damp_Ret = RotateDamp * targets[now].Speed / rotate_ret;
                Quaternion rot;
                if (bLookTarget)
                    rot = Quaternion.LookRotation(LookTarget.position - transform.position);
                else
                    rot = Quaternion.LookRotation(targets[target_Num].targets.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, delta_time * Rotate_Damp_Ret);
            }
        }
        else
        {
            if (bLookTarget)
                transform.LookAt(LookTarget);
        }

        if(isCam)
        {
            float fov = 0;
            
            
            if(prev_num < 0)
            {
                fov = targets[0].Cam_fov;
            }
            else
            {
                

                fov = targets[prev_num].Cam_fov + (targets[target_Num].Cam_fov- targets[prev_num].Cam_fov) * dis_pers;

            }

            _cam.fieldOfView = fov;
        }


        if (Vector3.Distance(transform.position, targets[target_Num].targets.position) <= ((targets[now].Speed / MinDistance)))
        {
            target_Num++;
            if (target_Num >= targets.Length)
                target_Num = 0;

            Gugan_Time = 0;
        }

        for (int i = 0; i < _ActEvent.Length; i++)
        {
            if (_ActEvent[i]._On)
            {
                if(_ActEvent[i]._Times <= PlayTime)
                {
                    _ActEvent[i]._On = false;
                    if (_ActEvent[i].Action == _ActType.LOOKTarget_OFF)
                        bLookTarget = false;
                    if (_ActEvent[i].Action == _ActType.LOOKTarget_ON)
                        bLookTarget = true;
                    if (_ActEvent[i].Action == _ActType.ROTATE_ON)
                        bRotate = true;
                    if (_ActEvent[i].Action == _ActType.ROTATE_OFF)
                        bRotate = false;
                }
            }

        }
    }

    [ContextMenu("GetData")]
    void GetData()
    {
        float speed = 40;
        float fov = 0;
        if (targets.Length > 0)
        {
            speed = targets[0].Speed;
            fov = targets[0].Cam_fov;
        }
        targets = new TargetPoint[Target_Root.childCount];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = new TargetPoint();
            targets[i].targets = Target_Root.GetChild(i);
            targets[i].Speed = speed;
            targets[i].Cam_fov = fov;
        }
    }

    [ContextMenu("GetData2")]
    void GetData2()
    {
        int old_Cnt = targets.Length;
        float[] speed_old = new float[old_Cnt];
        float[] fov_old = new float[old_Cnt];

        for (int i = 0; i < old_Cnt; i++)
        {
            speed_old[i] = targets[i].Speed;
            fov_old[i] = targets[i].Cam_fov;
        }


            float speed = 40;
        float fov = 0;


        if (targets.Length > 0)
        {
            speed = targets[0].Speed;
            fov = targets[0].Cam_fov;
        }
        targets = new TargetPoint[Target_Root.childCount];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = new TargetPoint();
            targets[i].targets = Target_Root.GetChild(i);
            if (i > 0)
            {
                targets[i].Speed = targets[i - 1].Speed;
                targets[i].Cam_fov = targets[i - 1].Cam_fov;
                
            }
            else
            {
                targets[i].Speed = speed;
                targets[i].Cam_fov = fov;
            }

            if(old_Cnt > i)
            {
                targets[i].Speed = speed_old[i];
                targets[i].Cam_fov = fov_old[i];

            }
        }
    }
}
