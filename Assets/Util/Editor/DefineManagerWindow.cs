#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class DefineManagerWindow : EditorWindow
{
    private const string PrefsKey = "DefineManagerWindow_ManagedDefines";
    private List<string> managedDefines = new List<string>();
    private string newDefineInput = "";

    [MenuItem("Tools/Define Manager")]
    public static void ShowWindow()
    {
        GetWindow<DefineManagerWindow>("Define Manager");
    }

    private void OnEnable()
    {
        LoadManagedDefines();
    }

    private void LoadManagedDefines()
    {
        if (EditorPrefs.HasKey(PrefsKey))
        {
            string saved = EditorPrefs.GetString(PrefsKey);
            managedDefines = saved.Split(';', System.StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        else
        {
            // 여기에 자주 사용하는 Define 목록을 적어주세요
            managedDefines = new List<string>
            {
                "USE_TEST_MODE",
                "OIDD_NONE",
                "SIM_TEST",
                "CASH_VER"
            };
            SaveManagedDefines();
        }
    }

    private void SaveManagedDefines()
    {
        EditorPrefs.SetString(PrefsKey, string.Join(";", managedDefines));
    }

    private void OnGUI()
    {
        if (managedDefines == null)
        {
            LoadManagedDefines();
        }

        // --- 1. Player Settings 제어 영역 ---
        GUILayout.Label("Player Settings", EditorStyles.boldLabel);

        // 스크린샷에 있던 'Use Player Log' 체크박스 제어
        bool useLog = EditorGUILayout.Toggle("Use Player Log", PlayerSettings.usePlayerLog);
        if (useLog != PlayerSettings.usePlayerLog)
        {
            PlayerSettings.usePlayerLog = useLog;
            Debug.Log($"[DefineManager] Use Player Log set to: {useLog}");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // 구분선
        EditorGUILayout.Space();

        // --- 2. Scripting Define Symbols 제어 영역 ---
        GUILayout.Label("Scripting Define Symbols", EditorStyles.boldLabel);

        // 현재 선택된 빌드 타겟 그룹 가져오기 (PC, Android, iOS 등)
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

        // 현재 설정된 Define 목록 가져오기
        string definesString = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        List<string> currentDefines = definesString.Split(';', System.StringSplitOptions.RemoveEmptyEntries).ToList();

        bool isChanged = false;
        int removeIndex = -1;

        for (int i = 0; i < managedDefines.Count; i++)
        {
            string define = managedDefines[i];
            bool hasDefine = currentDefines.Contains(define);

            EditorGUILayout.BeginHorizontal();
            
            bool toggle = EditorGUILayout.Toggle(define, hasDefine);
            if (toggle != hasDefine)
            {
                if (toggle) currentDefines.Add(define);
                else currentDefines.Remove(define);
                isChanged = true;
            }

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                removeIndex = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0)
        {
            string removedDefine = managedDefines[removeIndex];
            managedDefines.RemoveAt(removeIndex);
            SaveManagedDefines();

            if (currentDefines.Contains(removedDefine))
            {
                currentDefines.Remove(removedDefine);
                isChanged = true;
            }
        }

        if (isChanged)
        {
            // 변경사항 적용
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, string.Join(";", currentDefines.Distinct()));
            Debug.Log("Defines updated!");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // 구분선
        EditorGUILayout.Space();

        // --- 3. Define 추가 영역 ---
        GUILayout.Label("Add Managed Define", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        newDefineInput = EditorGUILayout.TextField("Define Symbol", newDefineInput);
        if (GUILayout.Button("Add", GUILayout.Width(60)))
        {
            if (!string.IsNullOrWhiteSpace(newDefineInput))
            {
                string newDefine = newDefineInput.Trim();
                if (!managedDefines.Contains(newDefine))
                {
                    managedDefines.Add(newDefine);
                    SaveManagedDefines();
                    newDefineInput = "";
                    GUI.FocusControl(null); // 입력 포커스 해제
                }
                else
                {
                    Debug.LogWarning($"[DefineManager] '{newDefine}' is already in the managed list.");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif