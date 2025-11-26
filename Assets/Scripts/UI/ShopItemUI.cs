using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sebbe
{
    public class ShopItemUI : MonoBehaviour
    {
        [Header("UI Refs")]
        public Image icon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI priceText;
        public Button buyButton;

        private int index = -1;
        private int price = 0;

        // Called by Shop when creating the entry
        public void Setup(Shop.ShopItem shopItem, int idx)
        {
            index = idx;
            price = shopItem != null ? shopItem.price : 0;

            if (icon != null)
                icon.sprite = shopItem != null && shopItem.item != null ? shopItem.item.itemIcon : null;

            if (nameText != null)
                nameText.text = shopItem != null && shopItem.item != null ? shopItem.item.itemName : "Unknown";

            if (priceText != null)
                priceText.text = shopItem != null ? shopItem.price.ToString() : "-";

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyClicked);
                // Initial interactable state
                UpdateButtonState(CoinManager.instance != null ? CoinManager.instance.totalCoins : 0);
            }
        }

        private void OnEnable()
        {
            if (CoinManager.instance != null)
                CoinManager.instance.OnCoinsChanged += UpdateButtonState;
        }

        private void OnDisable()
        {
            if (CoinManager.instance != null)
                CoinManager.instance.OnCoinsChanged -= UpdateButtonState;
        }

        private void UpdateButtonState(int currentCoins)
        {
            if (buyButton == null) return;
            buyButton.interactable = currentCoins >= price;
        }

        private void OnBuyClicked()
        {
            if (Shop.instance != null && index >= 0)
            {
                Shop.instance.BuyItem(index);
            }
        }
    }
}
