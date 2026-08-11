using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFoward : MonoBehaviour {

    public Vector3 Direction;
    public Vector3 Direction_R;
    public float Speed;
    public float Sin_Graph;
    public float Rotate;
    public float Add_Side=2;
    Vector3 DownDir;

    float Sin = 0;
    float SinNext = 0;
    public float Dir = 1;
    public float delay=0;
    public float Down_Pow=0;

    public float Org_delay;
    public Vector3 Org_Pos;
    public Vector3 Org_Rot;

    private void Awake()
    {
        Org_delay = delay;
        Org_Pos = transform.position;
        Org_Rot = transform.eulerAngles;
    }

    private void OnEnable()
    {
        transform.position = Org_Pos;
        transform.eulerAngles = Org_Rot;
        delay = Org_delay;

        Direction = transform.forward;
        Direction_R = transform.right;
        DownDir = transform.up;

        Sin = Mathf.PI;
        SinNext = Mathf.PI + 0.02f;
        
    }

    // Use this for initialization
    void Start () {

        Direction = transform.forward;
        Direction_R = transform.right;
        DownDir = transform.up;

        Sin = Mathf.PI;
        SinNext = Mathf.PI + 0.02f;

    }
	
	// Update is called once per frame
	void Update () {

        if (delay > 0)
        {
            delay -= Time.deltaTime;
            return;
        }
        

        Sin += Time.deltaTime * Sin_Graph;
        SinNext += Time.deltaTime * Sin_Graph;


        

        transform.position += Direction*Speed*Time.deltaTime + Mathf.Sin(Sin)* Direction_R*Dir* Add_Side+ DownDir*Time.deltaTime* Down_Pow;
        Vector3 NextPos = transform.position + Direction * Speed * Time.deltaTime + Mathf.Sin(SinNext) * Direction_R * Dir * Add_Side + DownDir * Time.deltaTime * Down_Pow;


        Quaternion old = transform.rotation;
        transform.LookAt(NextPos);
        transform.rotation = Quaternion.Slerp(old, transform.rotation, Rotate * Time.deltaTime);

        /*
        if (Sin >= Mathf.PI)
        {
            Sin -= Mathf.PI;
        }

        float t = SingCurve.Evaluate(Sin);
        */

        //transform.RotateAround(transform.position, Vector3.up, Mathf.Sin(Sin)* Rotate* Dir);

    }
}
