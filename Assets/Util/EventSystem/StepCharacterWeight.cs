using System;
using UnityEngine;

namespace StageEventSystem
{
    [Serializable]
    public class StepCharacterWeight
    {
        [Tooltip("The character definition candidate")]
        public CharacterDefinition character;

        [Tooltip("Relative weight for this character to appear in the step")]
        public int weight = 1;

        [Tooltip("The step number to transition to when this character is selected")]
        public int Step = 1;
    }
}
