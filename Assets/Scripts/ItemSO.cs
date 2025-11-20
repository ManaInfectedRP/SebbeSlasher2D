using UnityEngine;

namespace Sebbe
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Sebbe/Item")]
    public class ItemSO : ScriptableObject
    {
        public string itemName;
        public Sprite itemIcon;
        public int itemID;

        [Header("If Weapon")]
        public bool isWeapon = false;
        public AnimatorOverrideController weaponAnimatorOverride;
        public int weaponDamage;
        public float weaponRange;
        public float attackRate;

    }
}