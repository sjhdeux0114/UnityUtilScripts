using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlaySound : MonoBehaviour
{

    public SndClass SndName;
    public SndClass[] SndName_List;
    SoundManager sm;
    public float Loop_Times = 0;
    public bool AwakePlay = true;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        sm = GameObject.FindAnyObjectByType<SoundManager>();

        if (AwakePlay)
        {
            PlayFx();
        }
    }

    public void PlayFx()
    {
        if (Loop_Times > 0)
        {
            sm.Play_Effect_LoopTime(SndName, Loop_Times);
        }
        else
        {
            if (SndName_List.Length > 0)
            {
                int rnd = Random.Range(0, SndName_List.Length);
                sm.Play_Effect(SndName_List[rnd]);
            }
            else
                sm.Play_Effect(SndName);
        }
    }
}
