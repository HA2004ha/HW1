using Runner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Runner.UI
{
    /// <summary>
    /// Shown once the death slow-motion has finished. Deliberately separate from the pause menu:
    /// the player is looking at a result here, not at an option to undo it.
    /// </summary>
    public sealed class GameOverPanel : MenuPanel
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Text finalScoreLabel;
        [SerializeField] private Text bestScoreLabel;
        [SerializeField] private Text recordLabel;
        [SerializeField] private ScoreManager score;

        protected override Selectable DefaultFocus => restartButton;

        private bool pending;

        protected override void Awake()
        {
            base.Awake();

            if (score == null) score = FindAnyObjectByType<ScoreManager>();
            if (restartButton != null) restartButton.onClick.AddListener(() => Game?.Restart());
            if (menuButton != null) menuButton.onClick.AddListener(() => Game?.ReturnToMenu());

            Show(false);
        }

        private void OnEnable()
        {
            if (Game != null) Game.RunOver += OnRunOver;
        }

        private void OnDisable()
        {
            if (Game != null) Game.RunOver -= OnRunOver;
        }

        private void OnRunOver() => pending = true;

        protected override void Update()
        {
            if (pending)
            {
                pending = false;
                WriteResult();
                Show(true);
            }

            base.Update();
        }

        protected override void OnPhase(GamePhase phase)
        {
            if (phase == GamePhase.MainMenu || phase == GamePhase.Playing) Show(false);
        }

        private void WriteResult()
        {
            if (score == null) return;

            if (finalScoreLabel != null) finalScoreLabel.text = score.Current.ToString("N0");
            if (bestScoreLabel != null) bestScoreLabel.text = $"BEST  {score.HighScore:N0}";
            if (recordLabel != null) recordLabel.gameObject.SetActive(score.SetNewRecord);
        }
    }
}
