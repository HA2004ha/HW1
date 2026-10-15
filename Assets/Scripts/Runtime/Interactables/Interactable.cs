using UnityEngine;

namespace Runner.Interactables
{
    /// <summary>
    /// Shared base for everything the runner can meet on the road: hazards and pickups alike.
    /// The base owns the two things every interactable does identically - it rides the world
    /// scroll, and it returns itself to the pool once it has left the play area.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] protected World.WorldController world;
        [SerializeField] protected World.ObjectPool pool;

        protected SpriteRenderer Visual { get; private set; }
        protected Collider2D Shape { get; private set; }
        protected bool InUse { get; private set; }

        /// <summary>Half width of the collider, used by the spawner to keep hazards breathable.</summary>
        public float HalfWidth => Shape != null ? Shape.bounds.extents.x : 0.5f;

        protected virtual void Awake()
        {
            Visual = GetComponentInChildren<SpriteRenderer>();
            Shape = GetComponent<Collider2D>();

            var body = GetComponent<Rigidbody2D>();
            if (body == null) body = gameObject.AddComponent<Rigidbody2D>();

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            if (!InUse) return;
            Step(Time.deltaTime);
            if (world != null && transform.position.x < world.DespawnX - HalfWidth) Release();
        }

        /// <summary>Per-frame motion. Hazards override this to add their own flourish.</summary>
        protected virtual void Step(float deltaTime)
        {
            if (world != null) transform.Translate(world.ScrollVelocity * deltaTime, Space.World);
        }

        /// <summary>Called by the spawner right after the object leaves the pool.</summary>
        public virtual void Spawned()
        {
            InUse = true;
        }

        /// <summary>Called when the object is handed back to the pool.</summary>
        public virtual void Releasing()
        {
            InUse = false;
        }

        public void Inject(World.WorldController targetWorld, World.ObjectPool targetPool)
        {
            world = targetWorld;
            pool = targetPool;
        }

        protected void Release()
        {
            if (!InUse) return;

            Releasing();
            if (pool != null) pool.Release(gameObject);
            else Destroy(gameObject);
        }

        protected void SetColor(Color color)
        {
            if (Visual != null) Visual.color = color;
        }

        /// <summary>
        /// Unity's trigger message, funnelled through the base so every subclass reacts to the
        /// runner in exactly one place.
        /// </summary>
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
        }
    }
}
