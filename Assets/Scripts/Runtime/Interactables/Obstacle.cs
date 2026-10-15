using Runner.Player;
using UnityEngine;

namespace Runner.Interactables
{
    /// <summary>
    /// Anything that costs the runner health on contact. Ground and air hazards differ only in
    /// where they sit and how they move, so those details live in the subclasses.
    /// </summary>
    public abstract class Obstacle : Interactable
    {
        [Header("Hazard")]
        [SerializeField, Min(1)] protected int damage = 1;
        [SerializeField] protected bool destroyOnImpact = true;
        [SerializeField] protected Fx.BurstProfile impactProfile = Fx.BurstProfile.Spark;

        public int Damage => damage;

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!InUse) return;

            var player = other.GetComponentInParent<PlayerCharacter>();
            if (player == null) return;

            Interact(player);
        }

        protected virtual void Interact(PlayerCharacter player)
        {
            if (player == null || player.IsInvulnerable) return;

            player.ReceiveHit(transform.position, damage);

            if (!destroyOnImpact) return;

            Fx.EffectFactory.Instance?.SpawnBurst(impactProfile, transform.position);
            Release();
        }
    }
}
