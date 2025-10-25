using UnityEngine;
using GB.Inventory.Infrastructure.Installers;
public class SampleBootstrap : MonoBehaviour
{
    void Start()
    {
        var inst = Object.FindFirstObjectByType<InventoryInstaller>();
        var svc = inst.Service;
        svc.TryAdd("card-memory", 2, out _, out _);
        svc.TrySetSlotProfile(2, "Materials", out var r1);
        svc.TryAdd("wood", 30, out _, out _);
        // Busca el HUD y refresca
        var hud = Object.FindFirstObjectByType<GB.Inventory.Samples.UI.InventoryHUDPresenter>();
        hud.Refresh();
    }
}
