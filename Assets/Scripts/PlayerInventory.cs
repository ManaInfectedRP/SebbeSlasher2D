using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Sebbe
{
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private List<ItemSO> currentPlayerItems = new List<ItemSO>();

        public void AddItem(int itemID)
        {
            ItemSO item = WorldItemManager.instance.GetItemByID(itemID);
            if (item == null)
            {
                Debug.LogWarning($"Item with ID {itemID} not found in WorldItemManager.");
                return;
            }
            // Find first empty UI slot
            int emptyIndex = -1;
            for (int i = 0; i < InventorySystem.instance.instantiatedSlots.Count; i++)
            {
                InventorySlot slot = InventorySystem.instance.instantiatedSlots[i].GetComponent<InventorySlot>();
                if (slot != null && slot.IsEmpty())
                {
                    emptyIndex = i;
                    break;
                }
            }

            if (emptyIndex == -1)
            {
                Debug.LogWarning("No empty inventory slot available to add the item.");
                return;
            }

            currentPlayerItems.Add(item);
            InventorySystem.instance.instantiatedSlots[emptyIndex].GetComponent<InventorySlot>().SetItem(item);
            Debug.Log($"Added {item.itemName} to inventory (slot {emptyIndex}).");
        }

        // Return true if the inventory contains at least the required items (list may include duplicates)
        public bool HasItems(List<int> requiredItemIDs)
        {
            if (requiredItemIDs == null || requiredItemIDs.Count == 0) return true;

            var counts = new System.Collections.Generic.Dictionary<int, int>();
            foreach (var id in requiredItemIDs)
            {
                if (!counts.ContainsKey(id)) counts[id] = 0;
                counts[id]++;
            }

            var have = new System.Collections.Generic.Dictionary<int, int>();
            foreach (var item in currentPlayerItems)
            {
                if (!have.ContainsKey(item.itemID)) have[item.itemID] = 0;
                have[item.itemID]++;
            }

            foreach (var kv in counts)
            {
                int id = kv.Key;
                int required = kv.Value;
                int available = have.ContainsKey(id) ? have[id] : 0;
                if (available < required) return false;
            }

            return true;
        }

        // Remove the specified required items from inventory (list may include duplicates). Assumes HasItems was checked.
        public void RemoveItems(List<int> requiredItemIDs)
        {
            if (requiredItemIDs == null || requiredItemIDs.Count == 0) return;

            // For each required id, call RemoveItem which will also update UI slots
            foreach (var id in requiredItemIDs)
            {
                RemoveItem(id);
            }
        }

        public void RemoveItem(int itemID)
        {
            ItemSO item = currentPlayerItems.Find(i => i.itemID == itemID);
            if (item != null)
            {
                // Clear UI slot that shows this item (if any)
                for (int i = 0; i < InventorySystem.instance.instantiatedSlots.Count; i++)
                {
                    InventorySlot slot = InventorySystem.instance.instantiatedSlots[i].GetComponent<InventorySlot>();
                    if (slot != null && slot.GetCurrentItemID() == itemID)
                    {
                        slot.SetItem(null);
                        break;
                    }
                }

                currentPlayerItems.Remove(item);
                Debug.Log($"Removed {item.itemName} from inventory.");
            }
            else
            {
                Debug.LogWarning($"Item with ID {itemID} not found in player inventory.");
            }
        }

        public void InstantiteItemToInventorySlot(int itemID, Transform slotTransform)
        {
            ItemSO item = currentPlayerItems.Find(i => i.itemID == itemID);
            if (item != null)
            {
                InventorySlot invSlot = slotTransform.GetComponent<InventorySlot>();
                if (invSlot != null)
                {
                    invSlot.SetItem(item);
                }
            }
            else
            {
                Debug.LogWarning($"Item with ID {itemID} not found in player inventory.");
            }
        }

        // Equip an item that's currently in the player's inventory.
        // Swaps with currently equipped item for the same equipment slot (if any).
        public void EquipItem(int itemID)
        {
            ItemSO item = currentPlayerItems.Find(i => i.itemID == itemID);
            if (item == null)
            {
                Debug.LogWarning($"Item with ID {itemID} not found in player inventory.");
                return;
            }

            if (!item.isEquipment && !item.isKeyItem)
            {
                Debug.LogWarning($"Item {item.itemName} (ID {itemID}) is not equippable.");
                return;
            }

            // Find the UI slot showing this item (so we can replace it with the swapped item)
            GameObject foundSlotGO = null;
            for (int i = 0; i < InventorySystem.instance.instantiatedSlots.Count; i++)
            {
                InventorySlot slot = InventorySystem.instance.instantiatedSlots[i].GetComponent<InventorySlot>();
                if (slot != null && slot.GetCurrentItemID() == itemID)
                {
                    foundSlotGO = InventorySystem.instance.instantiatedSlots[i];
                    break;
                }
            }

            // Equip via EquipmentManager
            ItemSO previous = null;
            if (EquipmentManager.instance != null)
            {
                previous = EquipmentManager.instance.Equip(item);
            }
            else
            {
                Debug.LogWarning("EquipmentManager.instance not found in scene.");
            }

            // Remove the item from inventory
            currentPlayerItems.Remove(item);

            // If there was a previously equipped item, add it back into inventory and into the same UI slot if available
            if (previous != null)
            {
                currentPlayerItems.Add(previous);
                if (foundSlotGO != null)
                {
                    InventorySlot uiSlot = foundSlotGO.GetComponent<InventorySlot>();
                    if (uiSlot != null)
                        uiSlot.SetItem(previous);
                }
            }
            else
            {
                // No previous item; clear the UI slot that showed this item
                if (foundSlotGO != null)
                {
                    InventorySlot uiSlot = foundSlotGO.GetComponent<InventorySlot>();
                    if (uiSlot != null)
                        uiSlot.SetItem(null);
                }
            }

            Debug.Log($"Equipped {item.itemName}" + (previous != null ? $" (swapped with {previous.itemName})" : ""));
        }

        // Unequip an item by equipment slot name ("helmet", "armor", "boots").
        // Attempts to place the unequipped item back into an empty inventory slot.
        public void UnequipItem(string slotName)
        {
            if (EquipmentManager.instance == null)
            {
                Debug.LogWarning("EquipmentManager.instance not found in scene.");
                return;
            }

            ItemSO unequipped = EquipmentManager.instance.Unequip(slotName);
            if (unequipped == null)
            {
                Debug.LogWarning($"No item equipped in slot '{slotName}' to unequip.");
                return;
            }

            // Find first empty UI slot
            int emptyIndex = -1;
            for (int i = 0; i < InventorySystem.instance.instantiatedSlots.Count; i++)
            {
                InventorySlot slot = InventorySystem.instance.instantiatedSlots[i].GetComponent<InventorySlot>();
                if (slot != null && slot.IsEmpty())
                {
                    emptyIndex = i;
                    break;
                }
            }

            if (emptyIndex == -1)
            {
                Debug.LogWarning("No empty inventory slot available to place unequipped item.");
                // Still add to the logical inventory so player doesn't lose the item
                currentPlayerItems.Add(unequipped);
                return;
            }

            currentPlayerItems.Add(unequipped);
            InventorySystem.instance.instantiatedSlots[emptyIndex].GetComponent<InventorySlot>().SetItem(unequipped);
            Debug.Log($"Unequipped {unequipped.itemName} into slot {emptyIndex}.");
        }

        // Return the current stamina % (0-100) derived from equipped armor's damage reduction.
        public float GetStaminaPercent()
        {
            if (EquipmentManager.instance == null) return 0f;
            return EquipmentManager.instance.GetStaminaPercent();
        }
    }
}