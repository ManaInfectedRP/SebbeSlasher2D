using UnityEngine;

namespace Sebbe
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Sebbe/Item")]
    public class ItemSO : ScriptableObject
    {
        public string itemName;
        [TextArea] public string itemDescription;
        public Sprite itemIcon;
        public int itemID;

        public bool isStackable = false;
        public int maxStackSize = 1;

        [Header("If Weapon")]
        public bool isWeapon = false;

        public AnimatorOverrideController weaponAnimatorOverride;
        public int weaponDamage;
        public float weaponRange;
        public float attackRate;

        [Header("Ranged Weapon Settings")]
        public bool isRangedWeapon = false;
        public GameObject projectilePrefab;
        public float projectileSpeed;

        [Header("Equipment Settings")]
        public bool isEquipment = false;
        public bool isArmor = false;
        public bool isHelmet = false;
        public bool isBoots = false;
        public int defenseBonus;
        public float weight;

        [Header("Accessory Settings")]
        public bool isAmulet = false;
        public bool healthRegenFromAmulet = false;
        public float healthRegenRateFromAmulet;
        public float healthAmountFromAmulet;
        
        public bool isRing = false;
        public float increasedDamageFromCritFromRing;
        public float critChanceFromRing;


        [Header("Consumable Settings")]
        public bool isConsumable = false;
        public int healthRestore;
        public int staminaRestore;

        [Header("Misc Settings")]
        public bool isKeyItem = false;

    }
}