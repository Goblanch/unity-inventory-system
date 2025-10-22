using GB.Inventory.Infrastructure.Definitions;

namespace GB.Inventory.Infrastructure.Providers
{
    /// <summary>
    /// Proveedor interno para el registry: devuelve effectKey, payload y allowedPhases.
    /// No expone interfaz pública: se usa por el EffectRegistry en infraestructura
    /// </summary>
    public sealed class SoItemEffectInfoProvider
    {
        private readonly ItemDatabase _items;

        public SoItemEffectInfoProvider(ItemDatabase items) { _items = items; }

        public bool TryGet(string definitionId, out string effectKey, out object payload, out string[] allowedPhases)
        {
            effectKey = null;
            payload = null;
            allowedPhases = null;

            if (_items != null && _items.TryGet(definitionId, out var def) && def != null)
            {
                effectKey = string.IsNullOrWhiteSpace(def.EffectKey) ? null : def.EffectKey;
                payload = def.PayloadJson != null ? def.PayloadJson : null;
                allowedPhases = def.AllowedPhases ?? System.Array.Empty<string>();
                return true;
            }

            return false;
        }
    }
}