using GB.Inventory.Domain.Abstractions;
using System.Collections.Generic;

namespace GB.Inventory.Infrastructure.Effects
{
    public sealed class EffectRegistry : IEffectRegistry
    {
        private readonly Dictionary<string, IItemEffect> _byEffectKey = new();
        private readonly Dictionary<string, string> _defToEffectKey = new();
        private readonly Dictionary<string, object> _defToPayload = new();

        public EffectRegistry RegisterEffect(string effectKey, IItemEffect effect)
        {
            if (!string.IsNullOrWhiteSpace(effectKey) && effect != null)
            {
                _byEffectKey[effectKey] = effect;
            }
            return this;
        }

        public EffectRegistry RegisterDefinition(string definitionId, string effectKey, object payload = null)
        {
            if (!string.IsNullOrWhiteSpace(definitionId))
            {
                _defToEffectKey[definitionId] = string.IsNullOrWhiteSpace(effectKey) ? null : effectKey;
                if (payload != null) _defToPayload[definitionId] = payload;
            }

            return this;
        }

        public bool TryResolve(string effectKey, out IItemEffect effect)
        {
            effect = null;

            if (_defToEffectKey.TryGetValue(effectKey, out var eKey) && !string.IsNullOrWhiteSpace(eKey))
            {
                return _byEffectKey.TryGetValue(eKey, out effect);
            }

            return _byEffectKey.TryGetValue(effectKey, out effect);
        }

        public bool TryGetPayload(string definitionId, out object payload) =>
            _defToPayload.TryGetValue(definitionId, out payload);
    }
}