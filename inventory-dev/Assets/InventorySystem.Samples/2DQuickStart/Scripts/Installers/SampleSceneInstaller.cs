using UnityEngine;
using GB.Inventory.Application.Abstractions;
using GB.Inventory.Infrastructure.Installers;

namespace GB.Inventory.Samples
{
    public class SampleSceneInstaller : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private InventoryInstaller inventoryInstaller;
        [SerializeField] private GB.Inventory.Samples.UI.InventoryHUDPresenter hudPresenter;

        void Start()
        {
            if (inventoryInstaller == null)
            {
                inventoryInstaller = FindFirstObjectByType<InventoryInstaller>();
            }

            if (inventoryInstaller == null)
            {
                Debug.LogError("[SampleSceneInstaller] No se encontró InventoryInstaller en escena");
                return;
            }

            var svc = inventoryInstaller.Service;
            if (svc == null)
            {
                Debug.LogError("[SampleSceneInstaller] InventoryInstaller.Service es null");
                return;
            }

            if (hudPresenter == null)
            {
                hudPresenter = FindFirstObjectByType<GB.Inventory.Samples.UI.InventoryHUDPresenter>();
            }
            
            if(hudPresenter != null)
            {
                hudPresenter.Bind(svc);
            }
            else
            {
                Debug.LogWarning("[SampleSceneInstaller] No hay HUDPresenter asignado");
            }
        }
    }
}