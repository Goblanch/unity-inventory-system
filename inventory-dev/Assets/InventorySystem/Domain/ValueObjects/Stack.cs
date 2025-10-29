using System;

namespace GB.Inventory.Domain
{
    /// <summary>
    /// Represent a homogeneous stack of items sharing the same definition ID.
    /// </summary>
    public interface IStack
    {
        string DefinitionId { get; }
        int Count { get; }
        bool IsEmpty { get; }
    }

    internal sealed class Stack : IStack
    {
        public string DefinitionId { get; private set; }
        public int Count { get; private set; }
        public bool IsEmpty => Count <= 0;

        public Stack(string definitionId, int count)
        {
            if (string.IsNullOrWhiteSpace(definitionId))
                throw new ArgumentNullException("definitionId vacío");

            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            DefinitionId = definitionId;
            Count = count;
        }

        public void Add(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Count += amount;
        }

        public int RemoveUpTo(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            int take = Math.Min(amount, Count);
            Count -= take;
            return take;
        }
    }
}