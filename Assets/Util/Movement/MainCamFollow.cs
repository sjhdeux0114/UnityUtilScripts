using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamFollow : MonoBehaviour {

    public Camera _Cam;
    public Vector3 _Add_Pos;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        _Cam = Camera.main;
        if (_Cam != null)
        {
            
            transform.eulerAngles = _Cam.transform.eulerAngles;
            transform.position = _Cam.transform.position + _Add_Pos;
        }
		
	}
}
