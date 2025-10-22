using GB.Inventory.Domain.Abstractions;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

namespace GB.Inventory.Infrastructure.Effects
{
    /// <summary>
    /// Registry que:
    /// - Registra effectKey --> ItemEffect
    /// - Resuelve definitionId --> (effectKey, payload, allowedPhases) vía SoItemEffectInfoProvider
    /// </summary>
    public sealed class EffectRegistry : IEffectRegistry
    {
        private readonly Dictionary<string, IItemEffect> _byEffectKey = new();
        private readonly Providers.SoItemEffectInfoProvider _info;

        public EffectRegistry(Providers.SoItemEffectInfoProvider infoProvider)
        {
            _info = infoProvider;
        }

        public EffectRegistry RegisterEffect(string effectKey, IItemEffect effect)
        {
            if (!string.IsNullOrWhiteSpace(effectKey) && effect != null)
            {
                _byEffectKey[effectKey] = effect;
            }
            return this;
        }

        public bool TryResolve(string key, out IItemEffect effect)
        {
            effect = null;

            // Intentar como definitionId
            if (_info != null && _info.TryGet(key, out var effectKey, out _, out _))
            {
                if (!string.IsNullOrWhiteSpace(effectKey))
                    return _byEffectKey.TryGetValue(effectKey, out effect);

                return false;
            }

            // Fallback: quizá el key ya es un effect key
            return _byEffectKey.TryGetValue(key, out effect);
        }

        public bool TryGetPayload(string definitionId, out object payload)
        {
            payload = null;
            return _info != null && _info.TryGet(definitionId, out _, out payload, out _);
        }
        
        public string[] GetAllowedPhases(string definitionId)
        {
            if (_info != null && _info.TryGet(definitionId, out _, out _, out var phases))
                return phases ?? System.Array.Empty<string>();

            return System.Array.Empty<string>();
        }
    }
}