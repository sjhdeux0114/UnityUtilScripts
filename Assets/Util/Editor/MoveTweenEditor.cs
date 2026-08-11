using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(MoveTween))]
public class MoveTweenEditor : Editor
{
    private MoveTween moveTween;
    private SerializedProperty waypointsProp;

    private void OnEnable()
    {
        moveTween = (MoveTween)target;
        waypointsProp = serializedObject.FindProperty("waypoints");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 기본 인스펙터 표시
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("씬 뷰에서 각 포인트의 위치와 노란색 핸들을 드래그하여 곡률(각도/강도)을 조절할 수 있습니다.\nPath Type을 CubicBezier로 설정해야 개별 설정이 적용됩니다.", MessageType.Info);

        if (GUILayout.Button("현재 위치에 웨이포인트 추가"))
        {
            Undo.RecordObject(moveTween, "Add Waypoint");
            Waypoint newWaypoint = new Waypoint();
            newWaypoint.position = moveTween.bLocal ? moveTween.transform.localPosition : moveTween.transform.position;
            newWaypoint.angle = 0f;
            newWaypoint.strength = 2f;
            moveTween.waypoints.Add(newWaypoint);
            EditorUtility.SetDirty(moveTween);
        }

        if (GUILayout.Button("모든 포인트 삭제"))
        {
            if (EditorUtility.DisplayDialog("경고", "정말로 모든 포인트를 삭제하시겠습니까?", "예", "아니오"))
            {
                Undo.RecordObject(moveTween, "Clear Waypoints");
                moveTween.waypoints.Clear();
                EditorUtility.SetDirty(moveTween);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        if (moveTween == null || moveTween.waypoints == null) return;

        serializedObject.Update();

        bool useLocal = moveTween.bLocal;
        Transform parent = moveTween.transform.parent;

        for (int i = 0; i < waypointsProp.arraySize; i++)
        {
            SerializedProperty waypointProp = waypointsProp.GetArrayElementAtIndex(i);
            SerializedProperty posProp = waypointProp.FindPropertyRelative("position");
            SerializedProperty angleProp = waypointProp.FindPropertyRelative("angle");
            SerializedProperty strengthProp = waypointProp.FindPropertyRelative("strength");

            Vector3 localPos = posProp.vector3Value;
            Vector3 worldPos = (useLocal && parent != null) ? parent.TransformPoint(localPos) : localPos;

            // 1. 위치 이동 핸들
            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                posProp.vector3Value = (useLocal && parent != null) ? parent.InverseTransformPoint(newWorldPos) : newWorldPos;
            }

            // 2. 각도 및 강도 조절 핸들 (노란색 선 끝의 작은 구체)
            float currentAngle = angleProp.floatValue;
            float currentStrength = strengthProp.floatValue;
            Vector3 localTangent = Quaternion.Euler(0, 0, currentAngle) * Vector3.right;
            Vector3 worldTangent = (useLocal && parent != null) ? parent.TransformDirection(localTangent) : localTangent;
            Vector3 handlePos = worldPos + worldTangent * currentStrength;

            Handles.color = Color.yellow;
            Handles.DrawLine(worldPos, handlePos);

            EditorGUI.BeginChangeCheck();
            // FreeMoveHandle을 사용하여 방향과 길이를 동시에 조절
            #if UNITY_2022_1_OR_NEWER
            Vector3 newHandlePos = Handles.FreeMoveHandle(handlePos, 0.15f, Vector3.zero, Handles.SphereHandleCap);
            #else
            Vector3 newHandlePos = Handles.FreeMoveHandle(handlePos, Quaternion.identity, 0.15f, Vector3.zero, Handles.SphereHandleCap);
            #endif

            if (EditorGUI.EndChangeCheck())
            {
                Vector3 newLocalHandlePos = (useLocal && parent != null) ? parent.InverseTransformPoint(newHandlePos) : newHandlePos;
                Vector3 delta = newLocalHandlePos - posProp.vector3Value;
                angleProp.floatValue = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                strengthProp.floatValue = delta.magnitude;
            }

            Handles.Label(worldPos + Vector3.up * 0.4f, $"Waypoint {i}", new GUIStyle() {
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            });
        }

        serializedObject.ApplyModifiedProperties();
    }
}
