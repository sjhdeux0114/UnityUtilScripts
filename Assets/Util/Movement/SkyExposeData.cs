using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyExposeData : MonoBehaviour {

    public float Speed = 3;
    public AnimationCurve _Curve;
    public string _ShaderDataName;
    float _time_t = 0;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        _time_t += Time.deltaTime;
        if (_time_t >= Speed)
            _time_t -= Speed;

        float ret_time = _time_t / Speed;
        if (ret_time < 0) ret_time = 0;
        if (ret_time >= 1) ret_time = 1;
        float t = _Curve.Evaluate(ret_time);

        RenderSettings.skybox.SetFloat(_ShaderDataName, t);



    }
}
