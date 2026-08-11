using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    public Vector3[] PosList;
    public int PosIndex;

    public bool bFollow = false;
    public float fSpeed = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (bFollow)
        {
            if (PosIndex < 0 || PosIndex >= PosList.Length)
                return;

            transform.localPosition = Vector3.Lerp(transform.localPosition, PosList[PosIndex], Time.deltaTime * fSpeed);
        }
    }
}
