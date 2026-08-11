using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;

// This is an editor script for SimpleSpriteAnimator.
// It creates buttons in the inspector to easily test animations.

[CustomEditor(typeof(SimpleFollow))]
public class SimpleFollowEditor : Editor
{

    SimpleFollow animator = null;
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
        animator = (SimpleFollow)target;

        // --- Play Test Section ---
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Play Test (Editor)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Press a button below to preview the animation in the Scene view.", MessageType.Info);

        animator.OffsetPos = EditorGUILayout.Vector3Field("Move Position", animator.OffsetPos);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button($"Init"))
        {

            animator.Init();


        }

        if (GUILayout.Button($"Add pos+"))
        {
                
            animator.Set_TargetAddPos(animator.OffsetPos);
            bPlay = true;


        }
        if (GUILayout.Button($"Add pos-"))
        {
            animator.Set_TargetAddPos(-animator.OffsetPos);
            bPlay = true;
        }

        EditorGUILayout.EndHorizontal();

        // Add a Stop button
        if (bPlay)
        {
            if (GUILayout.Button("Stop"))
            {
                // For now, it just stops updating.
                bPlay = false;
                animator._Reset();
            }
        }


    }

    private void EditorUpdate()
    {
        if (EditorApplication.isPlaying)
        {
            bPlay = false;
            return;
        }
        if (animator == null)
            return;

        // In here you can check the current realtime, see if a certain
        // amount of time has elapsed, and perform some task.
        float delta = Time.realtimeSinceStartup - deltatimes;

        deltatimes = Time.realtimeSinceStartup;
        if (bPlay)
        {
            animator._Update(delta);
            EditorUtility.SetDirty(animator);
        }
    }

    
    bool bPlay = false;
    float deltatimes = 0;

}