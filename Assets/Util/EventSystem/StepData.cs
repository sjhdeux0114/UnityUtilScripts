using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageEventSystem
{
    [Serializable]
    public class StepData
    {
        public int stepNumber;

        [SerializeReference]
        public List<StepCharacterWeight> candidates = new List<StepCharacterWeight>();
    }
}
