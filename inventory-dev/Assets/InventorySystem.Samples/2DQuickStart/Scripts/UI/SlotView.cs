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

        public int Index { get; private set; }

        public void SetIndex(int index) => Index = index;

        public void RenderEmpty()
        {
            if (background) background.color = emptyColor;
            if (icon) { icon.enabled = false; icon.sprite = null; }
            if (countText) { countText.text = ""; }
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
                icon.enabled = sprite != null;
                icon.sprite = sprite;
                icon.preserveAspect = true;
            }

            if (countText)
            {
                countText.text = (count > 0) ? count.ToString() : "";
            }
        }
    }
}

