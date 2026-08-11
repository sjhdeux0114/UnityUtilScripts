using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;


public enum SOUND_TYPE
{
    _SND_0_FX = 0,
    _SND_1_BG,
    _SND_2_DOOR_BG,
    _SND_3_DOOR,
    _SND_4_BLACKJACK,
    _SND_5_X_GAME,
    _SND_6_JACKPOT,
    _SND_7_20000_OVER,
    _SND_8_50000_OVER,
    _SND_9_100000_OVER,
}

public enum SOUND_TYPE_KOR
{
    효과음 = 0,
    배경음,
    전투이벤트,
    성문이벤트,
    낚시이벤트,
    배당이벤트,
    잭팟이벤트,
    당첨음악1,
    당첨음악2,
    당첨음악3,
}


[System.Serializable]
public class SndClass : ISerializationCallbackReceiver
{
    public AudioClip Clip;

    [Range(0, 100)]
    public float vol = 100;
    [Range(0, 10)]
    public int type = 0;
    [HideInInspector]
    public int code;
    public int Channel;
    public SOUND_TYPE S_TYPE;
    // 코드에서 new로 만들 때도 동일한 기본값 보장
    public SndClass()
    {
        code = 0;
        vol = 100f;
        S_TYPE = SOUND_TYPE._SND_0_FX;
        _initialized = true;
    }

    public SndClass(AudioClip _Clip) : this()
    {
        Clip = _Clip;
    }

    // 내부 초기화 여부 (직렬화됨: 한 번 기본값 넣고 나면 유지)
    [SerializeField, HideInInspector]
    private bool _initialized;

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        if (!_initialized)
        {
            // 최초 한 번만 기본값 설정
            if (vol == 0f) vol = 100f;
            if (S_TYPE == 0) S_TYPE = SOUND_TYPE._SND_0_FX;

            _initialized = true;
        }
    }


}

[System.Serializable]
public class AudioGroup
{
    public AudioSource Audio_Effect;
    public int Type;
    [HideInInspector]
    public int code;
    public bool bPause;

    public void Stop()
    {
        Audio_Effect.Stop();
    }
    public void Play()
    {
        Audio_Effect.Play();
    }
    public bool isPlaying()
    {
        return Audio_Effect.isPlaying;
    }

    public void Set_Type(int n, int _c)
    {
        Type = n;
        code = _c;
    }
    public int Get_Type()
    {
        return Type;
    }
}

public class SoundManager : MonoBehaviour
{

    public static SoundManager _instance;
    public AudioMixerGroup Group = null;

    public static bool bInitOK = false;
    AudioSource Audio_BG = null;

    AudioGroup[] Audio_Effect;
    public int Effect_Channel = 1;

    public float MusicVol = 100;
    public float SndVol = 100;

    [SerializeField]
    private int MaxChannel = 48;

    void Awake()
    {
        MusicVol = 100;
        SndVol = 100;
        Init();
    }

    private void OnEnable()
    {
        SoundEventBus.OnPlayBG += PlayBG;
        SoundEventBus.OnPlayBGLastList += PlayBGLastList;
        SoundEventBus.OnPlayBGLock += PlayBGLock;
        SoundEventBus.OnPlayBGResume += PlayBGResume;
        SoundEventBus.OnPlayBGNext += PlayBGNext;
        SoundEventBus.OnStopBG += StopBG;
        SoundEventBus.OnStopBGFade += StopBGFade;
        SoundEventBus.OnChangeBGVol += ChangeBGVol;
        SoundEventBus.OnChangeBGVolSecond += ChangeBGVolSecond;
        SoundEventBus.OnChangeBGVolFade += ChangeBGVolFade;
        SoundEventBus.OnResumeBGVol += ResumeBGVol;

        SoundEventBus.OnPlayEffectClip += PlayEffect;
        SoundEventBus.OnPlayEffectSnd += PlayEffect;
        SoundEventBus.OnPlayEffectVol += PlayEffect;
        SoundEventBus.OnPlayEffectSpeed += PlayEffectSpeed;
        SoundEventBus.OnStopFX += StopFX;
        SoundEventBus.OnStopFX_N += StopFX_N;

        SoundEventBus.OnSet_Volume_List += Set_Volume_List;
        SoundEventBus.OnVolumeSilence += VolumeSilence;
        SoundEventBus.OnVolumeVerySmall += VolumeVerySmall;
        SoundEventBus.OnVolumeSmall += VolumeSmall;
        SoundEventBus.OnVolumeOn += VolumeOn;
        SoundEventBus.OnPauseSound += PauseSound;
        SoundEventBus.OnResumeSound += ResumeSound;
        SoundEventBus.OnStopEffectLoop += Play_Effect_StopLoop;
    }

