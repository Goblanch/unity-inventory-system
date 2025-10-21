using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Infrastructure.Definitions;

namespace GB.Inventory.Infrastructure.Providers
{
    /// <summary>
    /// Traduce ItemDefinition (SO) -> ItemMeta (Dominio)
    /// </summary>
    public sealed class SoItemMetadataProvider : IItemMetadataProvider
    {
        private readonly ItemDatabase _items;

        public SoItemMetadataProvider(ItemDatabase items)
        {
            _items = items;
        }

        public ItemMeta Get(string definitionId)
        {
            if (_items != null && _items.TryGet(definitionId, out var def) && def != null)
            {
                var typeId = def.Type != null ? def.Type.TypeId : "Unknown";
                var tags = def.Tags ?? System.Array.Empty<string>();
                return new ItemMeta
                {
                    DefinitionId = def.DefinitionId,
                    TypeId = typeId,
                    Tags = tags
                };
            }
            return new ItemMeta
            {
                DefinitionId = definitionId,
                TypeId = "Unknown",
                Tags = System.Array.Empty<string>()
            };
        }
        
        public bool TryGetEffectKey(string definitionId, out string effectKey, out object payload)
        {
            effectKey = null;
            payload = null;
            if (_items != null && _items.TryGet(definitionId, out var def) && def != null)
            {
                effectKey = string.IsNullOrWhiteSpace(def.EffectKey) ? null : def.EffectKey;

                // Payload simple: devolvemos el texto del JSON si existe
                if (def.PayloadJson != null) payload = def.PayloadJson.text;

                return true;
            }

            return true;
        }
    }
}


