using Runner.Fx;
using Runner.Interactables;
using UnityEditor;
using UnityEngine;

namespace Runner.EditorTools
{
    /// <summary>
    /// Prefabs for everything the spawner recycles. Colliders are triggers because hazards report
    /// contact through OnTriggerEnter2D rather than by physically stopping the runner.
    /// </summary>
    internal static class RunnerPrefabs
    {
        private const string Folder = "Assets/Prefabs";

        public static GameObject Spike { get; private set; }
        public static GameObject Crate { get; private set; }
        public static GameObject Saw { get; private set; }
        public static GameObject Drone { get; private set; }
        public static GameObject Mine { get; private set; }
        public static GameObject Coin { get; private set; }
        public static GameObject Burst { get; private set; }
        public static GameObject Ghost { get; private set; }

        public static void Build()
        {
            SpriteWriter.EnsureFolder(Folder);

            Spike = Hazard<GroundObstacle>("hazard_spike", RunnerPropArt.Spike, 0.86f, 0.72f, 1, 0.2f, 0.1f);
            Crate = Hazard<GroundObstacle>("hazard_crate", RunnerPropArt.Crate, 0.9f, 0.9f, 1, 0.2f, 0.2f);
            Saw = Hazard<GroundSaw>("hazard_saw", RunnerPropArt.Saw, 1.1f, 1.1f, 2, 0.2f, 0.55f);
            Drone = Hazard<AirObstacle>("hazard_drone", RunnerPropArt.Drone, 1.25f, 0.72f, 1, 0.3f, 0f);
            Mine = Hazard<AirObstacle>("hazard_mine", RunnerPropArt.Mine, 0.85f, 0.85f, 2, 0.2f, 0f);

            Coin = Pickup("pickup_coin", RunnerPropArt.Coin, 0.78f, 0.78f, 25);
            Burst = Effect("fx_burst", go => go.AddComponent<ShardBurst>());
            Ghost = Effect("fx_ghost", go =>
            {
                var visual = go.AddComponent<SpriteRenderer>();
                visual.sortingOrder = Runner.Core.SortOrder.Effect;
                go.AddComponent<FadingSprite>();
            });
        }

        private static GameObject Hazard<T>(string name, Sprite sprite, float width, float height,
            int damage, float triggerInset, float laneOffset) where T : Obstacle
        {
            var go = Root(name, sprite, width, height);
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width - triggerInset, height - triggerInset);
            collider.isTrigger = true;

            var obstacle = go.AddComponent<T>();
            var wire = Wire.On(obstacle)
                .Set("damage", damage)
                .Set("destroyOnImpact", true)
                .Set("impactProfile", BurstProfile.Spark);

            if (obstacle is AirObstacle air)
            {
                wire = wire
                    .Set("laneOffset", laneOffset)
                    .Set("bobAmplitude", 0.3f)
                    .Set("bobFrequency", 1.1f)
                    .Set("spinDegreesPerSecond", name.Contains("mine") ? 90f : 0f);
            }

            if (obstacle is GroundSaw saw)
            {
                wire = wire.Set("spinDegreesPerSecond", -520f);
            }

            if (obstacle is GroundObstacle ground)
            {
                wire = wire.Set("groundOffset", 0f);
            }

            wire.Apply();
            return Save(go, name);
        }

        private static GameObject Pickup(string name, Sprite sprite, float width, float height, int value)
        {
            var go = Root(name, sprite, width, height);
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width + 0.25f, height + 0.25f);
            collider.isTrigger = true;

            var coin = go.AddComponent<Coin>();
            Wire.On(coin)
                .Set("value", value)
                .Set("spinFrequency", 1.35f)
                .Set("minimumWidth", 0.16f)
                .Set("bobAmplitude", 0.07f)
                .Apply();

            return Save(go, name);
        }

        private static GameObject Effect(string name, System.Action<GameObject> decorate)
        {
            var go = new GameObject(name);
            decorate(go);
            return Save(go, name);
        }

        private static GameObject Root(string name, Sprite sprite, float width, float height)
        {
            var go = new GameObject(name);
            var visual = go.AddComponent<SpriteRenderer>();
            visual.sprite = sprite;
            visual.sortingOrder = name.StartsWith("pickup")
                ? Runner.Core.SortOrder.Collectible
                : Runner.Core.SortOrder.Obstacle;

            go.transform.localScale = Vector3.one;
            return go;
        }

        private static GameObject Save(GameObject go, string name)
        {
            var path = $"{Folder}/{name}.prefab";
            AssetDatabase.DeleteAsset(path);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }
    }
}
