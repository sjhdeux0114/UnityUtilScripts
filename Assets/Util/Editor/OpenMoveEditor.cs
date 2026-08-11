using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OpenMove))]
public class OpenMoveEditor : Editor
{
    OpenMove script = null;
    bool bPlay = false;
    float ViewRange = 0.0f;
    float OldViewRange = 0.0f;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        script = (OpenMove)target;

        // 값을 변경하기 전에 Undo 시스템에 등록합니다.
        // 이렇게 해야 Ctrl+Z로 되돌릴 수 있습니다.
        Undo.RecordObject(script, "OpenMove Inspector Change");

        EditorGUILayout.BeginHorizontal();
        // 1. 반환된 값을 변수에 할당합니다.
        float newMoveTime = EditorGUILayout.FloatField("Time", script.GetMode.MoveTime, GUILayout.Width(150));
        float newDelayTime = EditorGUILayout.FloatField("Delay", script.GetMode.DelayTime, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        // 2. 값이 변경되었는지 확인하고 저장 명령을 실행합니다.
        // 값이 변경되었는지 확인하고 에셋에 직접 저장합니다.
        if (newMoveTime != script.GetMode.MoveTime || newDelayTime != script.GetMode.DelayTime)
        {
            // ScriptableObject 인스턴스의 값을 업데이트합니다.
            script.GetMode.MoveTime = newMoveTime;
            script.GetMode.DelayTime = newDelayTime;

            // ScriptableObject 에셋을 'Dirty' 상태로 표시하여 변경 사항을 알립니다.
            EditorUtility.SetDirty(script.GetMode);

            // 변경된 에셋을 강제로 저장합니다.
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        ViewRange = EditorGUILayout.Slider("ViewRange", ViewRange, 0.0f, 1.0f);

        if (OldViewRange != ViewRange)
        {
            bPlay = false;
            script.ViewPer(ViewRange);
        }

        OldViewRange = ViewRange;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("<"))
        {
            script.MoveTypeIndex--;
        }
        script.MoveTypeIndex = EditorGUILayout.IntField("MoveTypeIndex", script.MoveTypeIndex);
        if (GUILayout.Button(">"))
        {
            script.MoveTypeIndex++;
        }
        EditorGUILayout.EndHorizontal();

        script.GetMode.Dest_Rot = EditorGUILayout.Vector3Field("Dest_Rot", script.GetMode.Dest_Rot);
        script.GetMode.Dest_Scale = EditorGUILayout.Vector3Field("Dest_Scale", script.GetMode.Dest_Scale);

        script.GetMode.Curve_PosX = EditorGUILayout.CurveField("Curve_PosX", script.GetMode.Curve_PosX);
        script.GetMode.Curve_PosY = EditorGUILayout.CurveField("Curve_PosY", script.GetMode.Curve_PosY);
        script.GetMode.Curve_PosZ = EditorGUILayout.CurveField("Curve_PosZ", script.GetMode.Curve_PosZ);
        script.GetMode.Curve_Rot = EditorGUILayout.CurveField("Curve_Rot", script.GetMode.Curve_Rot);
        script.GetMode.Curve_Scale = EditorGUILayout.CurveField("Curve_Scale", script.GetMode.Curve_Scale);

        if (GUILayout.Button("PLAY"))
        {
            script.Init();
            script.Reset();
            script.Play();
            bPlay = true;
        }
        if (GUILayout.Button("STOP"))
        {
            bPlay = false;
        }

        if (GUILayout.Button("커브초기화"))
        {
            script.Def_Curve();
        }
        if (GUILayout.Button("포지션 넣기"))
        {
            script.Def_Pos();
        }
        if (GUI.changed)
        {
            // 이 코드는 CurveField나 다른 UI 변경에 대해 잘 작동합니다.
            EditorUtility.SetDirty(script);

            // ScriptableObject 에셋을 'Dirty' 상태로 표시하여 변경 사항을 알립니다.
            EditorUtility.SetDirty(script.GetMode);

            // 변경된 에셋을 강제로 저장합니다.
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
            script.UpdateDelta(delta);
            EditorUtility.SetDirty(script);
            if (!script.isPlay)
                bPlay = false;
        }
    }
}
