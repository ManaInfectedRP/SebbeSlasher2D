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
    }
}