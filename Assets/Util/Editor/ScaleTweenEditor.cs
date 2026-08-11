using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(ScaleTween))]
public class ScaleTweenEditor : Editor
{
    private ScaleTween scaleTween;
    private SerializedProperty scalepointsProp;

    private void OnEnable()
    {
        scaleTween = (ScaleTween)target;
        scalepointsProp = serializedObject.FindProperty("scalepoints");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("씬 뷰에서 각 포인트의 스케일 박스와 스케일 핸들을 드래그하여 조절할 수 있습니다.", MessageType.Info);

        if (GUILayout.Button("현재 스케일로 포인트 추가"))
        {
            Undo.RecordObject(scaleTween, "Add Scale Point");
            ScaleInfo newPoint = new ScaleInfo();
            Vector3 currentScale = scaleTween.bLocal ? scaleTween.transform.localScale : scaleTween.transform.lossyScale;
            newPoint.Start_Scale = currentScale;
            newPoint.End_Scale = currentScale;
            scaleTween.scalepoints.Add(newPoint);
            EditorUtility.SetDirty(scaleTween);
        }

        if (GUILayout.Button("모든 포인트 삭제"))
        {
            if (EditorUtility.DisplayDialog("경고", "정말로 모든 포인트를 삭제하시겠습니까?", "예", "아니오"))
            {
                Undo.RecordObject(scaleTween, "Clear Scale Points");
                scaleTween.scalepoints.Clear();
                EditorUtility.SetDirty(scaleTween);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("인덱스별 테스트 버튼", EditorStyles.boldLabel);
        if (scaleTween.scalepoints != null && scaleTween.scalepoints.Count > 0)
        {
            for (int i = 0; i < scaleTween.scalepoints.Count; i++)
            {
                if (GUILayout.Button($"Index {i} 테스트 재생"))
                {
                    Undo.RecordObject(scaleTween, $"Test Play Index {i}");
                    scaleTween.Play(i);
                    if (!Application.isPlaying)
                    {
                        // 에디터 모드에서는 즉시 해당 인덱스의 끝 스케일로 업데이트하여 눈으로 확인할 수 있게 함
                        scaleTween.PreView = 1f;
                        EditorUtility.SetDirty(scaleTween);
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        if (scaleTween == null || scaleTween.scalepoints == null) return;

        serializedObject.Update();

        Transform t = scaleTween.transform;
        Vector3 basePos = t.position;
        Quaternion rot = t.rotation;

        for (int i = 0; i < scalepointsProp.arraySize; i++)
        {
            SerializedProperty pointProp = scalepointsProp.GetArrayElementAtIndex(i);
            SerializedProperty startScaleProp = pointProp.FindPropertyRelative("Start_Scale");
            SerializedProperty endScaleProp = pointProp.FindPropertyRelative("End_Scale");

            Vector3 startScale = startScaleProp.vector3Value;
            Vector3 endScale = endScaleProp.vector3Value;

            // Offset positions so they are laid out horizontally
            Vector3 startPos = basePos + t.right * (i * 4f);
            Vector3 endPos = basePos + t.right * (i * 4f + 2f);

            // Draw connecting lines
            Handles.color = Color.cyan;
            Handles.DrawLine(startPos, endPos);
            if (i < scalepointsProp.arraySize - 1)
            {
                Vector3 nextStartPos = basePos + t.right * ((i + 1) * 4f);
                Handles.color = Color.gray;
                Handles.DrawLine(endPos, nextStartPos);
            }

            bool isAllPlay = (scaleTween.scaleMode == ScaleTween.ScaleMode.AllPlay || scaleTween.scaleMode == ScaleTween.ScaleMode.AllPlayLoop || scaleTween.scaleMode == ScaleTween.ScaleMode.AllPlayPingPong);
            bool isActive = isAllPlay || (i == scaleTween.playIndex);

            // Draw start scale box and handle
            Handles.color = isActive ? Color.green : new Color(0f, 0.5f, 0f, 0.5f);
            Handles.DrawWireCube(startPos, startScale);
            Handles.Label(startPos + t.up * (startScale.y * 0.5f + 0.5f), $"{(isActive ? "[Active] " : "")}Point {i} Start\n{startScale}", new GUIStyle() {
                normal = { textColor = isActive ? Color.green : Color.gray },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            });

            EditorGUI.BeginChangeCheck();
            Vector3 newStartScale = Handles.ScaleHandle(startScale, startPos, rot, HandleUtility.GetHandleSize(startPos));
            if (EditorGUI.EndChangeCheck())
            {
                startScaleProp.vector3Value = newStartScale;
            }

            // Draw end scale box and handle
            Handles.color = isActive ? Color.yellow : new Color(0.5f, 0.5f, 0f, 0.5f);
            Handles.DrawWireCube(endPos, endScale);
            Handles.Label(endPos + t.up * (endScale.y * 0.5f + 0.5f), $"{(isActive ? "[Active] " : "")}Point {i} End\n{endScale}", new GUIStyle() {
                normal = { textColor = isActive ? Color.yellow : Color.gray },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            });

            EditorGUI.BeginChangeCheck();
            Vector3 newEndScale = Handles.ScaleHandle(endScale, endPos, rot, HandleUtility.GetHandleSize(endPos));
            if (EditorGUI.EndChangeCheck())
            {
                endScaleProp.vector3Value = newEndScale;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
