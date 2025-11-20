using UnityEngine;

namespace Sebbe
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        protected Enemy enemy;
        protected Rigidbody2D rb;
        protected Collider2D col;
        protected Animator anim;

        [Header("Movement")]
        [SerializeField] protected float patrolSpeed = 1.5f;
        [SerializeField] protected float chaseSpeed = 3f;
        [SerializeField] protected Transform groundCheck;
        [SerializeField] protected Transform wallCheck;
        [SerializeField] protected float groundCheckDistance = 0.2f;
        [SerializeField] [Range(1,7)] protected int groundCheckRays = 3;
        [SerializeField] protected float groundCheckSpread = 0.25f; // world units half-span from center
        [SerializeField] [Range(0f,1f)] protected float groundCheckForwardBias = 0.8f; // 0=behind, 0.5=centered, 1=forward
        [SerializeField] protected float idleBeforeFlip = 0.12f;
        [SerializeField] protected float wallCheckDistance = 0.2f;
        [SerializeField] protected LayerMask groundLayer;

        [Header("Detection & Combat")]
        [SerializeField] protected float detectionRadius = 5f;
        [SerializeField] protected LayerMask playerLayer;
        [SerializeField] protected float attackRange = 1f;
        [SerializeField] protected int attackDamage = 1;
        [SerializeField] protected float attackCooldown = 1f;

        protected Transform targetPlayer;
        protected float lastAttackTime = -999f;

        // track last world position to infer movement direction when scale isn't changing
        protected Vector3 lastPosition;

        protected int facing = 1; // 1 = right, -1 = left

        protected enum State { Patrol, Chase, Attack }
        protected State state = State.Patrol;
        private Coroutine flipCoroutine = null;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            anim = GetComponent<Animator>();
            enemy = GetComponent<Enemy>();

            if (groundCheck == null)
            {
                Debug.LogWarning("EnemyController: groundCheck not assigned.");
            }
            if (wallCheck == null)
            {
                Debug.LogWarning("EnemyController: wallCheck not assigned.");
            }
            // Initialize facing based on current visual localScale.x so ray origins and movement match prefab orientation
            facing = transform.localScale.x >= 0f ? 1 : -1;
            ApplyFacing();
            lastPosition = transform.position;
        }

        void Update()
        {
            SensePlayer();
            UpdateState();
            HandleMovement();
            TryAttack();
            UpdateFacingFromMovement();
        }

        protected virtual void SensePlayer()
        {
            // look for player within detection radius
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
            if (hit != null)
            {
                targetPlayer = hit.transform;
            }
            else
            {
                targetPlayer = null;
            }
        }

        // Update facing based on actual movement (position change). This helps when visual flip
        // is controlled by SpriteRenderer.flipX and transform.localScale isn't toggled.
        protected virtual void UpdateFacingFromMovement()
        {
            float dx = transform.position.x - lastPosition.x;
            float threshold = 0.01f;
            if (Mathf.Abs(dx) > threshold)
            {
                int newFacing = dx > 0f ? 1 : -1;
                if (newFacing != facing)
                {
                    facing = newFacing;
                    ApplyFacing();
                }
            }
            lastPosition = transform.position;
        }

        protected virtual void UpdateState()
        {
            if (targetPlayer == null)
            {
                state = State.Patrol;
                return;
            }

            float dist = Vector2.Distance(transform.position, targetPlayer.position);
            if (dist <= attackRange)
            {
                state = State.Attack;
            }
            else
            {
                state = State.Chase;
            }

            // If we leave Patrol state, cancel pending flip idle
            if (state != State.Patrol && flipCoroutine != null)
            {
                StopCoroutine(flipCoroutine);
                flipCoroutine = null;
                if (anim != null) anim.SetBool("isMoving", Mathf.Abs(rb.linearVelocity.x) > 0.05f);
            }
        }

        protected virtual void HandleMovement()
        {
            if (state == State.Patrol)
            {
                PatrolStep();
            }
            else if (state == State.Chase)
            {
                ChaseStep();
            }
            else
            {
                // Attack: stop horizontal movement
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            // Update animator 'isMoving' based on horizontal speed
            if (anim != null)
            {
                bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.05f;
                anim.SetBool("isMoving", isMoving);
            }
        }

        protected virtual void PatrolStep()
        {
            // Check ground ahead using multiple raycasts spanning in front/behind the groundCheck position
            bool groundAhead = true;
            if (groundCheck != null)
            {
                groundAhead = false;
                int rays = Mathf.Max(1, groundCheckRays);
                // compute center offset based on forward bias (positive = bias toward forward)
                float centerOffset = (groundCheckForwardBias - 0.5f) * 2f * groundCheckSpread;
                // use rotation-only right vector so scale flips don't invert our local offsets
                var rightDirNoScale = (transform.rotation * Vector3.right).normalized;

                bool[] hits = new bool[rays];
                for (int i = 0; i < rays; i++)
                {
                    float t = (rays == 1) ? 0f : (float)i / (rays - 1); // 0..1
                    float localX = Mathf.Lerp(-groundCheckSpread, groundCheckSpread, t) + centerOffset;
                    // Apply facing to orient offsets relative to the visual facing direction
                    Vector3 worldOffset = rightDirNoScale * (localX * facing);
                    Vector2 origin = (Vector2)groundCheck.position + (Vector2)worldOffset;
                    RaycastHit2D ghit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
                    Debug.DrawLine(origin, origin + Vector2.down * groundCheckDistance, ghit.collider != null ? Color.green : Color.red);
                    hits[i] = ghit.collider != null;
                }

                // Evaluate the two most-forward rays (based on facing). If both fail, treat as no ground.
                int forwardIndex = (facing >= 0) ? (rays - 1) : 0;
                int secondIndex = (facing >= 0) ? Mathf.Max(0, rays - 2) : Mathf.Min(rays - 1, 1);
                bool forwardHit = hits[forwardIndex];
                bool secondForwardHit = hits.Length > 1 ? hits[secondIndex] : forwardHit;
                groundAhead = forwardHit || secondForwardHit;
            }

            // Check wall ahead
            bool wallAhead = false;
            if (wallCheck != null)
            {
                // use rotation-only right vector and apply facing so we raycast in the visual forward direction
                Vector2 dir = (Vector2)((transform.rotation * Vector3.right).normalized * facing);
                RaycastHit2D whit = Physics2D.Raycast(wallCheck.position, dir, wallCheckDistance, groundLayer);
                wallAhead = whit.collider != null;
                Debug.DrawLine(wallCheck.position, wallCheck.position + (Vector3)dir * wallCheckDistance, wallAhead ? Color.red : Color.green);
            }

            bool shouldFlip = !groundAhead || wallAhead;
            if (shouldFlip)
            {
                if (flipCoroutine == null)
                {
                    flipCoroutine = StartCoroutine(FlipAfterIdle());
                }
            }
            else
            {
                // cancel pending flip if we regained ground
                if (flipCoroutine != null)
                {
                    StopCoroutine(flipCoroutine);
                    flipCoroutine = null;
                    if (anim != null)
                    {
                        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.05f;
                        anim.SetBool("isMoving", isMoving);
                    }
                }
            }

            // Only apply patrol velocity if there isn't a pending flip (which stops movement first)
            if (flipCoroutine == null)
            {
                rb.linearVelocity = new Vector2(facing * patrolSpeed, rb.linearVelocity.y);
            }
        }

        private System.Collections.IEnumerator FlipAfterIdle()
        {
            // stop horizontal movement and set animator isMoving false
            if (anim != null) anim.SetBool("isMoving", false);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            float wait = Mathf.Max(0f, idleBeforeFlip);
            if (wait > 0f)
            {
                yield return new WaitForSeconds(wait);
            }

            // Only flip if still patrolling
            if (state == State.Patrol)
            {
                Flip();
            }

            flipCoroutine = null;
        }

        protected virtual void ChaseStep()
        {
            if (targetPlayer == null) return;

            float dir = Mathf.Sign(targetPlayer.position.x - transform.position.x);
            facing = dir >= 0 ? 1 : -1;
            ApplyFacing();

            rb.linearVelocity = new Vector2(facing * chaseSpeed, rb.linearVelocity.y);
        }

        protected virtual void TryAttack()
        {
            if (enemy.isDead) return;
            if (state != State.Attack || targetPlayer == null) return;
            if (Time.time - lastAttackTime < attackCooldown) return;

            lastAttackTime = Time.time;
            // Trigger attack animation if animator present
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }
        }

        // Called from an Animation Event at the moment the enemy should apply damage.
        // Add an AnimationEvent in the enemy attack animation that calls this method.
        public void OnAttackHit()
        {
            if (enemy.isDead) return;
            // Try to apply damage to player by calling Player.TakeDamage(float)
            var hits = Physics2D.OverlapCircleAll(transform.position, attackRange, playerLayer);
            foreach (var h in hits)
            {
                var player = h.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage((float)attackDamage);
                    continue;
                }

                // fallback: call any method named TakeDamage (uses float)
                h.SendMessage("TakeDamage", (float)attackDamage, SendMessageOptions.DontRequireReceiver);
            }
        }

        protected void Flip()
        {
            facing *= -1;
            ApplyFacing();
        }

        protected virtual void ApplyFacing()
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * facing;
            transform.localScale = s;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Draw detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Draw ground-check ray origins (use rotation-only right and visual facing so editor tuning matches runtime)
            if (groundCheck != null)
            {
                int rays = Mathf.Max(1, groundCheckRays);
                float centerOffset = (groundCheckForwardBias - 0.5f) * 2f * groundCheckSpread;
                Vector3 rightNoScale = (transform.rotation * Vector3.right).normalized;
                int visualFacing = transform.localScale.x >= 0f ? 1 : -1;
                for (int i = 0; i < rays; i++)
                {
                    float t = (rays == 1) ? 0f : (float)i / (rays - 1);
                    float localX = Mathf.Lerp(-groundCheckSpread, groundCheckSpread, t) + centerOffset;
                    Vector3 worldOffset = rightNoScale * (localX * visualFacing);
                    Vector3 origin = groundCheck.position + worldOffset;

                    // Color: green if a ray would hit during Play mode, yellow otherwise
                    Color col = Color.yellow;
                    if (Application.isPlaying)
                    {
                        RaycastHit2D ghit = Physics2D.Raycast((Vector2)origin, Vector2.down, groundCheckDistance, groundLayer);
                        col = ghit.collider != null ? Color.green : Color.red;
                    }

                    Gizmos.color = col;
                    Gizmos.DrawSphere(origin, 0.03f);
                    Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
                }
            }

            // Draw wall check
            if (wallCheck != null)
            {
                Vector3 dirNoScale = (transform.rotation * Vector3.right).normalized * (transform.localScale.x >= 0f ? 1f : -1f);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(wallCheck.position, wallCheck.position + dirNoScale * wallCheckDistance);
            }

            // Draw attack range
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}