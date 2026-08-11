using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUpdown : MonoBehaviour {

    public AnimationCurve _Curve = new AnimationCurve();
    public Vector3 MoveDelta;
    public float MoveTime;
    float OldTime = 0;
    Vector3 Old_Pos;

    public Vector3 Move_Dir;
    public float MoveSpeed=1;

    // Use this for initialization
    void Start () {

        OldTime = 0;
        Old_Pos = transform.position;

    }
	
	// Update is called once per frame
	void Update () {

        OldTime += Time.deltaTime;
        if (OldTime >= MoveTime)
            OldTime -= MoveTime;

        float p = OldTime/MoveTime;
        float ret_t = _Curve.Evaluate(p);


        Vector3 y_pos = Old_Pos + MoveDelta * ret_t;

        Vector3 move = transform.position + Move_Dir * Time.deltaTime * MoveSpeed;
        move.y = y_pos.y;

        transform.position = move;

    }
}
