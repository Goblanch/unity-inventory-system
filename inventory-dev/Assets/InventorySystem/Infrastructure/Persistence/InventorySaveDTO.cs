using System;
using System.Collections.Generic;

namespace GB.Inventory.Infrastructure.Persistence
{
    /// <summary>
    /// Root DTO for saving an inventory snapshot to JSON.
    /// Keep it plain (no interfaces/Unity refs) for robust serialization.
    /// </summary>
    [Serializable]
    public class InventorySaveDTO
    {
        public int version = 1;
        public string savedAtIsoUtc;
        public int capacity;
        public List<SlotDTO> slots = new List<SlotDTO>();
    }

    /// <summary>
    /// Plain slot record. Only data required to rebuild the model.
    /// </summary>
    [Serializable]
    public class SlotDTO
    {
        public int index;
        public string slotProfileId;
        public string definitionId;
        public int count;
    }
}