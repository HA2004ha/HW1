using Runner.World;
using UnityEngine;

namespace Runner.Fx
{
    /// <summary>
    /// A copy of something that fades out while it rides the world scroll. Used for the dash
    /// afterimages and for the puff left behind when a hazard shatters.
    /// </summary>
    public sealed class FadingSprite : MonoBehaviour
    {
        private SpriteRenderer visual;
        private WorldController world;
        private float lifetime;
        private float age;
        private Vector3 drift;

        public void Play(SpriteRenderer source, Color tint, float duration, Vector3 driftVelocity, WorldController scrollSource)
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();

            visual.sprite = source.sprite;
            visual.flipX = source.flipX;
            visual.sortingLayerID = source.sortingLayerID;
            visual.sortingOrder = source.sortingOrder - 1;
            visual.color = tint;
            visual.enabled = true;

            transform.localScale = source.transform.localScale;
            lifetime = duration;
            age = 0f;
            drift = driftVelocity;
            world = scrollSource;
        }

        public void Stop()
        {
            age = lifetime;
            if (visual != null) visual.enabled = false;
        }

        private void Update()
        {
            age += Time.deltaTime;
            var t = lifetime <= 0f ? 1f : Mathf.Clamp01(age / lifetime);

            if (t >= 1f)
            {
                EffectFactory.Instance?.Release(gameObject);
                return;
            }

            if (world != null) transform.Translate(world.ScrollVelocity * Time.deltaTime, Space.World);
            transform.Translate(drift * Time.deltaTime, Space.World);

            var color = visual.color;
            color.a = (1f - t) * 0.75f;
            visual.color = color;
        }
    }
}
