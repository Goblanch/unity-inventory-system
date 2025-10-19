using NUnit.Framework;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Application;
using GB.Inventory.Application.Abstractions;
using System.Collections.Generic;
using UnityEditor;

namespace GB.Inventory.Tests
{
    // Dummies para tests
    class DictItemMetaProvider : IItemMetadataProvider
    {
        private readonly Dictionary<string, ItemMeta> _db;
        public DictItemMetaProvider(Dictionary<string, ItemMeta> db) { _db = db; }
        public ItemMeta Get(string definitionId) => _db[definitionId];
    }

    class DictSlotProfileProvider : ISlotProfileProvider
    {
        private readonly Dictionary<string, SlotProfile> _db;
        public DictSlotProfileProvider(Dictionary<string, SlotProfile> db) { _db = db; }
        public SlotProfile Get(string slotProfileId) => _db[slotProfileId];
    }

    public class SlotProfileFilterTests
    {
        private IInventoryService CreateSvc(out ISlotProfileProvider sp, out IItemMetadataProvider mp, int cap = 3)
        {
            // Metadatos de items
            var items = new Dictionary<string, ItemMeta>
            {
                ["card-memory"] = new ItemMeta { DefinitionId = "card-memory", TypeId = "Card", Tags = new[] { "Consumable" } },
                ["obj-hammer"] = new ItemMeta { DefinitionId = "obj-hammer", TypeId = "Object", Tags = new[] { "Consumable", "Tool" } },
                ["wood"] = new ItemMeta { DefinitionId = "wood", TypeId = "Resource", Tags = new[] { "Material" } },
            };
            mp = new DictItemMetaProvider(items);

            // Perfiles
            var profiles = new Dictionary<string, SlotProfile>
            {
                ["Consumables"] = new SlotProfile
                {
                    Id = "Consumables",
                    AllowedTypes = new[] { "Card", "Object" },
                    RequiredTags = new[] { "Consumable" },
                    BannedTags = new string[0],
                    HasStackableOverride = true,
                    StackableOverride = false, // no stack -> max 1
                    MaxStackOverride = 0
                },
                ["Materials"] = new SlotProfile
                {
                    Id = "Materials",
                    AllowedTypes = new[] { "Resource" },
                    RequiredTags = new string[0],
                    BannedTags = new string[0],
                    HasStackableOverride = true,
                    StackableOverride = true,
                    MaxStackOverride = 50
                }
            };
            sp = new DictSlotProfileProvider(profiles);

            var stacking = new SimpleStackingPolicy(defaultMax: 99);
            var filter = new SimpleSlotFilterPolicy(sp, mp, stacking);

            var inv = new InventoryModel(cap, stacking, filter, defaultSlotProfileId: "Consumables");
            return new InventoryService(inv);
        }

        [Test]
        public void Consumables_Slot_Allows_Card_And_Object_Not_Resouce()
        {
            var svc = CreateSvc(out var sp, out var mp, cap: 3);
            // default profule = "Consumables" en todos los slots

            Assert.IsTrue(svc.TryAdd("card-memory", 1, out var s1, out var r1), r1);
            Assert.IsTrue(svc.TryAdd("obj-hammer", 1, out var s2, out var r2), r2);

            // Resouce debería fallar en este perfil
            Assert.IsFalse(svc.TryAdd("wood", 1, out var s3, out var r3));
            Assert.IsNotNull(r3);
        }

        [Test]
        public void Consumables_Slot_Stack_Forced_To_1()
        {
            var svc = CreateSvc(out var sp, out var mp, cap: 2);
            // Consumables -> StackableOverride=false -> max 1
            Assert.IsTrue(svc.TryAdd("card-memory", 2, out var s, out var r), r);
            var view = svc.SlotsView;
            Assert.AreEqual(1, view[0].Count);
            Assert.AreEqual(1, view[1].Count);
        }

        [Test]
        public void Materials_Slot_Allows_Stack_Up_To_Override()
        {
            var svc = CreateSvc(out var sp, out var mp, cap: 3);

            // Cambiaos el perfil del slot 0 a "Materials"
            Assert.IsTrue(svc.TrySetSlotProfile(0, "Materials", out var why), why);

            // Añadimos 60 unidades de wood: Materials limita a max 50 por slot
            Assert.IsFalse(svc.TryAdd("wood", 60, out var slot, out var r)); // no cabe todo
            var view = svc.SlotsView;
            Assert.AreEqual(50, view[0].Count); // slot 0 (Materials)
            Assert.IsNull(view[1]); // Sigue vacío
        }
    }
}