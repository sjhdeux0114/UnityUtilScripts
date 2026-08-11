using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour {

    Vector3 StartPos = Vector3.zero;
    public Vector3 MoveDir;
    public float MoveTime = 3;
    float Times;
    public bool bLoop = true;
    public bool bLocal = false;
    public bool Save_Pos = false;
    // Use this for initialization

    private void Awake()
    {
        StartPos = transform.position;
        if (bLocal)
            StartPos = transform.localPosition;
        Times = MoveTime;

    }
    void Start () {
        

    }
	
	// Update is called once per frame
	void Update () {
        if(bLocal)
            transform.localPosition += MoveDir * Time.deltaTime;
        else
            transform.position += MoveDir * Time.deltaTime;
        Times -= Time.deltaTime;
        if(Times <= 0)
        {
            if(bLoop)
            {
                Times = MoveTime;
                transform.position = StartPos;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

    }

    private void OnDisable()
    {

        transform.position = StartPos;
        if (bLocal)
            transform.localPosition = StartPos;
        Times = MoveTime;
    }

    private void OnEnable()
    {
       
    }


}
