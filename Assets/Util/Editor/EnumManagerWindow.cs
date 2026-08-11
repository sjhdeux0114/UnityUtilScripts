using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

public class EnumManagerWindow : EditorWindow
{
    private List<EnumData> enums = new List<EnumData>();
    private string newEnumName = "";
    private Vector2 scrollPosition;

    private const string EnumFilePath = "Assets/EnumManager.cs";

    [MenuItem("Window/Animation Enum Manager")]
    public static void ShowWindow()
    {
        GetWindow<EnumManagerWindow>("Animation Enum Manager");
    }

    [System.Serializable]
    private class EnumData
    {
        public string enumName;
        public List<string> entries = new List<string>();
        public string newEntryName = ""; // Temporary field for GUI
        public bool isExpanded = true; // Toggle state
    }

    private void OnEnable()
    {
        LoadExistingEnums();
    }

    private void OnGUI()
    {
        GUILayout.Label("Enum Manager", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Section for adding a new enum
        EditorGUILayout.LabelField("Add New Enum", EditorStyles.boldLabel);
        newEnumName = EditorGUILayout.TextField("Enum Name", newEnumName);
        if (GUILayout.Button("Add New Enum") && !string.IsNullOrWhiteSpace(newEnumName))
        {
            AddEnum(newEnumName);
            newEnumName = "";
        }

        GUILayout.Space(20);

        // Use temporary variables to track items to remove or move
        string enumToRemove = null;

        // Scroll view for existing enums
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

        for (int i = 0; i < enums.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");

            // Add a foldout toggle for each enum
            EditorGUILayout.BeginHorizontal();
            enums[i].isExpanded = EditorGUILayout.Foldout(enums[i].isExpanded, enums[i].enumName, true, EditorStyles.foldoutHeader);
            if (GUILayout.Button("Remove Enum", GUILayout.Width(100)))
            {
                enumToRemove = enums[i].enumName;
            }
            EditorGUILayout.EndHorizontal();

            if (enums[i].isExpanded)
            {
                // Section for adding/removing entries within an enum
                enums[i].newEntryName = EditorGUILayout.TextField("New Entry", enums[i].newEntryName);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Entry") && !string.IsNullOrWhiteSpace(enums[i].newEntryName))
                {
                    AddEnumEntry(enums[i].enumName, enums[i].newEntryName);
                    enums[i].newEntryName = "";
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                // Display and manage existing entries
                string entryToRemove = null;
                string moveUpEntry = null;   // 순서 변경: 위로 이동할 항목
                string moveDownEntry = null; // 순서 변경: 아래로 이동할 항목

                for (int j = 0; j < enums[i].entries.Count; j++)
                {
                    EditorGUILayout.BeginHorizontal();

                    // 항목 이름 표시 (항목을 더 넓게 보이게 설정)
                    EditorGUILayout.LabelField(enums[i].entries[j], GUILayout.ExpandWidth(true));

                    // 순서 변경 버튼 추가
                    if (GUILayout.Button("↑", GUILayout.Width(20)))
                    {
                        moveUpEntry = enums[i].entries[j];
                    }
                    if (GUILayout.Button("↓", GUILayout.Width(20)))
                    {
                        moveDownEntry = enums[i].entries[j];
                    }

                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        entryToRemove = enums[i].entries[j];
                    }
                    // Add new 'Add' button for adding incremented entry
                    if (GUILayout.Button("Add", GUILayout.Width(60)))
                    {
                        AddIncrementedEntry(enums[i].enumName, enums[i].entries[j]);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // 항목 제거 로직 실행
                if (entryToRemove != null)
                {
                    RemoveEnumEntry(enums[i].enumName, entryToRemove);
                }

                // 항목 순서 변경 로직 실행
                if (moveUpEntry != null)
                {
                    MoveEnumEntry(enums[i].enumName, moveUpEntry, -1);
                }
                if (moveDownEntry != null)
                {
                    MoveEnumEntry(enums[i].enumName, moveDownEntry, 1);
                }
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        EditorGUILayout.EndScrollView();

        if (enumToRemove != null)
        {
            RemoveEnum(enumToRemove);
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Save Enums to File"))
        {
            SaveEnumsToFile();
        }
    }

    private void AddEnum(string enumName)
    {
        if (!IsValidIdentifier(enumName))
        {
            Debug.LogError($"'{enumName}' is not a valid C# identifier. Please use a valid name (e.g., no spaces or special characters).");
            return;
        }

        if (enums.Any(e => e.enumName == enumName))
        {
            Debug.LogWarning($"Enum '{enumName}' already exists.");
            return;
        }
        enums.Add(new EnumData { enumName = enumName });
    }

    private void RemoveEnum(string enumName)
    {
        enums.RemoveAll(e => e.enumName == enumName);
    }

    private void AddEnumEntry(string enumName, string entry)
    {
        if (!IsValidIdentifier(entry))
        {
            Debug.LogError($"'{entry}' is not a valid C# identifier. Please use a valid name (e.g., no spaces or special characters).");
            return;
        }

        var enumData = enums.FirstOrDefault(e => e.enumName == enumName);
        if (enumData != null)
        {
            if (enumData.entries.Contains(entry))
            {
                Debug.LogWarning($"Entry '{entry}' already exists in '{enumName}'.");
                return;
            }
            enumData.entries.Add(entry);
        }
    }

    // ⭐ 새로 추가된 메서드: Enum 항목의 순서를 변경합니다.
    private void MoveEnumEntry(string enumName, string entry, int direction) // direction: -1 for Up, 1 for Down
    {
        var enumData = enums.FirstOrDefault(e => e.enumName == enumName);
        if (enumData == null) return;

        int currentIndex = enumData.entries.IndexOf(entry);
        int newIndex = currentIndex + direction;

        // 경계 검사: 리스트 범위 내에 있는지 확인
        if (newIndex >= 0 && newIndex < enumData.entries.Count)
        {
            // 리스트에서 항목의 위치를 교환
            string itemToMove = enumData.entries[currentIndex];
            enumData.entries.RemoveAt(currentIndex);
            enumData.entries.Insert(newIndex, itemToMove);
        }
    }


    private void AddIncrementedEntry(string enumName, string baseEntry)
    {
        var enumData = enums.FirstOrDefault(e => e.enumName == enumName);
        if (enumData == null) return;

        // Use a regular expression to find numbers at the end of the string
        Regex regex = new Regex(@"(\d+)$");
        Match match = regex.Match(baseEntry);
        string newEntry = baseEntry;

        if (match.Success)
        {
            // If a number is found, increment it
            string prefix = baseEntry.Substring(0, baseEntry.Length - match.Length);
            int number = int.Parse(match.Value);
            number++;
            newEntry = prefix + number;
        }
        else
        {
            // If no number is found, append "1"
            newEntry = baseEntry + "1";
        }

        // Keep incrementing until a unique name is found
        while (enumData.entries.Contains(newEntry) && IsValidIdentifier(newEntry))
        {
            match = regex.Match(newEntry);
            if (match.Success)
            {
                string prefix = newEntry.Substring(0, newEntry.Length - match.Length);
                int number = int.Parse(match.Value);
                number++;
                newEntry = prefix + number;
            }
            else
            {
                newEntry += "1";
            }
        }

        if (IsValidIdentifier(newEntry))
        {
            enumData.entries.Add(newEntry);
        }
        else
        {
            Debug.LogError($"Failed to generate a valid new entry name for '{baseEntry}'.");
        }
    }

    private void RemoveEnumEntry(string enumName, string entry)
    {
        var enumData = enums.FirstOrDefault(e => e.enumName == enumName);
        if (enumData != null)
        {
            enumData.entries.Remove(entry);
        }
    }

    private void LoadExistingEnums()
    {
        enums.Clear();
        if (File.Exists(EnumFilePath))
        {
            string fileContent = File.ReadAllText(EnumFilePath);
            var lines = fileContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            EnumData currentEnum = null;
            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();
                // 'public static class CustomEnums'가 포함된 라인은 무시합니다.
                if (trimmedLine.Contains("public static class CustomEnums") || trimmedLine == "{") continue;

                if (trimmedLine.StartsWith("public enum"))
                {
                    // 이전 Enum이 처리 중이었다면 추가합니다.
                    if (currentEnum != null) enums.Add(currentEnum);

                    string enumName = trimmedLine.Split(' ')[2];
                    currentEnum = new EnumData { enumName = enumName };
                }
                else if (trimmedLine.StartsWith("}"))
                {
                    // Enum의 끝이라면 현재 Enum을 추가하고 초기화합니다.
                    if (currentEnum != null)
                    {
                        enums.Add(currentEnum);
                        currentEnum = null;
                    }
                }
                else if (currentEnum != null)
                {
                    // Enum 항목 처리
                    // 값 할당 (예: Entry = 5)는 무시하고 항목 이름만 가져옵니다.
                    string entry = trimmedLine.Split('=')[0].Replace(",", "").Trim();

                    if (!string.IsNullOrWhiteSpace(entry) && IsValidIdentifier(entry))
                    {
                        currentEnum.entries.Add(entry);
                    }
                }
            }
        }
        if (!enums.Any())
        {
            // Add a default animation enum if none exist
            enums.Add(new EnumData { enumName = "AnimationName", entries = new List<string> { "Idle", "Run", "Jump" } });
        }
    }

    private void SaveEnumsToFile()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// This file is managed by the EnumManagerWindow editor script.");
        sb.AppendLine("// Do not edit this file manually, as your changes will be overwritten.");
        sb.AppendLine();
        sb.AppendLine("public static class CustomEnums");
        sb.AppendLine("{");

        foreach (var enumData in enums)
        {
            // Enum 이름이 C# 식별자로 유효한지 다시 확인
            if (!IsValidIdentifier(enumData.enumName)) continue;

            sb.AppendLine($"\tpublic enum {enumData.enumName}");
            sb.AppendLine("\t{");

            for (int i = 0; i < enumData.entries.Count; i++)
            {
                // 항목이 C# 식별자로 유효한지 다시 확인
                if (!IsValidIdentifier(enumData.entries[i])) continue;

                sb.Append($"\t\t{enumData.entries[i]}");
                if (i < enumData.entries.Count - 1)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }
            sb.AppendLine("\t}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        // 디렉토리가 없다면 생성
        string directory = Path.GetDirectoryName(EnumFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(EnumFilePath, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log("Enums saved successfully!");
    }

    private bool IsValidIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // An identifier must start with a letter or an underscore
        if (!char.IsLetter(name[0]) && name[0] != '_')
            return false;

        // All subsequent characters must be letters, digits, or underscores
        for (int i = 1; i < name.Length; i++)
        {
            if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                return false;
        }

        return true;
    }
}