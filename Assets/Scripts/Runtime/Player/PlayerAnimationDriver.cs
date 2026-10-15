using Runner.World;
using UnityEngine;

namespace Runner.Player
{
    /// <summary>
    /// Translates the character's physics state into Animator parameters. Kept apart from
    /// PlayerCharacter so movement maths and animation bookkeeping never tangle together.
    /// </summary>
    public sealed class PlayerAnimationDriver : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private Animator animator;
        [SerializeField] private WorldController world;

        private void Reset()
        {
            player = GetComponent<PlayerCharacter>();
            animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            if (player == null) return;
            player.Jumped += OnJumped;
            player.Landed += OnLanded;
            player.Dashed += OnDashed;
        }

        private void OnDisable()
        {
            if (player == null) return;
            player.Jumped -= OnJumped;
            player.Landed -= OnLanded;
            player.Dashed -= OnDashed;
        }

        private void Update()
        {
            if (player == null || animator == null) return;

            animator.SetBool(AnimatorHash.Grounded, player.IsGrounded);
            animator.SetBool(AnimatorHash.Rising, player.IsRising);
            animator.SetFloat(AnimatorHash.Speed, world != null ? world.Speed : 0f);
        }

        public void PlayDeath()
        {
            if (animator != null) animator.SetTrigger(AnimatorHash.Death);
        }

        private void OnJumped() => animator?.SetTrigger(AnimatorHash.Jump);
        private void OnLanded() => animator?.SetTrigger(AnimatorHash.Land);
        private void OnDashed() => animator?.SetTrigger(AnimatorHash.Dash);
    }
}
