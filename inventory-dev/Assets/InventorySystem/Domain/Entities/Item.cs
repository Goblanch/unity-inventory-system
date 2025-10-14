using System;

namespace GB.Inventory.Domain
{
    public sealed class Item
    {
        public string Id { get; }
        public string DefinitionId { get; }

        public Item(string id, string definitionId)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            DefinitionId = definitionId ?? throw new ArgumentNullException(nameof(definitionId));
        }
    }
}   


