using System.Collections.Generic;
using Runner.Interactables;
using UnityEngine;

namespace Runner.World
{
    /// <summary>
    /// Lays out the road. Chunks are placed by distance rather than by time so the spacing stays
    /// readable no matter how fast the world scrolls, and everything that leaves the playfield
    /// goes straight back through the pool.
    /// </summary>
    public sealed class RunnerSpawner : MonoBehaviour
    {
        [SerializeField] private WorldController world;
        [SerializeField] private ObjectPool pool;

        [Header("Road furniture")]
        [SerializeField] private Interactable[] groundHazards;
        [SerializeField] private Interactable[] airHazards;
        [SerializeField] private Coin coinPrefab;

        [Header("Rhythm")]
        [SerializeField] private float openingClearRun = 14f;
        [SerializeField] private float minimumGap = 3.4f;
        [SerializeField] private float gapPerUnitOfSpeed = 0.42f;
        [SerializeField, Range(0f, 1f)] private float coinRunChance = 0.38f;
        [SerializeField, Range(0f, 1f)] private float coinAfterHazardChance = 0.5f;

        private readonly List<Interactable> active = new List<Interactable>();
        private float nextSpawnDistance;

        private void Start()
        {
            if (pool != null && groundHazards.Length + airHazards.Length > 0) return;

            Debug.LogError("Spawner is missing its pool or hazard prefabs; the road will stay empty.", this);
        }

        private void Update()
        {
            if (world == null || !world.Scrolling) return;
            if (world.DistanceTravelled < nextSpawnDistance) return;

            BuildChunk();
            PruneActive();
        }

        /// <summary>Hands every live object back to the pool - used when a run restarts.</summary>
        public void ClearRoad()
        {
            for (var i = active.Count - 1; i >= 0; i--)
            {
                if (active[i] != null) active[i].Releasing();
            }

            active.Clear();
            pool?.ReleaseAll();
            nextSpawnDistance = openingClearRun;
        }

        private void BuildChunk()
        {
            var x = world.SpawnX;
            var difficulty = world.Difficulty;
            float width;

            if (Random.value < coinRunChance && difficulty < 0.9f)
            {
                width = SpawnCoinRun(x);
            }
            else
            {
                width = SpawnHazard(x, difficulty);
                if (Random.value < coinAfterHazardChance) width += SpawnCoinRun(x + width + 1.2f);
            }

            nextSpawnDistance = world.DistanceTravelled + width + minimumGap + world.Speed * gapPerUnitOfSpeed;
        }

        private float SpawnHazard(float x, float difficulty)
        {
            var roll = Random.value;

            if (difficulty > 0.55f && roll < 0.26f)
            {
                var groundWidth = PlaceRandom(groundHazards, x);
                var airWidth = PlaceAt(airHazards, x + groundWidth + 2.1f, world.AirLaneY);
                return groundWidth + airWidth + 2.1f;
            }

            if (difficulty > 0.3f && roll < 0.5f) return PlaceRandom(airHazards, x, world.AirLaneY);

            if (difficulty > 0.45f && roll < 0.78f)
            {
                var travelled = 0f;
                var cluster = Random.Range(2, 4);

                for (var i = 0; i < cluster; i++)
                {
                    travelled += PlaceRandom(groundHazards, x + travelled) + 0.18f;
                }

                return travelled;
            }

            return PlaceRandom(groundHazards, x);
        }

        private float PlaceRandom(IReadOnlyList<Interactable> set, float x, float y = float.NaN)
        {
            if (set == null || set.Count == 0) return 1f;
            return PlaceOne(set[Random.Range(0, set.Count)], x, y);
        }

        private float PlaceAt(IReadOnlyList<Interactable> set, float x, float y = float.NaN)
        {
            if (set == null || set.Count == 0) return 1f;
            return PlaceOne(set[0], x, y);
        }

        /// <summary>
        /// Ground hazards reseat themselves on the road line when they spawn, so a NaN lane means
        /// "let the prefab decide".
        /// </summary>
        private float PlaceOne(Interactable prefab, float x, float y)
        {
            if (prefab == null) return 1f;

            var position = new Vector3(x, float.IsNaN(y) ? world.GroundTopY + 1f : y, 0f);
            var instance = Place(prefab, position);
            return instance != null ? instance.HalfWidth * 2f : prefab.HalfWidth * 2f;
        }

        private Interactable Place(Interactable prefab, Vector3 position)
        {
            if (pool == null || prefab == null) return null;

            var go = pool.Spawn(prefab.gameObject, position, Quaternion.identity);
            if (go == null) return null;

            var instance = go.GetComponent<Interactable>();
            if (instance == null) return null;

            instance.Inject(world, pool);
            instance.Spawned();
            active.Add(instance);
            return instance;
        }

        private float SpawnCoinRun(float x)
        {
            if (coinPrefab == null) return 0f;

            var count = Random.Range(3, 6);
            var spacing = 1.05f;
            var arc = Random.value < 0.55f;

            for (var i = 0; i < count; i++)
            {
                var t = count > 1 ? (float)i / (count - 1) : 0.5f;
                var y = arc
                    ? world.GroundTopY + 1.1f + Mathf.Sin(Mathf.PI * t) * 2.4f
                    : world.AirLaneY - 0.5f;

                Place(coinPrefab, new Vector3(x + i * spacing, y, 0f));
            }

            return count * spacing;
        }

        private void PruneActive()
        {
            for (var i = active.Count - 1; i >= 0; i--)
            {
                if (active[i] != null && active[i].transform.position.x > world.DespawnX) continue;
                active.RemoveAt(i);
            }
        }
    }
}
