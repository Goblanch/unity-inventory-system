namespace GB.Inventory.Domain.Abstractions
{
    /// <summary>
    /// Proveedor de metadatos por definición (tipo y tags) para políticas data-driven.
    /// En esta fase, es una interfaz; más adelantes lo implementaremos con SO
    /// </summary>
    public interface IItemMetadataProvider
    {
        ItemMeta Get(string definitionId);
    }
    
    public struct ItemMeta
    {
        public string DefinitionId;
        public string TypeId;
        public string[] Tags;
    }
}