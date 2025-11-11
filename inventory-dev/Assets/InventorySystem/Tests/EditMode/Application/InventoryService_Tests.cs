using NUnit.Framework;
using System.Collections.Generic;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Application;
using GB.Inventory.Tests.Common;
using GB.Inventory.Application.Abstractions;

namespace GB.Inventory.Tests.Application
{
    [TestFixture]
    public class InventoryService_Tests
    {
        private IInventoryService NewService(int cap = 3, string defaultProfile = "Any")
        {
            var items = new Dictionary<string, ItemMeta>
            {
                ["stone"] = new ItemMeta { DefinitionId = "stone", TypeId = "Resource", Tags = new[] { "Material" }, HasStackOverride = true, MaxStack = 50 },
                ["sword"] = new ItemMeta { DefinitionId = "sword", TypeId = "Weapon", Tags = new[] { "Tool" }, HasStackOverride = true, MaxStack = 1 }
            };
            var mp = new DictItemMetaProvider(items);

            var profiles = new Dictionary<string, SlotProfile>
            {
                ["Any"] = new SlotProfile
                {
                    Id = "Any",
                    AllowedTypes = System.Array.Empty<string>(),
                    RequiredTags = System.Array.Empty<string>(),
                    BannedTags = System.Array.Empty<string>(),
                    HasStackableOverride = false
                }
            };
            var sp = new DictSlotProfileProvider(profiles);

            var stacking = new PerItemStackingPolicy(mp, defaultMax: 99);
            var filter = new SimpleSlotFilterPolicy(sp, mp, stacking);
            var model = new InventoryModel(cap, stacking, filter, defaultProfile);

            return new InventoryService(model);
        }

        [Test]
        public void SlotsView_Reflects_Model_State()
        {
            var svc = NewService(cap: 2, defaultProfile: "Any");

            Assert.IsTrue(svc.TryAdd("stone", 30, out _, out _));
            var view = svc.SlotsView;
            Assert.IsNotNull(view[0]);
            Assert.AreEqual("stone", view[0].DefinitionId);
            Assert.AreEqual(30, view[0].Count);

            Assert.IsTrue(svc.TryAdd("sword", 1, out _, out _));
            view = svc.SlotsView;
            Assert.IsNotNull(view[1]);
            Assert.AreEqual("sword", view[1].DefinitionId);
            Assert.AreEqual(1, view[1].Count);
        }

        [Test]
        public void SetSlotProfile_Through_Service()
        {
            var svc = NewService(cap: 2, defaultProfile: "Any");
            Assert.IsTrue(svc.TrySetSlotProfile(0, "Any", out var why), why);
        }

        [Test]
        public void Capacity_Through_Service()
        {
            var svc = NewService(cap: 1, defaultProfile: "Any");
            Assert.AreEqual(1, svc.Capacity);

            Assert.IsTrue(svc.IncreaseCapacity(2, out var r1), r1);
            Assert.AreEqual(3, svc.Capacity);

            Assert.IsTrue(svc.SetCapacity(2, out var r2), r2);
            Assert.AreEqual(2, svc.Capacity);
        }
    }
}