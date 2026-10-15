using Runner.World;
using UnityEngine;

namespace Runner.Interactables
{
    /// <summary>
    /// Hazards that live on the road surface: spike strips, crates and ground saws.
    /// The only way past them is leaving the ground or dashing through the i-frames.
    /// </summary>
    public class GroundObstacle : Obstacle
    {
        [Tooltip("Extra height above the road so the sprite sits on the ground line.")]
        [SerializeField] protected float groundOffset = 0f;

        public override void Spawned()
        {
            base.Spawned();
            SnapToGround();
        }

        protected void SnapToGround()
        {
            if (world == null) return;

            var position = transform.position;
            var bottom = Shape != null ? Shape.bounds.min.y - position.y : 0f;
            position.y = world.GroundTopY - bottom + groundOffset;
            transform.position = position;
        }

        protected override void Step(float deltaTime)
        {
            base.Step(deltaTime);
            SnapToGround();
        }
    }
}
