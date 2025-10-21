using NUnit.Framework;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Domain;
using GB.Inventory.Application;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Infrastructure.Effects;

namespace GB.Inventory.Tests
{
    class TestCtx : ITurnContext { public string Phase => "Any"; }

    public class UseEffectTests
    {
        [Test]
        public void TryUse_Consumes_One_On_Success()
        {
            var stacking = new SimpleStackingPolicy(99);
            var filter = new GB.Inventory.Domain.Policies.SimpleSlotFilterPolicy(
                profiles: new DummyProfiles(), meta: new DummyMeta(), stacking);

            var inv = new InventoryModel(2, stacking, filter, "Default");

            // Registry con Echo para "apple"
            var reg = new EffectRegistry()
                .RegisterEffect("test", new TestEffect())
                .RegisterDefinition("apple", "test");

            var svc = new InventoryService(inv, reg);

            Assert.IsTrue(svc.TryAdd("apple", 3, out var slot, out var r1), r1);

            var ctx = new TestCtx();
            Assert.IsTrue(svc.TryUse(slot, ctx, out var res, out var r2), r2);
            Assert.IsTrue(res.Success);
            Assert.IsTrue(res.ConsumeOne);

            var view = svc.SlotsView;
            Assert.AreEqual(2, view[slot].Count);
        }

        // Dummies mínimos para SimpleSlotFilterPolicy
        class DummyProfiles : ISlotProfileProvider
        {
            public SlotProfile Get(string slotProfileId) => new SlotProfile
            {
                Id = slotProfileId,
                AllowedTypes = System.Array.Empty<string>(),
                RequiredTags = System.Array.Empty<string>(),
                BannedTags = System.Array.Empty<string>(),
                HasStackableOverride = false,
                StackableOverride = true,
                MaxStackOverride = 0
            };
        }

        class DummyMeta : IItemMetadataProvider
        {
            public ItemMeta Get(string definitionId) => new ItemMeta
            {
                DefinitionId = definitionId,
                TypeId = "Any",
                Tags = System.Array.Empty<string>()
            };
        }
    }
}
