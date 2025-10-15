// using NUnit.Framework;
// using GB.Inventory.Application;
// using GB.Inventory.Application.Abstractions;
// using GB.Inventory.Domain;

// namespace GB.Inventory.Tests
// {
//     public class InventoryCapacityTests
//     {
//         private IInventoryService CreateService(int initialCapacity = 3)
//         {
//             var inventory = new InventoryModel(initialCapacity);
//             return new InventoryService(inventory);
//         }

//         [Test]
//         public void DefaultCapacity_Is3()
//         {
//             var service = CreateService();
//             Assert.AreEqual(3, service.Capacity);
//         }

//         [Test]
//         public void CanAdd_UntilCapacity()
//         {
//             var service = CreateService(3);
//             for (int i = 0; i < 3; i++)
//             {
//                 var ok = service.TryAdd(new Item($"Item-{i}", "def"), out var index);
//                 Assert.IsTrue(ok);
//                 Assert.AreEqual(i, index);
//             }
//         }

//         [Test]
//         public void IncreaseCapacity_AllowMoreAdds()
//         {
//             var service = CreateService(3);
//             for (int i = 0; i < 3; i++) service.TryAdd(new Item($"i{i}", "def"), out _);

//             var okIncrease = service.IncreaseCapacity(2, out var reason);
//             Assert.IsTrue(okIncrease, reason);
//             Assert.AreEqual(5, service.Capacity);

//             var okAdd = service.TryAdd(new Item("i3", "def"), out var index);
//             Assert.IsTrue(okAdd);
//             Assert.AreEqual(3, index);
//         }

//         [Test]
//         public void ReduceCapacity_Fails_WhenTruncationNoEmpty()
//         {
//             var service = CreateService(3);
//             service.TryAdd(new Item("A", "def"), out _);
//             service.TryAdd(new Item("B", "def"), out _);
//             service.TryAdd(new Item("C", "def"), out _);

//             var ok = service.SetCapacity(2, out var reason);
//             Assert.IsFalse(ok);
//             Assert.IsNotNull(reason);
//         }

//         [Test]
//         public void ReduceCapacity_Succeeds_WhenTailsEmpty()
//         {
//             var service = CreateService(3);
//             service.TryAdd(new Item("A", "def"), out _);
//             service.TryAdd(new Item("B", "def"), out _);

//             var ok = service.SetCapacity(2, out var reason);
//             Assert.IsTrue(ok);
//             Assert.AreEqual(2, service.Capacity);
//         }

//         [Test]
//         public void RemoveItem_FreesSlot()
//         {
//             var service = CreateService(3);
//             service.TryAdd(new Item("A", "def"), out var index);
//             Assert.AreEqual(0, index);

//             var removedOk = service.TryRemoveAt(0, out var removed);
//             Assert.IsTrue(removedOk);
//             Assert.IsNotNull(removed);

//             var addOk = service.TryAdd(new Item("B", "def"), out var index2);
//             Assert.IsTrue(addOk);
//             Assert.AreEqual(0, index2);
//         }

//         [Test]
//         public void SetCapacity_Negative_Fails()
//         {
//             var service = CreateService(3);
//             var ok = service.SetCapacity(-1, out var reason);
//             Assert.IsFalse(ok);
//             Assert.IsNotNull(reason);
//         }

//         [Test]
//         public void IncreaseCapacity_ZeroOrNegative_Fails()
//         {
//             var service = CreateService(3);
//             var ok0 = service.IncreaseCapacity(0, out var r0);
//             var okNeg = service.IncreaseCapacity(-2, out var rNeg);

//             Assert.IsFalse(ok0);
//             Assert.IsFalse(okNeg);
//             Assert.IsNotNull(r0);
//             Assert.IsNotNull(rNeg);
//         }
//     }
// }
