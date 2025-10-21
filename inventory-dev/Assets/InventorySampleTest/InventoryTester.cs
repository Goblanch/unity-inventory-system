using UnityEngine;
using GB.Inventory.Infrastructure.Installers;
using GB.Inventory.Application.Abstractions;

public class InventoryTester : MonoBehaviour
{
    private IInventoryService _svc;

    void Start()
    {
        var installer = Object.FindFirstObjectByType<InventoryInstaller>();
        _svc = installer.Service;

        Debug.Log("=== INVENTARIO INICIAL ===");
        Print();

        var okAddCards = _svc.TryAdd("card-memory", 2, out _, out var r1);
        Debug.Log($"Add card-memory x2 --> ok={okAddCards} reason='{r1}'");
        Print();

        var okSetProf = _svc.TrySetSlotProfile(0, "Materials", out var why);
        Debug.Log($"SetSlotProfile(0,'Materials') --> ok={okSetProf} reason='{why}' currentProfile='{_svc.GetSlotProfileId(0)}'");

        var okClear = _svc.TryClear(0, out var clr);
        Debug.Log($"Clear(0) --> ok={okClear} reason='{clr}'");
        Print();

        var okAddWood = _svc.TryAdd("wood", 60, out _, out var r2);
        Debug.Log($"Add wood x60 --> ok={okAddWood} reason='{r2}'  (slot0Profile='{_svc.GetSlotProfileId(0)}')");
        Print();
    }

    private void Print()
    {
        var view = _svc.SlotsView;
        for (int i = 0; i < view.Count; i++)
        {
            if (view[i] == null) Debug.Log($"Slot {i}: [Empty]");
            else Debug.Log($"Slot {i}: {view[i].DefinitionId} x{view[i].Count}");
        }
    }
}
