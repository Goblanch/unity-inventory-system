using UnityEngine;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Application;
using GB.Inventory.Application.Abstractions;
using GB.Inventory.Infrastructure.Definitions;
using GB.Inventory.Infrastructure.Providers;

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

        // Runtime (public getters para acceder desde otros scripts)
        public IInventory Inventory { get; private set; }
        public IInventoryService Service { get; private set; }

        void Awake()
        {
            // Providers desde SO
            var itemMetaProvider = new SoItemMetadataProvider(itemDatabase);
            var slotProfileProvider = new SoSlotProfileProvider(slotProfileDatabase);

            // Políticas por defecto
            var stacking = new SimpleStackingPolicy(defaultMaxStack);
            var filter = new SimpleSlotFilterPolicy(slotProfileProvider, itemMetaProvider, stacking);

            // Modelo + Service
            var model = new InventoryModel(initialCapacity, stacking, filter, defaultSlotProfileId);
            Inventory = model;
            Service = new InventoryService(model);

            Debug.Log("[InventoryInstaller] Inventario inicializado");
        }
    }
}
