using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MovePathPos))]
public class MovePathPosEditor : Editor
{
    private MovePathPos path;
    private int selectedPointIndex = -1;

    // View options
    private bool showAllHandles = false;
    private bool showPathLines = true;
    private bool showHUD = true;
    private bool hudCollapsed = false;

    private float deltatimes;

    public override void OnInspectorGUI()
    {
        path = (MovePathPos)target;
        if (path == null) return;

        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Speed Range", EditorStyles.boldLabel);

        float currentMinSpeed = path.minSpeed;
        float currentMaxSpeed = path.maxSpeed;

        EditorGUILayout.MinMaxSlider(ref currentMinSpeed, ref currentMaxSpeed, 0.1f, 1000f);

        path.minSpeed = currentMinSpeed;
        path.maxSpeed = currentMaxSpeed;

        EditorGUILayout.LabelField("Min: " + path.minSpeed.ToString("F2"), "Max: " + path.maxSpeed.ToString("F2"));

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Line Mode", EditorStyles.boldLabel, GUILayout.Width(100));
        CLINE_TYPE newLineMode = (CLINE_TYPE)EditorGUILayout.EnumPopup(path.LineMode);
        if (newLineMode != path.LineMode)
        {
            Undo.RecordObject(path, "Change Line Mode");
            path.LineMode = newLineMode;
            EditorUtility.SetDirty(path);
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();

        if (path.LineMode == CLINE_TYPE.BEZIER)
        {
            EditorGUILayout.HelpBox("Bezier 모드: 씬 뷰에서 노란색 핸들을 드래그하여 각도(Angle)와 곡률 강도(Strength)를 실시간 조절할 수 있습니다.", MessageType.Info);
            if (selectedPointIndex >= 0 && selectedPointIndex < path.TargetPoints.Count)
            {
                CMovePath selPt = path.TargetPoints[selectedPointIndex];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Point {selectedPointIndex} 베지어 각도 및 곡률 조절", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                Vector3 effT = path.GetEffectiveTangent(selectedPointIndex);
                float curAngle = selPt.bCustomBezier ? selPt.angle : Mathf.Atan2(effT.y, effT.x) * Mathf.Rad2Deg;
                float curStrength = selPt.bCustomBezier ? selPt.strength : effT.magnitude;

                float newAngle = EditorGUILayout.Slider("Angle (각도 °)", curAngle, -180f, 180f);
                float newStrength = EditorGUILayout.FloatField("Strength (곡률 강도)", curStrength);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(path, "Adjust Bezier Angle");
                    selPt.bCustomBezier = true;
                    selPt.angle = newAngle;
                    selPt.strength = Mathf.Max(0.01f, newStrength);
                    selPt.Tangent = selPt.GetTangent() * selPt.strength;
                    EditorUtility.SetDirty(path);
                    SceneView.RepaintAll();
                }

                if (selPt.bCustomBezier)
                {
                    if (GUILayout.Button("선택한 포인트 자동(기본) 각도로 초기화", EditorStyles.miniButton))
                    {
                        Undo.RecordObject(path, "Reset Bezier to Auto");
                        selPt.bCustomBezier = false;
                        selPt.Tangent = Vector3.zero;
                        EditorUtility.SetDirty(path);
                        SceneView.RepaintAll();
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Scene View Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        showPathLines = EditorGUILayout.ToggleLeft("Show Path Line", showPathLines, GUILayout.Width(130));
        showAllHandles = EditorGUILayout.ToggleLeft("Show All Handles", showAllHandles, GUILayout.Width(130));
        showHUD = EditorGUILayout.ToggleLeft("Show HUD", showHUD);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // ADD, DELETE, SELECT
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ADD", GUILayout.Height(24)))
        {
            Undo.RecordObject(path, "Add Path Point");
            if (path.TargetPoints.Count > 0)
                path.TargetPoints.Add(new CMovePath(path.TargetPoints[path.TargetPoints.Count - 1]));
            else
                path.TargetPoints.Add(new CMovePath(null));
            selectedPointIndex = path.TargetPoints.Count - 1;
            EditorUtility.SetDirty(path);
        }

        if (path.TargetPoints.Count > 0)
        {
            GUI.enabled = (selectedPointIndex >= 0 && selectedPointIndex < path.TargetPoints.Count);
            if (GUILayout.Button("DELETE Selected Point", GUILayout.Height(24)))
            {
                DeletePoint(selectedPointIndex);
            }
            GUI.enabled = true;
        }
        EditorGUILayout.EndHorizontal();

        if (path.TargetPoints.Count > 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Select Points:", EditorStyles.boldLabel);

            // Fast navigation bar
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("◀ Prev Point", GUILayout.Width(100)))
            {
                SelectPoint(selectedPointIndex > 0 ? selectedPointIndex - 1 : path.TargetPoints.Count - 1, true);
            }

            string currentSelLabel = selectedPointIndex >= 0 ? $"Selected: Point {selectedPointIndex}" : "None Selected";
            GUILayout.Label(currentSelLabel, EditorStyles.centeredGreyMiniLabel);

            if (GUILayout.Button("Next Point ▶", GUILayout.Width(100)))
            {
                SelectPoint(selectedPointIndex < path.TargetPoints.Count - 1 ? selectedPointIndex + 1 : 0, true);
            }
            EditorGUILayout.EndHorizontal();

            // Point list
            Color defaultBg = GUI.backgroundColor;
            for (int i = 0; i < path.TargetPoints.Count; i++)
            {
                bool isSelected = (i == selectedPointIndex);
                if (isSelected)
                {
                    GUI.backgroundColor = new Color(1f, 0.88f, 0.35f, 1f); // Highlight selected
                }
                else
                {
                    GUI.backgroundColor = defaultBg;
                }

                GUILayout.BeginHorizontal("box");
                string buttonText = (isSelected ? "★ Point " : "Point ") + i;
                if (GUILayout.Button(buttonText, GUILayout.Width(100)))
                {
                    SelectPoint(i, true);
                }

                if (GUILayout.Button("SET", GUILayout.Width(50)))
                {
                    Undo.RecordObject(path, "Set Point to Transform");
                    if (path.bLocal)
                        path.TargetPoints[i].Pos = path.transform.localPosition;
                    else
                        path.TargetPoints[i].Pos = path.transform.position;
                    path.TargetPoints[i].Scale = path.transform.localScale;
                    path.TargetPoints[i].Rotation = path.transform.localEulerAngles;
                    EditorUtility.SetDirty(path);
                }

                if (GUILayout.Button("Get", GUILayout.Width(50)))
                {
                    Undo.RecordObject(path.transform, "Get Point to Transform");
                    if (path.bLocal)
                        path.transform.localPosition = path.TargetPoints[i].Pos;
                    else
                        path.transform.position = path.TargetPoints[i].Pos;
                    path.transform.localScale = path.TargetPoints[i].Scale;
                    path.transform.localEulerAngles = path.TargetPoints[i].Rotation;
                }

                // Coordinate label
                GUILayout.Label($"({path.TargetPoints[i].Pos.x:F1}, {path.TargetPoints[i].Pos.y:F1}, {path.TargetPoints[i].Pos.z:F1})", EditorStyles.miniLabel);

                GUILayout.EndHorizontal();
            }
            GUI.backgroundColor = defaultBg;

            EditorGUILayout.Space(10);
            if (path.PathMode == CMOVE_TYPE.TOTAL)
            {
                if (GUILayout.Button("Simulate Path", GUILayout.Height(30)))
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

            if (GUILayout.Button("Stop", GUILayout.Height(24)))
            {
                path.isPlaying = false;
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(path);
        }
    }

    #region Scene GUI & Point Picking

    private Vector3 GetWorldPos(int index)
    {
        if (path == null || path.TargetPoints == null || index < 0 || index >= path.TargetPoints.Count)
            return Vector3.zero;

        Vector3 pos = path.TargetPoints[index].Pos;
        if (path.bLocal && path.transform.parent != null)
        {
            return path.transform.parent.TransformPoint(pos);
        }
        return pos;
    }

    private void SetWorldPos(int index, Vector3 newWorldPos)
    {
        if (path == null || path.TargetPoints == null || index < 0 || index >= path.TargetPoints.Count)
            return;

        if (path.bLocal && path.transform.parent != null)
        {
            path.TargetPoints[index].Pos = path.transform.parent.InverseTransformPoint(newWorldPos);
        }
        else
        {
            path.TargetPoints[index].Pos = newWorldPos;
        }
    }

    private void SelectPoint(int index, bool syncTransform = false)
    {
        if (path == null || path.TargetPoints == null || path.TargetPoints.Count == 0)
        {
            selectedPointIndex = -1;
            return;
        }

        selectedPointIndex = Mathf.Clamp(index, -1, path.TargetPoints.Count - 1);

        if (selectedPointIndex >= 0 && syncTransform)
        {
            Undo.RecordObject(path.transform, "Sync Transform to Point");
            if (path.bLocal)
                path.transform.localPosition = path.TargetPoints[selectedPointIndex].Pos;
            else
                path.transform.position = path.TargetPoints[selectedPointIndex].Pos;
            path.transform.localScale = path.TargetPoints[selectedPointIndex].Scale;
            path.transform.localEulerAngles = path.TargetPoints[selectedPointIndex].Rotation;
        }

        Repaint();
        SceneView.RepaintAll();
    }

    private void FocusPoint(int index)
    {
        if (index < 0 || index >= path.TargetPoints.Count) return;
        Vector3 wp = GetWorldPos(index);
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.Frame(new Bounds(wp, Vector3.one * 2f), false);
        }
    }

    private void DeletePoint(int index)
    {
        if (path == null || path.TargetPoints == null || index < 0 || index >= path.TargetPoints.Count)
            return;

        Undo.RecordObject(path, "Delete Path Point");
        path.TargetPoints.RemoveAt(index);

        if (selectedPointIndex == index)
        {
            selectedPointIndex = Mathf.Clamp(index - 1, 0, path.TargetPoints.Count - 1);
            if (path.TargetPoints.Count == 0) selectedPointIndex = -1;
        }
        else if (selectedPointIndex > index)
        {
            selectedPointIndex--;
        }

        EditorUtility.SetDirty(path);
        Repaint();
        SceneView.RepaintAll();
    }

    private void AddPointAfter(int index)
    {
        if (path == null || path.TargetPoints == null) return;

        Undo.RecordObject(path, "Add Path Point");
        if (index < 0 || index >= path.TargetPoints.Count)
        {
            if (path.TargetPoints.Count > 0)
                path.TargetPoints.Add(new CMovePath(path.TargetPoints[path.TargetPoints.Count - 1]));
            else
                path.TargetPoints.Add(new CMovePath(null));
            selectedPointIndex = path.TargetPoints.Count - 1;
        }
        else
        {
            CMovePath newPt = new CMovePath(path.TargetPoints[index]);
            // Offset slightly in local coordinate
            newPt.Pos += new Vector3(0.5f, 0f, 0f);
            path.TargetPoints.Insert(index + 1, newPt);
            selectedPointIndex = index + 1;
        }

        EditorUtility.SetDirty(path);
        Repaint();
        SceneView.RepaintAll();
    }

    private void SpreadCluster(List<int> clusterIndices)
    {
        if (clusterIndices == null || clusterIndices.Count <= 1 || path == null) return;

        Undo.RecordObject(path, "Spread Clustered Points");
        float offsetStep = 0.5f;
        for (int k = 0; k < clusterIndices.Count; k++)
        {
            int idx = clusterIndices[k];
            float angle = (k / (float)clusterIndices.Count) * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * offsetStep;
            path.TargetPoints[idx].Pos += offset;
        }

        EditorUtility.SetDirty(path);
        Repaint();
        SceneView.RepaintAll();
    }

    private List<List<int>> FindClusters(Vector2[] screenPositions, float threshold = 28f)
    {
        List<List<int>> clusters = new List<List<int>>();
        bool[] visited = new bool[screenPositions.Length];

        for (int i = 0; i < screenPositions.Length; i++)
        {
            if (visited[i]) continue;
            List<int> group = new List<int> { i };
            for (int j = i + 1; j < screenPositions.Length; j++)
            {
                if (visited[j]) continue;
                if (Vector2.Distance(screenPositions[i], screenPositions[j]) < threshold)
                {
                    group.Add(j);
                    visited[j] = true;
                }
            }

            if (group.Count > 1)
            {
                visited[i] = true;
                clusters.Add(group);
            }
        }
        return clusters;
    }

    private void OnSceneGUI()
    {
        path = (MovePathPos)target;
        if (path == null || path.TargetPoints == null || path.TargetPoints.Count == 0) return;

        Event e = Event.current;
        int pointCount = path.TargetPoints.Count;

        // 1. Calculate world and screen positions for all points
        Vector3[] worldPositions = new Vector3[pointCount];
        Vector2[] screenPositions = new Vector2[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            worldPositions[i] = GetWorldPos(i);
            screenPositions[i] = HandleUtility.WorldToGUIPoint(worldPositions[i]);
        }

        // 2. Identify clusters (overlapping or very close points in screen space)
        List<List<int>> clusters = FindClusters(screenPositions, 28f);

        // 3. Handle Keyboard Shortcuts
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.LeftBracket || e.keyCode == KeyCode.Comma)
            {
                SelectPoint(selectedPointIndex > 0 ? selectedPointIndex - 1 : pointCount - 1);
                e.Use();
            }
            else if (e.keyCode == KeyCode.RightBracket || e.keyCode == KeyCode.Period)
            {
                SelectPoint(selectedPointIndex < pointCount - 1 ? selectedPointIndex + 1 : 0);
                e.Use();
            }
            else if (e.keyCode == KeyCode.Tab)
            {
                // Tab cycles within cluster if in one, otherwise cycles through all points
                List<int> myCluster = clusters.Find(c => c.Contains(selectedPointIndex));
                if (myCluster != null && myCluster.Count > 1)
                {
                    int nextIdx = myCluster[(myCluster.IndexOf(selectedPointIndex) + 1) % myCluster.Count];
                    SelectPoint(nextIdx);
                }
                else
                {
                    SelectPoint((selectedPointIndex + 1) % pointCount);
                }
                e.Use();
            }
            else if (e.keyCode == KeyCode.F && selectedPointIndex >= 0)
            {
                FocusPoint(selectedPointIndex);
                e.Use();
            }
            else if (e.keyCode == KeyCode.Escape)
            {
                SelectPoint(-1);
                e.Use();
            }
        }

        // 4. Handle Mouse Click (Selection & Clustered Click-Cycling)
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            Vector2 mousePos = e.mousePosition;
            List<int> hitIndices = new List<int>();
            for (int i = 0; i < pointCount; i++)
            {
                if (Vector2.Distance(mousePos, screenPositions[i]) <= 22f)
                {
                    hitIndices.Add(i);
                }
            }

            if (hitIndices.Count > 0)
            {
                // When multiple points are clustered under the cursor
                if (hitIndices.Count > 1)
                {
                    int curInHit = hitIndices.IndexOf(selectedPointIndex);
                    int nextIndex;
                    if (curInHit >= 0)
                    {
                        nextIndex = hitIndices[(curInHit + 1) % hitIndices.Count];
                    }
                    else
                    {
                        nextIndex = hitIndices[0];
                    }
                    SelectPoint(nextIndex);
                    e.Use();
                }
                else
                {
                    // Single point clicked
                    int clickedIdx = hitIndices[0];
                    if (clickedIdx != selectedPointIndex)
                    {
                        SelectPoint(clickedIdx);
                        e.Use();
                    }
                    // If already selected, do NOT use event so PositionHandle can be dragged
                }
            }
        }

        // 5. Handle Right-Click Context Menu
        if (e.type == EventType.ContextClick)
        {
            Vector2 mousePos = e.mousePosition;
            List<int> contextHits = new List<int>();
            for (int i = 0; i < pointCount; i++)
            {
                if (Vector2.Distance(mousePos, screenPositions[i]) <= 25f)
                {
                    contextHits.Add(i);
                }
            }

            if (contextHits.Count > 0)
            {
                GenericMenu menu = new GenericMenu();
                if (contextHits.Count > 1)
                {
                    menu.AddDisabledItem(new GUIContent($"Clustered Points ({contextHits.Count} points):"));
                    menu.AddSeparator("");
                    foreach (int idx in contextHits)
                    {
                        int targetIdx = idx;
                        string label = $"Select Point {targetIdx} ({path.TargetPoints[targetIdx].Pos.x:F1}, {path.TargetPoints[targetIdx].Pos.y:F1}, {path.TargetPoints[targetIdx].Pos.z:F1})";
                        menu.AddItem(new GUIContent(label), targetIdx == selectedPointIndex, () => SelectPoint(targetIdx));
                    }
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Spread Clustered Points Apart"), false, () => SpreadCluster(contextHits));
                }
                else
                {
                    int targetIdx = contextHits[0];
                    menu.AddItem(new GUIContent($"Select Point {targetIdx}"), targetIdx == selectedPointIndex, () => SelectPoint(targetIdx));
                    menu.AddItem(new GUIContent($"Focus Camera on Point {targetIdx}"), false, () => FocusPoint(targetIdx));
                    menu.AddItem(new GUIContent($"Add Point After Point {targetIdx}"), false, () => AddPointAfter(targetIdx));
                    menu.AddItem(new GUIContent($"Delete Point {targetIdx}"), false, () => DeletePoint(targetIdx));
                }
                menu.ShowAsContext();
                e.Use();
            }
        }

        // 6. Draw Path Connection Line
        if (showPathLines && pointCount > 1)
        {
            if (path.LineMode == CLINE_TYPE.BEZIER)
            {
                for (int i = 0; i < pointCount - 1; i++)
                {
                    path.GetBezierControlPoints(i, out Vector3 c1, out Vector3 c2);
                    Vector3 p0 = worldPositions[i];
                    Vector3 p3 = worldPositions[i + 1];
                    Vector3 worldC1 = path.bLocal && path.transform.parent != null ? path.transform.parent.TransformPoint(c1) : c1;
                    Vector3 worldC2 = path.bLocal && path.transform.parent != null ? path.transform.parent.TransformPoint(c2) : c2;

                    Handles.DrawBezier(p0, p3, worldC1, worldC2, new Color(0.2f, 0.85f, 1f, 0.9f), null, 3.5f);

                    // Direction arrow at mid-point of Bezier curve
                    Vector3 mid = MovePathPos.GetBezierPoint(p0, worldC1, worldC2, p3, 0.5f);
                    Vector3 nextMid = MovePathPos.GetBezierPoint(p0, worldC1, worldC2, p3, 0.55f);
                    Vector3 dir = (nextMid - mid).normalized;
                    if (dir != Vector3.zero)
                    {
                        float arrowSize = HandleUtility.GetHandleSize(mid) * 0.14f;
                        Handles.color = new Color(1f, 1f, 1f, 0.6f);
                        Handles.ArrowHandleCap(0, mid - dir * (arrowSize * 0.5f), Quaternion.LookRotation(dir), arrowSize, EventType.Repaint);
                    }
                }
            }
            else
            {
                Handles.color = new Color(0.2f, 0.8f, 1f, 0.7f);
                Handles.DrawAAPolyLine(3.5f, worldPositions);

                // Draw directional indicators
                Camera cam = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.camera : null;
                for (int i = 0; i < pointCount - 1; i++)
                {
                    Vector3 p1 = worldPositions[i];
                    Vector3 p2 = worldPositions[i + 1];
                    Vector3 dir = (p2 - p1);
                    float dist = dir.magnitude;
                    if (dist > 0.4f)
                    {
                        Vector3 mid = (p1 + p2) * 0.5f;
                        float arrowSize = HandleUtility.GetHandleSize(mid) * 0.14f;
                        Handles.color = new Color(1f, 1f, 1f, 0.6f);
                        Handles.ArrowHandleCap(0, mid - dir.normalized * (arrowSize * 0.5f), Quaternion.LookRotation(dir), arrowSize, EventType.Repaint);
                    }
                }
            }
        }

        // 7. Draw Point Markers & Labels
        Camera activeCam = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.camera : null;
        Vector3 camForward = activeCam != null ? activeCam.transform.forward : Vector3.up;

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 worldPos = worldPositions[i];
            bool isSelected = (i == selectedPointIndex);
            float baseSize = HandleUtility.GetHandleSize(worldPos);
            float handleSize = baseSize * (isSelected ? 0.18f : 0.12f);

            bool isInCluster = clusters.Any(c => c.Contains(i));

            // Set color based on state
            if (isSelected)
            {
                Handles.color = new Color(1f, 0.85f, 0.15f, 1f); // Vibrant Yellow
            }
            else if (i == 0)
            {
                Handles.color = new Color(0.2f, 0.95f, 0.35f, 0.95f); // Green (Start)
            }
            else if (i == pointCount - 1)
            {
                Handles.color = new Color(1f, 0.3f, 0.3f, 0.95f); // Red (End)
            }
            else
            {
                Handles.color = new Color(0.25f, 0.75f, 1f, 0.85f); // Cyan
            }

            // Draw point sphere cap
            Handles.SphereHandleCap(0, worldPos, Quaternion.identity, handleSize, EventType.Repaint);

            // Highlight rings
            if (isSelected)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(worldPos, camForward, handleSize * 0.9f);
            }
            else if (isInCluster)
            {
                // Clustered point indicator halo (Orange)
                Handles.color = new Color(1f, 0.55f, 0f, 0.9f);
                Handles.DrawWireDisc(worldPos, camForward, handleSize * 0.85f);
            }

            // Text Label
            if (!isInCluster || isSelected)
            {
                GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
                labelStyle.normal.textColor = isSelected ? Color.yellow : Color.white;
                labelStyle.fontSize = isSelected ? 12 : 10;
                labelStyle.alignment = TextAnchor.MiddleCenter;

                Vector3 labelOffset = camForward * -0.01f + Vector3.up * (handleSize * 0.8f);
                Handles.Label(worldPos + labelOffset, $"{i}", labelStyle);
            }
        }

