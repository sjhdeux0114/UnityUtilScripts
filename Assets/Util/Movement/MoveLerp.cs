using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class MoveLerp : MonoBehaviour
{
    public Vector3 targetPosition;
    public Vector3[] targetPostionList;
    public float speed = 1.0f;
    public bool isMoving = false;
    public UnityAction<int> Act_End;
    public UnityEvent Event_End;
    int Index;
    public int _Index => Index;

    [SerializeField]
    bool isShake = false;
    public bool IsShake
    {
        get { return isShake; }
        set
        {
            isShake = value;
            if (isShake)
            {

            }
        }
    }
    public Vector3 shakeRange;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetMove(bool val)
    {
        isMoving = val;
    }

    public void Set_Speed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void Set_TargetPosition(Vector3 newTargetPosition)
    {
        targetPosition = newTargetPosition;
    }
    [InspectorButton]
    void Set_Target1()
    {
        Set_TargetPositionNum(0);
    }
    [InspectorButton]
    void Set_Target2()
    {
        Set_TargetPositionNum(1);
    }
    [InspectorButton]
    void Set_Target3()
    {
        Set_TargetPositionNum(2);
    }
    [InspectorButton]
    void Set_Target4()
    {
        Set_TargetPositionNum(3);
    }
    [InspectorButton]
    void Set_Target5()
    {
        Set_TargetPositionNum(4);
    }
    [InspectorButton]
    void Set_Target6()
    {
        Set_TargetPositionNum(5);
    }
    [InspectorButton]
    void Set_Target7()
    {
        Set_TargetPositionNum(6);
    }
    [InspectorButton]
    void Set_Target8()
    {
        Set_TargetPositionNum(7);
    }
    public void Set_TargetPositionNum(int n)
    {
        if (n < 0 || n >= targetPostionList.Length)
        {
            Debug.LogError("Invalid target position index: " + n);
            return;
        }
        Index = n;
        targetPosition = targetPostionList[n];
        isMoving = true;
    }
    public void Set_TargetPositionDirect(Vector3 newTargetPosition)
    {

        transform.localPosition = newTargetPosition;
    }
    public void Set_TargetPositionNumDirect(int n)
    {
        if (n < 0 || n >= targetPostionList.Length)
        {
            Debug.LogError("Invalid target position index: " + n);
            return;
        }
        Index = n;
        isMoving = false;
        targetPosition = targetPostionList[n];
        transform.localPosition = targetPostionList[n];
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            // Move towards the target position
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, speed * Time.deltaTime);
            if (transform.localPosition == targetPosition)
            {
                isMoving = false;
                Act_End?.Invoke(Index);
                if (Index == targetPostionList.Length - 1)
                {
                    Event_End?.Invoke();
                }
            }

        }
        else if (IsShake)
        {
            transform.localPosition = targetPosition +
            new Vector3(Random.Range(-shakeRange.x, shakeRange.x), Random.Range(-shakeRange.y, shakeRange.y), Random.Range(-shakeRange.z, shakeRange.z));
        }


    }
}
