using NUnit.Framework;
using System.Collections.Generic;
using GB.Inventory.Domain;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Tests.Common;

namespace GB.Inventory.Tests.Domain
{
    [TestFixture]
    public class InventoryModel_StackingAndProfiles_Tests
    {
        private InventoryModel NewModel(
            int cap,
            string defaultProfileId,
            out IItemMetadataProvider mp,
            out ISlotFilterPolicy filter,
            out IStackingPolicy stacking)
        {
            var items = new Dictionary<string, ItemMeta>
            {
                ["card-memory"] = new ItemMeta { DefinitionId = "card-memory", TypeId = "Card", Tags = new[] { "Consumable" }, HasStackOverride = false },
                ["obj-hammer"] = new ItemMeta { DefinitionId = "obj-hammer", TypeId = "Object", Tags = new[] { "Consumable", "Tool" }, HasStackOverride = false },
                ["wood"] = new ItemMeta { DefinitionId = "wood", TypeId = "Resource", Tags = new[] { "Material" }, HasStackOverride = true, MaxStack = 50 },
                ["stone"] = new ItemMeta { DefinitionId = "stone", TypeId = "Resource", Tags = new[] { "Material" }, HasStackOverride = true, MaxStack = 50 },
            };
            mp = new DictItemMetaProvider(items);

            var profiles = new Dictionary<string, SlotProfile>
            {
                ["Consumables"] = new SlotProfile
                {
                    Id = "Consumables",
                    AllowedTypes = new[] { "Card", "Object" },
                    RequiredTags = new[] { "Consumable" },
                    BannedTags = System.Array.Empty<string>(),
                    HasStackableOverride = true,
                    StackableOverride = false,
                    MaxStackOverride = 0
                },
                ["Materials"] = new SlotProfile
                {
                    Id = "Materials",
                    AllowedTypes = new[] { "Resource" },
                    RequiredTags = System.Array.Empty<string>(),
                    BannedTags = System.Array.Empty<string>(),
                    HasStackableOverride = true,
                    StackableOverride = true,
                    MaxStackOverride = 50
                },
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

            stacking = new PerItemStackingPolicy(mp, defaultMax: 99);
            filter = new SimpleSlotFilterPolicy(sp, mp, stacking);

            return new InventoryModel(cap, stacking, filter, defaultProfileId);
        }

        [Test]
        public void Add_Distributes_According_To_Profile_Max()
        {
            var model = NewModel(3, "Consumables", out var mp, out var filter, out var stacking);

            Assert.IsTrue(model.TryAdd("card-memory", 2, out var slotIdx, out var r1), r1);

            var view = model.Slots;
            Assert.IsNotNull(view[0]);
            Assert.IsNotNull(view[1]);

            Assert.IsTrue(view[2] == null || view[2].Stack == null, "Se esperaba slot 2 vacío (null o Stack == null)");

            Assert.AreEqual("card-memory", view[0].Stack.DefinitionId);
            Assert.AreEqual("card-memory", view[1].Stack.DefinitionId);
            Assert.AreEqual(1, view[0].Stack.Count);
            Assert.AreEqual(1, view[1].Stack.Count);
        }

        [Test]
        public void SetProfile_Then_Add_Respects_New_Profile()
        {
            var model = NewModel(3, "Consumables", out var mp, out var filter, out var stacking);

            Assert.IsTrue(model.TrySetSlotProfile(0, "Materials", out var why), why);

            // Intentamos añadir 60: el perfil Materials clampea a 50 -> entra parcial
            Assert.IsFalse(model.TryAdd("wood", 60, out var idx, out var r2));

            var view = model.Slots;
            Assert.IsNotNull(view[0]);
            Assert.AreEqual("wood", view[0].Stack.DefinitionId);
            Assert.AreEqual(50, view[0].Stack.Count);

            Assert.IsTrue(view[1] == null || view[1].Stack == null);
            Assert.IsTrue(view[2] == null || view[2].Stack == null);
        }

        [Test]
        public void Split_Places_Chunk_In_First_Empty_Compatible_Slot()
        {
            var model = NewModel(4, "Any", out var mp, out var filter, out var stacking);

            Assert.IsTrue(model.TryAdd("stone", 30, out var idx, out var r), r);
            var v0 = model.Slots;

            Assert.IsNotNull(v0[0]);
            Assert.AreEqual("stone", v0[0].Stack.DefinitionId);
            Assert.AreEqual(30, v0[0].Stack.Count);

            Assert.IsTrue(model.TrySplit(0, 10, out var newIdx, out var why), why);

            var view = model.Slots;
            Assert.AreEqual(20, view[0].Stack.Count);
            Assert.IsNotNull(view[newIdx]);
            Assert.AreEqual("stone", view[newIdx].Stack.DefinitionId);
            Assert.AreEqual(10, view[newIdx].Stack.Count);
        }

        [Test]
        public void Move_Merges_Up_To_Max_And_Leaves_Reminder()
        {
            var model = NewModel(3, "Materials", out var mp, out var filter, out var stacking);

            // Con TryAdd, el segundo add mergea sobre el primero hasta 50 y deja el resto en otro
            Assert.IsTrue(model.TryAdd("wood", 45, out _, out _));
            Assert.IsTrue(model.TryAdd("wood", 10, out _, out _));

            var before = model.Slots;
            Assert.AreEqual(50, before[0].Stack.Count);
            Assert.AreEqual(5, before[1].Stack.Count);

            // Mover 1 -> 0 ya no puede aumentar. Debe devolver false y dejar todo igual
            Assert.IsFalse(model.TryMove(1, 0, out var reason));
            var after = model.Slots;
            Assert.AreEqual(50, after[0].Stack.Count);
            Assert.AreEqual(5, after[1].Stack.Count);
        }

        [Test]
        public void Capacity_Increase_And_Decrease_Safe()
        {
            var model = NewModel(2, "Any", out var mp, out var filter, out var stacking);

            Assert.IsTrue(model.IncreaseCapacity(2, out var r1), r1);
            Assert.AreEqual(4, model.Capacity);

            // Forzamos items más allá del índice 0 para que reducir a 1 falle
            Assert.IsTrue(model.TryAdd("stone", 60, out _, out _)); // 50 en 0 y 10 en 1

            var v = model.Slots;
            Assert.IsNotNull(v[0]);
            Assert.AreEqual(50, v[0].Stack.Count);
            Assert.IsNotNull(v[1]);
            Assert.AreEqual(10, v[1].Stack.Count);

            // Reducir a 1 debe fallar porque el slot 1 no está vacío
            Assert.IsFalse(model.TrySetCapacity(1, out var why));
            StringAssert.Contains("no está vacío", why);

            // Vaciar el slot1 y ahora reducir debe funcionar
            Assert.IsTrue(model.TryClear(1, out _));
            Assert.IsTrue(model.TrySetCapacity(1, out var r2), r2);
            Assert.AreEqual(1, model.Capacity);
        }
    }
}