using UnityEngine;

namespace Sebbe
{
    // Flying enemy with simple wander, player detection, chase and attack behavior.
    // Attach this to a prefab with Collider2D + Rigidbody2D (set to kinematic or dynamic as needed).
    public class FlyingEnemy : Enemy
    {
        [Header("Movement")]
        public float wanderRadius = 3f;            // radius (world units) to pick random wander targets around spawn
        public float wanderChangeInterval = 2.0f;  // seconds between picking new wander target
        public float wanderSpeed = 1.5f;
        public float chaseSpeed = 4f;

        [Header("Perception")]
        public float detectionRadius = 6f;         // when the player is within this radius, chase
        public LayerMask visionMask = ~0;          // layer mask used for overlap checks (optional)

        [Header("Attack")]
        public float attackRange = 0.8f;           // distance at which an attack is triggered
        public int attackDamage = 1;
        public float attackCooldown = 1.0f;        // seconds between attacks

        private Vector2 spawnPosition;
        private Vector2 wanderTarget;
        private float nextWanderTime = 0f;
        private float nextAttackTime = 0f;

        private Rigidbody2D rb;
        private Transform playerT;

        private void Awake()
        {
            // Do initialization in Awake so we don't hide Enemy.Start()
            spawnPosition = transform.position;
            wanderTarget = spawnPosition;
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                // add a dynamic rigidbody if none present so movement works reliably
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.simulated = true;
            }
        }

        private void Update()
        {
            // find player if not cached
            if (playerT == null && Player.instance != null)
            {
                playerT = Player.instance.transform;
            }

            // Timers
            if (nextWanderTime <= 0f)
            {
                nextWanderTime = Time.time + wanderChangeInterval;
                PickWanderTarget();
            }

            // Check perception
            bool playerDetected = false;
            float playerDistance = float.MaxValue;
            if (playerT != null)
            {
                playerDistance = Vector2.Distance(transform.position, playerT.position);
                if (playerDistance <= detectionRadius)
                {
                    // Optional: could add line-of-sight check via Raycast
                    playerDetected = true;
                }
            }

            // State handling: Chase if detected, otherwise wander
            if (playerDetected)
            {
                // Move towards player
                Vector2 dir = (playerT.position - transform.position);
                Vector2 vel = dir.normalized * chaseSpeed;
                MoveVelocity(vel);

                // Attack if within range and cooldown passed
                if (playerDistance <= attackRange && Time.time >= nextAttackTime)
                {
                    DoAttackOnPlayer();
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
            else
            {
                // Wander towards wanderTarget
                Vector2 dir = (wanderTarget - (Vector2)transform.position);
                Vector2 vel = dir.normalized * wanderSpeed;

                // Slow down when near target
                if (dir.magnitude < 0.2f) vel = Vector2.zero;

                MoveVelocity(vel);
            }

            // Reset wander timer
            if (Time.time >= nextWanderTime) nextWanderTime = 0f;
        }

        private void MoveVelocity(Vector2 vel)
        {
            if (rb != null)
            {
                rb.linearVelocity = vel;
            }
            else
            {
                transform.Translate(vel * Time.deltaTime, Space.World);
            }
        }

        private void PickWanderTarget()
        {
            Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
            wanderTarget = spawnPosition + randomOffset;
        }

        private void DoAttackOnPlayer()
        {
            // Trigger the attack animation; actual damage will be applied by the animation event calling OnAttackHit().
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }
            else
            {
                // Fallback: apply damage immediately if no animator exists
                if (Player.instance != null)
                {
                    Player.instance.TakeDamage(attackDamage);
                    if (rb != null)
                    {
                        Vector2 away = (transform.position - Player.instance.transform.position).normalized;
                        rb.linearVelocity = away * 1.2f;
                    }
                }
            }
        }

        // Called from an Animation Event on the Attack clip when the attack should land.
        public void OnAttackHit()
        {
            if (Player.instance == null) return;

            float dist = Vector2.Distance(transform.position, Player.instance.transform.position);
            if (dist <= attackRange + 0.5f)
            {
                Player.instance.TakeDamage(attackDamage);
                if (rb != null)
                {
                    Vector2 away = (transform.position - Player.instance.transform.position).normalized;
                    rb.linearVelocity = away * 1.2f;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnPosition, wanderRadius);
        }
    }
}
