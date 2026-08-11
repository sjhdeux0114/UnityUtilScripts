using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FOLLOW_TYPE
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public class FollowPostionStep : MonoBehaviour
{
    public FOLLOW_TYPE fType = FOLLOW_TYPE.UP;
    public Transform TR_Target;
    public Vector3 Org_Pos;
    public Vector3 DEF_Pos;

    public bool bReverse = false;
    public float Smooth = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (fType)
        {
            case FOLLOW_TYPE.UP:
                if (TR_Target.localPosition.y >= DEF_Pos.y)
                {
                    if (Smooth <= 0)
                    {
                        float delta = TR_Target.localPosition.y - DEF_Pos.y;
                        if (bReverse)
                            transform.localPosition = Org_Pos - new Vector3(0, delta, 0);
                        else
                            transform.localPosition = Org_Pos + new Vector3(0, delta, 0);
                    }
                    else
                    {
                        float delta = TR_Target.localPosition.y - DEF_Pos.y;
                        Vector3 WantPos = Vector3.zero;
                        if (bReverse)
                            WantPos = Org_Pos - new Vector3(0, delta, 0);
                        else
                            WantPos = Org_Pos + new Vector3(0, delta, 0);

                        transform.localPosition = Vector3.Lerp(transform.localPosition, WantPos, Time.deltaTime * Smooth);
                    }
                }
                else
                {
                    transform.localPosition = Org_Pos;
                }
                break;
        }

    }
}
