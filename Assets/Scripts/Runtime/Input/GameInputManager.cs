using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Runner.Input
{
    /// <summary>
    /// The single owner of every InputAction in the project. Nothing else is allowed to read
    /// devices directly; gameplay and UI code subscribe to the events exposed here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameInputManager : MonoBehaviour
    {
        [Tooltip("InputActionAsset authored in the Input System package editor.")]
        [SerializeField] private InputActionAsset actions;

        [Tooltip("Name of the map that drives the runner itself.")]
        [SerializeField] private string gameplayMapName = "Gameplay";

        [Tooltip("Name of the map consumed by InputSystemUIInputModule.")]
        [SerializeField] private string uiMapName = "UI";

        private const string MoveAction = "Move";
        private const string JumpAction = "Jump";
        private const string DashAction = "Dash";
        private const string PauseAction = "Pause";
        private const string RestartAction = "Restart";

        private InputActionMap gameplayMap;
        private InputActionMap uiMap;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction dashAction;
        private InputAction pauseAction;
        private InputAction restartAction;

        public static GameInputManager Instance { get; private set; }

        /// <summary>-1 .. 1 horizontal lean of the runner inside its allowed window.</summary>
        public float MoveAxis { get; private set; }

        public bool JumpHeld { get; private set; }

        public event Action JumpPressed;
        public event Action JumpReleased;
        public event Action DashPressed;
        public event Action PausePressed;
        public event Action RestartPressed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ResolveActions();
        }

        private void OnEnable()
        {
            uiMap?.Enable();

            if (pauseAction != null) pauseAction.performed += OnPausePerformed;
            if (restartAction != null) restartAction.performed += OnRestartPerformed;
        }

        private void OnDisable()
        {
            UnwireGameplay();
            uiMap?.Disable();

            if (pauseAction != null) pauseAction.performed -= OnPausePerformed;
            if (restartAction != null) restartAction.performed -= OnRestartPerformed;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            actions?.Disable();
        }

        private void ResolveActions()
        {
            if (actions == null)
            {
                Debug.LogError($"{name}: no InputActionAsset assigned, input is dead.", this);
                return;
            }

            gameplayMap = actions.FindActionMap(gameplayMapName);
            uiMap = actions.FindActionMap(uiMapName);

            if (gameplayMap == null)
            {
                Debug.LogError($"Input action map '{gameplayMapName}' was not found.", this);
                return;
            }

            moveAction = RequireAction(MoveAction);
            jumpAction = RequireAction(JumpAction);
            dashAction = RequireAction(DashAction);
            pauseAction = RequireAction(PauseAction);
            restartAction = RequireAction(RestartAction);
        }

        private InputAction RequireAction(string actionName)
        {
            var action = gameplayMap.FindAction(actionName, throwIfNotFound: false);
            if (action == null)
            {
                Debug.LogError($"Map '{gameplayMapName}' is missing the '{actionName}' action.", this);
            }

            return action;
        }

        /// <summary>Called by GameManager when a run becomes controllable.</summary>
        public void BeginRun()
        {
            UnwireGameplay();
            gameplayMap?.Enable();

            if (moveAction != null) moveAction.performed += OnMovePerformed;
            if (jumpAction != null)
            {
                jumpAction.started += OnJumpStarted;
                jumpAction.canceled += OnJumpCanceled;
            }

            if (dashAction != null) dashAction.performed += OnDashPerformed;
        }

        /// <summary>Called by GameManager while menus own the screen.</summary>
        public void EndRun()
        {
            UnwireGameplay();
            gameplayMap?.Disable();
            MoveAxis = 0f;
            JumpHeld = false;
        }

        private void UnwireGameplay()
        {
            if (moveAction != null) moveAction.performed -= OnMovePerformed;
            if (jumpAction != null)
            {
                jumpAction.started -= OnJumpStarted;
                jumpAction.canceled -= OnJumpCanceled;
            }

            if (dashAction != null) dashAction.performed -= OnDashPerformed;
        }

        private void OnMovePerformed(InputAction.CallbackContext ctx) => MoveAxis = ctx.ReadValue<float>();
        private void OnJumpStarted(InputAction.CallbackContext ctx) => JumpPressed?.Invoke();
        private void OnJumpCanceled(InputAction.CallbackContext ctx) => JumpReleased?.Invoke();
        private void OnDashPerformed(InputAction.CallbackContext ctx) => DashPressed?.Invoke();
        private void OnPausePerformed(InputAction.CallbackContext ctx) => PausePressed?.Invoke();
        private void OnRestartPerformed(InputAction.CallbackContext ctx) => RestartPressed?.Invoke();

        private void Update()
        {
            if (jumpAction != null && jumpAction.enabled) JumpHeld = jumpAction.IsPressed();
        }

        /// <summary>Human readable binding text, used by the control hints in the menus.</summary>
        public string BindingFor(string actionName)
        {
            var action = gameplayMap != null ? gameplayMap.FindAction(actionName, false) : null;
            if (action == null || action.bindings.Count == 0) return "?";

            for (var i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (binding.isComposite || binding.isPartOfComposite) continue;
                if (InputSystem.FindControls(binding.effectivePath).Count == 0) continue;

                return PrettyPath(binding.effectivePath);
            }

            return PrettyPath(action.bindings[0].effectivePath);
        }

        private static string PrettyPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "?";

            var trimmed = path.Replace("<", string.Empty).Replace(">", string.Empty);
            var parts = trimmed.Split('/');
            var last = parts[parts.Length - 1];
            return char.ToUpperInvariant(last[0]) + last.Substring(1);
        }
    }
}
