using NUnit.Framework;
using System.Collections.Generic;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Infrastructure.Persistence;
using GB.Inventory.Application;
using GB.Inventory.Application.Abstractions;

namespace GB.Inventory.Tests.Persistence
{
    // Dummies
    class DictItemMetaProvider : IItemMetadataProvider
    {
        private readonly Dictionary<string, ItemMeta> _db;
        public DictItemMetaProvider(Dictionary<string, ItemMeta> db) { _db = db; }
        public ItemMeta Get(string definitionId) => _db[definitionId];

        public bool TryGet(string definitionId, out ItemMeta meta)
        {
            _db.TryGetValue(definitionId, out meta);
            return true;
        }
    }

    class DictSlotProfileProvider : ISlotProfileProvider
    {
        private readonly Dictionary<string, SlotProfile> _db;
        public DictSlotProfileProvider(Dictionary<string, SlotProfile> db) { _db = db; }
        public SlotProfile Get(string slotProfileId) => _db[slotProfileId];

        public bool TryGet(string slotProfileId, out SlotProfile profile)
        {
            _db.TryGetValue(slotProfileId, out profile);
            return true;
        }
    }

    [TestFixture]
    public class InventorySerializerTests
    {
        private IInventoryService NewSvc(int cap, string defaultProfileId, out IItemMetadataProvider mp, out ISlotProfileProvider sp)
        {
            var items = new Dictionary<string, ItemMeta>
            {
                ["wood"] = new ItemMeta { DefinitionId = "wood", TypeId = "Resource", Tags = new[] { "Material" }, HasStackOverride = true, MaxStack = 50 },
                ["stone"] = new ItemMeta { DefinitionId = "stone", TypeId = "Resource", Tags = new[] { "Material" }, HasStackOverride = true, MaxStack = 50 },
                ["sword"] = new ItemMeta { DefinitionId = "sword", TypeId = "Weapon", Tags = new[] { "Equipment" }, HasStackOverride = true, MaxStack = 1 },
            };
            mp = new DictItemMetaProvider(items);

            var profiles = new Dictionary<string, SlotProfile>
            {
                ["Any"] = new SlotProfile { Id = "Any", AllowedTypes = System.Array.Empty<string>(), RequiredTags = System.Array.Empty<string>(), BannedTags = System.Array.Empty<string>(), HasStackableOverride = false },
                ["Materials"] = new SlotProfile { Id = "Materials", AllowedTypes = new[] { "Resource" }, RequiredTags = System.Array.Empty<string>(), BannedTags = System.Array.Empty<string>(), HasStackableOverride = true, StackableOverride = true, MaxStackOverride = 50 },
            };
            sp = new DictSlotProfileProvider(profiles);

            var stacking = new PerItemStackingPolicy(mp, defaultMax: 99);
            var filter = new SimpleSlotFilterPolicy(sp, mp, stacking);
            var model = new InventoryModel(cap, stacking, filter, defaultProfileId);
            return new InventoryService(model);
        }

        [Test]
        public void Capture_Then_Restore_Roundtrip_Works()
        {
            var svcA = NewSvc(3, "Any", out var mp, out var sp);

            // A Initial State
            Assert.IsTrue(svcA.TrySetSlotProfile(0, "Materials", out _));
            Assert.IsTrue(svcA.TryAdd("wood", 60, out _, out _));
            //Assert.IsTrue(svcA.TryAdd("stone", 10, out _, out _));
            Assert.IsTrue(svcA.TryAdd("sword", 1, out _, out var reason), reason);

            // Capture
            var dto = InventorySerializer.Capture(svcA);
            Assert.AreEqual(3, dto.capacity);
            Assert.GreaterOrEqual(dto.slots.Count, 2);

            // Restore in clean B inv.
            var svcB = NewSvc(3, "Any", out _, out _);
            Assert.IsTrue(InventorySerializer.Restore(svcB, dto, out var why), why);

            var view = svcB.SlotsView;
            // Slot 0: Materials with 50 wood
            Assert.AreEqual("Materials", svcB.GetSlotProfileId(0));
            Assert.IsNotNull(view[0]);
            Assert.AreEqual("wood", view[0].DefinitionId);
            Assert.AreEqual(50, view[0].Count);
            // Rest can distribute; checking existence
            Assert.That(view, Has.Some.Not.Null);
        }    
    }
}