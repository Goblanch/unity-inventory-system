using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Infrastructure.Persistence;
using GB.Inventory.Application;
using GB.Inventory.Application.Abstractions;
using UnityEngine;

namespace GB.Inventory.Tests.Persistence
{
    class DictItemMetaProvider2 : IItemMetadataProvider
    {
        private readonly Dictionary<string, ItemMeta> _db;
        public DictItemMetaProvider2(Dictionary<string, ItemMeta> db) { _db = db; }
        public ItemMeta Get(string definitionId) => _db[definitionId];

        public bool TryGet(string definitionId, out ItemMeta meta)
        {
            _db.TryGetValue(definitionId, out meta);
            return true;
        }
    }

    class DictSlotProfileProvider2 : ISlotProfileProvider
    {
        private readonly Dictionary<string, SlotProfile> _db;
        public DictSlotProfileProvider2(Dictionary<string, SlotProfile> db) { _db = db; }
        public SlotProfile Get(string slotProfileId) => _db[slotProfileId];

        public bool TryGet(string slotProfileId, out SlotProfile profile)
        {
            _db.TryGetValue(slotProfileId, out profile);
            return true;
        }
    }

    [TestFixture]
    public class JsonInventorySaveServiceTests
    {
        private IInventoryService NewSvc(out IItemMetadataProvider mp, out ISlotProfileProvider sp)
        {
            var items = new Dictionary<string, ItemMeta>
            {
                ["wood"] = new ItemMeta { DefinitionId = "wood", TypeId = "Resource", Tags = new[] { "Material" }, HasStackOverride = true, MaxStack = 50 },
                ["stone"] = new ItemMeta { DefinitionId = "stone", TypeId = "Resource", Tags = new[] { "Material" }, HasStackOverride = true, MaxStack = 50 }
            };
            mp = new DictItemMetaProvider2(items);

            var profiles = new Dictionary<string, SlotProfile>
            {
                ["Any"] = new SlotProfile { Id = "Any", AllowedTypes = System.Array.Empty<string>(), RequiredTags = System.Array.Empty<string>(), BannedTags = System.Array.Empty<string>(), HasStackableOverride = false },
                ["Materials"] = new SlotProfile { Id = "Materials", AllowedTypes = new[] { "Resource" }, RequiredTags = System.Array.Empty<string>(), BannedTags = System.Array.Empty<string>(), HasStackableOverride = true, StackableOverride = true, MaxStackOverride = 50 }
            };
            sp = new DictSlotProfileProvider2(profiles);

            var stacking = new PerItemStackingPolicy(mp, 99);
            var filter = new SimpleSlotFilterPolicy(sp, mp, stacking);
            var model = new InventoryModel(2, stacking, filter, "Any");
            return new InventoryService(model);
        }

        [Test]
        public void Save_Then_Load_File_Roundtrip_Works()
        {
            var svc = NewSvc(out var mp, out var sp);
            Assert.IsTrue(svc.TrySetSlotProfile(0, "Materials", out _));
            Assert.IsTrue(svc.TryAdd("wood", 75, out _, out _));

            // Save Service (uses persistentDataPath/GB.Inventory/testslot.json)
            var save = new JsonInventorySaveService("GB.Inventory.Tests");
            var slot = "testslot";

            // Save
            Assert.IsTrue(save.Save(svc, slot, out var whySave), whySave);
            Assert.IsTrue(save.Exists(slot));

            // Clean current inv (reduce capacity and back to 2 empty)
            Assert.IsTrue(svc.SetCapacity(2, out _));
            Assert.IsTrue(svc.TryClear(0, out _));
            Assert.IsTrue(svc.TryClear(1, out _));

            // Load
            Assert.IsTrue(save.Load(svc, slot, out var whyLoad), whyLoad);

            var view = svc.SlotsView;
            Assert.IsNotNull(view[0]);
            Assert.AreEqual("wood", view[0].DefinitionId);
            Assert.Greater(view[0].Count, 0);

            // File cleaning
            var path = save.GetSavePath(slot);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}