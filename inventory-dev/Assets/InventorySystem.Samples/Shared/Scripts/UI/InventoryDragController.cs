using UnityEngine;
using UnityEngine.UI;
using GB.Inventory.Application.Abstractions;

namespace GB.Inventory.Samples.UI
{
    public class InventoryDragController : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private Image dragGhostImage;
        [SerializeField] private ItemIconDatabase iconDb;
        [SerializeField] private InventoryHUDPresenter hudPresenter;
        [SerializeField] private DebugPanel debugPanel;

        private IInventoryService _svc;

        // Drag state
        private bool _dragging;
        private int _srcIndex = -1;
        private int _hoverIndex = -1;

        // Binding
        public void Bind(IInventoryService svc, InventoryHUDPresenter hud, ItemIconDatabase icons, DebugPanel panel)
        {
            _svc = svc;
            hudPresenter = hud;
            iconDb = icons;
            debugPanel = panel;
            SetupGhost(false);
        }

        private void SetupGhost(bool enable)
        {
            if (dragGhostImage)
            {
                dragGhostImage.raycastTarget = false;
                dragGhostImage.enabled = enable;
                var cg = dragGhostImage.GetComponent<CanvasGroup>();
                if (cg)
                {
                    cg.blocksRaycasts = false;
                    cg.ignoreParentGroups = true;
                    cg.alpha = enable ? 0.95f : 0f;
                }
            }
        }

        // Click Selection
        public void NotifySlotClicked(int index)
        {
            ClearAllSelection();
            var sv = hudPresenter?.GetSlotView(index);
            if (sv != null)
            {
                sv.SetSelected(true);
            }

            debugPanel?.SelectSlot(index);
        }

        private void ClearAllSelection()
        {
            if (hudPresenter == null) return;
            int cap = hudPresenter.SlotCount;
            for (int i = 0; i < cap; i++)
            {
                hudPresenter.GetSlotView(i)?.SetSelected(false);
                hudPresenter.GetSlotView(i)?.SetHighlight(false);
            }
        }

        // Drag and drop
        public void BeginDragFrom(int srcIndex, Vector2 screenPos)
        {
            if (_svc == null || hudPresenter == null || iconDb == null) return;
            var view = _svc.SlotsView;
            if ((uint)srcIndex >= (uint)view.Count || view[srcIndex] == null) return;

            _dragging = true;
            _srcIndex = srcIndex;
            _hoverIndex = -1;

            // Ghost icon
            if (dragGhostImage)
            {
                var def = view[srcIndex].DefinitionId;
                var sprite = iconDb.GetIcon(def);
                dragGhostImage.sprite = sprite;
                SetupGhost(true);
                MoveGhost(screenPos);
            }

            ClearAllSelection();
            hudPresenter.GetSlotView(srcIndex)?.SetSelected(true);
        }

        public void UpdateDrag(Vector2 screenPos)
        {
            if (!_dragging) return;
            MoveGhost(screenPos);
        }

        public void EndDrag(Vector2 screenPos)
        {
            if (!_dragging) return;

            // If valid hover: try move
            if (_hoverIndex >= 0 && _srcIndex >= 0 && _srcIndex != _hoverIndex)
            {
                _svc.TryMove(_srcIndex, _hoverIndex, out var reason);
                if (reason != null) Debug.Log($"[Drag] Move {_srcIndex} --> {_hoverIndex}: {reason}");
            }

            // Reset
            SetupGhost(false);
            _dragging = false;
            _srcIndex = -1;

            ClearAllSelection();
            hudPresenter.Refresh();
        }

        public void HoverSlot(int index, bool enter)
        {
            if (!_dragging || hudPresenter == null) return;

            if (_hoverIndex >= 0 && _hoverIndex != index)
                hudPresenter.GetSlotView(_hoverIndex)?.SetHighlight(false);

            _hoverIndex = enter ? index : -1;
            if (_hoverIndex >= 0)
                hudPresenter.GetSlotView(_hoverIndex)?.SetHighlight(true);
        }
        
        private void MoveGhost(Vector2 screenPos)
        {
            if (!dragGhostImage || rootCanvas == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform,
                screenPos,
                rootCanvas.worldCamera,
                out var local
            );
            (dragGhostImage.transform as RectTransform).anchoredPosition = local;
        }
    }
}