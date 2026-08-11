#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace StageEventSystem.Editor
{
    public class CreateEventWizard : EditorWindow
    {
        private string eventName = "FreeX";
        private string targetFolder = "Assets/Script";

        [MenuItem("Stage Event/Create New Event", false, 10)]
        public static void OpenWindow()
        {
            CreateEventWizard window = GetWindow<CreateEventWizard>("Create Event Wizard");
            window.minSize = new Vector2(350, 180);
            window.maxSize = new Vector2(500, 200);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Create Reusable Stage Event Scripts", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Event Name Input

            eventName = EditorGUILayout.TextField("Event Name", eventName);

            // Target Folder Selector

            EditorGUILayout.BeginHorizontal();
            targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Folder to Save Scripts", targetFolder, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Convert absolute path to relative path starting with Assets/
                    if (selectedPath.Contains(Application.dataPath))
                    {
                        targetFolder = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Please select a folder inside the Assets directory.", "OK");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", GUILayout.Height(30), GUILayout.Width(100)))
            {
                Close();
            }
            if (GUILayout.Button("Create Event", GUILayout.Height(30), GUILayout.Width(130)))
            {
                if (ValidateAndGenerate())
                {
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private bool ValidateAndGenerate()
        {
            // Validate name format
            if (string.IsNullOrEmpty(eventName))
            {
                EditorUtility.DisplayDialog("Error", "Event Name cannot be empty.", "OK");
                return false;
            }

            if (!Regex.IsMatch(eventName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                EditorUtility.DisplayDialog("Error", "Invalid Event Name. Must be a valid C# class name (no spaces, starts with letter).", "OK");
                return false;
            }

            // Create target folder path
            string fullFolderPath = Path.Combine(Application.dataPath, targetFolder.Substring("Assets".Length).TrimStart('/', '\\'));
            if (!Directory.Exists(fullFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(fullFolderPath);
                }
                catch (System.Exception ex)
                {
                    EditorUtility.DisplayDialog("Error", $"Failed to create folder: {ex.Message}", "OK");
                    return false;
                }
            }

            // File paths
            string eventScriptPath = Path.Combine(fullFolderPath, $"{eventName}.cs");
            string charInfoScriptPath = Path.Combine(fullFolderPath, $"{eventName}CharInfo.cs");

            // Overwrite checks
            if (File.Exists(eventScriptPath) || File.Exists(charInfoScriptPath))
            {
                bool overwrite = EditorUtility.DisplayDialog("Confirm Overwrite",

                    $"Files for event '{eventName}' already exist in this folder. Do you want to overwrite them?",

                    "Yes", "No");
                if (!overwrite) return false;
            }

            // Generate event class content
            string eventCode = @"using System.Collections;
using UnityEngine;
using StageEventSystem;

public class {eventName} : BaseStepEvent
{
    // TODO: Add game-specific components (e.g. video players, UI controllers, animators)

    public override void _Init(int point = 0, int data1 = 0, int data2 = 0, int data3 = 0)
    {
        base._Init(point, data1, data2, data3);
        // Custom initialization logic goes here
    }

    protected override bool IsCharacterValidForStep(CharacterDefinition character)
    {
        var info = character as {eventName}CharInfo;
        if (info != null)
        {
            // Example custom logic: filter character based on prize threshold
            return remainingPrizeMoney >= info.minPrizeMoney;
        }
        return false;
    }

    public override bool SetupStage()
    {
        var info = currentCharacter as {eventName}CharInfo;
        if (info == null)
        {
            Debug.LogError($""[{name}] Character info not found or invalid format for {currentCharacter}"");
            return false;
        }

        // Setup custom visual playlist or animations using info properties
        remainingAttacks = GetRandomAttackCount();
        return true;
    }

    public override IEnumerator _Main_Proc()
    {
        if (currentCharacter == null)
        {
            currentCharacter = SelectNextCharacter(currentStep);
        }

        bool isFinished = false;
        while (!isFinished)
        {
            if (!SetupStage()) yield break;

            Debug.Log($""[{name}] Step {currentStep} started with {currentCharacter.displayName}"");

            // Attack simulation loop
            while (remainingAttacks > 0)
            {
                remainingAttacks--;
                bool isVictory = CheckVictoryCondition();

                if (isVictory)
                {
                    isFinished = TransitionToNextStep();
                    break;
                }
                else
                {
                    yield return new WaitForSeconds(1.0f);
                }
            }

            if (!isFinished && remainingAttacks == 0)
            {
                HandleStepFailure();
                isFinished = true;
            }
        }

        yield return null;
    }
}
".Replace("{eventName}", eventName);

            // Generate character info ScriptableObject class content
            string charInfoCode = @"using UnityEngine;
using StageEventSystem;

[CreateAssetMenu(fileName = ""{eventName}CharInfo"", menuName = ""Stage Event/{eventName} Character Info"")]
public class {eventName}CharInfo : CharacterDefinition
{
    [Header(""Custom Event Settings"")]
    [Tooltip(""Minimum prize required to unlock/activate this character in a step"")]
    public int minPrizeMoney;

    // TODO: Add custom prefabs, animation states, or audio files here
}
".Replace("{eventName}", eventName);

            // Write files to disk
            try
            {
                File.WriteAllText(eventScriptPath, eventCode);
                File.WriteAllText(charInfoScriptPath, charInfoCode);
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to write files: {ex.Message}", "OK");
                return false;
            }

            // Refresh Unity Database to trigger compilation
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Successfully created event scripts:\n- {eventName}.cs\n- {eventName}CharInfo.cs", "OK");
            return true;
        }
    }
}
#endif
