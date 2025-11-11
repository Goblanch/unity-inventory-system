using UnityEngine;
using GB.Inventory.Application.Abstractions;
using GB.Inventory.Infrastructure.Persistence;

namespace GB.Inventory.Infrastructure.Installers
{
    /// <summary>
    /// Optional scene component to auto-load on Start and auto-save on quit.
    /// Place it alongside InventoryInstaller (or anywhere in scene).
    /// </summary>
    public sealed class InventoryPersistenceInstaller : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private InventoryInstaller inventoryInstaller;

        [Header("Settings")]
        [SerializeField] private string slotName = "default";
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private bool autoSaveOnQuit = true;
        [SerializeField] private bool dontDestroyOnLoad = false;

        private IInventorySaveService _saveService;
        private IInventoryService _svc;

        void Awake()
        {
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            _saveService = new JsonInventorySaveService();
        }

        void Start()
        {
            if (inventoryInstaller == null)
                inventoryInstaller = FindFirstObjectByType<InventoryInstaller>();

            _svc = inventoryInstaller ? inventoryInstaller.Service : null;

            if (_svc == null)
            {
                Debug.LogWarning("[InventoryPersistence] InventoryService not found.");
                return;
            }

            if (autoLoadOnStart && _saveService.Exists(slotName))
            {
                if (!_saveService.Load(_svc, slotName, out var why))
                    Debug.LogWarning($"[InventoryPersistence] Auto load failed: {why}");
                else
                    Debug.Log("[InventoryPersistence] Auto load OK.");
            }
        }

        void OnApplicationQuit()
        {
            if (!autoSaveOnQuit || _svc == null) return;

            if (!_saveService.Save(_svc, slotName, out var why))
                Debug.LogWarning($"[InventoryPersistence] Auto save failed: {why}");
            else
                Debug.Log("[InventoryService] Auto save OK.");
        }

        public void SaveNow()
        {
            if (_svc == null) return;
            if (!_saveService.Save(_svc, slotName, out var why))
                Debug.LogWarning($"[InventoryPersistence] SaveNow failed: {why}");
        }

        public void LoadNow()
        {
            if (_svc == null) return;
            if (!_saveService.Load(_svc, slotName, out var why))
                Debug.LogWarning($"[InventoryPersistence] LoadNow failed: {why}");
        }

        public void SetSlotName(string name) => slotName = name;
    }
}