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
        private ItemTypeDefinition MakeType(string id)
        {
            var o = ScriptableObject.CreateInstance<ItemTypeDefinition>();
            typeof(ItemTypeDefinition).GetField("typeId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, id);
            return o;
        }

        private ItemDefinition MakeItem(string defId, ItemTypeDefinition type, string[] tags)
        {
            var o = ScriptableObject.CreateInstance<ItemDefinition>();
            typeof(ItemDefinition).GetField("definitionId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, defId);
            typeof(ItemDefinition).GetField("type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, type);
            typeof(ItemDefinition).GetField("tags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, tags);
            return o;
        }

        private SlotProfileDefinition MakeProfile(string id, string[] allowedTypes, string[] requiredTags, bool hasStackOverride, bool stackable, int maxStackOverride)
        {
            var o = ScriptableObject.CreateInstance<SlotProfileDefinition>();
            typeof(SlotProfileDefinition).GetField("profileId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, id);
            typeof(SlotProfileDefinition).GetField("allowedTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, allowedTypes);
            typeof(SlotProfileDefinition).GetField("requiredTags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, requiredTags);
            typeof(SlotProfileDefinition).GetField("hasStackableOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, hasStackOverride);
            typeof(SlotProfileDefinition).GetField("stackableOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, stackable);
            typeof(SlotProfileDefinition).GetField("maxStackOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(o, maxStackOverride);
            return o;
        }

        [Test]
        public void SoProviders_EndToEnd_Config_Works()
        {
            // Tipos
            var tCard = MakeType("Card");
            var tObject = MakeType("Object");
            var tResource = MakeType("Resource");

            // Items
            var iCard = MakeItem("card-memory", tCard, new[] { "Consumable" });
            var iObj  = MakeItem("obj-hammer",  tObject, new[] { "Consumable", "Tool" });
            var iWood = MakeItem("wood",        tResource, new[] { "Material" });

            // Databases
            var itemDb = ScriptableObject.CreateInstance<ItemDatabase>();
            typeof(ItemDatabase).GetField("items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(itemDb, new[] { iCard, iObj, iWood });
            itemDb.RebuildIndex(); // fuerza reconstrucción del diccionario

            // Profiles
            var pConsumables = MakeProfile("Consumables", new[] { "Card", "Object" }, new[] { "Consumable" }, true, false, 0);
            var pMaterials   = MakeProfile("Materials",   new[] { "Resource" },      System.Array.Empty<string>(), true, true, 50);

            var profileDb = ScriptableObject.CreateInstance<SlotProfileDatabase>();
            typeof(SlotProfileDatabase).GetField("profiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(profileDb, new[] { pConsumables, pMaterials });
            profileDb.RebuildIndex();

            // Providers + Policies
            var itemMetaProvider = new SoItemMetadataProvider(itemDb);
            var slotProfileProvider = new SoSlotProfileProvider(profileDb);
            var stacking = new SimpleStackingPolicy(defaultMax: 99);
            var filter = new GB.Inventory.Domain.Policies.SimpleSlotFilterPolicy(slotProfileProvider, itemMetaProvider, stacking);

            // Modelo
            var inv = new InventoryModel(initialCapacity: 3, stacking, filter, defaultSlotProfileId: "Consumables");
            var svc = new InventoryService(inv);

            // Validaciones:
            Assert.IsTrue(svc.TryAdd("card-memory", 2, out _, out var r1), r1); // en "Consumables" -> max 1 por slot
            var view = svc.SlotsView;
            Assert.AreEqual(1, view[0].Count);
            Assert.AreEqual(1, view[1].Count);

            // Cambiar perfil del slot 0 a "Materials"
            Assert.IsTrue(svc.TrySetSlotProfile(0, "Materials", out var why), why);

            // Añadir 60 de wood -> slot 0 (Materials) permite stack hasta 50, el resto no cabe en los otros (Consumables no acepta Resource)
            Assert.IsFalse(svc.TryAdd("wood", 60, out _, out var r2));
            view = svc.SlotsView;
            Assert.AreEqual(50, view[0].Count);
            Assert.IsNull(view[1]); // sigue siendo Consumables, no acepta Resource
        }
    }
}
