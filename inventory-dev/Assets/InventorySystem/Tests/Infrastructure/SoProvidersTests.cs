using NUnit.Framework;
using UnityEngine;
using GB.Inventory.Infrastructure.Definitions;
using GB.Inventory.Infrastructure.Providers;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Domain;
using GB.Inventory.Application;

namespace GB.Inventory.Tests
{
    public class SoProvidersTests
    {
        // ==== Helpers para crear SO en EditMode ====

        private ItemTypeDefinition MakeType(string id)
        {
            var o = ScriptableObject.CreateInstance<ItemTypeDefinition>();
            typeof(ItemTypeDefinition)
                .GetField("typeId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, id);
            return o;
        }

        private ItemDefinition MakeItem(string defId, ItemTypeDefinition type, string[] tags)
        {
            var o = ScriptableObject.CreateInstance<ItemDefinition>();
            typeof(ItemDefinition)
                .GetField("definitionId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, defId);
            typeof(ItemDefinition)
                .GetField("type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, type);
            typeof(ItemDefinition)
                .GetField("tags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, tags);
            return o;
        }

        private SlotProfileDefinition MakeProfile(
            string id,
            string[] allowedTypes,
            string[] requiredTags,
            bool hasStackOverride,
            bool stackable,
            int maxStackOverride)
        {
            var o = ScriptableObject.CreateInstance<SlotProfileDefinition>();
            typeof(SlotProfileDefinition)
                .GetField("profileId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, id);
            typeof(SlotProfileDefinition)
                .GetField("allowedTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, allowedTypes);
            typeof(SlotProfileDefinition)
                .GetField("requiredTags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, requiredTags);
            typeof(SlotProfileDefinition)
                .GetField("bannedTags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, System.Array.Empty<string>());
            typeof(SlotProfileDefinition)
                .GetField("hasStackableOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, hasStackOverride);
            typeof(SlotProfileDefinition)
                .GetField("stackableOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, stackable);
            typeof(SlotProfileDefinition)
                .GetField("maxStackOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, maxStackOverride);
            return o;
        }

        // ==== Test ====

        [Test]
        public void SoProviders_EndToEnd_Config_Works()
        {
            // Tipos
            var tCard     = MakeType("Card");
            var tObject   = MakeType("Object");
            var tResource = MakeType("Resource");

            // Items
            var iCard = MakeItem("card-memory", tCard,   new[] { "Consumable" });
            var iObj  = MakeItem("obj-hammer",  tObject, new[] { "Consumable", "Tool" });
            var iWood = MakeItem("wood",        tResource, new[] { "Material" });

            // Databases (y reconstrucción de índices)
            var itemDb = ScriptableObject.CreateInstance<ItemDatabase>();
            typeof(ItemDatabase)
                .GetField("items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(itemDb, new[] { iCard, iObj, iWood });
            itemDb.RebuildIndex();

            var pConsumables = MakeProfile(
                id: "Consumables",
                allowedTypes: new[] { "Card", "Object" },
                requiredTags: new[] { "Consumable" },
                hasStackOverride: true,
                stackable: false,      // fuerza no apilar -> max 1 por slot
                maxStackOverride: 0
            );

            var pMaterials = MakeProfile(
                id: "Materials",
                allowedTypes: new[] { "Resource" },
                requiredTags: System.Array.Empty<string>(),
                hasStackOverride: true,
                stackable: true,
                maxStackOverride: 50   // límite por slot
            );

            var profileDb = ScriptableObject.CreateInstance<SlotProfileDatabase>();
            typeof(SlotProfileDatabase)
                .GetField("profiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(profileDb, new[] { pConsumables, pMaterials });
            profileDb.RebuildIndex();

            // Providers + Policies
            var itemMetaProvider   = new SoItemMetadataProvider(itemDb);
            var slotProfileProvider = new SoSlotProfileProvider(profileDb);
            var stacking           = new SimpleStackingPolicy(defaultMax: 99);
            var filter             = new GB.Inventory.Domain.Policies.SimpleSlotFilterPolicy(slotProfileProvider, itemMetaProvider, stacking);

            // Modelo + Servicio
            var inv = new InventoryModel(initialCapacity: 3, stacking, filter, defaultSlotProfileId: "Consumables");
            var svc = new InventoryService(inv);

            // 1) Consumibles: añadir 2 cartas -> max 1 por slot
            Assert.IsTrue(svc.TryAdd("card-memory", 2, out _, out var addCardsWhy), addCardsWhy);
            var view = svc.SlotsView;
            Assert.AreEqual(1, view[0].Count);
            Assert.AreEqual(1, view[1].Count);
            Assert.IsNull(view[2]);

            // 2) Cambiar slot 0 a Materials y limpiarlo
            Assert.IsTrue(svc.TrySetSlotProfile(0, "Materials", out var setProfWhy), setProfWhy);
            Assert.IsTrue(svc.TryClear(0, out var clrWhy), clrWhy);
            view = svc.SlotsView;
            Assert.IsNull(view[0]);          // quedó vacío tras limpiar
            Assert.AreEqual(1, view[1].Count);

            // 3) Añadir 60 de wood -> entra en slot 0 (Materials) hasta 50, resto no cabe
            Assert.IsFalse(svc.TryAdd("wood", 60, out _, out var addWoodWhy));
            view = svc.SlotsView;
            Assert.AreEqual(50, view[0].Count);   // llenó hasta override 50
            Assert.AreEqual(1, view[1].Count);    // carta sigue en slot 1
            Assert.IsNull(view[2]);               // slot 2 (Consumables) no acepta Resource
        }
    }
}
