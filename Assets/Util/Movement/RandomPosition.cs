using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPosition : MonoBehaviour {
    public GameObject Target;
    public Vector3 Pos_Min;
    public Vector3 Pos_Max;
    public float LoopTime=10;
    public float LoopTimeMax = 10;
    public bool bActive=false;

    // Use this for initialization
    void Start () {
        ChangePosition();


    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ChangePosition()
    {
        if(bActive)
        {
            Target.SetActive(true);
        }
        Vector3 reslut = new Vector3(Random.Range(Pos_Min.x,Pos_Max.x), Random.Range(Pos_Min.y, Pos_Max.y), Random.Range(Pos_Min.z, Pos_Max.z));
        Target.transform.localPosition = reslut;
        Invoke("ChangePosition", Random.Range( LoopTime, LoopTimeMax));

    }
}
