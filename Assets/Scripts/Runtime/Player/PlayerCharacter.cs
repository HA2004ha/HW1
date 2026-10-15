using System;
using Runner.Core;
using Runner.Fx;
using Runner.Input;
using Runner.World;
using UnityEngine;

namespace Runner.Player
{
    /// <summary>
    /// The runner itself: horizontal lean inside a fixed window, coyote-timed jumping, a dashing
    /// burst with i-frames, and the squash-and-stretch that makes all of it readable.
    /// Nothing here reads devices - input arrives through <see cref="GameInputManager"/> events.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerCharacter : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private GameInputManager input;
        [SerializeField] private WorldController world;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private ScoreManager score;
        [SerializeField] private SpriteRenderer visual;

        [Header("Strafe")]
        [SerializeField] private float strafeSpeed = 7.5f;
        [SerializeField] private Vector2 xWindow = new Vector2(-6.5f, 2.2f);

        [Header("Jump")]
        [SerializeField] private float jumpVelocity = 15.5f;
        [SerializeField] private float baseGravityScale = 3.6f;
        [SerializeField] private float fallingGravityScale = 4.8f;
        [SerializeField] private float releasedGravityScale = 9f;
        [SerializeField] private float coyoteWindow = 0.1f;
        [SerializeField] private float jumpBufferWindow = 0.12f;
        [SerializeField, Range(1, 3)] private int jumpsPerAirborne = 1;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 26f;
        [SerializeField] private float dashDuration = 0.16f;
        [SerializeField] private float dashCooldown = 0.55f;
        [SerializeField] private float ghostInterval = 0.03f;

        [Header("Reaction")]
        [SerializeField] private Vector2 knockback = new Vector2(-3.5f, 5f);
        [SerializeField] private float landSquashSpeed = 14f;

        public bool IsGrounded { get; private set; }
        public bool IsDashing { get; private set; }
        public bool IsAlive { get; private set; } = true;
        public bool IsControllable { get; private set; }
        public bool IsInvulnerable => health != null && health.IsInvulnerable;
        public float VerticalVelocity => body != null ? body.linearVelocity.y : 0f;
        public bool IsRising => VerticalVelocity > 0.5f;
        public WorldController World => world;
        public SpriteRenderer Visual => visual;

        private Rigidbody2D body;
        private Transform root;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private float dashTimer;
        private float dashCooldownTimer;
        private float ghostTimer;
        private int jumpsUsed;
        private float squash;
        private Vector3 restoredScale = Vector3.one;

        public event Action Jumped;
        public event Action Landed;
        public event Action Dashed;

        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            root = transform;

            body.gravityScale = baseGravityScale;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        protected virtual void OnEnable()
        {
            if (input != null)
            {
                input.JumpPressed += OnJumpPressed;
                input.DashPressed += OnDashPressed;
            }
        }

        protected virtual void OnDisable()
        {
            if (input != null)
            {
                input.JumpPressed -= OnJumpPressed;
                input.DashPressed -= OnDashPressed;
            }
        }

