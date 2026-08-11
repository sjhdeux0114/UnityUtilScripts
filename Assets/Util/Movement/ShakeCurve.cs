using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeCurve : MonoBehaviour
{
    public AnimationCurve _CurveX;
    public AnimationCurve _CurveY;
    public AnimationCurve _CurveZ;
    public float Pow;
    public float mTime;
    public bool bLoop;
    public bool bStart;
    bool bOn = false;
    float times = 0;
    Vector3 OrgRot;
    bool bLoad = false;
    // Start is called before the first frame update
    void Start()
    {
        if (!bLoad)
        {
            OrgRot = transform.localEulerAngles;

            bLoad = true;
        }

        if (bStart)
            Play();

    }

    public void Play()
    {
        if (!bLoad)
        {
            OrgRot = transform.localEulerAngles;
            bLoad = true;
        }
        bOn = true;
        times = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(bOn)
        {
            times += Time.deltaTime * (1.0f / mTime);
            transform.localEulerAngles = OrgRot + new Vector3(_CurveX.Evaluate(times),
                _CurveY.Evaluate(times),
                _CurveZ.Evaluate(times))* Pow;

            if(times >= 1)
            {
                if(bLoop)
                {
                    times = 0;
                }
                else
                {
                    bOn = false;
                }
            }

        }
    }
}
