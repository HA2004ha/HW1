using Runner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Runner.UI
{
    /// <summary>
    /// The pause overlay. The world is frozen by the time this appears, so everything here runs
    /// against unscaled time and the only job left is getting the player back in or out.
    /// </summary>
    public sealed class PauseMenuController : MenuPanel
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Text snapshotLabel;
        [SerializeField] private ScoreManager score;

        protected override Selectable DefaultFocus => resumeButton;

        protected override void Awake()
        {
            base.Awake();

            if (score == null) score = FindAnyObjectByType<ScoreManager>();
            if (resumeButton != null) resumeButton.onClick.AddListener(() => Game?.Resume());
            if (restartButton != null) restartButton.onClick.AddListener(() => Game?.Restart());
            if (menuButton != null) menuButton.onClick.AddListener(() => Game?.ReturnToMenu());

            Show(false);
        }

        protected override void OnPhase(GamePhase phase)
        {
            if (phase != GamePhase.Paused)
            {
                Show(false);
                return;
            }

            if (snapshotLabel != null && score != null)
            {
                snapshotLabel.text = $"SCORE  {score.Current:N0}      BEST  {score.HighScore:N0}";
            }

            Show(true);
        }
    }
}
