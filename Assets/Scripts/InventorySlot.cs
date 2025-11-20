using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Sebbe
{
    public class InventorySlot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image itemIcon;
        private ItemSO currentItem;
        
        public void SetItem(ItemSO item)
        {
            currentItem = item;
            if (itemIcon != null && item != null)
            {
                itemIcon.sprite = item.itemIcon;
                itemIcon.enabled = true;
            }
            else if (itemIcon != null)
            {
                itemIcon.enabled = false;
            }
        }
        public void RemoveItem()
        {
            if (Player.instance.inventory != null && currentItem != null)
            {
                Player.instance.inventory.RemoveItem(currentItem.itemID);
            }
            
            currentItem = null;
            if (itemIcon != null)
            {
                itemIcon.enabled = false;
                itemIcon.sprite = null;
            }
        }

        // Returns true if this slot currently has no item assigned
        public bool IsEmpty()
        {
            return currentItem == null;
        }

        // Returns the itemID of the current item, or -1 if empty
        public int GetCurrentItemID()
        {
            return currentItem != null ? currentItem.itemID : -1;
        }

        // Bind an Image component to this slot at runtime (used when weaponSlot lacks an InventorySlot)
        public void BindIcon(Image icon)
        {
            itemIcon = icon;
            if (itemIcon == null) return;
            itemIcon.enabled = currentItem != null;
            if (currentItem != null)
            {
                itemIcon.sprite = currentItem.itemIcon;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (currentItem == null) return;

            // If this is the equipped weapon slot, treat click as unequip and return to inventory
            if (InventorySystem.instance != null && InventorySystem.instance.weaponSlot == this.gameObject)
            {
                if (currentItem == null) return;

                ItemSO equipped = currentItem;

                // Unequip on player
                if (Player.instance != null)
                {
                    Player.instance.UnequipWeapon();
                }

                // Return item to player's inventory
                if (Player.instance != null && Player.instance.inventory != null)
                {
                    Player.instance.inventory.AddItem(equipped.itemID);
                }

                // Clear this slot UI
                SetItem(null);
                return;
            }

            // Only equip weapons from regular inventory slots
            if (currentItem == null) return;
            if (!currentItem.isWeapon) return;

            ItemSO itemToEquip = currentItem;

            // Place item into the weapon UI slot (if assigned)
            if (InventorySystem.instance != null && InventorySystem.instance.weaponSlot != null)
            {
                InventorySlot weaponUISlot = InventorySystem.instance.weaponSlot.GetComponent<InventorySlot>();

                // If the weapon slot exists but doesn't have an InventorySlot component, try to bind one at runtime
                if (weaponUISlot == null)
                {
                    // Try to find an Image to use as the icon in the weapon slot
                    Image icon = InventorySystem.instance.weaponSlot.GetComponentInChildren<Image>(true);
                    if (icon != null)
                    {
                        weaponUISlot = InventorySystem.instance.weaponSlot.AddComponent<InventorySlot>();
                        weaponUISlot.BindIcon(icon);
                    }
                    else
                    {
                        Debug.LogWarning("weaponSlot does not have an InventorySlot or an Image child to bind. Assign an InventorySlot component or add an Image child.");
                    }
                }

                if (weaponUISlot != null)
                {
                    // If there is an already equipped weapon, return it to the inventory (swap)
                    int currentlyEquippedID = weaponUISlot.GetCurrentItemID();
                    if (currentlyEquippedID != -1 && Player.instance != null && Player.instance.inventory != null)
                    {
                        Player.instance.inventory.AddItem(currentlyEquippedID);
                    }

                    weaponUISlot.SetItem(itemToEquip);
                }
            }

            // Tell the player to equip the item (updates animator and stats)
            if (Player.instance != null)
            {
                Player.instance.EquipWeapon(itemToEquip);
            }

            // Remove from player's inventory list and clear this slot UI
            if (Player.instance != null && Player.instance.inventory != null)
            {
                Player.instance.inventory.RemoveItem(itemToEquip.itemID);
            }

            SetItem(null);
        }
    }
}