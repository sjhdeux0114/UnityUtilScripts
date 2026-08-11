using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전역에서 사운드(BG, FX, Volume 제어 등)를 명령할 수 있는 정적(Static) 이벤트 버스입니다.
/// </summary>
public static class SoundEventBus
{
    // ==========================================
    // BGM Controls
    // ==========================================
    public static event Action<SndClass, bool> OnPlayBG;
    public static void PlayBG(SndClass snd, bool bloop = true) => OnPlayBG?.Invoke(snd, bloop);

    public static event Action OnPlayBGLastList;
    public static void PlayBGLastList() => OnPlayBGLastList?.Invoke();

    public static event Action<SndClass, bool> OnPlayBGLock;
    public static void PlayBGLock(SndClass snd, bool bloop = true) => OnPlayBGLock?.Invoke(snd, bloop);

    public static event Action OnPlayBGResume;
    public static void PlayBGResume() => OnPlayBGResume?.Invoke();

    public static event Action<SndClass, SOUND_TYPE, bool> OnPlayBGNext;
    public static void PlayBGNext(SndClass snd, SOUND_TYPE mode = SOUND_TYPE._SND_1_BG, bool bloop = true) => OnPlayBGNext?.Invoke(snd, mode, bloop);

    public static event Action<float> OnStopBG;
    public static void StopBG(float _t = 0) => OnStopBG?.Invoke(_t);

    public static event Action<float> OnStopBGFade;
    public static void StopBGFade(float _t = 1) => OnStopBGFade?.Invoke(_t);

    public static event Action<float> OnChangeBGVol;
    public static void ChangeBGVol(float vol) => OnChangeBGVol?.Invoke(vol);

    public static event Action<float, float> OnChangeBGVolSecond;
    public static void ChangeBGVolSecond(float vol, float fTime) => OnChangeBGVolSecond?.Invoke(vol, fTime);

    public static event Action<float, float> OnChangeBGVolFade;
    public static void ChangeBGVolFade(float vol, float _t = 1) => OnChangeBGVolFade?.Invoke(vol, _t);

    public static event Action OnResumeBGVol;
    public static void ResumeBGVol() => OnResumeBGVol?.Invoke();



    // ==========================================
    // Effect Controls
    // ==========================================
    public static event Action<AudioClip, SOUND_TYPE, bool, bool> OnPlayEffectClip;
    public static void PlayEffect(AudioClip snd, SOUND_TYPE mode = SOUND_TYPE._SND_0_FX, bool bloop = false, bool bSpecial = false) => OnPlayEffectClip?.Invoke(snd, mode, bloop, bSpecial);

    public static event Action<SndClass, bool, bool> OnPlayEffectSnd;
    public static void PlayEffect(SndClass snd, bool bloop = false, bool bSpecial = false) => OnPlayEffectSnd?.Invoke(snd, bloop, bSpecial);

    public static event Action<SndClass, float, bool> OnPlayEffectVol;
    public static void PlayEffect(SndClass snd, float vols, bool bloop = false) => OnPlayEffectVol?.Invoke(snd, vols, bloop);

    public static event Action<SndClass, float, bool, bool> OnPlayEffectSpeed;
    public static void PlayEffectSpeed(SndClass snd, float speed, bool bloop = false, bool bSpecial = false) => OnPlayEffectSpeed?.Invoke(snd, speed, bloop, bSpecial);

    public static event Action<int> OnStopFX;
    public static void StopFX(int type = 0) => OnStopFX?.Invoke(type);

    public static event Action<int> OnStopFX_N;
    public static void StopFX_N(int channel = 0) => OnStopFX_N?.Invoke(channel);
    public static event Action OnStopEffectLoop;
    public static void StopEffectLoop() => OnStopEffectLoop?.Invoke();


    // ==========================================
    // System & Volume Controls
    // ==========================================
    public static event Action<int[]> OnSet_Volume_List;
    public static void Set_Volume_List(int[] list) => OnSet_Volume_List?.Invoke(list);

    public static event Action OnVolumeSilence;
    public static void VolumeSilence() => OnVolumeSilence?.Invoke();

    public static event Action OnVolumeVerySmall;
    public static void VolumeVerySmall() => OnVolumeVerySmall?.Invoke();

    public static event Action OnVolumeSmall;
    public static void VolumeSmall() => OnVolumeSmall?.Invoke();

    public static event Action OnVolumeOn;
    public static void VolumeOn() => OnVolumeOn?.Invoke();

    public static event Action OnPauseSound;
    public static void PauseSound() => OnPauseSound?.Invoke();

    public static event Action OnResumeSound;
    public static void ResumeSound() => OnResumeSound?.Invoke();

    // NOTE: isBGPlaying과 같은 return 값이 필요한 메서드들은 상태 동기화나 Func를 사용할 수 있지만
    // 범용성을 위해 SoundManager.Instance 참조를 부분적으로 활용하거나 별도의 State 관리 객체를 두는 것이 좋습니다.
    // 여기서는 기존 코드를 최소한으로 건드리도록 유지하거나 SoundManager 쪽에 헬퍼를 둡니다.
}
