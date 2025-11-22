using UnityEngine;

namespace Sebbe
{
    // Simple projectile behaviour: moves in a straight line and deals damage on hit.
    public class Projectile : MonoBehaviour
    {
        public int damage = 1;
        public float speed = 6f;
        public float lifetime = 5f;
        // Vertical offset (world units) to apply when spawning this projectile
        // Use this to raise/lower the spawn point relative to the player's attack point or ground.
        public float spawnHeight = 0f;

        private Vector2 direction = Vector2.right;
        private Rigidbody2D rb;
        private bool wasCrit = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            if (lifetime > 0f) Destroy(gameObject, lifetime);
        }

        public void Initialize(Vector2 dir, float spd, int dmg, bool crit = false)
        {
            direction = dir.normalized;
            speed = spd;
            damage = dmg;
            wasCrit = crit;

            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
            else
            {
                // orient transform if no rigidbody
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void Update()
        {
            if (rb == null)
            {
                transform.Translate(direction * speed * Time.deltaTime, Space.World);
            }
            else
            {
                // Rotate the projectile to point in the direction of its velocity for a realistic look
                Vector2 vel = rb.linearVelocity;
                if (vel.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null) return;

            // Ignore triggers that belong to the player
            Player player = other.GetComponent<Player>();
            if (player != null) return;

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                // Spawn floating text at enemy position for projectile hits
                if (FloatingTextManager.instance != null)
                {
                    Color c = wasCrit ? new Color(1f, 0.55f, 0f) : Color.red; // orange for crits
                    FloatingTextManager.instance.Spawn(damage.ToString(), enemy.transform.position + Vector3.up * 1f, c, wasCrit, 1f);
                }
                Destroy(gameObject);
                return;
            }

            // Destroy on hitting anything else (optional: you can filter by layers)
            Destroy(gameObject);
        }
    }
}
