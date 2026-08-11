using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skyRotate : MonoBehaviour {

    Material skyMat;
    public float Speed;
    float _rotation;

	// Use this for initialization
	void Start () {

        skyMat = RenderSettings.skybox;
        _rotation = 0;

    }
	
	// Update is called once per frame
	void Update () {

        skyMat = RenderSettings.skybox;

        _rotation += Speed * Time.deltaTime;
        if (_rotation >= 360)
            _rotation -= 360;

        skyMat.SetFloat("_Rotation", _rotation);


    }
}
