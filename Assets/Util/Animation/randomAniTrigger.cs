using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomAniTrigger : MonoBehaviour
{

    public string TriggerName;
    public int per;

    // Use this for initialization
    void Start()
    {



    }

    // Update is called once per frame
    void Update()
    {




    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void OnEnable()
    {
        CancelInvoke();
        Invoke("Check_Random_Ani", 0.3f);
    }

    void Check_Random_Ani()
    {

        if (Random.Range(0, 100) < per)
        {
            GetComponent<Animator>().SetTrigger(TriggerName);
            Invoke("Check_Random_Ani", 3.5f);
        }
        else
        {
            Invoke("Check_Random_Ani", 0.3f);
        }
    }
}
