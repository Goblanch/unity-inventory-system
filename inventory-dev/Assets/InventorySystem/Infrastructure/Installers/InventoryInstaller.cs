using UnityEngine;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Application;
using GB.Inventory.Application.Abstractions;
using GB.Inventory.Infrastructure.Definitions;
using GB.Inventory.Infrastructure.Providers;
using GB.Inventory.Infrastructure.Effects;
using System;

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

        [Header("Effect Registry (custom)")]
        [Tooltip("Registros adicionales de efectos por reflexión. " +
                 "effectKey debe coincidir con el 'Effect Key' del ItemDefinition. " +
                 "effectTypeName debe ser el nombre de tipo completo (incl. namespace), p.ej. 'MyGame.Effects.TestEffect'.")]
        [SerializeField]
        private EffectBinding[] customEffects = new EffectBinding[]
        {
            // Ejemplo: new EffectBinding { effectKey = "test", effectTypeName = "MyGame.Effects.TestEffect" }
        };

        [Serializable]
        private struct EffectBinding
        {
            public string effectKey; // p. ej. "test"
            public string effectTypeName; // p. ej. "MyGame.Effects.TestEffect
        }

        // Public API
        public IInventory Inventory { get; private set; }
        public IInventoryService Service { get; private set; }

        void Awake()
        {
            // Providers desde SO
            var itemMetaProvider = new SoItemMetadataProvider(itemDatabase);
            var slotProfileProvider = new SoSlotProfileProvider(slotProfileDatabase);
            var effectInfoProvider = new SoItemEffectInfoProvider(itemDatabase);

            // Políticas por defecto
            var stacking = new SimpleStackingPolicy(defaultMaxStack);
            var filter = new SimpleSlotFilterPolicy(slotProfileProvider, itemMetaProvider, stacking);
            IUsagePhasePolicy phasePolicy = enableUsagePhases ? new DefaultUsagePhasePolicy() : null;

            // Modelo
            var model = new InventoryModel(initialCapacity, stacking, filter, defaultSlotProfileId);
            Inventory = model;

            // EffectRegistry: registramos aquí los efectos disponibles
            var registry = new EffectRegistry(effectInfoProvider);
            RegisterCustomEffectByReflection(registry);

            // Service
            Service = new InventoryService(model, registry, phasePolicy);

            Debug.Log("[InventoryInstaller] Inventario inicializado");
        }
        
        private void RegisterCustomEffectByReflection(EffectRegistry registry)
        {
            if (customEffects == null) return;

            for(int i = 0; i < customEffects.Length; i++)
            {
                var binding = customEffects[i];
                if (string.IsNullOrWhiteSpace(binding.effectKey) || string.IsNullOrWhiteSpace(binding.effectTypeName))
                    continue;

                var type = Type.GetType(binding.effectTypeName, throwOnError: false);
                if (type == null)
                {
                    Debug.LogWarning($"[InventoryInstaller] No se encontró el tipo '{binding.effectTypeName}'. " +
                                     $"Asegúrate de incluir el namespace completo y que el asmdef esté referenciado.");
                    continue;
                }

                if (!typeof(IItemEffect).IsAssignableFrom(type))
                {
                    Debug.LogWarning($"[InventoryInstaller] El tipo '{binding.effectTypeName}' no implementa IItemEffect.");
                    continue;
                }

                try
                {
                    var instance = Activator.CreateInstance(type) as IItemEffect;
                    if (instance == null)
                    {
                        Debug.LogWarning($"[InventoryInstaller] No se pudo instanciar '{binding.effectTypeName}'. " +
                                         $"Asegúrate de que tenga un constructor público sin parámetros.");
                        continue;
                    }

                    registry.RegisterEffect(binding.effectKey, instance);
#if UNITY_EDITOR
                    Debug.Log($"[InventoryInstaller] Registrado efecto '{binding.effectKey}' -> {binding.effectTypeName}");
#endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[InventoryInstaller] Error al instanciar '{binding.effectTypeName}': {ex.Message}");
                }
            }
        }
    }
}
