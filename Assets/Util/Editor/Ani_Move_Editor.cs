using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


[CustomEditor(typeof(Ani_Move_Control))]
public class Ani_Move_Editor : Editor
{
    private int selectedAnimationIndex = -1;
    private readonly int currentFrameIndex = 0;
    Ani_Move_Control Ani_Move = null;
    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector GUI first.
        base.OnInspectorGUI();

        // Get the target object (SimpleSpriteAnimator script).
        Ani_Move = (Ani_Move_Control)target;

        if(Ani_Move.animator == null)   Ani_Move.animator = Ani_Move.GetComponent<SimpleSpriteAnimator>();
        if (Ani_Move.M_Pos == null) Ani_Move.M_Pos = Ani_Move.GetComponent<MovePathPos>();



        // --- Play Test Section ---
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Play Test (Editor)", EditorStyles.boldLabel);


        if (!Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"Init - Dic:"))
            {
                Ani_Move._Init();

            }
            if (GUILayout.Button($"Init - String:"))
            {
                Ani_Move.Init_String();

            }
            EditorGUILayout.EndHorizontal();
        }

        if(Ani_Move.Start_aniName == null || Ani_Move.End_aniName == null)
        {
            Ani_Move._Init();
        }


        for (int i = 0; i < Ani_Move.M_Pos.TargetPoints.Count - 1; i++)
        {
            EditorGUILayout.BeginHorizontal();

            try
            {
                Ani_Move.Start_aniName[i] = GUILayout.TextField(Ani_Move.Start_aniName[i]);
                Ani_Move.End_aniName[i] = GUILayout.TextField(Ani_Move.End_aniName[i]);
            }
            catch(IndexOutOfRangeException ie)
            {
                Debug.Log(ie.Message);
                Ani_Move.Init_String();
            }


            if (GUILayout.Button($"Simulate {i} -> {i + 1}"))
            {
                if (Ani_Move.M_Pos.TargetPoints.Count > 1)
                {
                    bPlay = true;
                    Ani_Move.Play(i);
                    
                }
            }
            EditorGUILayout.EndHorizontal();
        }


        if (Ani_Move.M_Pos.PathMode == CMOVE_TYPE.TOTAL)
        {
            // 시뮬레이션 버튼 (에디터에서 이동 시뮬레이션)
            if (GUILayout.Button("Simulate Path"))
            {
                if (Ani_Move.M_Pos.TargetPoints.Count > 1)
                {
                    bPlay = true;
                    Ani_Move.Play();
                }
            }
        }
        else if (Ani_Move.M_Pos.PathMode == CMOVE_TYPE.ONE_PATH)
        {
            for (int i = 0; i < Ani_Move.M_Pos.TargetPoints.Count - 1; i++)
            {
                // 시뮬레이션 버튼 (에디터에서 이동 시뮬레이션)
                if (GUILayout.Button($"Simulate {i} -> {i + 1}"))
                {
                    if (Ani_Move.M_Pos.TargetPoints.Count > 1)
                    {
                        if (Application.isPlaying)
                            Ani_Move.M_Pos.PlayIndex(i);
                        else
                            SimulatePath(i);
                    }
                }
            }
        }

        if (GUILayout.Button("Stop"))
        {
            Ani_Move.M_Pos.isPlaying = false;
            Ani_Move.animator.Stop();
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Animation Test (Editor)", EditorStyles.boldLabel);



        // Animation selection buttons
        foreach (var animation in Ani_Move.animator.animations)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button($"Preview: {animation.animationName}"))
            {
                
                if(Application.isPlaying)
                {
                    Ani_Move.animator.Play(animation.animationName);
                }
                else
                {
                    Ani_Move.animator.Play(animation.animationName);

                    bPlay = true;

                }

            }
            if (Application.isPlaying)
            {
                if (GUILayout.Button($"ADD: {animation.animationName}"))
                {
                    Ani_Move.animator.AddNext(animation.animationName);
                }
            }

                EditorGUILayout.EndHorizontal();

        }

        // Add a Stop button
        if (bPlay)
        {
            if (GUILayout.Button("Stop"))
            {
                selectedAnimationIndex = -1;
                // You may want to revert to the initial sprite here if needed
                // For now, it just stops updating.
                bPlay = false;
            }
        }

        // Display current frame info
        if (selectedAnimationIndex != -1 && selectedAnimationIndex < Ani_Move.animator.animations.Count)
        {
            var currentAnimation = Ani_Move.animator.animations[selectedAnimationIndex];
            if (currentAnimation.sprites.Count > 0)
            {
                EditorGUILayout.LabelField($"Playing: {currentAnimation.animationName} (Frame: {currentFrameIndex + 1}/{currentAnimation.sprites.Count})");
            }
        }

        // 데이터가 변경되었음을 알림
        if (GUI.changed)
        {
            EditorUtility.SetDirty(Ani_Move);
        }

    }

    private void SimulatePath(int n = -1)
    {
        if (n >= 0)
        {
            Ani_Move.M_Pos.PlayIndex(n);
        }
        else
        {
            Ani_Move.M_Pos.Play();
        }
        deltatimes = Time.realtimeSinceStartup;


    }

    private void EditorUpdate()
    {
        if (EditorApplication.isPlaying)
        {
            bPlay = false;
            return;
        }
        if (Ani_Move == null)
            return;

        // In here you can check the current realtime, see if a certain
        // amount of time has elapsed, and perform some task.
        float delta = Time.realtimeSinceStartup - deltatimes;

        deltatimes = Time.realtimeSinceStartup;
        if (bPlay)
        {
            Ani_Move._Update(delta);
            EditorUtility.SetDirty(Ani_Move.animator);
        }
    }

    
    bool bPlay = false;
    float deltatimes = 0;

}