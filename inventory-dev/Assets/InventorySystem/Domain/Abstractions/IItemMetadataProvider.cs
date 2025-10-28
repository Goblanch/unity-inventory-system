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
    
    [Serializable]
    public sealed class ItemMeta
    {
        public string DefinitionId;
        public string TypeId;
        public string[] Tags;

        // Override stacking per item
        public bool HasStackOverride;
        public int MaxStack; // valid if HasStackOverride == true;
    }
}