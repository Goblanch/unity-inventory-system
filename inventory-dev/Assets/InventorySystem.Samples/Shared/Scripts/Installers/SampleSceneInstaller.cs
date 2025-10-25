using UnityEngine;
using GB.Inventory.Infrastructure.Installers;
using GB.Inventory.Samples.UI;

namespace GB.Inventory.Samples
{
    public class SampleSceneInstaller : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private InventoryInstaller inventoryInstaller;
        [SerializeField] private InventoryHUDPresenter hudPresenter;
        [SerializeField] private DebugPanel debugPanel;
        [SerializeField] private InventoryDragController dragController;
        [SerializeField] private ItemIconDatabase iconDb;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private UnityEngine.UI.Image dragGhostImage;

        void Start()
        {
            if (!inventoryInstaller)
                inventoryInstaller = FindFirstObjectByType<InventoryInstaller>();

            if (!hudPresenter)
                hudPresenter = FindFirstObjectByType<InventoryHUDPresenter>();

            if (!debugPanel)
                debugPanel = FindFirstObjectByType<DebugPanel>();

            if (!dragController)
                dragController = FindFirstObjectByType<InventoryDragController>();

            if (!inventoryInstaller || inventoryInstaller.Service == null)
            {
                Debug.LogError("[SampleSceneInstaller] InventoryInstaller / Service no disponible");
                return;
            }
            if (!hudPresenter)
            {
                Debug.LogError("[SampleSceneInstaller] HUDPresenter no asignado");
                return;
            }
            hudPresenter.Bind(inventoryInstaller.Service);

            if (debugPanel) debugPanel.Bind(inventoryInstaller.Service, hudPresenter);

            if (dragController)
            {
                // Asegurar wiring mínimo
                if (!dragController.GetComponent<Canvas>()) { }
                dragController.Bind(inventoryInstaller.Service, hudPresenter, iconDb, debugPanel);

                if (rootCanvas) dragController.GetType();
            }
            else
            {
                Debug.LogWarning("[SampleSceneInstaller] No hay InventoryDragController en escena.");
            }

            if (!debugPanel) debugPanel = FindFirstObjectByType<DebugPanel>();
            if (debugPanel) debugPanel.Bind(inventoryInstaller.Service, hudPresenter);
        }
    }
}