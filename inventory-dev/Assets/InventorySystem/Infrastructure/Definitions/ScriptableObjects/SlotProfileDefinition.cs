using UnityEngine;

namespace GB.Inventory.Infrastructure.Definitions
{
    [CreateAssetMenu(fileName = "SlotProfileDefinition", menuName = "GB/Inventory/Slot Profile")]
    public sealed class SlotProfileDefinition : ScriptableObject
    {
        [SerializeField] private string profileId = "Default";

        [Header("Filters")]
        [SerializeField] private string[] allowedTypes;     // si vacío => cualquiera
        [SerializeField] private string[] requiredTags;     // deben estar todos
        [SerializeField] private string[] bannedTags;       // ninguno debe estar

        [Header("Stack Overrides")]
        [SerializeField] private bool hasStackableOverride;
        [SerializeField] private bool stackableOverride;
        [SerializeField] private int maxStackOverride;  // >0 para aplicar

        public string ProfileId => profileId;
        public string[] AllowedTypes => allowedTypes ?? System.Array.Empty<string>();
        public string[] RequiredTags => requiredTags ?? System.Array.Empty<string>();
        public string[] BannedTags   => bannedTags   ?? System.Array.Empty<string>();
        public bool HasStackableOverride => hasStackableOverride;
        public bool StackableOverride    => stackableOverride;
        public int  MaxStackOverride     => maxStackOverride;
    }
}