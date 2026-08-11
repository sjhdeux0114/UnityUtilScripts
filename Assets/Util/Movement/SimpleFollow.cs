using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FollowType
{
    POSITION,
    TRANSFORM,
    STOP
}
public class SimpleFollow : MonoBehaviour
{
    public FollowType followType = FollowType.POSITION;

    public Vector3 TargetPos;
    public bool bUseFixTargetPos = false;
    public Vector3 FixTargetPos;
    public Transform TargetTransform;

    public float FollowSpeed = 5.0f;

    public Vector3 OffsetPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init()
    {
        Old_Pos = transform.position;
        TargetPos = transform.position;
        if (bUseFixTargetPos)
        {
            FixTargetPos = transform.position;
        }
    }

    private void OnEnable()
    {
        if (bUseFixTargetPos)
        {
            //transform.position = FixTargetPos;
        }
    }
    public void Change_Type(FollowType newType)
    {
        followType = newType;
    }

    public void Set_TargetPos(Vector3 newPos)
    {
        TargetPos = newPos;
    }

    public void Set_TargetAddPos(Vector3 newPos)
    {
        Old_Pos = transform.position;
        TargetPos += newPos;
    }

    public void Set_DefGap()
    {
        if (bUseFixTargetPos)
            transform.position = FixTargetPos - OffsetPos;
        Old_Pos = transform.position;
        TargetPos = transform.position + OffsetPos;
    }
    public void Set_DefGap_Inverse()
    {
        if (bUseFixTargetPos)
            transform.position = FixTargetPos + OffsetPos;
        Old_Pos = transform.position;
        TargetPos = transform.position - OffsetPos;
    }

    public void Set_TargetTransform(Transform newTransform)
    {
        Old_Pos = transform.position;
        TargetTransform = newTransform;
    }

    public void Set_Speed(float newSpeed)
    {
        FollowSpeed = newSpeed;
    }

    Vector3 Old_Pos;

    public void _Reset()
    {
        transform.position = Old_Pos;
        TargetPos = Old_Pos;
    }

    public void _Update(float dt)
    {
        switch (followType)
        {
            case FollowType.POSITION:
                {
                    Vector3 dir = TargetPos - transform.position;
                    transform.position += dir * FollowSpeed * dt;
                }
                break;
            case FollowType.TRANSFORM:
                {
                    if (TargetTransform != null)
                    {
                        Vector3 dir = TargetTransform.position - transform.position;
                        transform.position += dir * FollowSpeed * dt;
                    }
                }
                break;
            case FollowType.STOP:
                {
                    // Do nothing
                }
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        _Update(Time.deltaTime);


    }
}