    private void OnDisable()
    {
        SoundEventBus.OnPlayBG -= PlayBG;
        SoundEventBus.OnPlayBGLastList -= PlayBGLastList;
        SoundEventBus.OnPlayBGLock -= PlayBGLock;
        SoundEventBus.OnPlayBGResume -= PlayBGResume;
        SoundEventBus.OnPlayBGNext -= PlayBGNext;
        SoundEventBus.OnStopBG -= StopBG;
        SoundEventBus.OnStopBGFade -= StopBGFade;
        SoundEventBus.OnChangeBGVol -= ChangeBGVol;
        SoundEventBus.OnChangeBGVolSecond -= ChangeBGVolSecond;
        SoundEventBus.OnChangeBGVolFade -= ChangeBGVolFade;
        SoundEventBus.OnResumeBGVol -= ResumeBGVol;

        SoundEventBus.OnPlayEffectClip -= PlayEffect;
        SoundEventBus.OnPlayEffectSnd -= PlayEffect;
        SoundEventBus.OnPlayEffectVol -= PlayEffect;
        SoundEventBus.OnPlayEffectSpeed -= PlayEffectSpeed;
        SoundEventBus.OnStopFX -= StopFX;
        SoundEventBus.OnStopFX_N -= StopFX_N;

        SoundEventBus.OnSet_Volume_List -= Set_Volume_List;
        SoundEventBus.OnVolumeSilence -= VolumeSilence;
        SoundEventBus.OnVolumeVerySmall -= VolumeVerySmall;
        SoundEventBus.OnVolumeSmall -= VolumeSmall;
        SoundEventBus.OnVolumeOn -= VolumeOn;
        SoundEventBus.OnPauseSound -= PauseSound;
        SoundEventBus.OnResumeSound -= ResumeSound;
        SoundEventBus.OnStopEffectLoop -= Play_Effect_StopLoop;
    }

    public void Sound_OnOff(float val)
    {
        MusicVol = val;
        SndVol = val;
    }

    public void SetSound_Vol(float db)
    {
        Group.audioMixer.SetFloat("Master", db);
    }


    public void Init()
    {


        Audio_BG = (AudioSource)gameObject.AddComponent<AudioSource>();
        if (Group)
        {
            Audio_BG.outputAudioMixerGroup = Group;

        }
        Audio_Effect = new AudioGroup[MaxChannel];
        GameObject g = new GameObject();
        g.name = "SoundClips";
        g.transform.parent = this.transform;

        for (int i = 0; i < Audio_Effect.Length; i++)
        {
            Audio_Effect[i] = new AudioGroup();
            Audio_Effect[i].Audio_Effect = (AudioSource)g.AddComponent<AudioSource>();
            if (Group)
                Audio_Effect[i].Audio_Effect.outputAudioMixerGroup = Group;
        }
        bInitOK = true;


        Effect_Channel = 1;

    }

    void SoundOn()
    {
        SndVol = 100;
        MusicVol = 100;

    }



