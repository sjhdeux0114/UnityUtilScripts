using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTiming : MonoBehaviour {

    public GameObject[] Objs;
    public bool[] Snd_On;
    public bool bStart;
    public float[] OnTimes;
    public float LastWaitTimes;

    public SndClass _Bomb;
    public bool bAutoHide = false;
    public bool FrontDelay = false;
    // Use this for initialization
    void OnEnable () {
		if(bStart)
        {
            _Play();
        }
	}

    public void _Play()
    {
        StartCoroutine(_PlayOn());

    }

    IEnumerator _PlayOn()
    {
        for (int i = 0; i < Objs.Length; i++)
        {
            Objs[i].SetActive(false);
        }
        yield return new WaitForFixedUpdate();

        

        for (int i=0;i< Objs.Length;i++)
        {
            if(FrontDelay)
                yield return new WaitForSeconds(OnTimes[i]);

            if (Snd_On[i])
                SoundManager.Instance.Play_Effect(_Bomb);
            Objs[i].SetActive(false);
            yield return new WaitForEndOfFrame();
            Objs[i].SetActive(true);
            if (!FrontDelay)
                yield return new WaitForSeconds(OnTimes[i]);
        }

        yield return new WaitForSeconds(LastWaitTimes);

        for (int i = 0; i < Objs.Length; i++)
        {
            Objs[i].SetActive(false);
        }
        if (bAutoHide)
            gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update () {

	}
}
