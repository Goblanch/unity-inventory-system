using UnityEngine;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Application;
using GB.Inventory.Application.Abstractions;
using GB.Inventory.Infrastructure.Definitions;
using GB.Inventory.Infrastructure.Providers;
using GB.Inventory.Infrastructure.Effects;

namespace GB.Inventory.Infrastructure.Installers
{
    /// <summary>
    /// Arrástralo a un GameObject en escena y asigna las bases de datos.
    /// Crea InventoryModel + InventoryService con políticas por defecto.
    /// </summary>
    public class InventoryInstaller : MonoBehaviour
    {
        [Header("Initial Config")]
        [SerializeField] private int initialCapacity = 3;
        [SerializeField] private string defaultSlotProfileId = "Consumables";

        [Header("Databases (ScriptableObjects)")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private SlotProfileDatabase slotProfileDatabase;

        [Header("Policies (defaults)")]
        [SerializeField] private int defaultMaxStack = 99;
        [Tooltip("Si se desactiva, las fases se ignoran aunque tengan AllowedPhases configurados")]
        [SerializeField] private bool enableUsagePhases = true;

        // Public API
        public IInventory Inventory { get; private set; }
        public IInventoryService Service { get; private set; }

        void Awake()
        {
            // Providers desde SO
            var itemMetaProvider = new SoItemMetadataProvider(itemDatabase);
            var slotProfileProvider = new SoSlotProfileProvider(slotProfileDatabase);
            var effecrInfoProvider = new SoItemEffectInfoProvider(itemDatabase);

            // Políticas por defecto
            var stacking = new SimpleStackingPolicy(defaultMaxStack);
            var filter = new SimpleSlotFilterPolicy(slotProfileProvider, itemMetaProvider, stacking);
            IUsagePhasePolicy phasePolicy = enableUsagePhases ? new DefaultUsagePhasePolicy() : null;

            // Modelo
            var model = new InventoryModel(initialCapacity, stacking, filter, defaultSlotProfileId);
            Inventory = model;

            // EffectRegistry: registramos aquí los efectos disponibles
            var registry = new EffectRegistry(effecrInfoProvider).RegisterEffect("test", new TestEffect());

            // Service
            Service = new InventoryService(model);

            Debug.Log("[InventoryInstaller] Inventario inicializado");
        }
    }
}
