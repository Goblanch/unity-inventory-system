using NUnit.Framework;
using System.Collections.Generic;
using GB.Inventory.Tests.Common;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Domain.Policies;

namespace GB.Inventory.Tests.Domain
{
    [TestFixture]
    public class PerItemStackingPolicyTests
    {
        [Test]
        public void Returns_PerItem_Override_When_Present_Else_Default()
        {
            var db = new Dictionary<string, ItemMeta>
            {
                ["stone"] = new ItemMeta { DefinitionId = "stone", TypeId = "Resource", Tags = new[] { "Material" }, HasStackOverride = true, MaxStack = 50 },
                ["sword"] = new ItemMeta { DefinitionId = "sword", TypeId = "Weapon", Tags = new[] { "Tool" }, HasStackOverride = true, MaxStack = 1 },
                ["berry"] = new ItemMeta { DefinitionId = "berry", TypeId = "Food", Tags = new[] { "Consumable" }, HasStackOverride = false, MaxStack = 0 }
            };

            IItemMetadataProvider mp = new DictItemMetaProvider(db);
            var policy = new PerItemStackingPolicy(mp, defaultMax: 99);

            Assert.AreEqual(50, policy.GetMaxPerStack("stone", "Resource"));
            Assert.AreEqual(1, policy.GetMaxPerStack("sword", "Weapon"));
            Assert.AreEqual(99, policy.GetMaxPerStack("berry", "Food")); // sin override -> default
            Assert.AreEqual(99, policy.GetMaxPerStack("", "Unknown")); // id vacío -> default
        }
    }
}