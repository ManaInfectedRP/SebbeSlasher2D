using UnityEngine;
using System.Collections.Generic;

namespace Sebbe
{
    // Simple singleton to track equipped items and calculate damage reduction/stamina %
    public class EquipmentManager : MonoBehaviour
    {
        public static EquipmentManager instance;

        public ItemSO equippedHelmet;
        public ItemSO equippedArmor;
        public ItemSO equippedBoots;
        public ItemSO equippedAmulet;
        public ItemSO equippedRing;

        // Key item slot (single). Use this when a key needs to be placed into a dedicated slot.
        public ItemSO equippedKeyItem;
        // Publicly accessible damage reduction percent (0-100) calculated from equipped items
        [HideInInspector] public float damageReduction = 0f;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            instance = this;
        }

        // Equip an item and return the previously equipped item for that slot (if any)
        public ItemSO Equip(ItemSO item)
        {
            if (item == null) return null;

            if (item.isHelmet)
            {
                ItemSO previous = equippedHelmet;
                equippedHelmet = item;
                UpdateDamageReduction();
                return previous;
            }
            else if (item.isArmor)
            {
                ItemSO previous = equippedArmor;
                equippedArmor = item;
                UpdateDamageReduction();
                return previous;
            }
            else if (item.isBoots)
            {
                ItemSO previous = equippedBoots;
                equippedBoots = item;
                UpdateDamageReduction();
                return previous;
            }
            else if (item.isAmulet)
            {
                ItemSO previous = equippedAmulet;
                equippedAmulet = item;
                UpdateDamageReduction();
                return previous;
            }
            else if (item.isRing)
            {
                ItemSO previous = equippedRing;
                equippedRing = item;
                UpdateDamageReduction();
                return previous;
            }
            else if (item.isKeyItem)
            {
                ItemSO previous = equippedKeyItem;
                equippedKeyItem = item;
                UpdateDamageReduction();
                return previous;
            }

            return null;
        }

        // Unequip by slot name: "helmet", "armor", "boots". Returns the unequipped item (or null)
        public ItemSO Unequip(string slot)
        {
            slot = slot.ToLower();
            if (slot == "helmet")
            {
                ItemSO prev = equippedHelmet;
                equippedHelmet = null;
                UpdateDamageReduction();
                return prev;
            }
            if (slot == "armor")
            {
                ItemSO prev = equippedArmor;
                equippedArmor = null;
                UpdateDamageReduction();
                return prev;
            }
            if (slot == "boots")
            {
                ItemSO prev = equippedBoots;
                equippedBoots = null;
                UpdateDamageReduction();
                return prev;
            }

            if (slot == "key")
            {
                ItemSO prev = equippedKeyItem;
                equippedKeyItem = null;
                UpdateDamageReduction();
                return prev;
            }
            if (slot == "amulet")
            {
                ItemSO prev = equippedAmulet;
                equippedAmulet = null;
                UpdateDamageReduction();
                return prev;
            }
            if (slot == "ring")
            {
                ItemSO prev = equippedRing;
                equippedRing = null;
                UpdateDamageReduction();
                return prev;
            }

            return null;
        }

        // Calculate a damage reduction percent [0..100] based on equipped defense bonuses.
        // Formula: reduction = totalDefense / (totalDefense + 50) * 100 -> soft diminishing returns.
        public float GetDamageReductionPercent()
        {
            // Keep the calculation centralized and also update the public field
            UpdateDamageReduction();
            return damageReduction;
        }

        // Recalculate damage reduction and store it in the public field
        private void UpdateDamageReduction()
        {
            int totalDefense = 0;
            if (equippedHelmet != null) totalDefense += equippedHelmet.defenseBonus;
            if (equippedArmor != null) totalDefense += equippedArmor.defenseBonus;
            if (equippedBoots != null) totalDefense += equippedBoots.defenseBonus;

            float reduction = 0f;
            if (totalDefense > 0)
            {
                reduction = (totalDefense / (float)(totalDefense + 50)) * 100f;
            }

            damageReduction = Mathf.Clamp(reduction, 0f, 100f);
        }

        // Expose a "stamina %" that maps the damage reduction into a 0-100 value
        // This returns the same number as GetDamageReductionPercent() so other systems
        // can read it as the stamina-preservation percent.
        public float GetStaminaPercent()
        {
            return GetDamageReductionPercent();
        }
    }
}
