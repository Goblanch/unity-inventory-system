using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Tests.Common;

namespace GB.Inventory.Tests.Domain
{
    [TestFixture]
    public class SimpleSlotFilterPolicyTests
    {
        [Test]
        public void Profile_Consumables_Allows_Card_And_Object_Blocks_Resource_And_Forces_Max1()
        {
            // Items
            var items = new Dictionary<string, ItemMeta>
            {
                ["card-memory"] = new ItemMeta { DefinitionId = "card-memory", TypeId = "Card", Tags = new[] { "Consumable" }, HasStackOverride = false },
                ["obj-hammer"] = new ItemMeta { DefinitionId = "obj-hammer", TypeId = "Object", Tags = new[] { "Consumable", "Tool" }, HasStackOverride = false },
                ["wood"] = new ItemMeta { DefinitionId = "wood", TypeId = "Resource", Tags = new[] { "Material" }, HasStackOverride = false },
            };

            var mp = new DictItemMetaProvider(items);

            // Perfiles
            var profiles = new Dictionary<string, SlotProfile>
            {
                ["Consumables"] = new SlotProfile
                {
                    Id = "Consumables",
                    AllowedTypes = new[] { "Card", "Object" },
                    RequiredTags = new[] { "Consumable" },
                    BannedTags = System.Array.Empty<string>(),
                    HasStackableOverride = true,
                    StackableOverride = false, // no apilable -> 1
                    MaxStackOverride = 0
                }
            };

            var sp = new DictSlotProfileProvider(profiles);

            // Stacking base 99, pero el perfil forzará 1
            var stacking = new FixedStackingPolicy(99);
            var filter = new SimpleSlotFilterPolicy(sp, mp, stacking);

            int max;
            string why;

            Assert.IsTrue(filter.CanAccept("Consumables", "card-memory", out max, out why), why);
            Assert.AreEqual(1, max);

            Assert.IsTrue(filter.CanAccept("Consumables", "obj-hammer", out max, out why), why);
            Assert.AreEqual(1, max);

            Assert.IsFalse(filter.CanAccept("Consumables", "wood", out max, out why));
        }

        [Test]
        public void Profile_Materials_Allows_Resource_And_Clamps_Max_To_Override()
        {
            var items = new Dictionary<string, ItemMeta>
            {
                ["wood"] = new ItemMeta { DefinitionId = "wood", TypeId = "Resource", Tags = new[] { "Material" }, HasStackOverride = true, MaxStack = 80 },
            };
            var mp = new DictItemMetaProvider(items);

            var profiles = new Dictionary<string, SlotProfile>
            {
                ["Materials"] = new SlotProfile
                {
                    Id = "Materials",
                    AllowedTypes = new[] { "Resource" },
                    RequiredTags = System.Array.Empty<string>(),
                    BannedTags = System.Array.Empty<string>(),
                    HasStackableOverride = true,
                    StackableOverride = true,
                    MaxStackOverride = 50 // limita por perfil
                }
            };

            var sp = new DictSlotProfileProvider(profiles);

            // Stacking por item (wood=80), pero perfil clamp a 50
            var stacking = new PerItemStackingPolicy(mp, 99);
            var filter = new SimpleSlotFilterPolicy(sp, mp, stacking);

            int max;
            string why;
            Assert.IsTrue(filter.CanAccept("Materials", "wood", out max, out why), why);
            Assert.AreEqual(50, max);
        }

        [Test]
        public void Profile_Any_Allows_All_Uses_PerItem_Or_Default_Max()
        {
            var items = new Dictionary<string, ItemMeta>
            {
                ["stone"] = new ItemMeta { DefinitionId = "stone", TypeId = "Resource", Tags = new[] { "Material" }, HasStackOverride = true, MaxStack = 50 },
                ["sword"] = new ItemMeta { DefinitionId = "sword", TypeId = "Weapon", Tags = new[] { "Tool" }, HasStackOverride = true, MaxStack = 1 },
                ["berry"] = new ItemMeta { DefinitionId = "berry", TypeId = "Food", Tags = new[] { "Consumable" }, HasStackOverride = false }
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

            int max;
            string why;

            Assert.IsTrue(filter.CanAccept("Any", "stone", out max, out why), why);
            Assert.AreEqual(50, max);

            Assert.IsTrue(filter.CanAccept("Any", "sword", out max, out why), why);
            Assert.AreEqual(1, max);

            Assert.IsTrue(filter.CanAccept("Any", "berry", out max, out why), why);
            Assert.AreEqual(99, max);
        }
    }
}