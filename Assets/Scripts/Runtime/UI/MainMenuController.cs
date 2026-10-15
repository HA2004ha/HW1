using Runner.Core;
using Runner.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Runner.UI
{
    /// <summary>
    /// The title screen. Doubles as the record board, because the high score is the only thing
    /// worth showing someone who has not started running yet.
    /// </summary>
    public sealed class MainMenuController : MenuPanel
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text highScoreLabel;
        [SerializeField] private Text runCountLabel;
        [SerializeField] private Text controlsLabel;
        [SerializeField] private ScoreManager score;

        protected override Selectable DefaultFocus => startButton;

        protected override void Awake()
        {
            base.Awake();

            if (score == null) score = FindAnyObjectByType<ScoreManager>();
            if (startButton != null) startButton.onClick.AddListener(StartRun);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);

            Show(true);
        }

        private void Start()
        {
            Refresh();
            WriteControls();
        }

        protected override void OnPhase(GamePhase phase) => Show(phase == GamePhase.MainMenu);

        public void StartRun() => Game?.StartRun();

        private void Quit() => Game?.QuitGame();

        private void Refresh()
        {
            if (score == null) return;

            if (highScoreLabel != null) highScoreLabel.text = $"BEST  {score.HighScore:N0}";
            if (runCountLabel != null)
            {
                var runs = PlayerPrefs.GetInt("Runner.Runs", 0);
                runCountLabel.text = runs > 0 ? $"RUNS  {runs}" : "FIRST RUN";
            }
        }

        private void WriteControls()
        {
            if (controlsLabel == null) return;

            var input = FindAnyObjectByType<GameInputManager>();
            if (input == null)
            {
                controlsLabel.text = "SPACE  jump        SHIFT  dash        ESC  pause";
                return;
            }

            controlsLabel.text =
                $"{input.BindingFor("Jump").ToUpperInvariant()}  jump        " +
                $"{input.BindingFor("Dash").ToUpperInvariant()}  dash        " +
                $"{input.BindingFor("Pause").ToUpperInvariant()}  pause";
        }
    }
}
