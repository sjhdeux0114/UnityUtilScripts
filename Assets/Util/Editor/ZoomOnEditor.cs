using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ZoomOn))]
public class ZoomonEditor : Editor
{
    ZoomOn script = null;
    bool bPlay = false;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        script = (ZoomOn)target;

        if (GUILayout.Button("Init"))
        {
            script._Init();
        }
        if (GUILayout.Button("PLAY"))
        {
            script._Reset();
            script.Play();
            bPlay = true;
        }
        if (GUILayout.Button("PLAY-Back"))
        {
            script.PlayBackward();
            bPlay = true;
        }
        if (GUILayout.Button("STOP"))
        {
            bPlay = false;
        }


    }

    private float m_LastEditorUpdateTime;
    float deltatimes = 0;
    protected virtual void OnEnable()
    {
#if UNITY_EDITOR
        m_LastEditorUpdateTime = Time.realtimeSinceStartup;
        deltatimes = Time.realtimeSinceStartup;
        EditorApplication.update += OnEditorUpdate;
#endif
    }

    protected virtual void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= OnEditorUpdate;
#endif
    }

    protected virtual void OnEditorUpdate()
    {
        if (EditorApplication.isPlaying)
        {
            bPlay = false;
            return;
        }
        if (script == null)
            return;

        // In here you can check the current realtime, see if a certain
        // amount of time has elapsed, and perform some task.
        float delta = Time.realtimeSinceStartup - deltatimes;

        deltatimes = Time.realtimeSinceStartup;
        if (bPlay)
        {
            script._Update(delta);
            EditorUtility.SetDirty(script);
        }
    }
}