        private void Update()
        {
            if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;
            if (ghostTimer > 0f) ghostTimer -= Time.deltaTime;

            if (IsDashing)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f) EndDash();
            }

            if (!IsGrounded && coyoteTimer > 0f) coyoteTimer -= Time.deltaTime;
            if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;

            ApplySquash();
        }

        private void FixedUpdate()
        {
            var velocity = body.linearVelocity;

            velocity.x = ResolveHorizontal(velocity.x);
            body.gravityScale = ResolveGravity();
            body.linearVelocity = velocity;

            // After the write-back, not before: a jump launched inside TryConsumeBufferedJump pushes
            // on the rigidbody itself, and assigning the stale snapshot afterwards would erase it.
            TryConsumeBufferedJump();
            ClampToWindow();
        }

        private float ResolveHorizontal(float current)
        {
            if (!IsControllable || !IsAlive) return 0f;

            if (IsDashing) return dashSpeed;

            var lean = input != null ? input.MoveAxis : 0f;
            return current + (lean * strafeSpeed - current) * Mathf.Min(1f, Time.fixedDeltaTime * 12f);
        }

        private float ResolveGravity()
        {
            if (IsDashing) return 0f;
            if (!IsGrounded && IsRising && input != null && !input.JumpHeld) return releasedGravityScale;
            if (!IsGrounded && !IsRising) return fallingGravityScale;
            return baseGravityScale;
        }

        private void TryConsumeBufferedJump()
        {
            if (jumpBufferTimer <= 0f || !IsControllable || !IsAlive) return;

            var canUseCoyote = coyoteTimer > 0f && jumpsUsed == 0;
            if (!IsGrounded && !canUseCoyote && jumpsUsed >= jumpsPerAirborne) return;

            LaunchJump();
        }

        private void LaunchJump()
        {
            var velocity = body.linearVelocity;
            velocity.y = jumpVelocity;
            body.linearVelocity = velocity;

            IsGrounded = false;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            jumpsUsed++;
            squash = -0.22f;

            Jumped?.Invoke();
            EffectFactory.Instance?.SpawnBurst(BurstProfile.Dust, FootPosition());
        }

        private void OnJumpPressed()
        {
            if (!IsControllable || !IsAlive) return;
            jumpBufferTimer = jumpBufferWindow;
        }

        private void OnDashPressed()
        {
            if (!IsControllable || !IsAlive || IsDashing || dashCooldownTimer > 0f) return;

            IsDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashDuration + dashCooldown;
            ghostTimer = 0f;
            squash = 0.3f;

            Dashed?.Invoke();
        }

        private void EndDash()
        {
            IsDashing = false;
            var velocity = body.linearVelocity;
            velocity.x = strafeSpeed * 0.6f;
            body.linearVelocity = velocity;
        }

        private void ClampToWindow()
        {
            var position = root.position;
            position.x = Mathf.Clamp(position.x, xWindow.x, xWindow.y);
            root.position = position;
        }

        private void OnCollisionEnter2D(Collision2D collision) => RegisterGround(collision);
        private void OnCollisionStay2D(Collision2D collision) => RegisterGround(collision);

        private void RegisterGround(Collision2D collision)
        {
            for (var i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y <= 0.55f) continue;

                var wasAirborne = !IsGrounded;
                var impact = body.linearVelocity.y;

                IsGrounded = true;
                coyoteTimer = coyoteWindow;
                jumpsUsed = 0;

                if (!wasAirborne || impact > -4f) return;

                squash = Mathf.Clamp(impact * 0.02f, -0.3f, -0.12f);
                Landed?.Invoke();
                EffectFactory.Instance?.SpawnBurst(BurstProfile.Dust, FootPosition());
                return;
            }
        }

        private void ApplySquash()
        {
            squash = Mathf.MoveTowards(squash, 0f, Time.deltaTime * landSquashSpeed);

            var stretch = Mathf.Clamp(-body.linearVelocity.y * 0.012f, -0.12f, 0.16f);
            var x = 1f - stretch - squash;
            var y = 1f + stretch + squash;

            root.localScale = new Vector3(Mathf.Max(0.4f, x), Mathf.Max(0.4f, y), 1f);

            if (IsDashing && ghostTimer <= 0f && visual != null)
            {
                EffectFactory.Instance?.SpawnGhost(visual);
                ghostTimer = ghostInterval;
            }
        }

        private Vector3 FootPosition() => root.position + new Vector3(0f, visual != null ? visual.bounds.min.y - visual.transform.position.y : -0.5f, 0f);

        public void ReceiveHit(Vector3 from, int damage)
        {
            if (!IsAlive || IsInvulnerable) return;

            var velocity = body.linearVelocity;
            velocity.x = knockback.x;
            velocity.y = knockback.y;
            body.linearVelocity = velocity;

            squash = 0.25f;
            EffectFactory.Instance?.SpawnBurst(BurstProfile.Impact, from);
            health?.TakeDamage(damage);
        }

        public void EarnScore(int value) => score?.AddScore(value);

        /// <summary>Menus and the game-over screen take over control, so the body must stop dead.</summary>
        public void SetControllable(bool value)
        {
            IsControllable = value;
            if (!value)
            {
                var velocity = body.linearVelocity;
                velocity.x = 0f;
                body.linearVelocity = velocity;
                if (IsDashing) EndDash();
            }
        }

        public void Restore()
        {
            IsAlive = true;
            IsGrounded = false;
            IsDashing = false;
            jumpsUsed = 0;
            squash = 0f;
            dashTimer = 0f;
            dashCooldownTimer = 0f;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;

            root.localScale = restoredScale;
            body.gravityScale = baseGravityScale;
            body.linearVelocity = Vector2.zero;
            body.position = new Vector2(xWindow.x + 1.5f, world != null ? world.GroundTopY + 1f : 0f);
        }

        public void Die()
        {
            IsAlive = false;
            SetControllable(false);
        }
    }
}
