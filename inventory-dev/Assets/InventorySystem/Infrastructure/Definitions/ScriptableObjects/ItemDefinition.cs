using UnityEngine;

namespace GB.Inventory.Infrastructure.Definitions
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "GB/Inventory/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string definitionId;   // único
        [SerializeField] private ItemTypeDefinition type;   // referencia al tipo
        [SerializeField] private string[] tags;         // "Consumable", "Tool", "Material"

        [Header("Effect (optional)")]
        [SerializeField] private string effectKey;
        [SerializeField] private TextAsset payloadJson; // Opcional: datos para el efecto

        public string DefinitionId => definitionId;
        public ItemTypeDefinition Type => type;
        public string[] Tags => tags ?? System.Array.Empty<string>();

        public string EffectKey => effectKey;
        public TextAsset PayloadJson => payloadJson;
    }
}