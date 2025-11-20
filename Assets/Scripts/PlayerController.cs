using UnityEngine;

namespace Sebbe{
    public class PlayerController : MonoBehaviour
    {
        private Player player;
        [HideInInspector] public Rigidbody2D rb;
        [HideInInspector] public Animator anim;

        [Header("Player Settings")]

        [SerializeField] private float speed = 5f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float moveAmount = 0f;

        [Header("Hold To Jump Settings")]
        [SerializeField] private float maxJumpTime = 0.3f;
        [SerializeField] private float holdForce = 3f;
        private bool isJumping;
        private float jumpTimeCounter;
        [SerializeField] private float apexVelocityThreshold = 0f; // when vertical velocity falls below or equal this, consider at apex (0 = start falling)
        private bool apexTriggered = false;
        private bool wasGrounded = true;


        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.1f;
        public LayerMask groundLayer;

        [Header("Ground Check Timing")]
        [SerializeField] private float groundIgnoreAfterJump = 0.05f;
        private float groundIgnoreTimer = 0f;

        private float facingDirection = 1;
        private float horizontal;
        private float vertical;

        [Header("Sprinting")]
        [SerializeField] private float sprintMultiplier = 1.6f;
        private bool isSprinting = false;
        [SerializeField] private string sprintBoolName = "isSprinting";
        [SerializeField] private float sprintJumpMultiplier = 1.25f;

        [Header("Sliding")]
        [SerializeField] private float slideSpeed = 8f;
        [SerializeField] private float slideDuration = 0.5f;
        private bool isSliding = false;
        private float slideTimer = 0f;
        private float slideDirection = 1f;
        [SerializeField] private string slideBoolName = "isSliding";

        [Header("Climbing")]
        public bool isClimbing = false;
        public bool canClimb = false;
        // keep track of how many ladder triggers we're inside so overlapping ladders don't cancel climbing
        private int climbContactCount = 0;
        [SerializeField] private float climbSpeed = 3f;
        private float originalGravityScale = 1f;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            player = GetComponent<Player>();
            if (rb != null) originalGravityScale = rb.gravityScale;
        }

        void Update()
        {
            //Movement Input
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            // If the player is currently attacking, block movement input
            if (player != null && player.isAttacking)
            {
                horizontal = 0f;
                // allow vertical for climb input if already climbing, otherwise block
                if (!isClimbing) vertical = 0f;
            }
            moveAmount = Mathf.Abs(horizontal);

            // Start climbing only when in climb area and the player presses Vertical
            if (!isClimbing && canClimb && Mathf.Abs(vertical) > 0.1f)
            {
                isClimbing = true;
                // zero vertical velocity so climb starts cleanly
                if (rb != null) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }

            // If player leaves climb area while climbing, stop climbing
            if (isClimbing && !canClimb)
            {
                isClimbing = false;
            }

            // Climbing animator bool
            if (anim != null) anim.SetBool("isClimbing", isClimbing);

            // Sprinting: hold LeftShift/RightShift while moving horizontally (disabled while climbing)
            bool sprintKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            isSprinting = !isClimbing && sprintKey && Mathf.Abs(horizontal) > 0.1f;
            if (anim != null) anim.SetBool(sprintBoolName, isSprinting);

            // When climbing, disable jumping and suspend gravity
            if (isClimbing)
            {
                if (rb != null) rb.gravityScale = 0f;
            }
            else
            {
                if (rb != null) rb.gravityScale = originalGravityScale;
            }

            // Start slide when player presses LeftCtrl while moving and grounded
            if (!isClimbing && !isSliding && IsGrounded() && Mathf.Abs(horizontal) > 0.1f && Input.GetKeyDown(KeyCode.LeftControl))
            {
                isSliding = true;
                slideTimer = slideDuration;
                slideDirection = Mathf.Sign(horizontal);
                // stop sprint when sliding
                isSprinting = false;
                if (anim != null) anim.SetBool(slideBoolName, true);
                // apply immediate slide velocity
                if (rb != null) rb.linearVelocity = new Vector2(slideDirection * slideSpeed, rb.linearVelocity.y);
            }

            // handle slide timer
            if (isSliding)
            {
                slideTimer -= Time.deltaTime;
                if (slideTimer <= 0f)
                {
                    isSliding = false;
                    if (anim != null) anim.SetBool(slideBoolName, false);
                }
            }

            anim.SetFloat("moveAmount", moveAmount);
            if (horizontal > .1f && facingDirection < 0 || horizontal < -.1f && facingDirection > 0)
            {
                Flip();
            }

            //Jump Input (disabled while climbing)
            if (!isClimbing && Input.GetButtonDown("Jump") && IsGrounded())
            {
                isJumping = true;
                apexTriggered = false;
                wasGrounded = false;
                groundIgnoreTimer = groundIgnoreAfterJump; // ignore ground checks for a short time
                jumpTimeCounter = maxJumpTime;
                anim.SetTrigger("Player_Jump_Start");
                float appliedJump = jumpForce * (isSprinting ? sprintJumpMultiplier : 1f);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, appliedJump);
            }