        // 8. Draw Position Handles (Only for Selected Point by default)
        if (showAllHandles)
        {
            for (int i = 0; i < pointCount; i++)
            {
                DrawPositionHandleForPoint(i, worldPositions[i]);
            }
        }
        else if (selectedPointIndex >= 0 && selectedPointIndex < pointCount)
        {
            DrawPositionHandleForPoint(selectedPointIndex, worldPositions[selectedPointIndex]);

            // Draw Bezier Angle & Tangent Handle for Selected Point
            if (path.LineMode == CLINE_TYPE.BEZIER)
            {
                Vector3 worldPos = worldPositions[selectedPointIndex];
                Vector3 tangent = path.GetEffectiveTangent(selectedPointIndex);
                Vector3 worldTangent = (path.bLocal && path.transform.parent != null)
                    ? path.transform.parent.TransformVector(tangent)
                    : tangent;

                Vector3 handlePos = worldPos + worldTangent;

                // 1. 노란색 탄젠트 선
                Handles.color = Color.yellow;
                Handles.DrawLine(worldPos, handlePos);

                // 2. 각도 시각화 아크 (원호)
                float handleDist = worldTangent.magnitude;
                if (handleDist > 0.05f)
                {
                    float angleDeg = Mathf.Atan2(worldTangent.y, worldTangent.x) * Mathf.Rad2Deg;
                    float arcRadius = Mathf.Min(handleDist * 0.45f, HandleUtility.GetHandleSize(worldPos) * 0.7f);
                    Handles.color = new Color(1f, 0.9f, 0.2f, 0.2f);
                    Handles.DrawSolidArc(worldPos, camForward, Vector3.right, angleDeg, arcRadius);
                    Handles.color = new Color(1f, 0.9f, 0.2f, 0.8f);
                    Handles.DrawWireArc(worldPos, camForward, Vector3.right, angleDeg, arcRadius);
                }

                // 3. 각도 조절 핸들 구체
                float tangentSize = HandleUtility.GetHandleSize(handlePos) * 0.12f;
                Handles.color = new Color(1f, 0.85f, 0.15f, 1f);

                EditorGUI.BeginChangeCheck();
                #if UNITY_2022_1_OR_NEWER
                Vector3 newHandlePos = Handles.FreeMoveHandle(handlePos, tangentSize, Vector3.zero, Handles.SphereHandleCap);
                #else
                Vector3 newHandlePos = Handles.FreeMoveHandle(handlePos, Quaternion.identity, tangentSize, Vector3.zero, Handles.SphereHandleCap);
                #endif
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(path, "Adjust Bezier Angle");
                    Vector3 newLocalHandlePos = (path.bLocal && path.transform.parent != null)
                        ? path.transform.parent.InverseTransformPoint(newHandlePos)
                        : newHandlePos;

                    Vector3 delta = newLocalHandlePos - path.TargetPoints[selectedPointIndex].Pos;
                    float newAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                    float newStrength = delta.magnitude;

                    CMovePath selPt = path.TargetPoints[selectedPointIndex];
                    selPt.bCustomBezier = true;
                    selPt.angle = newAngle;
                    selPt.strength = Mathf.Max(0.01f, newStrength);
                    selPt.Tangent = delta;

                    EditorUtility.SetDirty(path);
                    Repaint();
                }

                // 4. 각도 수치 라벨
                CMovePath curPoint = path.TargetPoints[selectedPointIndex];
                float displayAngle = curPoint.bCustomBezier ? curPoint.angle : Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
                float displayStrength = curPoint.bCustomBezier ? curPoint.strength : tangent.magnitude;
                GUIStyle angleStyle = new GUIStyle(EditorStyles.boldLabel) {
                    normal = { textColor = Color.yellow },
                    fontSize = 11
                };
                Handles.Label(handlePos + Vector3.up * (tangentSize * 1.5f), $"∠ {displayAngle:F1}° (Str: {displayStrength:F1})", angleStyle);
            }
        }

        // 9. GUI Overlay (Cluster Badges & Floating HUD)
        Handles.BeginGUI();
        DrawClusterBadges(clusters, screenPositions);

        if (showHUD)
        {
            DrawSceneHUD(clusters, pointCount);
        }
        Handles.EndGUI();
    }

    private void DrawPositionHandleForPoint(int index, Vector3 worldPos)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(path, "Move Path Point");
            SetWorldPos(index, newWorldPos);
            EditorUtility.SetDirty(path);
        }
    }

    private void DrawClusterBadges(List<List<int>> clusters, Vector2[] screenPositions)
    {
        if (clusters == null || clusters.Count == 0) return;

        foreach (var cluster in clusters)
        {
            if (cluster.Count <= 1) continue;

            // Calculate average screen position for the cluster
            Vector2 avgScreen = Vector2.zero;
            foreach (int idx in cluster)
            {
                avgScreen += screenPositions[idx];
            }
            avgScreen /= cluster.Count;

            // Check if on screen
            if (avgScreen.x < 0 || avgScreen.x > Screen.width || avgScreen.y < 0 || avgScreen.y > Screen.height)
                continue;

            float buttonWidth = 32f;
            float totalWidth = 6f + cluster.Count * (buttonWidth + 3f);
            Rect groupRect = new Rect(avgScreen.x + 16f, avgScreen.y - 18f, totalWidth, 24f);

            GUI.Box(groupRect, GUIContent.none, EditorStyles.helpBox);

            float bx = groupRect.x + 3f;
            Color prevBg = GUI.backgroundColor;

            for (int c = 0; c < cluster.Count; c++)
            {
                int ptIdx = cluster[c];
                bool isCurSelected = (ptIdx == selectedPointIndex);

                if (isCurSelected)
                    GUI.backgroundColor = new Color(1f, 0.85f, 0.2f, 1f); // Selected
                else
                    GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 0.9f);

                Rect btnRect = new Rect(bx, groupRect.y + 2f, buttonWidth, 20f);
                if (GUI.Button(btnRect, $"#{ptIdx}", EditorStyles.miniButton))
                {
                    SelectPoint(ptIdx);
                }

                bx += buttonWidth + 3f;
            }
            GUI.backgroundColor = prevBg;
        }
    }

    private void DrawSceneHUD(List<List<int>> clusters, int pointCount)
    {
        float hudWidth = 250f;
        Rect area = new Rect(12, 12, hudWidth, hudCollapsed ? 26 : 148);

        GUILayout.BeginArea(area, EditorStyles.helpBox);

        // Header
        GUILayout.BeginHorizontal();
        string headerText = $"MovePath: {path.gameObject.name} ({pointCount} pts)";
        if (GUILayout.Button(hudCollapsed ? "▶ " + headerText : "▼ " + headerText, EditorStyles.boldLabel))
        {
            hudCollapsed = !hudCollapsed;
        }
        GUILayout.EndHorizontal();

        if (!hudCollapsed)
        {
            // Navigation row
            GUILayout.BeginHorizontal();
            GUI.enabled = pointCount > 0;
            if (GUILayout.Button("◀ Prev", GUILayout.Width(60)))
            {
                SelectPoint(selectedPointIndex > 0 ? selectedPointIndex - 1 : pointCount - 1);
            }

            string selText = selectedPointIndex >= 0 ? $"Point {selectedPointIndex}" : "Select...";
            if (GUILayout.Button(selText, EditorStyles.popup))
            {
                GenericMenu jumpMenu = new GenericMenu();
                for (int i = 0; i < pointCount; i++)
                {
                    int idx = i;
                    jumpMenu.AddItem(new GUIContent($"Point {idx} ({path.TargetPoints[idx].Pos.x:F1}, {path.TargetPoints[idx].Pos.y:F1})"), idx == selectedPointIndex, () => SelectPoint(idx));
                }
                jumpMenu.ShowAsContext();
            }

            if (GUILayout.Button("Next ▶", GUILayout.Width(60)))
            {
                SelectPoint(selectedPointIndex < pointCount - 1 ? selectedPointIndex + 1 : 0);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            // Clustered points quick switcher
            if (selectedPointIndex >= 0 && clusters != null)
            {
                List<int> myCluster = clusters.Find(c => c.Contains(selectedPointIndex));
                if (myCluster != null && myCluster.Count > 1)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("⚠️ Near Points:", EditorStyles.miniLabel, GUILayout.Width(85));
                    foreach (int cIdx in myCluster)
                    {
                        if (cIdx == selectedPointIndex) continue;
                        int targetIdx = cIdx;
                        if (GUILayout.Button($"#{targetIdx}", EditorStyles.miniButton, GUILayout.Width(30)))
                        {
                            SelectPoint(targetIdx);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            // Point Action buttons
            GUILayout.BeginHorizontal();
            GUI.enabled = selectedPointIndex >= 0;
            if (GUILayout.Button("Focus (F)", EditorStyles.miniButton))
            {
                FocusPoint(selectedPointIndex);
            }
            if (GUILayout.Button("+ Add", EditorStyles.miniButton))
            {
                AddPointAfter(selectedPointIndex);
            }
            if (GUILayout.Button("- Del", EditorStyles.miniButton))
            {
                DeletePoint(selectedPointIndex);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            // View Toggles row
            GUILayout.BeginHorizontal();
            showPathLines = GUILayout.Toggle(showPathLines, "Line", EditorStyles.miniButton, GUILayout.Width(42));
            showAllHandles = GUILayout.Toggle(showAllHandles, "All", EditorStyles.miniButton, GUILayout.Width(38));

            string modeLabel = path.LineMode == CLINE_TYPE.BEZIER ? "Bezier" : "Linear";
            if (GUILayout.Button(modeLabel, EditorStyles.miniButton, GUILayout.Width(52)))
            {
                Undo.RecordObject(path, "Toggle Line Mode");
                path.LineMode = (path.LineMode == CLINE_TYPE.LINE) ? CLINE_TYPE.BEZIER : CLINE_TYPE.LINE;
                EditorUtility.SetDirty(path);
                SceneView.RepaintAll();
            }

            if (selectedPointIndex >= 0 && GUILayout.Button("Deselect", EditorStyles.miniButton))
            {
                SelectPoint(-1);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }

    #endregion

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

        float delta = Time.realtimeSinceStartup - deltatimes;
        deltatimes = Time.realtimeSinceStartup;

        if (path.isPlaying)
        {
            path._Update(delta);
            EditorUtility.SetDirty(path);
        }
    }
}
