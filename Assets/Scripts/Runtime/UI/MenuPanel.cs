using Runner.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Runner.UI
{
    /// <summary>
    /// Shared plumbing for the full-screen overlays: a fade that runs on unscaled time so menus
    /// still animate while the world is frozen, and a single rule for when buttons are clickable.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class MenuPanel : MonoBehaviour
    {
        [SerializeField] protected float fadeSpeed = 7f;

        protected CanvasGroup Group { get; private set; }
        protected bool Visible { get; private set; }
        protected GameManager Game => GameManager.Instance;

        protected abstract Selectable DefaultFocus { get; }

        private GamePhase tracked = GamePhase.MainMenu;

        protected virtual void Awake()
        {
            Group = GetComponent<CanvasGroup>();
            Group.alpha = Visible ? 1f : 0f;
        }

        /// <summary>
        /// Panels watch the phase themselves instead of subscribing to PhaseChanged: a panel that
        /// is enabled before the GameManager has published its instance would otherwise never hear
        /// about a transition, and a pause menu that cannot open is worse than a redundant check.
        /// </summary>
        protected virtual void Update()
        {
            if (Game != null && Game.Phase != tracked)
            {
                tracked = Game.Phase;
                OnPhase(tracked);
            }

            var target = Visible ? 1f : 0f;
            Group.alpha = Mathf.MoveTowards(Group.alpha, target, Time.unscaledDeltaTime * fadeSpeed);
            Group.interactable = Visible && Group.alpha > 0.55f;
            Group.blocksRaycasts = Visible;
        }

        protected void Show(bool value)
        {
            if (Visible == value) return;

            Visible = value;
            if (!value) return;

            Group.alpha = 0f;

            if (DefaultFocus != null && EventSystem.current != null) DefaultFocus.Select();
        }

        protected abstract void OnPhase(GamePhase phase);
    }
}
