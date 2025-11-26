using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sebbe
{
    public class TradeRecipeUI : MonoBehaviour
    {
        public TextMeshProUGUI resultText;
        public TextMeshProUGUI ingredientsText;
        public Button tradeButton;

        private int index = -1;
        private System.Collections.Generic.List<int> requiredIDs = new System.Collections.Generic.List<int>();
        private float nextCheck = 0f;

        public void Setup(Shop.TradeRecipe recipe, int idx)
        {
            index = idx;
            requiredIDs.Clear();

            if (recipe != null)
            {
                if (recipe.resultItem != null && resultText != null)
                    resultText.text = recipe.resultItem.itemName;

                // Build ingredients string and requiredIDs list
                var counts = new System.Collections.Generic.Dictionary<int, int>();
                foreach (var it in recipe.requiredItems)
                {
                    if (it == null) continue;
                    requiredIDs.Add(it.itemID);
                    if (!counts.ContainsKey(it.itemID)) counts[it.itemID] = 0;
                    counts[it.itemID]++;
                }

                if (ingredientsText != null)
                {
                    var parts = new System.Collections.Generic.List<string>();
                    foreach (var kv in counts)
                    {
                        var so = WorldItemManager.instance.GetItemByID(kv.Key);
                        string name = so != null ? so.itemName : "Unknown";
                        if (kv.Value > 1) parts.Add($"{name} x{kv.Value}"); else parts.Add(name);
                    }
                    ingredientsText.text = string.Join(" + ", parts.ToArray());
                }
            }

            if (tradeButton != null)
            {
                tradeButton.onClick.RemoveAllListeners();
                tradeButton.onClick.AddListener(OnTradeClicked);
                UpdateButtonInteractable();
            }
        }

        private void Update()
        {
            // Poll inventory occasionally to update interactable state
            if (Time.time >= nextCheck)
            {
                nextCheck = Time.time + 0.25f;
                UpdateButtonInteractable();
            }
        }

        private void UpdateButtonInteractable()
        {
            if (tradeButton == null) return;
            if (Player.instance == null || Player.instance.inventory == null)
            {
                tradeButton.interactable = false;
                return;
            }

            tradeButton.interactable = Player.instance.inventory.HasItems(requiredIDs);
        }

        private void OnTradeClicked()
        {
            if (Shop.instance != null && index >= 0)
            {
                Shop.instance.AttemptTrade(index);
            }
        }
    }
}
