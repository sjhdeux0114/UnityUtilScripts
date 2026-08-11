#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StageEventSystem.Editor
{
    public class BaseEventSimulatorWindow : EditorWindow
    {
        private BaseStepEvent targetEvent;
        private int virtualPrizeMoney = 0;
        private bool isSimulating = false;
        private List<string> logs = new List<string>();
        private Vector2 scrollPos;

        // Auto simulation results
        private bool isMultiRun = false;
        private int simStartingPrizeMoney;
        private List<SimRunResult> simRunResults = new List<SimRunResult>();
        private List<CharacterDefinition> simAppearedCharacters = new List<CharacterDefinition>();

        private struct SimRunResult
        {
            public int runIndex;
            public List<CharacterDefinition> appearedCharacters;
            public int remainingPrizeMoney;
            public EventResult finalResult;
        }

        [MenuItem("Window/Event System Simulator")]
        public static void OpenWindow()
        {
            BaseEventSimulatorWindow window = GetWindow<BaseEventSimulatorWindow>("Event Simulator");
            window.Show();
        }

        public static void ShowWindow(BaseStepEvent target)
        {
            BaseEventSimulatorWindow window = GetWindow<BaseEventSimulatorWindow>("Event Simulator");
            window.targetEvent = target;
            window.Show();
        }

        private void OnEnable()
        {
            if (targetEvent == null)
            {
                targetEvent = FindAnyObjectByType<BaseStepEvent>();
            }
        }

        private void OnGUI()
        {
            // Keyboard space and return detection
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
                if (isSimulating)
                {
                    AdvanceSimulation();
                    e.Use();
                }
                else
                {
                    logs.Clear();
                    StartSimulation(false);
                    e.Use();
                }
            }
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
            {
                StartSimulation(true);
                e.Use();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Generic Event System Simulator (Space/Return Test)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            targetEvent = EditorGUILayout.ObjectField("Target Event Script", targetEvent, typeof(BaseStepEvent), true) as BaseStepEvent;
            virtualPrizeMoney = EditorGUILayout.IntField("Virtual Starting Prize", virtualPrizeMoney);

            if (targetEvent == null)
            {
                EditorGUILayout.HelpBox("Please assign an active BaseStepEvent component in the scene.", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Simulation / Reset", GUILayout.Height(25)))
            {
                StartSimulation(false);
            }
            if (GUILayout.Button("Run 10x Auto Simulation", GUILayout.Height(25)))
            {
                StartSimulation(true);
            }
            if (isSimulating && GUILayout.Button("Next Attack (Space)", GUILayout.Height(25)))
            {
                AdvanceSimulation();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (isSimulating || logs.Count > 0)
            {
                if (!isMultiRun)
                {
                    DrawStatusPanel();
                }
                if (!isSimulating)
                {
                    DrawSummaryPanel();
                }
                DrawLogsPanel();
            }
        }

        private void StartSimulation(bool bAutoPlay = false)
        {
            if (targetEvent == null) return;
            int retTestMoney = virtualPrizeMoney;
            if (virtualPrizeMoney <= 0)
            {
                if (Random.Range(0, 100) < 30) retTestMoney = 20000;
                else if (Random.Range(0, 100) < 60) retTestMoney = Random.Range(20, 91) * 1000;
                else retTestMoney = Random.Range(20, 301) * 1000;
            }

            simStartingPrizeMoney = retTestMoney;
            simRunResults.Clear();
            logs.Clear();

            if (bAutoPlay)
            {
                isMultiRun = true;
                isSimulating = false;

                for (int i = 0; i < 10; i++)
                {
                    targetEvent._Init(retTestMoney);
                    List<CharacterDefinition> runCharacters = new List<CharacterDefinition>();
                    if (targetEvent.currentCharacter != null)
                    {
                        runCharacters.Add(targetEvent.currentCharacter);
                    }

                    targetEvent.SetupStage();

                    int safetyCounter = 0;
                    const int MAX_SAFETY_ITERATIONS = 1000;
                    bool runSimulating = true;

                    while (runSimulating)
                    {
                        if (targetEvent.remainingAttacks <= 0)
                        {
                            runSimulating = false;
                            break;
                        }

                        targetEvent.remainingAttacks--;
                        bool isVictory = targetEvent.CheckVictoryCondition();

                        if (isVictory)
                        {
                            bool isFinished = targetEvent.TransitionToNextStep();
                            if (!isFinished)
                            {
                                targetEvent.currentCharacter = targetEvent.NextCharacter;
                                targetEvent.NextCharacter = null;
                                targetEvent.SetupStage();
                                if (targetEvent.currentCharacter != null)
                                {
                                    runCharacters.Add(targetEvent.currentCharacter);
                                }
                            }
                            else
                            {
                                runSimulating = false;
                            }
                        }
                        else
                        {
                            if (targetEvent.remainingAttacks == 0)
                            {
                                targetEvent.HandleStepFailure();
                                runSimulating = false;
                            }
                        }

                        safetyCounter++;
                        if (safetyCounter >= MAX_SAFETY_ITERATIONS)
                        {
                            targetEvent.HandleStepFailure();
                            runSimulating = false;
                            break;
                        }
                    }

                    simRunResults.Add(new SimRunResult
                    {
                        runIndex = i + 1,
                        appearedCharacters = runCharacters,
                        remainingPrizeMoney = targetEvent.remainingPrizeMoney,
                        finalResult = targetEvent.result
                    });
                }

                logs.Add($"<color=red>====== [10x Auto Simulation Complete] Start Prize: {simStartingPrizeMoney:N0} ======</color>");
            }
            else
            {
                isMultiRun = false;
                targetEvent._Init(virtualPrizeMoney);
                simAppearedCharacters.Clear();
                if (targetEvent.currentCharacter != null)
                {
                    simAppearedCharacters.Add(targetEvent.currentCharacter);
                }
                targetEvent.SetupStage();

                logs.Add($"<color=red>====== [Simulation Started] Start Prize: {targetEvent.remainingPrizeMoney:N0} ======</color>");
                logs.Add($"<b>Step {targetEvent.currentStep} Entered: {GetColoredCharacterLabel(targetEvent.currentCharacter)} Selected, Attacks: {targetEvent.remainingAttacks}</b>");

                isSimulating = true;
            }
            Repaint();
        }

        private void AdvanceSimulation()
        {
            if (!isSimulating || targetEvent == null) return;
            if (targetEvent.remainingAttacks <= 0) return;

            targetEvent.remainingAttacks--;
            int attPerSave = targetEvent.IPerAttack;

            bool isVictory = targetEvent.CheckVictoryCondition();

            if (isVictory)
            {
                bool isFinished = targetEvent.TransitionToNextStep();
                if (!isFinished)
                {
                    targetEvent.currentCharacter = targetEvent.NextCharacter;
                    targetEvent.NextCharacter = null;
                    targetEvent.SetupStage();
                    if (targetEvent.currentCharacter != null)
                    {
                        simAppearedCharacters.Add(targetEvent.currentCharacter);
                    }

                    logs.Add($"[Result] Transition to Next Step: <b><color=green>SUCCESS</color></b> (Next: {GetColoredCharacterLabel(targetEvent.currentCharacter)})");
                    logs.Add($"-> <b>Attack 2 (Slow Motion)</b> performed, Remaining Attacks: {targetEvent.remainingAttacks}, Chance: {attPerSave}%");
                    logs.Add($"----------------------------------------------------");
                    logs.Add($"<b>Step {targetEvent.currentStep} Entered: {GetColoredCharacterLabel(targetEvent.currentCharacter)} Selected, Attacks: {targetEvent.remainingAttacks}</b>");
                }
                else
                {
                    logs.Add($"[Result] Transition to Next Step: <b><color=green>FINAL SUCCESS</color></b>");
                    logs.Add($"-> <b>Attack 2 (Slow Motion)</b> performed, Remaining Attacks: {targetEvent.remainingAttacks}, Chance: {attPerSave}%");
                    logs.Add($"<color=green><b>====== [Final Success] Step {targetEvent.maxSteps} Cleared! ======</b></color>");
                    isSimulating = false;
                }
            }
            else
            {
                logs.Add($"[Result] Transition to Next Step: <b><color=yellow>PENDING</color></b>");
                logs.Add($"-> <b>Attack 1 (Normal Attack)</b> performed, Remaining Attacks: {targetEvent.remainingAttacks}, Chance: {attPerSave}%");

                if (targetEvent.remainingAttacks == 0)
                {
                    logs.Add($"<color=red><b>====== [Final Failure] Fully depleted attacks without clearing step. ======</b></color>");
                    targetEvent.HandleStepFailure();
                    isSimulating = false;
                }
            }

            scrollPos = new Vector2(0, logs.Count * 22);
            Repaint();
        }

        private void DrawStatusPanel()
        {
            if (targetEvent == null) return;

            EditorGUILayout.Space();
            GUILayout.Label("Simulation Real-Time Status", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Simulation State:", isSimulating ? "<color=green><b>Running (Spacebar Active)</b></color>" : "<color=red><b>Ended</b></color>", new GUIStyle(EditorStyles.label) { richText = true });
            EditorGUILayout.LabelField("Current Step:", $"Step {targetEvent.currentStep} / {targetEvent.maxSteps}");
            EditorGUILayout.LabelField("Current Character:", GetColoredCharacterLabel(targetEvent.currentCharacter), new GUIStyle(EditorStyles.label) { richText = true });
            EditorGUILayout.LabelField("Remaining Attacks:", $"{targetEvent.remainingAttacks} attacks");
            EditorGUILayout.LabelField("Remaining Prize:", GetColoredPrizeMoneyLabel(targetEvent.remainingPrizeMoney), new GUIStyle(EditorStyles.label) { richText = true });
            EditorGUILayout.EndVertical();
        }

        private void DrawSummaryPanel()
        {
            if (isSimulating) return;
            if (logs.Count == 0) return;

            EditorGUILayout.Space();
            GUILayout.Label("Simulation Summary Results", EditorStyles.boldLabel);

            if (isMultiRun)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Start Prize Pool:", GetColoredPrizeMoneyLabel(simStartingPrizeMoney), new GUIStyle(EditorStyles.label) { richText = true });
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("No.", EditorStyles.boldLabel, GUILayout.Width(45));
                GUILayout.Label("Appeared Characters (By Step)", EditorStyles.boldLabel, GUILayout.Width(550));
                GUILayout.Label("Remaining Prize", EditorStyles.boldLabel, GUILayout.Width(100));
                GUILayout.Label("Result", EditorStyles.boldLabel, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();

                for (int i = 0; i < simRunResults.Count; i++)
                {
                    var run = simRunResults[i];
                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Label($"{run.runIndex}", GUILayout.Width(45));

                    List<string> charLabels = new List<string>();
                    for (int j = 0; j < run.appearedCharacters.Count; j++)
                    {
                        charLabels.Add($"S{j + 1}:{GetColoredCharacterLabel(run.appearedCharacters[j])}");
                    }
                    string charListStr = string.Join(" → ", charLabels);
                    GUILayout.Label(charListStr, new GUIStyle(EditorStyles.label) { richText = true }, GUILayout.Width(550));

                    GUILayout.Label(GetColoredPrizeMoneyLabel(run.remainingPrizeMoney), new GUIStyle(EditorStyles.label) { richText = true }, GUILayout.Width(100));

                    string resultStr = run.finalResult == EventResult.Clear
                        ? "<color=green><b>CLEAR</b></color>"
                        : "<color=red><b>FAIL</b></color>";
                    GUILayout.Label(resultStr, new GUIStyle(EditorStyles.label) { richText = true }, GUILayout.Width(100));

                    EditorGUILayout.EndHorizontal();
                }

                int clearCount = 0;
                for (int i = 0; i < simRunResults.Count; i++)
                {
                    if (simRunResults[i].finalResult == EventResult.Clear) clearCount++;
                }
                float successRate = (clearCount / (float)simRunResults.Count) * 100f;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Success Statistics:", $"<b>Total runs: {simRunResults.Count}</b> | Clear: <color=green><b>{clearCount}</b></color> | Success Rate: <b>{successRate:F1}%</b>", new GUIStyle(EditorStyles.label) { richText = true });
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Start Prize:", GetColoredPrizeMoneyLabel(simStartingPrizeMoney), new GUIStyle(EditorStyles.label) { richText = true });
                EditorGUILayout.LabelField("Remaining Prize:", GetColoredPrizeMoneyLabel(targetEvent.remainingPrizeMoney), new GUIStyle(EditorStyles.label) { richText = true });

                List<string> charLabels = new List<string>();
                for (int i = 0; i < simAppearedCharacters.Count; i++)
                {
                    charLabels.Add($"Step{i + 1}: {GetColoredCharacterLabel(simAppearedCharacters[i])}");
                }
                string charSequence = string.Join(" → ", charLabels);
                EditorGUILayout.LabelField("Character Sequence:", charSequence, new GUIStyle(EditorStyles.label) { richText = true });

                string finalStr = targetEvent.result == EventResult.Clear
                    ? "<color=green><b>SUCCESS (CLEAR)</b></color>"
                    : "<color=red><b>FAIL</b></color>";
                EditorGUILayout.LabelField("Final Result:", finalStr, new GUIStyle(EditorStyles.label) { richText = true });
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawLogsPanel()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Detailed Simulation Logs", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
            GUIStyle logStyle = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };

            for (int i = 0; i < logs.Count; i++)
            {
                EditorGUILayout.LabelField(logs[i], logStyle);
            }
            EditorGUILayout.EndScrollView();
        }

        private string GetColoredCharacterLabel(CharacterDefinition character)
        {
            if (character == null) return "<color=grey>None</color>";

            string name = character.displayName;
            if (string.IsNullOrEmpty(name)) name = character.name;

            string colorCode = "white";
            switch (character.characterId)
            {
                case 0:
                    colorCode = "#4A90E2"; // Sky Blue
                    break;
                case 1:
                    colorCode = "#00BCD4"; // Cyan
                    break;
                case 2:
                    colorCode = "#FF69B4"; // Hot Pink
                    break;
                case 3:
                    colorCode = "#BA55D3"; // Medium Orchid
                    break;
                case 4:
                    colorCode = "#00E5FF"; // Aqua/Ice
                    break;
                case 5:
                    colorCode = "#FF4500"; // OrangeRed
                    break;
                case 6:
                    colorCode = "#4CAF50"; // Lime Green
                    break;
                case 7:
                    colorCode = "#FFEB3B"; // Bright Yellow
                    break;
                case 8:
                    colorCode = "#9C27B0"; // Deep Purple
                    break;
                case 9:
                    colorCode = "#FF5722"; // Deep Orange
                    break;
                default:
                    colorCode = "white";
                    break;
            }

            return $"<color={colorCode}><b>{name}</b></color>";
        }

        private string GetColoredPrizeMoneyLabel(int prizeMoney)
        {
            string colorCode;
            if (prizeMoney < 10000)
            {
                colorCode = "#A0A0A0"; // Gray
            }
            else if (prizeMoney < 20000)
            {
                colorCode = "#4A90E2"; // Soft Blue
            }
            else if (prizeMoney < 30000)
            {
                colorCode = "#2E7D32"; // Green
            }
            else if (prizeMoney < 50000)
            {
                colorCode = "#E67E22"; // Orange
            }
            else if (prizeMoney < 100000)
            {
                colorCode = "#9B59B6"; // Purple
            }
            else if (prizeMoney < 200000)
            {
                colorCode = "#E91E63"; // Pink
            }
            else
            {
                colorCode = "#FFD700"; // Gold (20만원 이상)
            }

            return $"<color={colorCode}><b>{prizeMoney:N0}원</b></color>";
        }
    }
}
#endif
