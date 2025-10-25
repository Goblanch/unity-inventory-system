using UnityEngine;
using UnityEngine.UI;

namespace GB.Inventory.Samples.UI
{
    public class SlotView : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private Text countText;

        [Header("Style")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        [SerializeField] private Color filledColor = Color.white;
        [SerializeField] private Color selectedTint = new Color(0.3f, 0.6f, 1f, 0.35f);
        [SerializeField] private Color highlightTint = new Color(1f, 0.85f, 0.2f, 0.35f);

        public int Index { get; private set; }

        private bool _selected;
        private bool _highlight;

        public void SetIndex(int index) => Index = index;

        public void RenderEmpty()
        {
            if (background) background.color = emptyColor;
            if (icon) { icon.enabled = false; icon.sprite = null; }
            if (countText) { countText.text = string.Empty; }
            gameObject.name = $"SlotView[{Index}]_Empty";
        }

        public void Render(string definitionId, int count, Sprite sprite)
        {
            if (string.IsNullOrEmpty(definitionId))
            {
                RenderEmpty();
                return;
            }

            if (background) background.color = filledColor;
            if (icon)
            {
                icon.sprite = sprite;
                icon.enabled = sprite != null;
                icon.preserveAspect = true;
            }

            if (countText)
            {
                countText.text = (count > 0) ? count.ToString() : "";
            }

            gameObject.name = $"SlotView[{Index}]_{definitionId}_x{count}]";
        }

        public void SetSelected(bool value)
        {
            _selected = value;
            ApplyOverlayTint();
        }

        public void SetHighlight(bool value)
        {
            _highlight = value;
            ApplyOverlayTint();
        }
        
        private void ApplyOverlayTint()
        {
            // Simple: mix tints over bg
            if (!background) return;

            if (_selected && _highlight)
                background.color = Color.Lerp(filledColor, (selectedTint + highlightTint) * 0.5f, 0.7f);
            else if (_selected)
                background.color = Color.Lerp(filledColor, selectedTint, 0.7f);
            else if (_highlight)
                background.color = Color.Lerp(filledColor, highlightTint, 0.7f);
            else{}
        }
    }
}

