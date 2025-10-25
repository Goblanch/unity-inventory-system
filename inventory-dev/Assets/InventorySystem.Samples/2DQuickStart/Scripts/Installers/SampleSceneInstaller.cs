using UnityEngine;
using GB.Inventory.Application.Abstractions;
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

        void Start()
        {
            if (!inventoryInstaller)
                inventoryInstaller = FindFirstObjectByType<InventoryInstaller>();

            if (!hudPresenter)
                hudPresenter = FindFirstObjectByType<InventoryHUDPresenter>();

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

            if (!debugPanel) debugPanel = FindFirstObjectByType<DebugPanel>();
            if (debugPanel) debugPanel.Bind(inventoryInstaller.Service, hudPresenter);
        }
    }
}