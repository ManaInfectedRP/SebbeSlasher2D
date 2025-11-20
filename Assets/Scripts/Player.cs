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
        [SerializeField] private Slider healthSlider;



        [Header("Player Damage")]
        public int damage = 2;
        public float attackRange = 1f;
        public float attackRate = 1f;
        [HideInInspector] public float nextAttackTime = 0f;
        [HideInInspector] public bool isAttacking = false;

        
        [Header("Weapon Settings")]
        // `hasObtainedSword` means the player picked up the sword item (persistent pickup).
        // `hasFoundSword` means the player currently has a weapon equipped (used for combat/animation).
        [HideInInspector] public bool hasObtainedSword = false;
        public bool hasFoundSword = false;

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

            // Apply combat stats from item
            damage = weaponItem.weaponDamage;
            attackRange = weaponItem.weaponRange;
            attackRate = weaponItem.attackRate;

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
        }

        private void Start()
        {
            health = maxHealth;
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = health;
            }
            // cache Rigidbody2D if present for knockback application
            rb = GetComponent<Rigidbody2D>();

            // Ensure animator is assigned and set the appropriate controller
            if (animator == null) animator = GetComponent<Animator>();
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

        public void TakeDamage(float damage)
        {
            health -= damage;
            WorldEffectsManager.instance.SpawnBloodSplatter(transform.position, Quaternion.identity);
            if (healthSlider != null)
            {
                healthSlider.value = health;
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

        private void Die()
        {
            // Handle player death here
            Debug.Log("Player has died.");
        }
    }
}