using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundVol : MonoBehaviour {

    public SoundManager _Sm;
    public AudioSource _Audio;
    public bool bBg;
	// Use this for initialization
	void Start () {


		
	}

    private void OnEnable()
    {
        if(!_Sm)
        {
            _Sm = SoundManager.Instance;
        }

        if(!_Audio)
        {
            _Audio = GetComponent<AudioSource>();
        }
        if (bBg)
            _Audio.volume = _Sm.MusicVol / 100.0f;
        else
            _Audio.volume = _Sm.SndVol / 100.0f;
        _Audio.Play();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void PlaySnd(SndClass n)
    {
        _Sm.Play_Effect(n);
    }
}
