using UnityEngine;

namespace Sebbe
{
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy Stats")]
        public int maxHealth = 3;
        private int currentHealth;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string hitTriggerName = "Hit";
        [SerializeField] private string deathAnimationName = "Dead";
        [SerializeField] private float deathDelay = 2f;
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private Vector3 coinSpawnOffset = new Vector3(0f, 0.2f, 0f);

        [Header("VFX")]
        [SerializeField] private Vector3 hitSpawnOffset = new Vector3(0f, 0.2f, 0f);
        public bool isDead = false;

        void Start()
        {
            currentHealth = maxHealth;
            if (animator == null) animator = GetComponent<Animator>();
        }
        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            Debug.Log($"[Enemy] '{name}' took {damage} damage. Health now {currentHealth}/{maxHealth}.");

            if (currentHealth > 0)
            {
                // spawn hit VFX if assigned
                if (WorldEffectsManager.instance != null)
                {
                    WorldEffectsManager.instance.SpawnSlimeDamageEffect(transform.position + hitSpawnOffset, Quaternion.identity);
                }
                if (animator != null && !string.IsNullOrEmpty(hitTriggerName))
                {
                    animator.SetTrigger(hitTriggerName);
                }
            }
            else
            {
                Die();
            }
        }
        private void Die()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log($"[Enemy] '{name}' died.");

            // Play death animation by setting a bool
            if (animator != null && !string.IsNullOrEmpty(deathAnimationName))
            {
                animator.Play(deathAnimationName);
            }

            // Disable colliders and physics so the dead enemy doesn't interact
            var cols = GetComponents<Collider2D>();
            foreach (var c in cols)
            {
                if (c != null) c.enabled = false;
            }

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = false;
            }

            // Start coroutine to finalize death (spawn coin + destroy after delay)
            StartCoroutine(DeathCoroutine());
        }

        private System.Collections.IEnumerator DeathCoroutine()
        {
            // Wait for the configured delay (allow death animation to play)
            yield return new WaitForSeconds(Mathf.Max(0f, deathDelay));

            // Spawn coin if assigned
            if (coinPrefab != null)
            {
                Vector3 spawnPos = transform.position + coinSpawnOffset;
                Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            }

            // Destroy the enemy GameObject
            Destroy(gameObject);
        }
    }
}
