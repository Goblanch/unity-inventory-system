using NUnit.Framework;
using GB.Inventory.Application;
using GB.Inventory.Application.Abstractions;
using GB.Inventory.Domain;

namespace GB.Inventory.Tests
{
    public class InventoryCapacityTests
    {
        private IInventoryService CreateService(int initialCapacity = 3)
        {
            var inventory = new InventoryModel(initialCapacity);
            return new InventoryService(inventory);
        }
    }
}
