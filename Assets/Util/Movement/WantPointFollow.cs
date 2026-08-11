using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WantPointFollow : MonoBehaviour
{
    public Vector3[] Want_List;
    public bool bWorld = false;
    public Vector3 TargetPos;
    public float Speed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetTargetPosition(Vector3 targetPos,float sp=-1)
    {
        TargetPos = targetPos;
        if(sp > 0) Speed = sp;
    }
    public void SetTargetIndex(int n , float sp=-1)
    {
        if(n < 0 || n >= Want_List.Length) return;
        TargetPos = Want_List[n];
        if(sp > 0) Speed = sp;
    }
    public void SetTargetIndexDirect(int n)
    {
        if (n < 0 || n >= Want_List.Length) return;
        TargetPos = Want_List[n];

        if (bWorld)
            transform.position =  TargetPos;
        else
            transform.localPosition = TargetPos;
    }

    // Update is called once per frame
    void Update()
    {
        if(bWorld)
            transform.position = Vector3.MoveTowards(transform.position, TargetPos, Speed * Time.deltaTime);
        else
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, TargetPos, Speed * Time.deltaTime);
    }
}
