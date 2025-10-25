using UnityEngine;
using UnityEngine.EventSystems;

namespace GB.Inventory.Samples.UI
{
    [RequireComponent(typeof(SlotView))]
    public class SlotInputRouter : MonoBehaviour, IPointerClickHandler, IBeginDragHandler,
    IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private SlotView _view;
        private InventoryDragController _drag;

        void Awake()
        {
            _view = GetComponent<SlotView>();
            _drag = FindFirstObjectByType<InventoryDragController>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _drag?.NotifySlotClicked(_view.Index);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _drag?.BeginDragFrom(_view.Index, eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _drag?.UpdateDrag(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _drag?.EndDrag(eventData.position);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _drag?.HoverSlot(_view.Index, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _drag?.HoverSlot(_view.Index, false);
        }
    }
}