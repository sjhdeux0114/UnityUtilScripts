using UnityEngine;

namespace StageEventSystem
{
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Event System/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [Tooltip("Unique identifier for this character")]
        public int characterId;

        [Tooltip("User-friendly display name")]
        public string displayName;
    }
}
