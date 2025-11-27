using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sebbe
{
    public class Player : MonoBehaviour
    {
        public static Player instance;
        [HideInInspector] public PlayerInventory inventory;
        [HideInInspector] public PlayerCombatManager combatManager;


        [Header("Player Settings")]
        private float health;
        [SerializeField] private float maxHealth = 100f;

        private float mana;
        [SerializeField] private float maxMana = 100f;

        private float stamina;
        [SerializeField] private float maxStamina = 100f;


        [Header("Player Damage")]
        public int damage = 2;
        public float attackRange = 1f;
        public float attackRate = 1f;
        public float nextAttackTime = 0f;
        [HideInInspector] public bool isAttacking = false;

        
        [Header("Weapon Settings")]
        // `hasObtainedSword` means the player picked up the sword item (persistent pickup).
        // `hasFoundSword` means the player currently has a weapon equipped (used for combat/animation).
        [HideInInspector] public bool hasObtainedSword = false;
        public bool hasFoundSword = false;
        
        // Reference to the currently equipped ItemSO (null when unequipped)
        [HideInInspector] public ItemSO equippedWeapon;
        [HideInInspector] public ItemSO equippedHelmet;
        [HideInInspector] public ItemSO equippedArmorItem;
        [HideInInspector] public ItemSO equippedBootsItem;
        [HideInInspector] public ItemSO equippedKeyItem;
        [HideInInspector] public ItemSO equippedAmulet;
        [HideInInspector] public ItemSO equippedRing;
        // True when a key item is currently equipped in the key slot
        [HideInInspector] public bool hasEquippedKey = false;
        // Mirror of equipment manager's damage reduction percent for quick access
        [HideInInspector] public float damageReductionPercent = 0f;
        // Amulet regen bookkeeping
        private float nextAmuletHealTime = 0f;
        [HideInInspector] public bool hasEquippedAmulet = false;
        // Instance of the amulet visual/effect attached to the player when an amulet with effect is equipped
        private GameObject activeAmuletEffect = null;

        [Header("Animator")]
        [HideInInspector] public Animator animator;
        [Tooltip("Assign the default runtime animator controller (used when the player has the sword).")]
        [SerializeField] private RuntimeAnimatorController defaultAnimatorController;
        [Tooltip("Assign the Player_NW AnimatorOverrideController (used when player has no weapon).")]
        [SerializeField] private AnimatorOverrideController playerNWOverrideController;
        // Store base combat stats so we can restore them on unequip
        private int baseDamage;
        private float baseAttackRange;
        private float baseAttackRate;
        void Awake()
        {
            inventory = GetComponent<PlayerInventory>();
            combatManager = GetComponent<PlayerCombatManager>();
            animator = GetComponent<Animator>();

            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            // Cache base stats
            baseDamage = damage;
            baseAttackRange = attackRange;
            baseAttackRate = attackRate;

            health = maxHealth;
            mana = maxMana;
        }

        // Public helper: other systems can query whether the player can attack/slide
        public bool CanUseWeapon() { return hasFoundSword; }

        // Call this when the player picks up the sword. It flips the flag and switches the animator controller.
        public void FoundSword()
        {
            // Mark that the player has acquired the sword item. Do not auto-equip here.
            if (hasObtainedSword) return;
            hasObtainedSword = true;
            Debug.Log("Player picked up the sword. Equip it from the inventory to use it.");
        }

        // Equip a weapon by applying the item's animator override and stats.
        public void EquipWeapon(ItemSO weaponItem)
        {
            if (weaponItem == null)
            {
                Debug.LogWarning("EquipWeapon called with null ItemSO.");
                return;
            }

            if (animator == null) animator = GetComponent<Animator>();

            // Store reference to equipped ItemSO
            equippedWeapon = weaponItem;

            // Apply combat stats from item
            damage = weaponItem.weaponDamage;
            attackRange = weaponItem.weaponRange;
            attackRate = weaponItem.attackRate;
            nextAttackTime = weaponItem.attackRate;

            WorldStatsManager.instance.UpdateAttackStatsUI();

            hasFoundSword = true;
            if (animator != null) animator.SetBool("hasFoundSword", true);

            if (weaponItem.weaponAnimatorOverride != null && animator != null)
            {
                animator.runtimeAnimatorController = weaponItem.weaponAnimatorOverride;
            }
            else if (defaultAnimatorController != null && animator != null)
            {
                // Fallback to the default controller if no override provided
                animator.runtimeAnimatorController = defaultAnimatorController;
            }
        }

        // Unequip currently equipped weapon: revert animation state and controller.
        public void UnequipWeapon()
        {
            hasFoundSword = false;
            // Clear equipped reference
            equippedWeapon = null;
            if (animator == null) animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("hasFoundSword", false);
                if (defaultAnimatorController != null)
                {
                    animator.runtimeAnimatorController = defaultAnimatorController;
                }
            }
            // Restore base combat stats
            damage = baseDamage;
            attackRange = baseAttackRange;
            attackRate = baseAttackRate;
            
            WorldStatsManager.instance.UpdateAttackStatsUI();  
        }

        // Equip equipment item (helmet/armor/boots/key items). Mirrors weapon equip flow.
        public void EquipEquipment(ItemSO equipmentItem)
        {
            if (equipmentItem == null)
            {
                Debug.LogWarning("EquipEquipment called with null ItemSO.");
                return;
            }

            if (EquipmentManager.instance == null)
            {
                Debug.LogWarning("EquipmentManager.instance not found in scene.");
                return;
            }

            ItemSO previous = EquipmentManager.instance.Equip(equipmentItem);

            // Update local references for quick access
            if (equipmentItem.isHelmet) equippedHelmet = equipmentItem;
            if (equipmentItem.isArmor) equippedArmorItem = equipmentItem;
            if (equipmentItem.isBoots) equippedBootsItem = equipmentItem;
            if (equipmentItem.isKeyItem) equippedKeyItem = equipmentItem;
            if (equipmentItem.isKeyItem) hasEquippedKey = true;
            if (equipmentItem.isAmulet) { equippedAmulet = equipmentItem; hasEquippedAmulet = true; }
            if (equipmentItem.isRing) { equippedRing = equipmentItem; }

            // Instantiate amulet effect if this amulet provides one
            if (equipmentItem.isAmulet && equipmentItem.hasEffect && equipmentItem.amuletEffectPrefab != null)
            {
                // Destroy any existing effect first
                if (activeAmuletEffect != null)
                {
                    Destroy(activeAmuletEffect);
                    activeAmuletEffect = null;
                }

                // Instantiate the effect as a child of the player so it follows position/rotation
                activeAmuletEffect = Instantiate(equipmentItem.amuletEffectPrefab, transform);
                if (activeAmuletEffect != null)
                {
                    if (equipmentItem.overrideAmuletEffectTransform)
                    {
                        activeAmuletEffect.transform.localPosition = equipmentItem.amuletEffectLocalPosition;
                        activeAmuletEffect.transform.localRotation = Quaternion.Euler(equipmentItem.amuletEffectLocalRotationEuler);
                        activeAmuletEffect.transform.localScale = equipmentItem.amuletEffectLocalScale;
                    }
                    else
                    {
                        // Use the prefab's transform as authored. When instantiated with a parent,
                        // Unity preserves world position by default for this overload; leaving it as-is
                        // keeps the prefab's designed offset/rotation relative to the player.
                    }
                }
            }

            // If there was a previously equipped item, add it back to the player's inventory
            if (previous != null && inventory != null)
            {
                inventory.AddItem(previous.itemID);
            }

            // Update mirrored damage reduction value
            if (EquipmentManager.instance != null)
            {
                damageReductionPercent = EquipmentManager.instance.GetDamageReductionPercent();
            }

            // Refresh UI/stats displays
            if (WorldStatsManager.instance != null)
            {
                WorldStatsManager.instance.UpdatePlayerUI();
            }
        }

        // Unequip equipment by slot name: "helmet", "armor", "boots"
        public void UnequipEquipment(string slotName)
        {
            if (EquipmentManager.instance == null)
            {
                Debug.LogWarning("EquipmentManager.instance not found in scene.");
                return;
            }

            ItemSO unequipped = EquipmentManager.instance.Unequip(slotName);
            if (unequipped == null)
            {
                Debug.LogWarning($"No equipment found in slot '{slotName}' to unequip.");
                return;
            }

            // Clear local references
            if (slotName.ToLower() == "helmet") equippedHelmet = null;
            if (slotName.ToLower() == "armor") equippedArmorItem = null;
            if (slotName.ToLower() == "boots") equippedBootsItem = null;
            if (slotName.ToLower() == "key") equippedKeyItem = null;
            if (slotName.ToLower() == "key") hasEquippedKey = false;
            if (slotName.ToLower() == "amulet")
            {
                equippedAmulet = null;
                hasEquippedAmulet = false;
                // Destroy visual/effect attached by the amulet
                if (activeAmuletEffect != null)
                {
                    Destroy(activeAmuletEffect);
                    activeAmuletEffect = null;
                }
            }
            if (slotName.ToLower() == "ring") { equippedRing = null; }

            // Add back to player's inventory
            if (inventory != null)
            {
                inventory.AddItem(unequipped.itemID);
            }

            // Update mirrored damage reduction value
            if (EquipmentManager.instance != null)
            {
                damageReductionPercent = EquipmentManager.instance.GetDamageReductionPercent();
            }

            // Refresh UI/stats displays
            if (WorldStatsManager.instance != null)
            {
                WorldStatsManager.instance.UpdatePlayerUI();
            }
        }

        private void Start()
        {
            // cache Rigidbody2D if present for knockback application
            rb = GetComponent<Rigidbody2D>();

            // Ensure animator is assigned and set the appropriate controller
            if (animator == null) animator = GetComponent<Animator>();
        }

        private void LateUpdate()
        {
            // Handle amulet-based health regeneration as periodic ticks
            if (hasEquippedAmulet && equippedAmulet != null && equippedAmulet.healthRegenFromAmulet)
            {
                float interval = Mathf.Max(0.1f, equippedAmulet.healthRegenRateFromAmulet);
                if (Time.time >= nextAmuletHealTime)
                {
                    float amount = equippedAmulet.healthAmountFromAmulet;
                    health = Mathf.Min(maxHealth, health + amount);
                    if (WorldStatsManager.instance != null)
                        WorldStatsManager.instance.UpdateHealthUI();
                    nextAmuletHealTime = Time.time + interval;

                    // Spawn green floating text above player for heal
                    if (FloatingTextManager.instance != null)
                    {
                        Color healColor = Color.green;
                        FloatingTextManager.instance.Spawn($"+{amount}", transform.position + Vector3.up * 1.2f, healColor, false, 1f);
                    }
                }
            }
        }

        public void Update()
        {
            // Keep the animator parameter in sync with equipped state.
            if (animator != null)
            {
                animator.SetBool("hasFoundSword", hasFoundSword);
            }
        }

        [HideInInspector] public Rigidbody2D rb;
        // Last damage amount applied to player (after reduction). -1 means no damage taken yet.
        [HideInInspector] public float lastDamageTaken = -1f;

        public void TakeDamage(float damage)
        {
            // If player controller indicates invulnerability (e.g., during knockback), ignore damage
            var pcCheck = GetComponent<PlayerController>();
            if (pcCheck != null && pcCheck.IsInvulnerable()) return;

            // Apply damage reduction from equipped armor (percentage 0-100)
            float reductionPercent = 0f;
            if (EquipmentManager.instance != null)
            {
                reductionPercent = EquipmentManager.instance.GetDamageReductionPercent();
            }

            float damageAfterReduction = damage * (1f - Mathf.Clamp01(reductionPercent / 100f));

            health -= damageAfterReduction;

            if (WorldEffectsManager.instance != null)
            {
                WorldEffectsManager.instance.SpawnBloodSplatter(transform.position, Quaternion.identity);
            }

            if (WorldStatsManager.instance != null)
            {
                WorldStatsManager.instance.UpdateHealthUI();
            }

            Debug.Log($"Player took {damageAfterReduction} damage (reduced from {damage} by {reductionPercent}%).");

            // Record last damage taken for UI display
            lastDamageTaken = damageAfterReduction;

            // Spawn floating text above player for damage taken
            if (FloatingTextManager.instance != null)
            {
                Color dmgColor = Color.red;
                FloatingTextManager.instance.Spawn(((int)Mathf.Ceil(damageAfterReduction)).ToString(), transform.position + Vector3.up * 1.2f, dmgColor, false, 1.2f);
            }

            if (health <= 0)
            {
                Die();
            }
        }

        // Apply an immediate knockback velocity to the player.
        // This overwrites the player's current horizontal and vertical velocity.
        public void ApplyKnockback(Vector2 velocity)
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = velocity;
            }
        }

        #region Getters for WorldStatsManager
        public float GetCurrentHealth()
        {
            return health;
        }

        public float GetMaxHealth()
        {
            return maxHealth;
        }

        public float GetCurrentMana()
        {
            return mana;
        }
        public float GetMaxMana()
        {
            return maxMana;
        }
        
        public float GetCurrentStamina()
        {
            return stamina;
        }

        public float GetMaxStamina()
        {
            return maxStamina;
        }

        public int GetDamage()
        {
            return damage;
        }

        public float GetAttackRange()
        {
            return attackRange;
        }

        public float GetAttackRate()
        {
            return attackRate;
        }

        #endregion

        private void Die()
        {
            // Handle player death here
            Debug.Log("Player has died.");
        }
    }
}