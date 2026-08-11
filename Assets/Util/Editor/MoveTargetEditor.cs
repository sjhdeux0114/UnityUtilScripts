using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MoveTarget))]
public class MoveTargetEditor : Editor {
    int Number=0;

    void OnSceneGUI()
    {
        MoveTarget script = (MoveTarget)target;

        Handles.color = script.LineColor;

        for (int i=0;i<script.targets.Length;i++)
        {
            int next = i + 1;
            if (next >= script.targets.Length)
                next = 0;
            if(i == 0)
                Handles.color = Color.white;
            else
                Handles.color = script.LineColor;
            Handles.Label(script.targets[i].targets.position, "" + i);
            Handles.DrawLine(script.targets[i].targets.position, script.targets[next].targets.position);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MoveTarget script = (MoveTarget)target;

        if (GUILayout.Button("LOOK"))
        {
            if (script.LookTarget)
                script.transform.LookAt(script.LookTarget);
        }


        if (GUILayout.Button("MOVE"))
        {
            if (Number < script.targets.Length)
            {
                script.transform.position = script.targets[Number].targets.position;
                if (script.LookTarget)
                    script.transform.LookAt(script.LookTarget);

                Number++;
                if (Number >= script.targets.Length)
                    Number = 0;
            }
        }


    }
}
