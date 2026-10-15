using UnityEngine;

namespace Runner.Interactables
{
    /// <summary>
    /// The staple pickup: a spinning credit chip. The spin is faked by squashing the sprite
    /// horizontally, which keeps it smooth without needing a sprite sheet per coin.
    /// </summary>
    public sealed class Coin : Collectible
    {
        [SerializeField] private float spinFrequency = 1.4f;
        [SerializeField, Range(0.05f, 1f)] private float minimumWidth = 0.18f;
        [SerializeField] private float bobAmplitude = 0.08f;

        private float phase;
        private float baseY;

        public override void Spawned()
        {
            base.Spawned();
            phase = Random.value * Mathf.PI * 2f;
            baseY = transform.position.y;
        }

        protected override void Step(float deltaTime)
        {
            base.Step(deltaTime);

            phase += deltaTime * spinFrequency * Mathf.PI * 2f;
            var width = Mathf.Lerp(minimumWidth, 1f, Mathf.Abs(Mathf.Cos(phase)));
            transform.localScale = new Vector3(width, 1f, 1f);

            if (bobAmplitude <= 0f) return;
            var position = transform.position;
            position.y = baseY + Mathf.Sin(phase * 0.7f) * bobAmplitude;
            transform.position = position;
        }

        public override void Releasing()
        {
            base.Releasing();
            transform.localScale = Vector3.one;
        }
    }
}
