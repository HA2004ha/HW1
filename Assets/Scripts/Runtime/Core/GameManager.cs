using System;
using Runner.Input;
using Runner.Player;
using Runner.World;
using UnityEngine;

namespace Runner.Core
{
    /// <summary>
    /// The run's state machine. Menus, the pause overlay and the death sequence are all just
    /// transitions out of Playing, and every one of them has to hand control back to the same
    /// three systems - input, world scroll and the character body.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameInputManager input;
        [SerializeField] private WorldController world;
        [SerializeField] private RunnerSpawner spawner;
        [SerializeField] private ScoreManager score;
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerAnimationDriver animationDriver;
        [SerializeField] private Fx.EffectFactory effects;

        [Header("Death sequence")]
        [SerializeField] private float deathSequenceSeconds = 1.1f;
        [SerializeField] private float deathSlowMotion = 0.35f;

        public static GameManager Instance { get; private set; }

        public GamePhase Phase { get; private set; } = GamePhase.MainMenu;
        public bool IsRunning => Phase == GamePhase.Playing;

        public event Action<GamePhase> PhaseChanged;

        /// <summary>Raised once the death slow-motion has played out and the panel may appear.</summary>
        public event Action RunOver;

        private float deathTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            if (input != null)
            {
                input.PausePressed += TogglePause;
                input.RestartPressed += Restart;
            }

            if (health != null) health.Depleted += HandleDeath;
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.PausePressed -= TogglePause;
                input.RestartPressed -= Restart;
            }

            if (health != null) health.Depleted -= HandleDeath;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            EnterMenu();
        }

        private void Update()
        {
            if (Phase != GamePhase.GameOver || deathTimer <= 0f) return;

            deathTimer -= Time.unscaledDeltaTime;
            if (deathTimer > 0f) return;

            deathTimer = 0f;
            Time.timeScale = 0f;
            RunOver?.Invoke();
        }

        public void EnterMenu()
        {
            Time.timeScale = 1f;
            Phase = GamePhase.MainMenu;
            deathTimer = 0f;

            input?.EndRun();
            world?.ResetRun();
            spawner?.ClearRoad();
            player?.SetControllable(false);
            player?.Restore();
            health?.Restore();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            PhaseChanged?.Invoke(Phase);
        }

        public void StartRun()
        {
            Time.timeScale = 1f;
            Phase = GamePhase.Playing;

            score?.BeginRun();
            spawner?.ClearRoad();
            world?.ResetRun();
            player?.Restore();
            health?.Restore();
            player?.SetControllable(true);
            world?.SetScrolling(true);

            input?.BeginRun();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;

            PhaseChanged?.Invoke(Phase);
        }

        public void TogglePause()
        {
            if (Phase == GamePhase.Paused)
            {
                Resume();
                return;
            }

            if (Phase != GamePhase.Playing) return;

            Phase = GamePhase.Paused;
            Time.timeScale = 0f;
            input?.EndRun();
            world?.PauseScrolling();
            player?.SetControllable(false);
            Cursor.visible = true;
            PhaseChanged?.Invoke(Phase);
        }

        public void Resume()
        {
            if (Phase != GamePhase.Paused) return;

            Phase = GamePhase.Playing;
            Time.timeScale = 1f;
            world?.SetScrolling(true);
            player?.SetControllable(true);
            input?.BeginRun();
            Cursor.visible = false;
            PhaseChanged?.Invoke(Phase);
        }

        public void Restart()
        {
            if (Phase == GamePhase.MainMenu) return;
            Time.timeScale = 1f;
            StartRun();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            EnterMenu();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void HandleDeath()
        {
            if (Phase == GamePhase.GameOver) return;

            Phase = GamePhase.GameOver;
            deathTimer = deathSequenceSeconds;

            player?.Die();
            animationDriver?.PlayDeath();
            world?.SetScrolling(false);
            input?.EndRun();
            effects?.Shake(0.45f, 0.5f);
            score?.CommitRun();

            Time.timeScale = deathSlowMotion;
            Cursor.visible = true;
            PhaseChanged?.Invoke(Phase);
        }
    }
}
