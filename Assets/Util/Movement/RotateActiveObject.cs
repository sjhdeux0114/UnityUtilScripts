using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RotateActiveObject : MonoBehaviour {
    public GameObject[] Obj;
    public bool bOn;
    public float[] ChangeTime;
    float times = 0;
    int cnt = 0;
    public SndClass Snd_Num;
    public bool bLoop;
    bool bOld;
    public bool bTest = false;
    public UnityEvent EndEvent;

	// Use this for initialization
	void Start () {
        


    }
    public void SetAction()
    {
        Invoke("ActOn", 1);
    }

    void ActOn()
    {
        
        Debug.Log("Tiger On");
        SoundManager.Instance.Play_Effect(Snd_Num);
        bOn = true;

    }
    // Update is called once per frame
    void Update () {
        if(bTest)
        {
            SoundManager.Instance.Play_Effect_Stop();
            Invoke("ActOn", 1);
            bTest = false;
            //ActOn();
        }

        if(bOn)
        {

            for (int i = 0; i < Obj.Length; i++)
            {
                if (i != cnt && Obj[i] != Obj[cnt])
                {
                    Obj[i].SetActive(false);
                }
            }

            Obj[cnt].SetActive(true);


            times +=Time.deltaTime;
            if(times >= ChangeTime[cnt])
            {
                times -= ChangeTime[cnt];
                cnt++;
                if (cnt >= Obj.Length)
                {
                    
                    if(bLoop)
                    {
                        cnt = 0;
                    }
                    else
                    {
                        
                        bOn = false;
                        EndEvent.Invoke();
                    }
                }

            }


        }
        else
        {
            for(int i=0;i<Obj.Length;i++)
            {
                Obj[i].SetActive(false);
            }
            times = 0;
            cnt = 0;
        }

        bOld = bOn;

    }
}
