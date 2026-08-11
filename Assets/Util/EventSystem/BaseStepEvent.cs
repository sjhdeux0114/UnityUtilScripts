using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StageEventSystem
{
    public enum EventResult
    {
        Pending,
        Clear,
        Fail
    }

    public abstract class BaseStepEvent : MonoBehaviour
    {
        public const int START_STEP = 1;

        [Header("Stage Configuration")]
        [Tooltip("List of configuration data for each step/stage")]
        public List<StepData> stepDatas = new List<StepData>();

        [Header("State")]
        [Tooltip("The currently active character")]
        public CharacterDefinition currentCharacter;

        [Tooltip("The currently active character")]
        public CharacterDefinition NextCharacter;

        [Tooltip("The current active step number")]
        public int currentStep = START_STEP;

        [Tooltip("Number of remaining attacks in the current stage")]
        public int remainingAttacks = 0;

        [Tooltip("Total prize pool point allocated for this event")]
        public int totalPrizeMoney;

        [Tooltip("Current remaining prize money pool")]
        public int remainingPrizeMoney;

        [Tooltip("Maximum number of steps, determined from stepDatas")]
        public int maxSteps = 5;

        [Header("Attack Count Configuration")]
        [Tooltip("Minimum number of attacks allowed per step")]
        public int minAttackCount = 3;

        [Tooltip("Maximum number of attacks allowed per step")]
        public int maxAttackCount = 5;

        [HideInInspector]
        public int IPerAttack = 0;
        [HideInInspector]
        public int IPerAttackDelta = 0;

        public virtual bool CheckVictoryCondition()
        {
            return false;
        }
        public void Start_Event()
        {
            StartCoroutine(START());
        }


        public IEnumerator START()
        {
            yield return _Open_Proc();
            yield return _Main_Proc();
            yield return _End_Proc();
        }

        public virtual bool SetupStage()
        {
            return false;
        }

        public virtual IEnumerator _Open_Proc()
        {
            yield return null;
        }

        public virtual IEnumerator _Main_Proc()
        {
            yield return null;
        }

        public virtual IEnumerator _End_Proc()
        {
            yield return null;
        }

        public EventResult result = EventResult.Pending;

        public virtual void _Init(int point = 0, int data1 = 0, int data2 = 0, int data3 = 0)
        {
            result = EventResult.Pending;
            remainingPrizeMoney = point;
            totalPrizeMoney = point;
            currentStep = START_STEP;
            NextCharacter = null;

            if (stepDatas != null && stepDatas.Count > 0)
            {
                maxSteps = stepDatas[stepDatas.Count - 1].stepNumber;
            }

            currentCharacter = SelectNextCharacter(currentStep);
        }

        /// <summary>
        /// Selects a character for the given step based on the step configuration and custom validation rules.
        /// </summary>
        public virtual CharacterDefinition SelectNextCharacter(int nextStep)
        {
            StepData stepData = GetStepData(nextStep);
            if (stepData != null && stepData.candidates != null && stepData.candidates.Count > 0)
            {
                List<StepCharacterWeight> validCandidates = new List<StepCharacterWeight>();
                int totalWeight = 0;

                foreach (var candidateWeight in stepData.candidates)
                {
                    if (candidateWeight.character != null && IsCharacterValidForStep(candidateWeight.character))
                    {
                        validCandidates.Add(candidateWeight);
                        totalWeight += Mathf.Max(1, candidateWeight.weight);
                    }
                }

                if (validCandidates.Count > 0)
                {
                    int randomVal = Random.Range(0, totalWeight);
                    int currentSum = 0;

                    foreach (var info in validCandidates)
                    {
                        currentSum += Mathf.Max(1, info.weight);
                        if (randomVal < currentSum)
                        {
                            currentStep = info.Step;
                            return info.character;
                        }
                    }
                    currentStep = validCandidates[0].Step;
                    return validCandidates[0].character; // fallback
                }
            }
            return null;
        }
        public virtual bool IsNextOK(int nextStep)
        {
            StepData stepData = GetStepData(nextStep);
            if (stepData != null && stepData.candidates != null && stepData.candidates.Count > 0)
            {
                List<StepCharacterWeight> validCandidates = new List<StepCharacterWeight>();
                int totalWeight = 0;

                foreach (var candidateWeight in stepData.candidates)
                {
                    if (candidateWeight.character != null && IsCharacterValidForStep(candidateWeight.character))
                    {
                        validCandidates.Add(candidateWeight);
                        totalWeight += Mathf.Max(1, candidateWeight.weight);
                    }
                }

                if (validCandidates.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Custom validation rule for characters. Can be overridden in derived classes to filter based on prize money, unlocks, etc.
        /// </summary>
        protected virtual bool IsCharacterValidForStep(CharacterDefinition character)
        {
            return true;
        }

        public StepData GetStepData(int step)
        {
            if (stepDatas == null) return null;
            return stepDatas.Find(x => x.stepNumber == step);
        }

        /// <summary>
        /// Transitions to the next step. Returns true if the event has successfully ended.
        /// </summary>
        public virtual bool TransitionToNextStep(CharacterDefinition nextChar = null)
        {
            if (currentStep < maxSteps)
            {
                currentStep++;
                currentCharacter = nextChar ?? SelectNextCharacter(currentStep);
                return false; // Event continues
            }
            else
            {
                result = EventResult.Clear;
                return true; // Event cleared successfully
            }
        }

        /// <summary>
        /// Handles failure when attack count is fully depleted.
        /// </summary>
        public virtual void HandleStepFailure()
        {
            result = EventResult.Fail;
        }

        protected virtual int GetRandomAttackCount()
        {
            int rand = Random.Range(minAttackCount, maxAttackCount + 1);
            IPerAttackDelta = (int)(100.0f / (float)rand) + 2;
            IPerAttack = IPerAttackDelta;
            return rand;
        }
    }
}
