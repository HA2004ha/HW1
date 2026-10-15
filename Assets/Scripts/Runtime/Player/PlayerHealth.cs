using System;
using Runner.Core;
using UnityEngine;

namespace Runner.Player
{
    /// <summary>
    /// The three hearts the runner is allowed to lose. Also owns the post-hit grace period: the
    /// sprite burns red for a beat and then flickers, which is how the player reads "that hit was
    /// free" without any UI.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 3;
        [SerializeField] private float invulnerableSeconds = 1.25f;
        [SerializeField] private float flashSeconds = 0.3f;
        [SerializeField, Min(0.05f)] private float flickerInterval = 0.07f;
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer visual;

        public int Current { get; private set; }
        public int Max => maxHealth;
        public bool IsInvulnerable { get; private set; }
        public bool IsDepleted => Current <= 0;

        public event Action<int, int> Changed;
        public event Action Damaged;
        public event Action Depleted;

        private Color baseColor = Color.white;
        private float invulnerableTimer;
        private float flashTimer;
        private float flickerTimer;
        private bool opaque = true;

        private void Awake()
        {
            if (player == null) player = GetComponent<PlayerCharacter>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (visual == null) visual = GetComponentInChildren<SpriteRenderer>();
            if (visual != null) baseColor = visual.color;
        }

        private void OnEnable() => Restore();

        private void Update()
        {
            if (!IsInvulnerable) return;

            var delta = Time.deltaTime;
            invulnerableTimer -= delta;
            flashTimer -= delta;
            flickerTimer -= delta;

            if (flickerTimer <= 0f)
            {
                flickerTimer = flickerInterval;
                opaque = !opaque;
            }

            if (invulnerableTimer > 0f)
            {
                ApplyTint();
                return;
            }

            IsInvulnerable = false;
            if (visual != null) visual.color = baseColor;
        }

        private void ApplyTint()
        {
            if (visual == null) return;

            var tint = flashTimer > 0f ? Color.Lerp(baseColor, Palette.Damage, 0.85f) : baseColor;
            tint.a = opaque ? 1f : 0.28f;
            visual.color = tint;
        }

        public void TakeDamage(int amount = 1)
        {
            if (IsInvulnerable || IsDepleted) return;

            Current = Mathf.Max(0, Current - amount);
            IsInvulnerable = true;
            invulnerableTimer = invulnerableSeconds;
            flashTimer = flashSeconds;
            flickerTimer = 0f;
            opaque = true;

            ApplyTint();
            if (animator != null) animator.SetTrigger(AnimatorHash.Hit);

            Changed?.Invoke(Current, Max);
            Damaged?.Invoke();

            if (Current <= 0)
            {
                if (animator != null) animator.SetTrigger(AnimatorHash.Death);
                Depleted?.Invoke();
            }
        }

        public void Restore()
        {
            Current = maxHealth;
            IsInvulnerable = false;
            invulnerableTimer = 0f;
            flashTimer = 0f;
            if (visual != null) visual.color = baseColor;
            Changed?.Invoke(Current, Max);
        }
    }
}
