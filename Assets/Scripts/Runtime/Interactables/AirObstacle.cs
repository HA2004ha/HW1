using Runner.World;
using UnityEngine;

namespace Runner.Interactables
{
    /// <summary>
    /// Hazards that occupy the air lane. They punish the reflex of holding jump: the safe play is
    /// to stay on the road and time the jump for the gap beside them.
    /// </summary>
    public class AirObstacle : Obstacle
    {
        [Header("Flight")]
        [SerializeField] protected float bobAmplitude = 0.35f;
        [SerializeField] protected float bobFrequency = 1.6f;
        [SerializeField] protected float spinDegreesPerSecond;
        [SerializeField] protected float laneOffset;

        private float phase;
        private float baseY;

        public override void Spawned()
        {
            base.Spawned();

            if (world == null) return;

            baseY = world.AirLaneY + laneOffset;
            phase = Random.value * Mathf.PI * 2f;

            var position = transform.position;
            position.y = baseY;
            transform.position = position;
        }

        protected override void Step(float deltaTime)
        {
            base.Step(deltaTime);

            if (spinDegreesPerSecond != 0f) transform.Rotate(0f, 0f, spinDegreesPerSecond * deltaTime);
            if (bobAmplitude <= 0f || world == null) return;

            phase += deltaTime * bobFrequency * Mathf.PI * 2f;
            var position = transform.position;
            position.y = baseY + Mathf.Sin(phase) * bobAmplitude;
            transform.position = position;
        }
    }
}
