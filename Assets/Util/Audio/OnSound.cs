using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSound : MonoBehaviour
{

    public SndClass SndNum;
    public SndClass[] SndNums;
    public bool loop = false;
    public bool bHideMute = false;
    public bool bFirstMute = false;
    public float DelayTimes = 0;
    int Cnt = 0;

    // Use this for initialization
    void Start()
    {

    }

    private void OnDisable()
    {
        if (bHideMute)
        {
            SoundManager.Instance.Play_Effect_Stop_N(SndNum);
        }

    }

    public void PlaySounds(int n)
    {
        if (n < 0 || n >= SndNums.Length)
            return;
        SoundEventBus.PlayEffect(SndNums[n]);
    }

    IEnumerator PlayOn()
    {
        yield return new WaitForSeconds(DelayTimes);
        if (bFirstMute && Cnt == 0)
        {

        }
        else
        {
            if (SndNum.Clip != null)
                SoundManager.Instance.Play_Effect(SndNum, loop);
        }
        Cnt++;
    }

    private void OnEnable()
    {
        StartCoroutine(PlayOn());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
