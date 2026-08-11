using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChangeAni : MonoBehaviour
{
    public Material _Mat;
    public string MatName;
    public float Times;
    public AnimationCurve _TimeCurve;
    public AnimationCurve _ValueCurve;
    public bool bOn;
    float TimeCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(bOn)
        {
            TimeCount += Time.deltaTime;

            if (TimeCount >= Times)
                TimeCount -= Times;

            float p = TimeCount / Times;
            if (p >= 1)
                p = 1;
            p = _TimeCurve.Evaluate(p);


            _Mat.SetFloat(MatName, _ValueCurve.Evaluate(p));




        }
        else
        {
            TimeCount = 0;
        }
        
    }
}
