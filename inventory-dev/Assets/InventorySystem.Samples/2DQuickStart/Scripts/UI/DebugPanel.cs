using UnityEngine;
using UnityEngine.UI;
using GB.Inventory.Application.Abstractions;

namespace GB.Inventory.Samples.UI
{
    public class DebugPanel : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private InputField definitionIdInput;
        [SerializeField] private InputField countInput;
        [SerializeField] private InputField slotIndexInput;
        [SerializeField] private InputField profileIdInput;
        [SerializeField] private Button addButton;
        [SerializeField] private Button useButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button setProfileButton;
        [SerializeField] private Button increaseCapacityButton;
        [SerializeField] private InputField capacityDeltaInput;
        [SerializeField] private Button setCapacityButton;
        [SerializeField] private InputField capacityInput;
        [SerializeField] private Button refreshButton;

        private IInventoryService _svc;
        private InventoryHUDPresenter _hud;

        void Awake()
        {
            HookButtons();
        }

        public void Bind(IInventoryService service, InventoryHUDPresenter hud)
        {
            _svc = service;
            _hud = hud;
        }

        public void SelectSlot(int index)
        {
            if (slotIndexInput) slotIndexInput.text = index.ToString();
        }

        private void HookButtons()
        {
            if (addButton) addButton.onClick.AddListener(OnAdd);
            if (useButton) useButton.onClick.AddListener(OnUse);
            if (clearButton) clearButton.onClick.AddListener(OnClear);
            if (setProfileButton) setProfileButton.onClick.AddListener(OnSetProfile);
            if (increaseCapacityButton) increaseCapacityButton.onClick.AddListener(OnIncreaseCapacity);
            if (setCapacityButton) setCapacityButton.onClick.AddListener(OnSetCapacity);
            if (refreshButton) refreshButton.onClick.AddListener(OnRefresh);
        }

        private int ReadInt(InputField input, int fallback)
        {
            if (!input) return fallback;
            if (int.TryParse(input.text, out var v)) return v;
            return fallback;
        }

        private string ReadString(InputField input, string fallback)
        {
            if (!input) return fallback;
            return string.IsNullOrWhiteSpace(input.text) ? fallback : input.text.Trim();
        }

        private void OnAdd()
        {
            if (_svc == null) return;
            var def = ReadString(definitionIdInput, "card-memory");
            var count = ReadInt(countInput, 1);
            _svc.TryAdd(def, count, out _, out var reason);
            Debug.Log($"[DebugPanel] Add {def} x{count} --> {reason ?? "OK"}");
            _hud?.Refresh();
        }

        private void OnUse()
        {
            if (_svc == null) return;
            var slot = ReadInt(slotIndexInput, 0);
            _svc.TryUse(slot, null, out var result, out var reason);
            Debug.Log($"[DebugPanel] Use slot {slot} --> ok={result.Success}, consume={result.ConsumeOne}, msg={result.Message}, why={reason}");
            _hud.Refresh();
        }

        private void OnClear()
        {
            if (_svc == null) return;
            var slot = ReadInt(slotIndexInput, 0);
            _svc.TryClear(slot, out var reason);
            Debug.Log($"[DebugPanel] Clear slot {slot} --> {reason ?? "OK"}");
            _hud?.Refresh();
        }

        private void OnSetProfile()
        {
            if (_svc == null) return;
            var slot = ReadInt(slotIndexInput, 0);
            var profile = ReadString(profileIdInput, "Materials");
            _svc.TrySetSlotProfile(slot, profile, out var reason);
            Debug.Log($"[DebugPanel] SetProfile slot {slot}='{profile}' --> {reason ?? "OK"}");
            _hud?.Refresh();
        }

        private void OnIncreaseCapacity()
        {
            if (_svc == null) return;
            var delta = ReadInt(capacityDeltaInput, 1);
            _svc.IncreaseCapacity(delta, out var reason);
            Debug.Log($"[DebugPanel] IncreaseCapacity +{delta} --> {reason ?? "OK"}");
            _hud?.Bind(_svc); // Rebuild grid if capacity changed
        }

        private void OnSetCapacity()
        {
            if (_svc == null) return;
            var cap = ReadInt(capacityInput, _svc.Capacity);
            _svc.SetCapacity(cap, out var reason);
            Debug.Log($"[DebugPanel] SetCapacity {cap} --> {reason ?? "OK"}");
            _hud?.Bind(_svc); // Rebuild grid if capacity changed
        }

        private void OnRefresh()
        {
            _hud?.Refresh();
        }
    }
}