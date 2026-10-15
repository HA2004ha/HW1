using System;
using UnityEngine;

namespace Runner.World
{
    /// <summary>
    /// Owns everything that defines "how fast the world moves" plus the shared horizontal
    /// landmarks the spawner, the parallax rig and the despawn rules all agree on.
    /// </summary>
    public sealed class WorldController : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private float startSpeed = 9f;
        [SerializeField] private float maxSpeed = 21f;
        [SerializeField] private float speedRampPerSecond = 0.18f;

        [Header("World landmarks")]
        [Tooltip("World Y of the walkable surface.")]
        [SerializeField] private float groundTopY = -2.6f;
        [Tooltip("Y band that airborne hazards occupy - jumping into it is what costs health.")]
        [SerializeField] private float airLaneY = 1.15f;
        [SerializeField] private float spawnX = 13f;
        [SerializeField] private float despawnX = -13.5f;
        [SerializeField] private float viewLeftX = -10f;

        public static WorldController Instance { get; private set; }

        public float Speed { get; private set; }
        public bool Scrolling { get; private set; }
        public float DistanceTravelled { get; private set; }

        public Vector2 ScrollVelocity => new Vector2(-Speed, 0f);
        public float GroundTopY => groundTopY;
        public float AirLaneY => airLaneY;
        public float SpawnX => spawnX;
        public float DespawnX => despawnX;
        public float ViewLeftX => viewLeftX;
        public float MaxSpeed => maxSpeed;

        /// <summary>0 at the start of a run, 1 once the world has reached its top speed.</summary>
        public float Difficulty => maxSpeed <= startSpeed
            ? 1f
            : Mathf.Clamp01((Speed - startSpeed) / (maxSpeed - startSpeed));

        public event Action<WorldController> ScrollSpeedChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable() => ResetRun();

        private void Update()
        {
            if (!Scrolling) return;

            var previous = Speed;
            Speed = Mathf.Min(maxSpeed, Speed + speedRampPerSecond * Time.deltaTime);
            DistanceTravelled += Speed * Time.deltaTime;

            if (!Mathf.Approximately(previous, Speed)) ScrollSpeedChanged?.Invoke(this);
        }

        public void ResetRun()
        {
            Speed = startSpeed;
            DistanceTravelled = 0f;
            Scrolling = false;
            ScrollSpeedChanged?.Invoke(this);
        }

        public void SetScrolling(bool enabled) => Scrolling = enabled;

        /// <summary>Freezes the world mid-run, used by the pause menu and the death sequence.</summary>
        public void PauseScrolling() => Scrolling = false;
    }
}