    public void Play_Effect_Stop(int start_num = 0, int Soundtype = -1)
    {
        for (int i = start_num; i < Audio_Effect.Length; i++)
        {
            if (Soundtype == -1 || Soundtype == Audio_Effect[i].Type)
                Audio_Effect[i].Stop();
        }

    }
    public void Play_Effect_StopLoop()
    {
        Audio_Effect[Audio_Effect.Length - 1].Stop();

    }
    public void Play_Effect_Stop_Fade(int start_num = 0, int Soundtype = -1)
    {

        StartCoroutine(StopAll(Soundtype));


    }
    public void All_Sound_Hide()
    {
        Audio_BG.Pause();

        for (int i = 0; i < Audio_Effect.Length; i++)
        {
            if (Audio_Effect[i].isPlaying())
            {
                Audio_Effect[i].Audio_Effect.Pause();
            }
            else
                Audio_Effect[i].code = 0;
        }

    }

    public void Sound_Resume()
    {
        Audio_BG.Play();

        for (int i = 0; i < Audio_Effect.Length; i++)
        {
            if (Audio_Effect[i].code > 0)
            {
                Audio_Effect[i].Audio_Effect.Play();
            }
        }


    }

    bool StopIng = false;
    IEnumerator StopAll(int Soundtype = -1)
    {
        StopIng = true;
        for (int j = 0; j < 10; j++)
        {
            float p = (6 - (j + 1)) * 0.1f;
            for (int i = 0; i < Audio_Effect.Length; i++)
            {
                if (Audio_Effect[i].isPlaying())
                {
                    if (Soundtype == -1 || Soundtype == Audio_Effect[i].Get_Type())
                        Audio_Effect[i].Audio_Effect.volume = Audio_Effect[i].Audio_Effect.volume * p;
                }
            }

            yield return new WaitForSeconds(0.06f);
        }

        for (int i = 0; i < Audio_Effect.Length; i++)
        {
            if (Audio_Effect[i].isPlaying())
            {
                if (Soundtype == -1 || Soundtype == Audio_Effect[i].Get_Type())
                    Audio_Effect[i].Stop();
            }
        }
        StopIng = false;

    }

    public void Play_Effect_Stop_N(SndClass snd)
    {
        for (int i = 0; i < Audio_Effect.Length; i++)
        {
            if (Audio_Effect[i].code == snd.code)
            {
                Audio_Effect[i].Stop();
            }
        }

    }

    public void BG_FadeOut(float _t = 2)
    {
        StartCoroutine(PlayBG_FadeOut(_t));
    }

    bool fadeBG = false;

