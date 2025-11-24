using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Sebbe
{
    // Simple UI tooltip for inventory items. Add this to a UI GameObject under a Canvas and assign
    // `tooltipRoot` (the panel) and `tooltipText` (TextMeshProUGUI). The tooltip will be positioned
    // at the current mouse position when shown.
    public class InventoryTooltip : MonoBehaviour
    {
        public static InventoryTooltip instance;

        [Header("UI References")]
        public GameObject tooltipRoot;
        public TextMeshProUGUI tooltipText;
        public Vector2 screenOffset = new Vector2(12f, -12f);

        private RectTransform canvasRect;

        void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) Destroy(gameObject);

            if (tooltipRoot != null) tooltipRoot.SetActive(false);

            Canvas c = GetComponentInParent<Canvas>();
            if (c != null)
            {
                canvasRect = c.GetComponent<RectTransform>();
            }
        }

        void Update()
        {
            if (tooltipRoot != null && tooltipRoot.activeSelf)
            {
                Vector2 pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out pos);
                tooltipRoot.GetComponent<RectTransform>().anchoredPosition = pos + screenOffset;
            }
        }

        public void Show(ItemSO item)
        {
            if (tooltipRoot == null || tooltipText == null || item == null) return;

            // Ensure tooltip does not block pointer events (prevents flicker when hovering)
            var cg = tooltipRoot.GetComponent<CanvasGroup>();
            if (cg == null) cg = tooltipRoot.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            if (tooltipText != null) tooltipText.raycastTarget = false;

            // Build tooltip content
            string s = item.itemName;
            if (!string.IsNullOrEmpty(item.itemDescription))
            {
                s += "\n" + item.itemDescription;
            }

            // Add stats
            if (item.isWeapon)
            {
                s += $"\nDamage: {item.weaponDamage}";
                s += $"\nRange: {item.weaponRange}";
                s += $"\nAttack Rate: {item.attackRate}";
            }
            if (item.isArmor || item.isHelmet || item.isBoots)
            {
                s += $"\nDefense: {item.defenseBonus}";
            }
            if (item.isAmulet && item.healthRegenFromAmulet)
            {
                s += $"\nAmulet Regen: {item.healthAmountFromAmulet} hp / {item.healthRegenRateFromAmulet}s";
            }
            if (item.isRing)
            {
                s += $"\nCrit Chance: {item.critChanceFromRing}%";
                s += $"\nCrit Bonus: {item.increasedDamageFromCritFromRing}%";
            }

            tooltipText.text = s;
            tooltipRoot.SetActive(true);
        }

        public void Hide()
        {
            if (tooltipRoot == null) return;
            tooltipRoot.SetActive(false);
        }
    }
}
