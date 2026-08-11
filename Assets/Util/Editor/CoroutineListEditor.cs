// file:///d:/forkE/OniMusha/OniMusha/Assets/Editor/CoroutineListEditor.cs
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

[CustomEditor(typeof(CoroutineList))]
public class CoroutineListEditor : Editor
{
    SerializedProperty eventsProp;
    SerializedProperty testIndexProp;

    void OnEnable()
    {
        // Grab the serialized fields we need
        eventsProp = serializedObject.FindProperty("Events");
        testIndexProp = serializedObject.FindProperty("TestIndex");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object representation
        serializedObject.Update();

        // Draw the default inspector (shows all fields)
        DrawDefaultInspector();

        var starget = (CoroutineList)target;

        // Space before our custom UI
        EditorGUILayout.Space();

        // ==== Test All Events Button ====
        if (GUILayout.Button("Test All Event Groups"))
        {
            // Calls EventCall for each defined group
            for (int i = 0; i < starget.Events.Count; i++)
                starget.EventCall(i);
        }

        // ==== Test Specific Index ====
        EditorGUILayout.PropertyField(testIndexProp, new GUIContent("Test Index"));
        if (GUILayout.Button("Test Event at Test Index"))
        {

            starget.EventCall(starget.TestIndex);
        }

        // ==== Per‑Group Buttons (optional quick testing) ====
        if (eventsProp != null && eventsProp.isArray)
        {
            EditorGUILayout.LabelField("Quick Test Buttons", EditorStyles.boldLabel);
            for (int i = 0; i < eventsProp.arraySize; i++)
            {
                // Show a button for each event group
                if (GUILayout.Button($"Test Event Group {i}"))
                {

                    starget.EventCall(i);
                }
            }
        }

        // Apply any property changes
        serializedObject.ApplyModifiedProperties();
    }
}
