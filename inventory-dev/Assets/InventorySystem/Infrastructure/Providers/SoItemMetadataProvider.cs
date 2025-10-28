using System;
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
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public bool TryGet(string definitionId, out ItemMeta meta)
        {
            meta = null;

            if (string.IsNullOrWhiteSpace(definitionId)) return false;

            ItemDefinition def;
            if (_items.TryGet(definitionId, out def) && def != null)
            {
                var typeId = def.Type != null ? def.Type.TypeId : "Unknown";
                var tags = def.Tags ?? Array.Empty<string>();

                meta = new ItemMeta
                {
                    DefinitionId = def.DefinitionId,
                    TypeId = typeId,
                    Tags = tags,
                    HasStackOverride = def.OverrideStacking,
                    MaxStack = def.OverrideStacking ? (def.MaxStack > 0 ? def.MaxStack : 1) : 0
                };
                return true;
            }

            return false;
        }

        // Compatibilidad: algunos sitios pueden seguir usando Get
        public ItemMeta Get(string definitionId)
        {
            ItemMeta meta;
            return TryGet(definitionId, out meta) ? meta : null;
        }
        
        public bool TryGetEffectKey(string definitionId, out string effectKey, out object payload)
        {
            effectKey = null;
            payload = null;

            if (string.IsNullOrWhiteSpace(definitionId)) return false;

            ItemDefinition def;
            if (_items.TryGet(definitionId, out def) && def != null)
            {
                effectKey = string.IsNullOrWhiteSpace(def.EffectKey) ? null : def.EffectKey;

                // Si usas TextAsset para el JSON, devolvemos el texto
                if (def.PayloadJson != null)
                    payload = def.PayloadJson.text;

                return true;
            }

            return false;
        }
    }
}


