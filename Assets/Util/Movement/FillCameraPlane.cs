using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillCameraPlane : MonoBehaviour
{
    public Camera _Cam;
    public float Distance;
    public Vector3 Rotate_Angle = new Vector3(90,0,0);
    public bool bQuad;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_Cam == null)
            _Cam = Camera.main;

        float pos = Distance;// (_Cam.nearClipPlane + 0.01f);

        transform.position = _Cam.transform.position + _Cam.transform.forward * pos;
        transform.LookAt(_Cam.transform);
        transform.Rotate(Rotate_Angle);

        float h = (Mathf.Tan(_Cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f) ;
        if(bQuad)
            transform.localScale = new Vector3(h * _Cam.aspect, h, 1.0f);
        else
            transform.localScale = new Vector3(h * _Cam.aspect, 1.0f, h);

    }
}
