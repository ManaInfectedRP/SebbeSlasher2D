using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Sebbe
{
    public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image itemIcon;
        private ItemSO currentItem;
        [Header("Per-Slot Tooltip (optional)")]
        [Tooltip("Optional panel owned by this slot to show item info when hovered. If unset, global InventoryTooltip is used.")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipPanelText;
        [SerializeField] private Vector2 panelOffset = new Vector2(0f, 40f);
        
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

            // If this is one of the equipment slots (helmet/armor/boots), unequip and return to inventory
            if (InventorySystem.instance != null && InventorySystem.instance.helmetSlot == this.gameObject)
            {
                if (currentItem == null) return;
                ItemSO equipped = currentItem;
                if (Player.instance != null)
                {
                    Player.instance.UnequipEquipment("helmet");
                }
                // Inventory update handled by Player.UnequipEquipment()
                SetItem(null);
                return;
            }

            if (InventorySystem.instance != null && InventorySystem.instance.armorSlot == this.gameObject)
            {
                if (currentItem == null) return;
                ItemSO equipped = currentItem;
                if (Player.instance != null)
                {
                    Player.instance.UnequipEquipment("armor");
                }
                // Inventory update handled by Player.UnequipEquipment()
                SetItem(null);
                return;
            }

            if (InventorySystem.instance != null && InventorySystem.instance.bootsSlot == this.gameObject)
            {
                if (currentItem == null) return;
                ItemSO equipped = currentItem;
                if (Player.instance != null)
                {
                    Player.instance.UnequipEquipment("boots");
                }
                // Inventory update handled by Player.UnequipEquipment()
                SetItem(null);
                return;
            }

            // If this is the key slot, unequip key and return to inventory
            if (InventorySystem.instance != null && InventorySystem.instance.keySlot == this.gameObject)
            {
                if (currentItem == null) return;
                ItemSO equipped = currentItem;
                if (Player.instance != null)
                {
                    Player.instance.UnequipEquipment("key");
                }
                // Inventory update handled by Player.UnequipEquipment()
                SetItem(null);
                return;
            }

            // If this is the amulet slot, unequip amulet and return to inventory
            if (InventorySystem.instance != null && InventorySystem.instance.amuletSlot == this.gameObject)
            {
                if (currentItem == null) return;
                ItemSO equipped = currentItem;
                if (Player.instance != null)
                {
                    Player.instance.UnequipEquipment("amulet");
                }
                // Inventory update handled by Player.UnequipEquipment()
                SetItem(null);
                return;
            }

            // If this is the ring slot, unequip ring and return to inventory
            if (InventorySystem.instance != null && InventorySystem.instance.ringSlot == this.gameObject)
            {
                if (currentItem == null) return;
                ItemSO equipped = currentItem;
                if (Player.instance != null)
                {
                    Player.instance.UnequipEquipment("ring");
                }
                // Inventory update handled by Player.UnequipEquipment()
                SetItem(null);
                return;
            }

            // Only equip weapons or equipment from regular inventory slots
            if (currentItem == null) return;

            ItemSO itemToEquip = currentItem;

            // If the item is equipment (helmet/armor/boots/key), handle equipping to the corresponding slot
            if (itemToEquip.isEquipment)
            {
                GameObject targetSlot = null;
                if (itemToEquip.isHelmet && InventorySystem.instance != null) targetSlot = InventorySystem.instance.helmetSlot;
                else if (itemToEquip.isArmor && InventorySystem.instance != null) targetSlot = InventorySystem.instance.armorSlot;
                else if (itemToEquip.isBoots && InventorySystem.instance != null) targetSlot = InventorySystem.instance.bootsSlot;
                else if (itemToEquip.isAmulet && InventorySystem.instance != null) targetSlot = InventorySystem.instance.amuletSlot;
                else if (itemToEquip.isRing && InventorySystem.instance != null) targetSlot = InventorySystem.instance.ringSlot;

                if (targetSlot != null)
                {
                    InventorySlot equipUISlot = targetSlot.GetComponent<InventorySlot>();
                    if (equipUISlot == null)
                    {
                        Image icon = targetSlot.GetComponentInChildren<Image>(true);
                        if (icon != null)
                        {
                            equipUISlot = targetSlot.AddComponent<InventorySlot>();
                            equipUISlot.BindIcon(icon);
                        }
                        else
                        {
                            Debug.LogWarning("Equipment slot does not have an InventorySlot or an Image child to bind. Assign an InventorySlot component or add an Image child.");
                        }
                    }

                    if (equipUISlot != null)
                    {
                        int currentlyEquippedID = equipUISlot.GetCurrentItemID();
                        // Previously equipped item will be handled by Player.EquipEquipment() (it adds the previous back to inventory).

                        equipUISlot.SetItem(itemToEquip);
                    }

                    // Tell the player to equip the item
                    if (Player.instance != null)
                    {
                        Player.instance.EquipEquipment(itemToEquip);
                    }

                    // Remove from player's inventory list and clear this slot UI
                    if (Player.instance != null && Player.instance.inventory != null)
                    {
                        Player.instance.inventory.RemoveItem(itemToEquip.itemID);
                    }

                    SetItem(null);
                    return;
                }
            }

            // If the item is a key item, try to place it in the key slot
            if (itemToEquip.isKeyItem && InventorySystem.instance != null)
            {
                GameObject targetKeySlot = InventorySystem.instance.keySlot;
                if (targetKeySlot != null)
                {
                    InventorySlot keyUISlot = targetKeySlot.GetComponent<InventorySlot>();
                    if (keyUISlot == null)
                    {
                        Image icon = targetKeySlot.GetComponentInChildren<Image>(true);
                        if (icon != null)
                        {
                            keyUISlot = targetKeySlot.AddComponent<InventorySlot>();
                            keyUISlot.BindIcon(icon);
                        }
                        else
                        {
                            Debug.LogWarning("Key slot does not have an InventorySlot or an Image child to bind. Assign an InventorySlot component or add an Image child.");
                        }
                    }

                    if (keyUISlot != null)
                    {
                        int currentlyEquippedID = keyUISlot.GetCurrentItemID();
                        // Previously equipped key is handled in Player.EquipEquipment()

                        keyUISlot.SetItem(itemToEquip);
                    }

                    if (Player.instance != null)
                    {
                        Player.instance.EquipEquipment(itemToEquip);
                    }

                    if (Player.instance != null && Player.instance.inventory != null)
                    {
                        Player.instance.inventory.RemoveItem(itemToEquip.itemID);
                    }

                    SetItem(null);
                    return;
                }
            }

            // Only equip weapons from regular inventory slots
            if (!itemToEquip.isWeapon) return;

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

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItem == null) return;

            // If a per-slot tooltip panel has been assigned, show and populate it
            if (tooltipPanel != null && tooltipPanelText != null)
            {
                tooltipPanel.SetActive(true);
                tooltipPanelText.text = BuildTooltipText(currentItem);

                // Position the tooltip panel in the Canvas so it aligns correctly with the slot
                var slotRT = GetComponent<RectTransform>();
                var panelRT = tooltipPanel.GetComponent<RectTransform>();
                // Prefer the slot's parent canvas so coordinates match the slot's UI space
                Canvas slotCanvas = slotRT.GetComponentInParent<Canvas>();
                Canvas targetCanvas = slotCanvas != null ? slotCanvas : tooltipPanel.GetComponentInParent<Canvas>();
                if (slotRT != null && panelRT != null && targetCanvas != null)
                {
                    // Reparent the panel to the target canvas so anchoredPosition is valid
                    panelRT.SetParent(targetCanvas.transform, true);

                    // Ensure layout elements are rebuilt so size matches content
                    LayoutRebuilder.ForceRebuildLayoutImmediate(panelRT);

                    RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
                    Camera cam = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera;

                    // Convert the slot's world position to a local point in the target canvas
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, slotRT.position);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out Vector2 localPoint);

                    // Use top-left pivot for predictable offsetting
                    panelRT.pivot = new Vector2(0f, 1f);
                    panelRT.anchoredPosition = localPoint + panelOffset;
                }

                    // Ensure tooltip panel does not block pointer events (avoids flicker)
                    CanvasGroup cg = tooltipPanel.GetComponent<CanvasGroup>();
                    if (cg == null) cg = tooltipPanel.AddComponent<CanvasGroup>();
                    cg.blocksRaycasts = false;
                    if (tooltipPanelText != null) tooltipPanelText.raycastTarget = false;

                return;
            }

            // Fallback to global tooltip if no per-slot panel assigned
            if (InventoryTooltip.instance != null)
            {
                InventoryTooltip.instance.Show(currentItem);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Hide per-slot panel if used
            if (tooltipPanel != null && tooltipPanelText != null)
            {
                tooltipPanel.SetActive(false);
                return;
            }

            // Fallback hide
            if (InventoryTooltip.instance != null)
            {
                InventoryTooltip.instance.Hide();
            }
        }

        // Compose a simple tooltip string for per-slot panel
        private string BuildTooltipText(ItemSO item)
        {
            if (item == null) return string.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(item.itemName);
            if (!string.IsNullOrEmpty(item.itemDescription))
            {
                sb.AppendLine();
                sb.Append(item.itemDescription);
            }

            if (item.isWeapon)
            {
                sb.AppendLine();
                sb.Append($"Damage: {item.weaponDamage}");
                sb.AppendLine();
                sb.Append($"Range: {item.weaponRange}");
                sb.AppendLine();
                sb.Append($"Attack Rate: {item.attackRate}");
            }
            if (item.isArmor || item.isHelmet || item.isBoots)
            {
                sb.AppendLine();
                sb.Append($"Defense: {item.defenseBonus}");
            }
            if (item.isAmulet && item.healthRegenFromAmulet)
            {
                sb.AppendLine();
                sb.Append($"Amulet Regen: {item.healthAmountFromAmulet} hp / {item.healthRegenRateFromAmulet}s");
            }
            if (item.isRing)
            {
                sb.AppendLine();
                sb.Append($"Crit Chance: {item.critChanceFromRing}%");
                sb.AppendLine();
                sb.Append($"Crit Bonus: {item.increasedDamageFromCritFromRing}%");
            }

            return sb.ToString();
        }
    }
}