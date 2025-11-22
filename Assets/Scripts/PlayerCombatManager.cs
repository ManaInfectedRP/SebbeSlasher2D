using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sebbe
{
    public class PlayerCombatManager : MonoBehaviour
    {
        public Player player;

        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask enemyLayers = ~0; // default to all layers to avoid misconfiguration
        [SerializeField] private bool drawGizmos = true;

        [Header("Animation")]
        [SerializeField] private bool useAnimationEvent = true;

        // Animator bool for the attack state and how long to keep it true
        [SerializeField] private string attackBoolName = "isAttacking";
        [SerializeField] private float attackDuration = 0.5f;


        // When using animation events, damage is applied from the animation by calling `OnAttackHit()`.
        private bool attackPending = false;
        private Coroutine attackBoolCoroutine = null;
        private bool attackInProgress = false;

        private void Start()
        {
            player = GetComponent<Player>();
            if (player == null)
            {
                Debug.LogError("Player component not found on PlayerCombatManager GameObject.");
            }

            if (attackPoint == null)
            {
                attackPoint = transform;
            }
        }

        private void Update()
        {
            if (player == null) return;

            // Decrement cooldown timer (player.nextAttackTime is seconds remaining)
            if (player.nextAttackTime > 0f)
            {
                player.nextAttackTime = Mathf.Max(0f, player.nextAttackTime - Time.deltaTime);
            }

            // Left click to attack (only if cooldown finished, not already attacking,
            // the click didn't happen over UI, and the inventory UI is not open).
            // Use GetMouseButton so holding the mouse button will continue attacks.
            if (Input.GetMouseButton(0) &&
                !InventorySystem.instance.inventoryOpen &&
                player.nextAttackTime <= 0f && !attackInProgress)
            {
                // attackRate is attacks per second; compute cooldown as 1 / rate
                float cooldown = 1f / Mathf.Max(0.0001f, player.attackRate);
                player.nextAttackTime = cooldown; // set immediately to block further input
                attackInProgress = true;
                DoAttack();
            }
        }

        private void DoAttack()
        {
            // Mark player and animator as attacking and set the attack bool for the configured duration
            if (player != null) player.isAttacking = true;

            // Set the attack bool for the configured duration
            if (player.animator != null && !string.IsNullOrEmpty(attackBoolName))
            {
                player.animator.SetBool(attackBoolName, true);
                if (attackBoolCoroutine != null)
                {
                    StopCoroutine(attackBoolCoroutine);
                }
                attackBoolCoroutine = StartCoroutine(ResetAttackBoolCoroutine());
            }

            if (useAnimationEvent)
            {
                // Defer damage application to the animation event
                attackPending = true;
                return;
            }

            // Immediate damage (no animation event)
            // If the currently equipped weapon is a ranged weapon, spawn a projectile instead
            if (player != null && player.equippedWeapon != null && player.equippedWeapon.isRangedWeapon)
            {
                SpawnProjectile();
            }
            else
            {
                PerformAttackDamage();
            }
        }

        // Called from an animation event (or elsewhere) to end the attack early
        public void OnAttackEnd()
        {
            if (attackBoolCoroutine != null)
            {
                StopCoroutine(attackBoolCoroutine);
                attackBoolCoroutine = null;
            }

            if (player.animator != null && !string.IsNullOrEmpty(attackBoolName))
            {
                player.animator.SetBool(attackBoolName, false);
            }

            // clear combat flags
            attackPending = false;
            attackInProgress = false;
            if (player != null) player.isAttacking = false;
        }

        private IEnumerator ResetAttackBoolCoroutine()
        {
            float wait = Mathf.Max(0f, attackDuration);
            if (wait > 0f)
            {
                yield return new WaitForSeconds(wait);
            }

            if (player.animator != null && !string.IsNullOrEmpty(attackBoolName))
            {
                player.animator.SetBool(attackBoolName, false);
            }

            // If we were using animation events but none fired, apply damage as a fallback
            if (useAnimationEvent && attackPending)
            {
                // If ranged weapon is equipped, spawn projectile as fallback
                if (player != null && player.equippedWeapon != null && player.equippedWeapon.isRangedWeapon)
                {
                    SpawnProjectile();
                }
                else
                {
                    PerformAttackDamage();
                }
                attackPending = false;
            }

            attackBoolCoroutine = null;
            // clear combat flags when coroutine ends
            attackPending = false;
            attackInProgress = false;
            if (player != null) player.isAttacking = false;
        }

        // This method performs the actual damage application. It can be called from an
        // animation event (named `OnAttackHit`) or used directly when `useAnimationEvent` is false.
        public void OnAttackHit()
        {
            if (!attackPending && useAnimationEvent)
            {
                // If using animation events but there was no pending attack, ignore.
                return;
            }

            // If the currently equipped weapon is ranged, spawn a projectile. Otherwise apply melee damage.
            if (player != null && player.equippedWeapon != null && player.equippedWeapon.isRangedWeapon)
            {
                SpawnProjectile();
            }
            else
            {
                PerformAttackDamage();
            }

            attackPending = false;
        }

        // AnimationEvent-friendly overload: allows the animator to pass a damage value.
        // Create an Animation Event that calls `OnAttackHitFloat` and set the float parameter.
        public void OnAttackHitFloat(float damage)
        {
            if (!attackPending && useAnimationEvent)
            {
                // Ignore spurious events when no attack is pending
                return;
            }

            // If ranged weapon equipped, spawn projectile with provided damage value
            if (player != null && player.equippedWeapon != null && player.equippedWeapon.isRangedWeapon)
            {
                SpawnProjectile((int)damage);
            }
            else
            {
                // Use the provided damage value instead of the player's default damage
                PerformAttackDamage((int)damage);
            }

            attackPending = false;
        }

        // Spawn a projectile for the player's equipped ranged weapon.
        // If overrideDamage is -1, use the weapon's configured damage.
        public void SpawnProjectile(int overrideDamage = -1)
        {
            if (player == null) return;

            ItemSO weapon = player.equippedWeapon;
            if (weapon == null || !weapon.isRangedWeapon || weapon.projectilePrefab == null)
            {
                // Fallback to melee
                PerformAttackDamage(overrideDamage >= 0 ? overrideDamage : (player != null ? player.damage : 0));
                return;
            }

            Vector3 spawnPos = attackPoint != null ? attackPoint.position : player.transform.position;

            // Fire strictly in the direction the player is facing (left/right).
            float facingSign = Mathf.Sign(player.transform.localScale.x);
            if (facingSign == 0f) facingSign = 1f; // default to right
            Vector2 dir = new Vector2(facingSign, 0f);

            // If the projectile prefab defines a spawnHeight, offset spawn position vertically
            float prefabSpawnHeight = 0f;
            Projectile prefabProjectileComp = weapon.projectilePrefab.GetComponent<Projectile>();
            if (prefabProjectileComp != null)
            {
                prefabSpawnHeight = prefabProjectileComp.spawnHeight;
            }

            Vector3 finalSpawnPos = spawnPos + new Vector3(0f, prefabSpawnHeight, 0f);

            GameObject projGO = Instantiate(weapon.projectilePrefab, finalSpawnPos, Quaternion.identity);

            Projectile proj = projGO.GetComponent<Projectile>();
            int baseDamage = overrideDamage >= 0 ? overrideDamage : weapon.weaponDamage;
            bool wasCrit = false;
            int dmgToDeal = baseDamage;

            // Apply ring crit chance/damage to projectiles as well
            if (player != null && player.equippedRing != null && player.equippedRing.isRing)
            {
                float critChance = player.equippedRing.critChanceFromRing;
                float critBonus = player.equippedRing.increasedDamageFromCritFromRing;
                if (Random.value <= Mathf.Clamp01(critChance / 100f))
                {
                    dmgToDeal = Mathf.CeilToInt(baseDamage * (1f + critBonus / 100f));
                    wasCrit = true;
                }
            }

            if (proj != null)
            {
                proj.Initialize(dir.normalized, weapon.projectileSpeed, dmgToDeal, wasCrit);
            }
            else
            {
                // If the prefab lacks a Projectile script, try to set Rigidbody2D velocity
                Rigidbody2D r = projGO.GetComponent<Rigidbody2D>();
                if (r != null)
                {
                    r.linearVelocity = dir.normalized * weapon.projectileSpeed;
                }
                // Schedule destruction to avoid orphaned objects
                Destroy(projGO, 5f);
            }
        }

        private void PerformAttackDamage()
        {
            PerformAttackDamage(player != null ? player.damage : 0);
        }

        private void PerformAttackDamage(int damage)
        {
            float range = Mathf.Max(0.01f, player != null ? player.attackRange : 0.5f);
            // Find all enemies in range on the specified layers
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayers);

            if (hitColliders == null || hitColliders.Length == 0)
            {
                return;
            }

            foreach (var col in hitColliders)
            {
                if (col == null) continue;
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    int finalDamage = damage;
                    // Apply ring crit chance/damage if a ring is equipped
                    bool wasCrit = false;
                    if (player != null && player.equippedRing != null && player.equippedRing.isRing)
                    {
                        float critChance = player.equippedRing.critChanceFromRing;
                        float critBonus = player.equippedRing.increasedDamageFromCritFromRing;
                        if (Random.value <= Mathf.Clamp01(critChance / 100f))
                        {
                            finalDamage = Mathf.CeilToInt(damage * (1f + critBonus / 100f));
                            wasCrit = true;
                            Debug.Log($"Critical hit! {damage} -> {finalDamage} (bonus {critBonus}%)");
                        }
                    }

                    enemy.TakeDamage(finalDamage);

                    // Spawn floating text at enemy position
                    if (FloatingTextManager.instance != null)
                    {
                        Color c = wasCrit ? new Color(1f, 0.5f, 0f) : Color.red; // orange for crit, red otherwise
                        FloatingTextManager.instance.Spawn(finalDamage.ToString(), enemy.transform.position + Vector3.up * 1f, c, wasCrit, 1.2f);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || attackPoint == null) return;
            Gizmos.color = Color.red;
            float r = 1f;
            var p = GetComponent<Player>();
            if (p != null) r = p.attackRange;
            Gizmos.DrawWireSphere(attackPoint.position, r);
        }

    }
}
