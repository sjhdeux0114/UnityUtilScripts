using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayActive : MonoBehaviour
{

    public GameObject[] Obj;
    public float Delay = 3;
    public bool bEnable = false;
    public bool bDisabled = false;

    float unscale_Time = 0;

    // Use this for initialization
    void Start()
    {
        if (!bEnable)
            Invoke("Obj_On", Delay);

    }

    private void OnEnable()
    {
        if (bEnable)
        {
            if (Time.timeScale > 0)
                Invoke("Obj_On", Delay);
            else
                unscale_Time = Delay;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (unscale_Time > 0)
        {
            unscale_Time -= Time.unscaledDeltaTime;
            if (unscale_Time <= 0)
            {
                unscale_Time = 0;
                Obj_On();
            }
        }

    }

    void Obj_On()
    {
        for (int i = 0; i < Obj.Length; i++)
            Obj[i].SetActive(true);
    }

    void OnDisable()
    {
        CancelInvoke();
        if (bDisabled)
        {
            for (int i = 0; i < Obj.Length; i++)
            {
                if (Obj[i] != null)
                    Obj[i].SetActive(false);
            }

        }
    }


}
