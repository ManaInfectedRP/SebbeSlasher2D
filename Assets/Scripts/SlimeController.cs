using UnityEngine;

namespace Sebbe
{
    public class SlimeController : EnemyController
    {
        private SpriteRenderer sr;
        
        [Header("Slime Specific Settings")]
        [SerializeField] private bool initialFlip = false;
        [SerializeField] private bool invertFlip = false; // when true, visual flip is inverted to match animation direction

        protected override void Awake()
        {
            base.Awake();
            sr = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            if (sr != null)
            {
                // Apply the inspector-configured initial flip (optionally inverted)
                sr.flipX = initialFlip ^ invertFlip;
                facing = (sr.flipX ^ invertFlip) ? -1 : 1; // derive facing so movement logic is consistent
                ApplyFacing();
            }
        }

        protected override void ApplyFacing()
        {
            if (sr != null)
            {
                // use SpriteRenderer.flipX so prefab uses Sprite flip instead of localScale flip
                sr.flipX = facing < 0;
                // also ensure localScale.x matches facing so other systems depending on scale behave as expected
                Vector3 s = transform.localScale;
                s.x = Mathf.Abs(s.x) * facing;
                transform.localScale = s;
            }
            else
            {
                base.ApplyFacing();
            }
        }

        // Ensure visual flip stays in sync with internal facing every frame
        void LateUpdate()
        {
            if (sr == null) return;
            bool shouldFlip = facing < 0;
            // apply inversion if requested so sprite's flip matches animation orientation
            bool visualFlip = shouldFlip ^ invertFlip;
            if (sr.flipX != visualFlip)
            {
                sr.flipX = visualFlip;
            }
        }
    }
}