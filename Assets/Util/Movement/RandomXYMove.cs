using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomXYMove : MonoBehaviour
{
    public F_POINT PosX;
    public F_POINT PosY;
    public F_POINT PosXRnd;
    public F_POINT PosYRnd;
    public F_POINT MoveSpeed;
    public F_POINT LoopDelay;
    public F_POINT RND_Scale = new F_POINT(1, 1);
    public bool bLoop;
    public bool bVertical = false;
    public bool bRND_End = false;
    public AnimationCurve _Curve;
    public float CurveSpeed;
    public float CurvePow;
    public float AddScale = 0;
    public bool bScale = true;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void OnEnable()
    {
        StartCoroutine(MoveProc());
    }

    IEnumerator MoveProc()
    {
        int cnt = 1;
        while (cnt > 0)
        {
            float CurveSpeedIn = CurveSpeed - CurveSpeed * 0.2f + Random.Range(0.0f, CurveSpeed * 0.4f);
            if (bVertical)
                transform.localPosition = new Vector3(Random.Range(PosX.min, PosX.max), PosY.min, 0);
            else
                transform.localPosition = new Vector3(PosX.min, Random.Range(PosY.min, PosY.max), 0);
            if (bScale)
                transform.localScale = Vector3.one * Random.Range(RND_Scale.min, RND_Scale.max);

            yield return new WaitForSeconds(Random.Range(LoopDelay.min, LoopDelay.max));
            float _Delta = -1;
            float _Cross_Delta = 1.0f / (transform.localPosition.y - Random.Range(PosYRnd.min, PosYRnd.max));

            if ((PosX.min - PosX.max) < 0)
                _Delta = 1;

            if (bVertical)
            {
                _Delta = -1;
                if ((PosY.min - PosY.max) < 0)
                    _Delta = 1;
                _Cross_Delta = (transform.localPosition.x - Random.Range(PosXRnd.min, PosXRnd.max));
            }

            if (!bRND_End)
                _Cross_Delta = 0;
            float fSpeed = Random.Range(MoveSpeed.min, MoveSpeed.max);
            while (true)
            {
                if (bVertical)
                {

                    float TimesGap = (Time.time * 100) % CurveSpeedIn;
                    float per = TimesGap / CurveSpeedIn;
                    transform.localPosition += new Vector3(_Cross_Delta * Time.deltaTime + _Curve.Evaluate(per) * CurvePow,
                        _Delta * fSpeed * Time.deltaTime, 0);
                    if (_Delta > 0)
                    {
                        if (transform.localPosition.y >= PosY.max)
                            break;
                    }
                    else
                    {
                        if (transform.localPosition.y <= PosY.max)
                            break;
                    }

                }
                else
                {
                    transform.localPosition += new Vector3(_Delta * fSpeed * Time.deltaTime, _Cross_Delta * Time.deltaTime, 0);
                    if (_Delta > 0)
                    {
                        if (transform.localPosition.x >= PosX.max)
                            break;
                    }
                    else
                    {
                        if (transform.localPosition.x <= PosX.max)
                            break;
                    }
                }

                transform.localScale += AddScale * Vector3.one * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            if (bLoop)
                cnt++;
            else
                cnt--;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
