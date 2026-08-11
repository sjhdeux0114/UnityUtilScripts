using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SendTarget
{
    public GameObject Target;
    public string Msg;
}

public class SendMsg : MonoBehaviour
{
    public SendTarget[] Msg;
    public UnityEvent[] Events;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ActiveTarget(int i)
    {
        if (i < 0 || i >= Msg.Length)
            return;

        Msg[i].Target.SetActive(true);
    }
    public void DeActiveTarget(int i)
    {
        if (i < 0 || i >= Msg.Length)
            return;
        Msg[i].Target.SetActive(false);
    }

    public void Send_Msg(int i)
    {
        if (i < 0 || i >= Msg.Length)
            return;

        Msg[i].Target.SendMessage(Msg[i].Msg,SendMessageOptions.DontRequireReceiver);
    }
    public void Send_Event(int i)
    {
        if (i < 0 || i >= Events.Length)
            return;

        Events[i].Invoke();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
