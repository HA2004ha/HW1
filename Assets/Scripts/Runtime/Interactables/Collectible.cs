using Runner.Player;
using UnityEngine;

namespace Runner.Interactables
{
    /// <summary>
    /// Anything the runner wants to touch: coins, batteries, shields. Shares the trigger plumbing
    /// with hazards but resolves into a reward instead of damage.
    /// </summary>
    public abstract class Collectible : Interactable
    {
        [SerializeField, Min(0)] protected int value = 10;

        public int Value => value;

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!InUse) return;

            var player = other.GetComponentInParent<PlayerCharacter>();
            if (player == null) return;

            Collect(player);
        }

        protected virtual void Collect(PlayerCharacter player)
        {
            player.EarnScore(value);
            Fx.EffectFactory.Instance?.SpawnBurst(Fx.BurstProfile.Collect, transform.position);
            Release();
        }
    }
}
