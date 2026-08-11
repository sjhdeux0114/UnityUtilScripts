using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class OnPressEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject target;
    public string msg;
    public string value;

    public string Lop_msg;
    public string Loop_value;
    bool pressing = false;
    public float StartTime = 0.5f;
    public float DurationTime = 0.1f;
    float buttonTime = 0;
    float startDelayTime = 0;

    public void clear()
    {
        buttonTime = 0;
        startDelayTime = 0;
        pressing = false;
    }
    public void SendAction()
    {
        target.SendMessage(msg, value);
    }
    public void SendLoopAction()
    {
        target.SendMessage(Lop_msg, Loop_value);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        SendAction();
        //Debug.Log("press");
        buttonTime = 0;
        startDelayTime = StartTime;
        pressing = true;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("up");
        buttonTime = 0;
        startDelayTime = 0;
        pressing = false;
    }

    void Update()
    {
        if (pressing)
        {
            startDelayTime -= Time.deltaTime;
            if (startDelayTime <= 0)
            {
                buttonTime -= Time.deltaTime;
                if (buttonTime <= 0)
                {
                    buttonTime = DurationTime;
                    //Debug.Log("send");
                    SendLoopAction();
                }
            }
        }
    }
 
}
