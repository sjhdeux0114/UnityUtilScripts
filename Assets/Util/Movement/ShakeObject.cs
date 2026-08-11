using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeObject : MonoBehaviour {
    public Vector3 Rotate_Value;
    public AnimationCurve _Value;
    public float Def_Pow = 1;
    public float Def_Time = 0.5f;
    bool bPlay = false;
    public bool bTest = false;
    public Animator _StopAni;

	// Use this for initialization
	void Start () {

		
	}
    public void ShakeObj()
    {
        if (!bPlay)
        {
            bPlay = true;
            StartCoroutine(ShakeProc(Def_Time, Def_Pow));
        }

    }

    public void Shake(float t,float power)
    {
        if (!bPlay)
        {
            bPlay = true;
            StartCoroutine(ShakeProc(t, power));
        }

    }

    public void ShakeRotate(float t, float power)
    {
        if (!bPlay)
        {
            bPlay = true;
            StartCoroutine(ShakeRotateProc(t, power));
        }

    }

    IEnumerator ShakeProc(float t,float p)
    {
        if (_StopAni)
            _StopAni.enabled = false;
        float times = 0;
        Vector3 Org = transform.position;
        while (times < 1)
        {
            times += Time.deltaTime * (1.0f / t);

            if (times >= 1) times = 1;
            float tmp = _Value.Evaluate(times);

            transform.position = Org + p * Rotate_Value * tmp;

            yield return new WaitForEndOfFrame();

            
        }
        if (_StopAni)
            _StopAni.enabled = true;
        transform.position = Org;
        bPlay = false;
    }
    IEnumerator ShakeRotateProc(float t, float p)
    {
        if (_StopAni)
            _StopAni.enabled = false;
        float times = 0;
        Vector3 Org = transform.localEulerAngles;
        while (times < 1)
        {
            times += Time.deltaTime * (1.0f / t);

            if (times >= 1) times = 1;
            float tmp = _Value.Evaluate(times);

            transform.localEulerAngles = Org + p * Rotate_Value * tmp;

            yield return new WaitForEndOfFrame();


        }
        if(_StopAni)
            _StopAni.enabled = true;
        transform.localEulerAngles = Org;
        bPlay = false;
    }

    // Update is called once per frame
    void Update () {

        if (bTest)
        {
            bTest = false;
            ShakeObj();
        }


    }
}
