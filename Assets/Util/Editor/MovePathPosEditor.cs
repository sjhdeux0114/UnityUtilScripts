using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MovePathPos))]
public class MovePathPosEditor : Editor
{
    private MovePathPos path;
    private int selectedPointIndex = -1;


    public override void OnInspectorGUI()
    {
        path = (MovePathPos)target;
        DrawDefaultInspector();

        EditorGUILayout.LabelField("Speed Range", EditorStyles.boldLabel);

        float currentMinSpeed = path.minSpeed;
        float currentMaxSpeed = path.maxSpeed;

        EditorGUILayout.MinMaxSlider(ref currentMinSpeed, ref currentMaxSpeed, 0.1f, 1000f);

        path.minSpeed = currentMinSpeed;
        path.maxSpeed = currentMaxSpeed;

        EditorGUILayout.LabelField("Min: " + path.minSpeed.ToString("F2"), "Max: " + path.maxSpeed.ToString("F2"));


        // ADD, DELETE, SELECT 
        if (GUILayout.Button("ADD"))
        {
            if (path.TargetPoints.Count > 0)
                path.TargetPoints.Add(new CMovePath(path.TargetPoints[path.TargetPoints.Count - 1]));
            else
                path.TargetPoints.Add(new CMovePath(null));
        }

        if (path.TargetPoints.Count > 0)
        {
            // DELETE ��ư
            if (GUILayout.Button("DELETE Selected Point"))
            {
                if (selectedPointIndex != -1 && selectedPointIndex < path.TargetPoints.Count)
                {
                    path.TargetPoints.RemoveAt(selectedPointIndex);
                    selectedPointIndex = -1;
                }
            }

            // SELECT ��ư��
            GUILayout.Label("Select Points:", EditorStyles.boldLabel);

            for (int i = 0; i < path.TargetPoints.Count; i++)
            {
                GUILayout.BeginHorizontal();
                string buttonText = "Select " + i;
                if (GUILayout.Button(buttonText))
                {
                    selectedPointIndex = i;
                    if (path.bLocal)
                        path.transform.localPosition = path.TargetPoints[i].Pos;
                    else
                        path.transform.position = path.TargetPoints[i].Pos;
                    path.transform.localScale = path.TargetPoints[i].Scale;
                    path.transform.localEulerAngles = path.TargetPoints[i].Rotation;
                }
                if (GUILayout.Button("SET"))
                {
                    if (path.bLocal)
                        path.TargetPoints[i].Pos = path.transform.localPosition;
                    else
                        path.TargetPoints[i].Pos = path.transform.position;
                    path.TargetPoints[i].Scale = path.transform.localScale;
                    path.TargetPoints[i].Rotation = path.transform.localEulerAngles;

                }
                if (GUILayout.Button("Get"))
                {
                    if (path.bLocal)
                        path.transform.localPosition = path.TargetPoints[i].Pos;
                    else
                        path.transform.position = path.TargetPoints[i].Pos;
                    path.transform.localScale = path.TargetPoints[i].Scale;
                    path.transform.localEulerAngles = path.TargetPoints[i].Rotation;
                }
                GUILayout.EndHorizontal();
            }

            if (path.PathMode == CMOVE_TYPE.TOTAL)
            {

                if (GUILayout.Button("Simulate Path"))
                {
                    if (path.TargetPoints.Count > 1)
                    {
                        if (Application.isPlaying)
                            path.Play();
                        else
                            SimulatePath();
                    }
                }
            }
            else if (path.PathMode == CMOVE_TYPE.ONE_PATH)
            {
                for (int i = 0; i < path.TargetPoints.Count - 1; i++)
                {

                    if (GUILayout.Button($"Simulate {i} -> {i + 1}"))
                    {
                        if (path.TargetPoints.Count > 1)
                        {
                            if (Application.isPlaying)
                                path.PlayIndex(i);
                            else
                                SimulatePath(i);
                        }
                    }
                }
            }

            if (GUILayout.Button("Stop"))
            {
                path.isPlaying = false;
            }
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(path);
        }
    }

    // e:\forkE\OniMusha\OniMusha\Assets\Script\Util_Script\Util\Editor\MovePathPosEditor.cs

    private void OnSceneGUI()
    {
        path = (MovePathPos)target;
        if (path == null || path.TargetPoints == null) return;

        Handles.color = Color.yellow;

        for (int i = 0; i < path.TargetPoints.Count; i++)
        {
            // 1. 표시할 월드 좌표 계산
            Vector3 worldPos = path.TargetPoints[i].Pos;
            if (path.bLocal && path.transform.parent != null)
            {
                worldPos = path.transform.parent.TransformPoint(path.TargetPoints[i].Pos);
            }

            EditorGUI.BeginChangeCheck();
            // 2. 핸들 표시 및 조작
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(path, "Move Path Point");

                // 3. 조작된 월드 좌표를 다시 로컬 좌표로 변환하여 저장
                if (path.bLocal && path.transform.parent != null)
                {
                    path.TargetPoints[i].Pos = path.transform.parent.InverseTransformPoint(newWorldPos);
                }
                else
                {
                    path.TargetPoints[i].Pos = newWorldPos;
                }

                EditorUtility.SetDirty(path);
            }

            // 인덱스 라벨 표시
            Handles.Label(worldPos + Vector3.up * 0.2f, i.ToString());
        }
    }

    float deltatimes;

    private void SimulatePath(int n = -1)
    {
        if (n >= 0)
        {
            path.PlayIndex(n);
        }
        else
        {
            path.Play();
        }
        deltatimes = Time.realtimeSinceStartup;

        Debug.Log($"Simulation start. {EditorApplication.timeSinceStartup}");

    }

    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (Application.isPlaying)
            return;
        if (path == null)
            return;
        // In here you can check the current realtime, see if a certain
        // amount of time has elapsed, and perform some task.
        float delta = Time.realtimeSinceStartup - deltatimes;

        deltatimes = Time.realtimeSinceStartup;
        if (path.isPlaying)
        {
            path._Update(delta);
            EditorUtility.SetDirty(path);
        }
    }


}
