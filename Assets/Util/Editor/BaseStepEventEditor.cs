#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StageEventSystem.Editor
{
    [CustomEditor(typeof(BaseStepEvent), true)]
    public class BaseStepEventEditor : UnityEditor.Editor
    {
        private SerializedProperty stepDatasProp;

        protected virtual void OnEnable()
        {
            stepDatasProp = serializedObject.FindProperty("stepDatas");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 1. Draw all fields except stepDatas
            DrawPropertiesExcluding(serializedObject, "stepDatas");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Step Configuration (Custom Editor)", EditorStyles.boldLabel);

            // Auto detect Candidate Data Type for this event (e.g. FreeXEvent -> FreeXStepCharacterWeight)
            Type candidateType = GetCandidateTypeForEvent(target);

            // 2. Draw stepDatas list
            if (stepDatasProp != null)
            {
                for (int i = 0; i < stepDatasProp.arraySize; i++)
                {
                    SerializedProperty stepDataProp = stepDatasProp.GetArrayElementAtIndex(i);
                    SerializedProperty stepNumberProp = stepDataProp.FindPropertyRelative("stepNumber");
                    SerializedProperty candidatesProp = stepDataProp.FindPropertyRelative("candidates");

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    // Step header line
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"[Step {stepNumberProp.intValue}]", EditorStyles.boldLabel, GUILayout.Width(70));

                    stepNumberProp.intValue = EditorGUILayout.IntField("Step Number", stepNumberProp.intValue, GUILayout.Width(350));

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Delete Step", GUILayout.Width(90)))
                    {
                        stepDatasProp.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        return;
                    }
                    EditorGUILayout.EndHorizontal();

                    // Step candidates and weight information
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Candidate Characters:", EditorStyles.miniLabel);

                    EditorGUI.indentLevel++;
                    for (int j = 0; j < candidatesProp.arraySize; j++)
                    {
                        SerializedProperty candidateProp = candidatesProp.GetArrayElementAtIndex(j);
                        if (candidateProp.managedReferenceValue == null || candidateProp.managedReferenceValue.GetType() != candidateType)
                        {
                            StepCharacterWeight oldVal = candidateProp.managedReferenceValue as StepCharacterWeight;
                            StepCharacterWeight newVal = (StepCharacterWeight)Activator.CreateInstance(candidateType);
                            if (oldVal != null)
                            {
                                newVal.character = oldVal.character;
                                newVal.weight = oldVal.weight;
                                newVal.Step = oldVal.Step;
                            }
                            else
                            {
                                newVal.Step = stepNumberProp.intValue;
                                newVal.weight = 1;
                            }
                            candidateProp.managedReferenceValue = newVal;
                        }

                        SerializedProperty characterProp = candidateProp.FindPropertyRelative("character");
                        SerializedProperty weightProp = candidateProp.FindPropertyRelative("weight");
                        SerializedProperty stepProp = candidateProp.FindPropertyRelative("Step");

                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        EditorGUILayout.BeginHorizontal();

                        // Character asset field selector
                        if (characterProp != null)
                            EditorGUILayout.PropertyField(characterProp, GUIContent.none, GUILayout.Width(180));

                        EditorGUILayout.LabelField("Weight", GUILayout.Width(50));
                        if (weightProp != null)
                            weightProp.intValue = EditorGUILayout.IntField(weightProp.intValue, GUILayout.Width(50));

                        EditorGUILayout.LabelField("Next Step", GUILayout.Width(70));
                        if (stepProp != null)
                            stepProp.intValue = EditorGUILayout.IntField(stepProp.intValue, GUILayout.Width(50));

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("-", GUILayout.Width(25)))
                        {
                            candidatesProp.DeleteArrayElementAtIndex(j);
                            serializedObject.ApplyModifiedProperties();
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            EditorGUI.indentLevel--;
                            EditorGUILayout.EndVertical();
                            return;
                        }
                        EditorGUILayout.EndHorizontal();

                        // Safely render any extra fields defined in derived candidateType (e.g., NextBinkName)
                        if (candidateProp.managedReferenceValue != null)
                        {
                            Type valType = candidateProp.managedReferenceValue.GetType();
                            var fields = valType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            foreach (var field in fields)
                            {
                                if (field.Name == "character" || field.Name == "weight" || field.Name == "Step")
                                    continue;

                                SerializedProperty childProp = candidateProp.FindPropertyRelative(field.Name);
                                if (childProp != null)
                                {
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(childProp, true);
                                    EditorGUI.indentLevel--;
                                }
                            }
                        }

                        EditorGUILayout.EndVertical();
                    }

                    if (GUILayout.Button("+ Add Candidate", GUILayout.Width(130)))
                    {
                        AddCandidate(candidatesProp, stepNumberProp.intValue, candidateType);
                    }
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("+ Add New Step", GUILayout.Height(30)))
                {
                    int index = stepDatasProp.arraySize;
                    stepDatasProp.InsertArrayElementAtIndex(index);
                    SerializedProperty newStep = stepDatasProp.GetArrayElementAtIndex(index);
                    newStep.FindPropertyRelative("stepNumber").intValue = index + 1;
                    newStep.FindPropertyRelative("candidates").ClearArray();
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Logic Simulator", EditorStyles.boldLabel);
                if (GUILayout.Button("Open Simulator Window", GUILayout.Height(30)))
                {
                    BaseEventSimulatorWindow.ShowWindow((BaseStepEvent)target);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private Type GetCandidateTypeForEvent(UnityEngine.Object targetObj)
        {
            if (targetObj != null)
            {
                string eventName = targetObj.GetType().Name;
                if (eventName.EndsWith("Event"))
                {
                    string prefix = eventName.Substring(0, eventName.Length - 5);
                    foreach (var t in TypeCache.GetTypesDerivedFrom<StepCharacterWeight>())
                    {
                        if (!t.IsAbstract && !t.IsInterface && t.Name.StartsWith(prefix))
                        {
                            return t;
                        }
                    }
                }
            }

            // Fallback to first non-abstract derived class in project
            foreach (var t in TypeCache.GetTypesDerivedFrom<StepCharacterWeight>())
            {
                if (!t.IsAbstract && !t.IsInterface)
                {
                    return t;
                }
            }

            return typeof(StepCharacterWeight);
        }

        private void AddCandidate(SerializedProperty candidatesProp, int stepNumber, Type candidateType)
        {
            int index = candidatesProp.arraySize;
            candidatesProp.InsertArrayElementAtIndex(index);
            SerializedProperty newCandidate = candidatesProp.GetArrayElementAtIndex(index);
            StepCharacterWeight instance = (StepCharacterWeight)Activator.CreateInstance(candidateType);
            instance.Step = stepNumber;
            instance.weight = 1;
            newCandidate.managedReferenceValue = instance;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
