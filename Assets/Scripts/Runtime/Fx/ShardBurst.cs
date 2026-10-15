using System.Collections.Generic;
using Runner.Core;
using Runner.World;
using UnityEngine;

namespace Runner.Fx
{
    /// <summary>
    /// A short-lived spray of sprite shards. One component drives a whole burst, so a coin pickup
    /// costs a single pooled object instead of one per particle.
    /// </summary>
    public sealed class ShardBurst : MonoBehaviour
    {
        private const int MaxShards = 24;

        [SerializeField] private WorldController world;

        private readonly List<SpriteRenderer> visuals = new List<SpriteRenderer>();
        private readonly List<Vector3> velocities = new List<Vector3>();
        private readonly List<float> ages = new List<float>();
        private readonly List<float> lifetimes = new List<float>();
        private readonly List<float> spins = new List<float>();

        private Sprite sprite;
        private int activeCount;

        public void Configure(Sprite sharedSprite, WorldController source)
        {
            sprite = sharedSprite;
            world = source;
        }

        public void Play(Color color, int count, float speed, float lifetime, Vector2 sizeRange)
        {
            for (var i = 0; i < count; i++)
            {
                var visual = AcquireSlot();
                var angle = Random.value * Mathf.PI * 2f;
                var magnitude = Random.Range(0.35f, 1f) * speed;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) * 0.8f + 0.4f, 0f);

                visual.transform.position = transform.position;
                visual.transform.rotation = Quaternion.identity;
                visual.color = color;
                visual.size = new Vector2(Random.Range(sizeRange.x, sizeRange.y), Random.Range(sizeRange.x, sizeRange.y));
                visual.sortingOrder = SortOrder.Effect + i;
                visual.enabled = true;

                velocities.Add(direction.normalized * magnitude);
                ages.Add(0f);
                lifetimes.Add(lifetime * Random.Range(0.7f, 1.25f));
                spins.Add(Random.Range(-420f, 420f));
                activeCount++;
            }
        }

        public void Stop()
        {
            for (var i = 0; i < visuals.Count; i++) visuals[i].enabled = false;

            velocities.Clear();
            ages.Clear();
            lifetimes.Clear();
            spins.Clear();
            activeCount = 0;
        }

        private SpriteRenderer AcquireSlot()
        {
            for (var i = 0; i < visuals.Count; i++)
            {
                if (!visuals[i].enabled) return visuals[i];
            }

            if (visuals.Count >= MaxShards) return visuals[0];

            var go = new GameObject("shard");
            go.transform.SetParent(transform, false);
            var visual = go.AddComponent<SpriteRenderer>();
            visual.sprite = sprite;
            visuals.Add(visual);
            return visual;
        }

        private void Update()
        {
            if (activeCount == 0) return;

            var delta = Time.deltaTime;
            var scroll = world != null ? world.ScrollVelocity : Vector2.zero;

            for (var i = 0; i < visuals.Count; i++)
            {
                var visual = visuals[i];
                if (!visual.enabled) continue;

                ages[i] += delta;
                var t = ages[i] / Mathf.Max(0.0001f, lifetimes[i]);

                if (t >= 1f)
                {
                    visual.enabled = false;
                    activeCount--;
                    continue;
                }

                velocities[i] += (new Vector3(0f, -12f, 0f) - velocities[i] * 3.4f) * delta;
                visual.transform.position += (velocities[i] + (Vector3)scroll) * delta;
                visual.transform.Rotate(0f, 0f, spins[i] * delta);

                var color = visual.color;
                color.a = Mathf.Pow(1f - t, 1.6f);
                visual.color = color;
            }

            if (activeCount == 0) EffectFactory.Instance?.Release(gameObject);
        }
    }
}
