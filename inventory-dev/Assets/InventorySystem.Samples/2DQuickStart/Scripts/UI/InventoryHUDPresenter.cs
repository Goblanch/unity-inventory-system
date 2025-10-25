using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GB.Inventory.Application.Abstractions;

namespace GB.Inventory.Samples.UI
{
    public class InventoryHUDPresenter : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private Transform slotsGrid;
        [SerializeField] private SlotView slotPrefab;
        [SerializeField] private ItemIconDatabase iconDb;

        private IInventoryService _svc;
        private readonly List<SlotView> _views = new List<SlotView>();

        public void Bind(IInventoryService service)
        {
            _svc = service;
            BuildGrid();
            Refresh();
        }

        private void BuildGrid()
        {
            // Limpia
            for (int i = slotsGrid.childCount - 1; i >= 0; i--)
            {
                Destroy(slotsGrid.GetChild(i).gameObject);
            }
            _views.Clear();

            if (_svc == null || slotPrefab == null || slotsGrid == null) return;

            int cap = _svc.Capacity;
            for(int i = 0; i < cap; i++)
            {
                var v = Instantiate(slotPrefab, slotsGrid);
                v.SetIndex(i);
                v.RenderEmpty();
                _views.Add(v);
            }
        }
        
        public void Refresh()
        {
            if (_svc == null || _views.Count == 0) return;
            var view = _svc.SlotsView;

            for(int i = 0; i < _views.Count; i++)
            {
                var sv = _views[i];
                if(view[i] == null)
                {
                    sv.RenderEmpty();
                }
                else
                {
                    var def = view[i].DefinitionId;
                    var count = view[i].Count;
                    var sprite = iconDb ? iconDb.GetIcon(def) : null;
                    sv.Render(def, count, sprite);
                }
            }
        }
    }
}