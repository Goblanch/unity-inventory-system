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
            return new ItemMeta {
                DefinitionId = definitionId,
                TypeId = "Unknown",
                Tags = System.Array.Empty<string>()
            };
        }
    }
}


