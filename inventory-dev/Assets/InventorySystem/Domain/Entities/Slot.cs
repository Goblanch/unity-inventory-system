using System;
using Codice.Client.BaseCommands.Merge.Xml;
using GB.Inventory.Domain.Abstractions;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;

namespace GB.Inventory.Domain
{
    // ?: Futuro: Contenedores multi-stack?
    internal sealed class Slot : ISlot
    {
        public int Index { get; }
        public IStack Stack => _stack;
        public bool IsEmpty => _stack == null;

        private Stack _stack;

        public Slot(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            Index = index;
            _stack = null;
        }

        public bool TryMergeIn(string definitionId, int amount, IStackingPolicy policy, out int merged, out string reason)
        {
            reason = null;
            merged = 0;

            if (IsEmpty) return false;

            if (_stack.DefinitionId != definitionId)
            {
                reason = $"Slot {Index} contiene {_stack.DefinitionId}. No coincide con {definitionId}";
                return false;
            }

            if (!policy.CanMerge(definitionId, _stack.Count, amount, out var canMerge, out reason))
                return false;

            if (canMerge <= 0)
            {
                if (reason == null) reason = "No se puede añadir al stack existente";
                return false;
            }

            _stack.Add(canMerge);
            merged = canMerge;
            return true;
        }

        public bool TryCreate(string definitionId, int amount)
        {
            if (!IsEmpty) return false;
            _stack = new Stack(definitionId, amount);
            return true;
        }

        public bool TryTake(int amount, out int taken, out string reason)
        {
            reason = null;
            taken = 0;

            if (IsEmpty)
            {
                reason = $"Slot {Index} vacío";
                return false;
            }

            taken = _stack.RemoveUpTo(amount);
            if (_stack.IsEmpty) _stack = null;
            return taken > 0;
        }

        public bool TrySplit(int amount, out Stack split, out string reason)
        {
            reason = null;
            split = null;

            if (IsEmpty)
            {
                reason = $"Slot {Index} vacío";
                return false;
            }

            if (amount <= 0 || amount >= _stack.Count)
            {
                reason = $"Cantidad inválida para split (1...{_stack.Count - 1})";
                return false;
            }

            var def = _stack.DefinitionId;
            var taken = _stack.RemoveUpTo(amount);
            split = new Stack(def, taken);
            if (_stack.IsEmpty) _stack = null;
            return true;
        }

        public bool TryClear(out Stack removed)
        {
            removed = _stack;
            _stack = null;
            return removed != null;
        }
    }
}