    IEnumerator PlayBG_FadeOut(float _time)
    {
        if (fadeBG) yield break;
        fadeBG = true;
        float t = _time / 10;
        float vol_t = Audio_BG.volume / 10;

        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(t);
            Audio_BG.volume -= vol_t;
        }
        Stop_BG();
        fadeBG = false;


    }
    public void Stop_BG(float _t = 0)
    {
        bStopbg = true;
        if (_t > 0)
        {
            BG_FadeOut(_t);
            return;
        }
        Audio_BG.Stop();

    }
    public float Get_Bg_Vol()
    {
        return Audio_BG.volume;
    }
    public void BG_Vol_Resume()
    {
        if (Old_Bg_Vol > 0)
            Set_BG_Vol(Old_Bg_Vol);
    }
    float Old_Bg_Vol = 1.0f;
    public void Set_BG_Vol(float vol = -1)
    {
        if (vol < 0)
            vol = MusicVol;

        if (vol > 1)
            vol /= 100;

        if (Audio_BG.volume > 0)
            Old_Bg_Vol = Audio_BG.volume;
        float mainbg = MusicVol;
        mainbg /= 100.0f;
        Audio_BG.volume = vol * mainbg;


    }
    public void Play_BG(SndClass snd, bool bLoop = true)
    {
        float vol = snd.vol;
        if (vol < 0)
            vol = snd.vol;

        if (vol > 1)
            vol /= 100;


        float mainbg = MusicVol;
        mainbg /= 100.0f;
        Audio_BG.clip = snd.Clip;

        Audio_BG.volume = vol * mainbg;


        Audio_BG.loop = bLoop;


        Audio_BG.Stop();

        Audio_BG.Play();
        //        Debug.Log("SOund BG");

    }
    bool bBG_Pause = false;
    public void BG_Pause()
    {
        if (Audio_BG.isPlaying)
        {
            bBG_Pause = true;
            Audio_BG.Pause();
        }
    }
    public void BG_Resume()
    {
        if (bBG_Pause)
            Audio_BG.Play();
    }

    public void AllPause()
    {
        for (int i = 0; i < Audio_Effect.Length; i++)
        {
            if (Audio_Effect[i].isPlaying())
            {
                Audio_Effect[i].Audio_Effect.Pause();
                Audio_Effect[i].bPause = true;
            }


        }
    }
    public void AllResume()
    {
        for (int i = 0; i < Audio_Effect.Length; i++)
        {
            if (Audio_Effect[i].bPause)
                Audio_Effect[i].Audio_Effect.Play();
            Audio_Effect[i].bPause = false;

        }
    }

    float Target_BG_Vol;
    bool bChange_BG_Vol = false;

    IEnumerator BG_Vol_Fade_Proc(float vol, float _t)
    {
        if (bChange_BG_Vol)
        {
            Target_BG_Vol = vol;
            yield break;
        }
        Target_BG_Vol = vol;

        bChange_BG_Vol = true;
        float mainbg = MusicVol;
        mainbg /= 100.0f;

        float times = 0;

        float stVol = Audio_BG.volume;

        while (times < _t)
        {
            yield return new WaitForEndOfFrame();
            times += Time.deltaTime;
            float per = times / _t;
            float EndVol = Target_BG_Vol * mainbg;

            Audio_BG.volume = Mathf.Lerp(stVol, EndVol, per);
        }

        Audio_BG.volume = Target_BG_Vol * mainbg;


        bChange_BG_Vol = false;

    }



    public void Change_BGVol_Fade(float vol, float _t)
    {
        if (vol > 1)
            vol /= 100.0f;

        StartCoroutine(BG_Vol_Fade_Proc(vol, _t));
    }

    public void Change_BGVol_Second(float vol, float _t)
    {
        StartCoroutine(ChanegeVol_Second(vol, _t));
    }

    IEnumerator ChanegeVol_Second(float vol, float _t)
    {
        float saveVol = Audio_BG.volume;

        Change_BGVol(vol);

        yield return new WaitForSeconds(_t);


        Change_BGVol(saveVol);
    }

    float SaveBG_Vol = 1.0f;

    public void ResumeBG_Vol()
    {
        Audio_BG.volume = SaveBG_Vol;
    }

    public void Change_BGVol(float vol)
    {
        if (vol > 1)
            vol /= 100;


        float mainbg = MusicVol;
        mainbg /= 100.0f;

        SaveBG_Vol = Audio_BG.volume;
        Audio_BG.volume = vol * mainbg;


    }
    bool bStopbg = false;
    IEnumerator BG_Next(SndClass snd, float vol, bool bLoop = true)
    {
        Debug.Log("BG Wait");
        Audio_BG.loop = false;
        bStopbg = false;
        while (Audio_BG.isPlaying)
        {
            if (bStopbg)
            {
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }

        Audio_BG.Stop();
        yield return new WaitForEndOfFrame();
        Debug.Log("BG Next Play");
        Play_BGVol(snd, vol, bLoop);

    }
    public void Play_BGNext(SndClass snd, float vol, bool bLoop = true)
    {
        StartCoroutine(BG_Next(snd, vol, bLoop));
    }
    IEnumerator BG_Vol_Times(float vol, float _t)
    {
        float oldVol = Audio_BG.volume;
        Audio_BG.volume = vol;
        yield return new WaitForSeconds(_t);
        Audio_BG.volume = oldVol;
    }

    public void Play_BGVol_Time(float vol, float _t)
    {
        StartCoroutine(BG_Vol_Times(vol, _t));
    }

    public void Play_BGVol(SndClass snd, float vol, bool bLoop = true)
    {
        if (vol < 0)
            vol = snd.vol;

        if (vol > 1)
            vol /= 100;

        float mainbg = MusicVol;
        mainbg /= 100.0f;


        if (Audio_BG.isPlaying && snd.Clip == Audio_BG.clip)
        {
            Audio_BG.volume = vol * mainbg;
            return;
        }


        Audio_BG.clip = snd.Clip;

        Audio_BG.volume = vol * mainbg;
        SaveBG_Vol = Audio_BG.volume;

        Audio_BG.loop = bLoop;


        Audio_BG.Stop();

        Audio_BG.Play();

    }


    public void Play_BG_NoLoop(SndClass snd, float vol = -1)
    {
        if (!snd.Clip)
            return;
        if (vol < 0)
            vol = MusicVol;

        if (vol > 1)
            vol /= 100;

        vol = vol * snd.vol * 0.01f;
        Audio_BG.Stop();


        float mainbg = MusicVol;
        mainbg /= 100.0f;
        Audio_BG.clip = snd.Clip;
        Audio_BG.volume = vol * mainbg;
        Audio_BG.loop = false;
        Audio_BG.Play();

    }
    public bool isPlayingBG(SndClass snd = null)
    {
        if (snd == null)
            return Audio_BG.isPlaying;
        else
        {
            if (Audio_BG.isPlaying && Audio_BG.clip == snd.Clip)
                return true;
            else
                return false;


        }
    }
    public bool isPlaying(SndClass snd)
    {
        for (int i = 0; i < Audio_Effect.Length; i++)
        {
            if (Audio_Effect[i].code == snd.code)
                return true;
        }

        return false;
    }

    public void Play_Effect_LoopTime(SndClass snd, float _time)
    {
        StartCoroutine(PlayEffect_EndTime(snd, _time));
    }

    IEnumerator PlayEffect_EndTime(SndClass snd, float _time)
    {
        int save_channel = Effect_Channel;
        Play_Effect(snd, true);
        yield return new WaitForSeconds(_time);
        Play_Effect_Stop_N(snd);

        //        Debug.Log("snd stop-1");
    }

    public int Play_Effect(SndClass snd, bool loop = false, bool bSpecial = false)
    {
        if (!bInitOK) return 1;
        if (!snd.Clip)
            return 1;

        if (StopIng)
            return 1;
        float vol = snd.vol;
        if (vol < 0)
            vol = SndVol;
        if (vol > 1)
            vol /= 100.0f;

        vol = vol * (snd.vol * 0.01f);


        int p = Effect_Channel;
        if (bSpecial)
            p = 0;

        if (loop)
            p = Audio_Effect.Length - 1;
        snd.Channel = p;
        Effect_Channel++;
        if (Effect_Channel >= Audio_Effect.Length - 1) Effect_Channel = 1;

        float mainfx = SndVol;
        mainfx /= 100.0f;

        //        Debug.Log("vol:" + vol * mainfx);

        Audio_Effect[p].Stop();
        Audio_Effect[p].Audio_Effect.clip = snd.Clip;
        Audio_Effect[p].Audio_Effect.pitch = 1.0f;
        Audio_Effect[p].Audio_Effect.volume = vol * mainfx;
        Audio_Effect[p].Audio_Effect.loop = loop;
        Audio_Effect[p].Play();

        Audio_Effect[p].Set_Type(snd.type, snd.code);

        return p;

        //        Debug.Log(((SND_LIST)num).ToString() + " : " + Audio_Effect[p].volume);

    }

    public void Stop_Effect(int n)
    {
        if (n < 0 || n >= Audio_Effect.Length)
            return;

        if (Audio_Effect[n].isPlaying())
        {
            Audio_Effect[n].Stop();
        }
    }
    public void Play_EffectVolSpeed(SndClass snd, float vol, float speed, bool loop = false, bool bSpeical = false)
    {
        if (!snd.Clip)
            return;

        if (StopIng)
            return;
        if (vol < 0)
            vol = SndVol;
        if (vol > 1)
            vol /= 100.0f;

        vol = vol * (snd.vol * 0.01f);


        int p = Effect_Channel;
        if (bSpeical)
            p = 0;
        if (loop)
            p = Audio_Effect.Length - 1;
        snd.Channel = p;
        Effect_Channel++;
        if (Effect_Channel >= Audio_Effect.Length - 1) Effect_Channel = 1;

        float mainfx = SndVol;
        mainfx /= 100.0f;

        //        Debug.Log("vol:" + vol * mainfx);

        Audio_Effect[p].Stop();
        Audio_Effect[p].Audio_Effect.clip = snd.Clip;
        Audio_Effect[p].Audio_Effect.pitch = speed;
        Audio_Effect[p].Audio_Effect.volume = vol * mainfx;
        Audio_Effect[p].Audio_Effect.loop = loop;
        Audio_Effect[p].Play();

        Audio_Effect[p].Set_Type(snd.type, snd.code);
    }
    public void Play_EffectVol(SndClass snd, float vol, bool loop = false, bool bSpeical = false)
    {
        if (!snd.Clip)
            return;

        if (StopIng)
            return;
        if (vol < 0)
            vol = SndVol;
        if (vol > 1)
            vol /= 100.0f;

        vol = vol * (snd.vol * 0.01f);


        int p = Effect_Channel;
        if (bSpeical)
            p = 0;
        if (loop)
            p = Audio_Effect.Length - 1;
        snd.Channel = p;
        Effect_Channel++;
        if (Effect_Channel >= Audio_Effect.Length - 1) Effect_Channel = 1;

        float mainfx = SndVol;
        mainfx /= 100.0f;

        //        Debug.Log("vol:" + vol * mainfx);

        Audio_Effect[p].Stop();
        Audio_Effect[p].Audio_Effect.clip = snd.Clip;
        Audio_Effect[p].Audio_Effect.pitch = 1.0f;
        Audio_Effect[p].Audio_Effect.volume = vol * mainfx;
        Audio_Effect[p].Audio_Effect.loop = loop;
        Audio_Effect[p].Play();

        Audio_Effect[p].Set_Type(snd.type, snd.code);

        //        Debug.Log(((SND_LIST)num).ToString() + " : " + Audio_Effect[p].volume);

    }
    public void Play_Effect(AudioClip snd, float vol = 1.0f, bool loop = false, bool bSpecial = false)
    {
        if (StopIng)
            return;

        if (vol < 0)
            vol = SndVol;
        if (vol > 1)
            vol /= 100.0f;

        int p = Effect_Channel;
        if (bSpecial)
            p = 0;
        if (loop)
            p = Audio_Effect.Length - 1;
        Effect_Channel++;
        if (Effect_Channel >= Audio_Effect.Length - 1) Effect_Channel = 1;

        float mainfx = SndVol;
        mainfx /= 100.0f;

        //        Debug.Log("vol:" + vol * mainfx);

        Audio_Effect[p].Stop();
        Audio_Effect[p].Audio_Effect.clip = snd;
        Audio_Effect[p].Audio_Effect.pitch = 1.0f;
        Audio_Effect[p].Audio_Effect.volume = mainfx * vol;
        Audio_Effect[p].Audio_Effect.loop = loop;
        Audio_Effect[p].Play();

        Audio_Effect[p].Set_Type(0, 0);

        //        Debug.Log(((SND_LIST)num).ToString() + " : " + Audio_Effect[p].volume);

    }
    public void Play_EffectVol(AudioClip snd, float vol, bool loop = false)
    {
        if (StopIng)
            return;
        if (vol < 0)
            vol = SndVol;
        if (vol > 1)
            vol /= 100.0f;
        int p = Effect_Channel;
        if (loop)
            p = Audio_Effect.Length - 1;
        Effect_Channel++;
        if (Effect_Channel >= Audio_Effect.Length - 1) Effect_Channel = 1;

        float mainfx = SndVol;
        mainfx /= 100.0f;

        //        Debug.Log("vol:" + vol * mainfx);

        Audio_Effect[p].Stop();
        Audio_Effect[p].Audio_Effect.clip = snd;
        Audio_Effect[p].Audio_Effect.pitch = 1.0f;
        Audio_Effect[p].Audio_Effect.volume = mainfx * vol;
        Audio_Effect[p].Audio_Effect.loop = loop;
        Audio_Effect[p].Play();

        Audio_Effect[p].Set_Type(0, 0);

        //        Debug.Log(((SND_LIST)num).ToString() + " : " + Audio_Effect[p].volume);

    }
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<SoundManager>();
                if (_instance == null)
                {
                    Debug.Log("There needs to be one active SoundManager script on a GameObject in your scene.");
                }
            }
            return _instance;
        }
    }

    //static 함수들

    static int[] Volume_List = new int[10] { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 };
    public void Set_Volume_List(int[] list)
    {
        if (!bInitOK) return;
        Volume_List = list;
    }
    static public float Get_Volume_Listf(int index)
    {
        if (!bInitOK) return 0;
        if (index < 0 || index >= Volume_List.Length)
            return 0;
        return (float)Volume_List[index] / 100.0f;
    }
    static public int Get_Volume_List(int index)
    {
        if (!bInitOK) return 0;

        if (index < 0 || index >= Volume_List.Length)
            return 0;
        return Volume_List[index];
    }
    static public bool bLockBG = false;
    static public SndClass _SaveBG;
    static public SOUND_TYPE _SaveType;
    static public List<SndClass> SaveBgList = new List<SndClass>();
    public bool isBGPlaying(SndClass _snd = null)
    {
        if (!bInitOK) return false;
        return Instance.isPlayingBG(_snd);
    }
    public void PlayEffect(AudioClip snd, SOUND_TYPE mode = SOUND_TYPE._SND_0_FX, bool bloop = false, bool bSpecial = false)
    {
        if (!bInitOK) return;
        float vol = Get_Volume_Listf((int)mode);

        Instance.Play_Effect(snd, vol, bloop, bSpecial);
    }

    public void PlayEffect(SndClass snd, bool bloop = false, bool bSpecial = false)
    {

        if (!bInitOK) return;
        if (snd == null) return;
        if (snd.Clip == null) return;
        float vol = Get_Volume_Listf((int)snd.S_TYPE);
        Instance.Play_EffectVol(snd, vol, bloop, bSpecial);
    }
    public void PlayEffect(SndClass snd, float vols, bool bloop = false)
    {
        if (!bInitOK) return;
        float vol = ((float)snd.vol / 100.0f) * (float)vols / 100.0f;
        Instance.Play_EffectVol(snd, vol, bloop);
    }
    public void PlayBG(SndClass snd, bool bloop = true)
    {
        if (!bInitOK) return;

        _SaveType = snd.S_TYPE;
        _SaveBG = snd;
        if (SaveBgList.Count > 10)
            SaveBgList.RemoveAt(0);
        SaveBgList.Add(snd);
        if (bLockBG)
        {

            return;
        }
        Debug.LogFormat("start bgm: {0}", snd.Clip.name);
        float vol = ((float)snd.vol / 100.0f) * Get_Volume_Listf((int)snd.S_TYPE);
        Instance.Play_BGVol(snd, vol, bloop);
    }
    public void PlayBGLastList()
    {
        if (!bInitOK) return;
        if (SaveBgList.Count >= 2)
        {
            SndClass snd = SaveBgList[SaveBgList.Count - 2];
            if (bLockBG)
            {
                return;
            }
            Debug.LogFormat("start bgm: {0}", snd.Clip.name);
            float vol = ((float)snd.vol / 100.0f) * Get_Volume_Listf((int)snd.S_TYPE);
            Instance.Play_BGVol(snd, vol, true);
        }
    }
    public void Volume_Hide(float vol, float _t)
    {
        if (!bInitOK) return;
        Instance.Play_BGVol_Time(vol, _t);
    }
    public void PlayBGLock(SndClass snd, bool bloop = true)
    {
        if (!bInitOK) return;
        bLockBG = true;
        float vol = ((float)snd.vol / 100.0f) * Get_Volume_Listf((int)snd.S_TYPE);
        Instance.Play_BGVol(snd, vol, bloop);
    }
    public void PlayBGResume()
    {
        if (!bInitOK) return;
        if (_SaveBG != null)
        {
            PlayBG(_SaveBG);
        }

        _SaveBG = null;
    }
    public void PlayBGNext(SndClass snd, SOUND_TYPE mode = SOUND_TYPE._SND_1_BG, bool bloop = true)
    {
        if (!bInitOK) return;
        _SaveType = mode;
        _SaveBG = snd;
        if (bLockBG)
        {

            return;
        }
        float vol = ((float)snd.vol / 100.0f) * Get_Volume_Listf((int)snd.S_TYPE);
        Instance.Play_BGNext(snd, vol, bloop);
    }

    public void ChangeBGVolSecond(float vol, float fTime)
    {
        if (!bInitOK) return;
        if (bLockBG)
            return;

        Instance.Change_BGVol_Second(vol, fTime);
    }

    public void ChangeBGVol(float vol)
    {
        if (!bInitOK) return;
        if (bLockBG)
            return;

        Instance.Change_BGVol(vol);
    }
    public void ResumeBGVol()
    {
        if (!bInitOK) return;
        if (bLockBG)
            return;

        Instance.ResumeBG_Vol();
    }

    public void ChangeBGVolFade(float vol, float _t = 1)
    {
        if (!bInitOK) return;
        if (bLockBG)
            return;

        Instance.Change_BGVol_Fade(vol, _t);
    }

    public void StopBG(float _t = 0)
    {
        if (!bInitOK) return;
        Debug.Log("BG STOP");
        if (bLockBG)
            return;

        if (_t <= 0)
            Instance.Stop_BG();
        else
            Instance.BG_FadeOut(_t);
    }
    public void StopBGFade(float _t = 1)
    {
        if (!bInitOK) return;
        Debug.Log("BG STOP Fade");
        if (bLockBG)
            return;

        Instance.BG_FadeOut(_t);
    }

    public void VolumeSilence()
    {
        if (!bInitOK) return;
        Instance.SetSound_Vol(-50);
    }
    public void VolumeVerySmall()
    {
        if (!bInitOK) return;
        Instance.SetSound_Vol(-25);
    }
    public void VolumeSmall()
    {
        if (!bInitOK) return;
        Instance.SetSound_Vol(-10);
    }
    public void VolumeOn()
    {
        if (!bInitOK) return;
        Instance.SetSound_Vol(0);
    }

    public void PauseSound()
    {
        if (!bInitOK) return;
        Instance.AllPause();
    }
    public void ResumeSound()
    {
        if (!bInitOK) return;
        Instance.AllResume();
    }
    public void StopFX(int type = 0)
    {
        if (!bInitOK) return;
        //        Debug.Log("FX SND STOP");

        Instance.Play_Effect_Stop(0, type);
    }
    public void StopFX_N(int channel = 0)
    {
        if (!bInitOK) return;
        //        Debug.Log("FX SND STOP");
        Instance.Stop_Effect(channel);
    }

    public void PlayEffectSpeed(SndClass snd, float speed, bool bloop = false, bool bSpecial = false)
    {
        if (!bInitOK) return;
        float vol = Get_Volume_Listf((int)snd.S_TYPE);
        Instance.Play_EffectVolSpeed(snd, vol, speed, bloop, bSpecial);
    }
}