            // If currently climbing and player presses Jump, jump off the ladder
            if (isClimbing && Input.GetButtonDown("Jump"))
            {
                isClimbing = false;
                if (rb != null)
                {
                    float appliedJump = jumpForce * (isSprinting ? sprintJumpMultiplier : 1f);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, appliedJump);
                }
            }

            //Continusous Jump
            if (Input.GetButton("Jump") && isJumping == true)
            {
                if (jumpTimeCounter > 0)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, holdForce);
                    jumpTimeCounter -= Time.deltaTime;
                }
                else
                {
                    isJumping = false;
                }
            }

            //Stop Jump
            if (Input.GetButtonUp("Jump"))
            {
                isJumping = false;
                // let the phase detection handle the next animation
            }

            // decrement ground ignore timer
            if (groundIgnoreTimer > 0f)
            {
                groundIgnoreTimer -= Time.deltaTime;
                if (groundIgnoreTimer < 0f) groundIgnoreTimer = 0f;
            }

            // Detect leaving ground (walk off ledge) and trigger falling animation
            bool groundedNow = IsGrounded();
            if (wasGrounded && !groundedNow && !isClimbing)
            {
                wasGrounded = false;
                apexTriggered = false;
                // If the player didn't start a jump (i.e. walked off), go straight to falling animation
                if (!isJumping)
                {
                    anim.SetTrigger("Player_Jump_Idle");
                }
            }

            // Apex detection: when moving upward slows or starts falling
            if (!wasGrounded)
            {
                if (!apexTriggered && rb.linearVelocity.y <= apexVelocityThreshold)
                {
                    apexTriggered = true;
                    anim.SetTrigger("Player_Jump_Idle");
                }

                // Landing detection (ignore for a short time after jump to allow physics to update)
                if (groundIgnoreTimer <= 0f && IsGrounded())
                {
                    wasGrounded = true;
                    apexTriggered = false;
                    anim.SetTrigger("Player_Jump_Landing");
                }
            }
        }
        void FixedUpdate()
        {
            if (isClimbing)
            {
                rb.linearVelocity = new Vector2(horizontal * speed, vertical * climbSpeed);
            }
            else if (isSliding)
            {
                // maintain slide velocity in the initial direction
                rb.linearVelocity = new Vector2(slideDirection * slideSpeed, rb.linearVelocity.y);
            }
            else
            {
                // If attacking, lock horizontal movement
                if (player != null && player.isAttacking)
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                }
                else
                {
                    float currentSpeed = isSprinting ? speed * sprintMultiplier : speed;
                    rb.linearVelocity = new Vector2(horizontal * currentSpeed, rb.linearVelocity.y);
                }
            }
        }

        bool IsGrounded()
        {
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        void OnDrawGizmos()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Draw the gizmo when the object is selected in the editor for easier tweaking
        void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        void Flip()
        {
            facingDirection *= -1;

            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        // Called by Ladder triggers so multiple overlapping ladder colliders are tracked correctly
        public void AddClimbContact()
        {
            climbContactCount = Mathf.Max(0, climbContactCount) + 1;
            canClimb = true;
        }

        public void RemoveClimbContact()
        {
            climbContactCount = Mathf.Max(0, climbContactCount - 1);
            if (climbContactCount <= 0)
            {
                climbContactCount = 0;
                canClimb = false;
                isClimbing = false;
            }
        }
    }
}