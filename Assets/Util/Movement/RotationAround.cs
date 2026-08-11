using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RotationAround : MonoBehaviour
{
    public Vector3 RootPos;
    bool bStart = false;
    public float fSpeed = 10.0f;
    public Vector3 MoveDelta;
    public Vector3 MovePow;


    // Start is called before the first frame update
    void Start()
    {
        
    }



    public void ResetPos()
    {
        transform.position = RootPos;
    }
    public void ResetRootPos()
    {
        RootPos = transform.position;
    }

    private void OnEnable()
    {
        if (!bStart)
        {
            
            RootPos = transform.position;
            bStart = true;
        }

        transform.position = RootPos + MovePow;
    }

    // Update is called once per frame
    void Update()
    {
        // 회전축이 0벡터가 아니어야 함
        if (MoveDelta != Vector3.zero)
        {
            float angle = fSpeed * Time.time;
            Vector3 axis = MoveDelta.normalized; // 회전축
            Vector3 offset = MovePow; // 반지름 방향 벡터
            Vector3 pos = RootPos + Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis) * offset;
            transform.position = pos;
        }
    }

}
