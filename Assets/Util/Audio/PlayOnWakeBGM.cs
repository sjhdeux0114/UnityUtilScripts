using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnWakeBGM : MonoBehaviour
{
    public SndClass BGM;

    void OnEnable()
    {
        SoundManager.Instance.Play_BG(BGM);
    }
}
