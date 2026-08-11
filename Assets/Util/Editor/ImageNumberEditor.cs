using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(ImageNumber))]
public class ImageNumberEditor : Editor
{
    ImageNumber script = null;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        script = (ImageNumber)target;

        if (GUILayout.Button("update"))
        {


            script._Number_Update();
        }
        if (GUILayout.Button("Init"))
        {


            script._Init();
            script._Number_Update();
        }


    }

}
