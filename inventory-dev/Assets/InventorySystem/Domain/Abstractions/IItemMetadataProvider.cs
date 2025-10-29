using System;

namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Proveedor de metadatos por definición (tipo y tags) para políticas data-driven.
    /// En esta fase, es una interfaz; más adelantes lo implementaremos con SO
    /// </summary>
    public interface IItemMetadataProvider
    {
        ItemMeta Get(string definitionId);

        bool TryGet(string definitionId, out ItemMeta meta);
    }

    /// <summary>
    /// Read-only metadata about an item definition as used by the inventory domain:
    /// - DefinitionId: unique ID.
    /// - TypeId: item type.
    /// - Tags: classification labels.
    /// - HasStackableOverride/MaxStack: optional per-item stack limit override.
    /// </summary>
    [Serializable]
    public sealed class ItemMeta
    {
        public string DefinitionId;
        public string TypeId;
        public string[] Tags;

        /// <summary>
        /// Indicates wether this item defines its own MaxStack override
        /// </summary>
        public bool HasStackOverride;
        /// <summary>Per-item max stack (valid only if <see cref="HasStackOverride"/> is true and &gt; 0).</summary>
        public int MaxStack; // valid if HasStackOverride == true;
    }
}