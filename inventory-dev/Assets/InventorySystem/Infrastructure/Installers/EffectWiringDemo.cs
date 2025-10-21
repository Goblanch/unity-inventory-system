using UnityEngine;
using GB.Inventory.Infrastructure.Installers;
using GB.Inventory.Infrastructure.Effects;
using GB.Inventory.Application;

public class EffectWiringDemo : MonoBehaviour, GB.Inventory.Domain.Abstractions.ITurnContext
{
    public string Phase => "Any";

    private InventoryInstaller _installer;
    private EffectRegistry _registry;

    void Awake()
    {
        _installer = FindFirstObjectByType<InventoryInstaller>();

        _registry = new EffectRegistry().RegisterEffect("test", new TestEffect());

        _registry.RegisterDefinition("card-memory", "test");
        _registry.RegisterDefinition("obj-hammer", "test");

        var inv = _installer.Inventory;
        var svc = new InventoryService(inv, _registry);
        typeof(InventoryInstaller).GetProperty("Service")?.SetValue(_installer, svc, null);
    }

    private void Start()
    {
        var svc = _installer.Service;

        // Probar un uso:
        svc.TryAdd("card-memory", 1, out var slot, out var why);
        svc.TryUse(slot, this, out var res, out var whyUse);
        Debug.Log($"TryUse result: success={res.Success} consume={res.ConsumeOne} msg='{res.Message}' reason='{whyUse}'");
    }
}