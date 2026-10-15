using Runner.Core;
using Runner.World;
using UnityEngine;

namespace Runner.Fx
{
    /// <summary>
    /// Every cosmetic flourish the game plays goes through this one component, so effects can be
    /// disabled wholesale without touching gameplay code.
    /// </summary>
    public sealed class EffectFactory : MonoBehaviour
    {
        [SerializeField] private WorldController world;
        [SerializeField] private ObjectPool pool;
        [SerializeField] private CameraRig cameraRig;

        [Header("Sprites")]
        [SerializeField] private Sprite shardSprite;
        [SerializeField] private GameObject burstPrefab;
        [SerializeField] private GameObject ghostPrefab;

        public static EffectFactory Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (pool == null) pool = GetComponent<ObjectPool>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SpawnBurst(BurstProfile profile, Vector3 position)
        {
            if (pool == null || burstPrefab == null) return;

            var go = pool.Spawn(burstPrefab, position, Quaternion.identity);
            if (go == null) return;

            var burst = go.GetComponent<ShardBurst>();
            if (burst == null) return;

            burst.Configure(shardSprite, world);
            burst.Play(ProfileColor(profile), ShardCount(profile), ShardSpeed(profile), 0.55f, ShardSize(profile));
        }

        /// <summary>Dash afterimage: a translucent copy of the runner left on the road.</summary>
        public void SpawnGhost(SpriteRenderer source, float duration = 0.28f)
        {
            if (pool == null || ghostPrefab == null || source == null) return;

            var go = pool.Spawn(ghostPrefab, source.transform.position, source.transform.rotation);
            if (go == null) return;

            var ghost = go.GetComponent<FadingSprite>();
            if (ghost != null) ghost.Play(source, Palette.Cyan, duration, new Vector3(-1.5f, 0f, 0f), world);
        }

        public void Shake(float amount, float duration) => cameraRig?.Shake(amount, duration);

        public void Release(GameObject instance)
        {
            if (pool == null || instance == null) return;

            var burst = instance.GetComponent<ShardBurst>();
            burst?.Stop();
            instance.GetComponent<FadingSprite>()?.Stop();
            pool.Release(instance);
        }

        private static int ShardCount(BurstProfile profile)
        {
            switch (profile)
            {
                case BurstProfile.Collect: return 8;
                case BurstProfile.Dust: return 5;
                case BurstProfile.Impact: return 14;
                default: return 10;
            }
        }

        private static float ShardSpeed(BurstProfile profile)
        {
            switch (profile)
            {
                case BurstProfile.Collect: return 4.5f;
                case BurstProfile.Dust: return 2.6f;
                case BurstProfile.Impact: return 8f;
                default: return 6f;
            }
        }

        private static Vector2 ShardSize(BurstProfile profile)
        {
            return profile == BurstProfile.Collect
                ? new Vector2(0.16f, 0.34f)
                : new Vector2(0.12f, 0.3f);
        }

        private static Color ProfileColor(BurstProfile profile)
        {
            switch (profile)
            {
                case BurstProfile.Collect: return Palette.Amber;
                case BurstProfile.Dust: return Palette.WithAlpha(Palette.IceBlue, 0.7f);
                case BurstProfile.Impact: return Palette.Damage;
                default: return Palette.HotPink;
            }
        }
    }
}
