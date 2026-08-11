#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class AudioPreviewer
{
    private const string PreviewObjectName = "Audio Previewer";

    private static readonly List<MethodInfo> stopMethods = new List<MethodInfo>();
    private static MethodInfo playMethod;
    private static AudioSource previewSource;
    private static GameObject previewObject;

    public static float Volume { get; set; } = 1f;

    static AudioPreviewer()
    {
        InitializeAudioUtil();
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        AssemblyReloadEvents.beforeAssemblyReload -= Stop;
        AssemblyReloadEvents.beforeAssemblyReload += Stop;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    public static void Play(AudioClip clip)
    {
        Play(clip, Volume);
    }

    public static void Play(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            return;
        }

        Volume = Mathf.Clamp01(volume);
        Stop();

        if (TryPlayWithAudioSource(clip, Volume))
        {
            return;
        }

        PlayWithAudioUtil(clip);
    }

    public static void Stop()
    {
        if (previewSource != null)
        {
            previewSource.Stop();
        }

        if (previewObject != null)
        {
            UnityEngine.Object.DestroyImmediate(previewObject);
            previewObject = null;
            previewSource = null;
        }

        foreach (MethodInfo method in stopMethods)
        {
            try
            {
                method.Invoke(null, null);
            }
            catch
            {
            }
        }

        AudioSource[] allAudioSources = Resources.FindObjectsOfTypeAll<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource != null && audioSource.hideFlags == HideFlags.HideAndDontSave)
            {
                audioSource.Stop();

                if (audioSource.gameObject != previewObject)
                {
                    UnityEngine.Object.DestroyImmediate(audioSource.gameObject);
                }
            }
        }
    }

    private static void InitializeAudioUtil()
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

        if (audioUtilClass == null)
        {
            return;
        }

        BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        playMethod = audioUtilClass.GetMethod(
            "PlayPreviewClip",
            flags,
            null,
            new[] { typeof(AudioClip), typeof(int), typeof(bool) },
            null);

        if (playMethod == null)
        {
            playMethod = audioUtilClass.GetMethod(
                "PlayClip",
                flags,
                null,
                new[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null);
        }

        if (playMethod == null)
        {
            playMethod = audioUtilClass.GetMethod(
                "PlayClip",
                flags,
                null,
                new[] { typeof(AudioClip) },
                null);
        }

        AddStopMethod(audioUtilClass, flags, "StopAllPreviewClips");
        AddStopMethod(audioUtilClass, flags, "StopAllClips");
        AddStopMethod(audioUtilClass, flags, "StopAllClip");
    }

    private static void AddStopMethod(Type audioUtilClass, BindingFlags flags, string methodName)
    {
        MethodInfo method = audioUtilClass.GetMethod(methodName, flags, null, Type.EmptyTypes, null);
        if (method != null)
        {
            stopMethods.Add(method);
        }
    }

    private static bool TryPlayWithAudioSource(AudioClip clip, float volume)
    {
        try
        {
            previewObject = EditorUtility.CreateGameObjectWithHideFlags(
                PreviewObjectName,
                HideFlags.HideAndDontSave,
                typeof(AudioSource));

            previewSource = previewObject.GetComponent<AudioSource>();
            previewSource.playOnAwake = false;
            previewSource.clip = clip;
            previewSource.volume = Mathf.Clamp01(volume);
            previewSource.loop = false;
            previewSource.Play();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarningFormat("AudioPreviewer: AudioSource preview failed. Falling back to AudioUtil. {0}", e.Message);
            CleanupAudioSourcePreview();
            return false;
        }
    }

    private static void PlayWithAudioUtil(AudioClip clip)
    {
        if (playMethod == null)
        {
            Debug.LogWarning("AudioPreviewer: UnityEditor.AudioUtil play method was not found.");
            return;
        }

        try
        {
            int paramCount = playMethod.GetParameters().Length;

            if (paramCount == 3)
            {
                playMethod.Invoke(null, new object[] { clip, 0, false });
            }
            else if (paramCount == 1)
            {
                playMethod.Invoke(null, new object[] { clip });
            }
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("AudioPreviewer: audio preview failed. {0}", e.Message);
        }
    }

    private static void Update()
    {
        if (previewSource != null)
        {
            previewSource.volume = Volume;

            if (!previewSource.isPlaying)
            {
                CleanupAudioSourcePreview();
            }
        }
    }

    private static void CleanupAudioSourcePreview()
    {
        if (previewObject != null)
        {
            UnityEngine.Object.DestroyImmediate(previewObject);
        }

        previewObject = null;
        previewSource = null;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredPlayMode)
        {
            Stop();
        }
    }
}
#endif
