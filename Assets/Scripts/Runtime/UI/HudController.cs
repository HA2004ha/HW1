using System.Collections.Generic;
using Runner.Core;
using Runner.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Runner.UI
{
    /// <summary>
    /// The always-on readout: distance score, the record being chased, and the health bar that
    /// turns the abstract "3 lives" rule into something the player can feel.
    /// </summary>
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private ScoreManager score;
        [SerializeField] private PlayerHealth health;

        [Header("Labels")]
        [SerializeField] private Text scoreLabel;
        [SerializeField] private Text highScoreLabel;
        [SerializeField] private Text pickupLabel;

        [Header("Health bar")]
        [SerializeField] private Image healthFill;
        [SerializeField] private List<Image> healthPips = new List<Image>();
        [SerializeField] private Color healthyColor = Palette.Cyan;
        [SerializeField] private Color dangerColor = Palette.Damage;

        [SerializeField] private CanvasGroup group;
        [SerializeField] private float fadeSpeed = 5f;

        private float displayedScore;
        private float pickupFade;

        private void Awake()
        {
            game = game != null ? game : GameManager.Instance;
            if (score == null) score = FindAnyObjectByType<ScoreManager>();
        }

        private void OnEnable()
        {
            if (score != null)
            {
                score.HighScoreChanged += OnHighScoreChanged;
                score.Scored += OnScored;
            }

            if (health != null) health.Changed += OnHealthChanged;
        }

        private void OnDisable()
        {
            if (score != null)
            {
                score.HighScoreChanged -= OnHighScoreChanged;
                score.Scored -= OnScored;
            }

            if (health != null) health.Changed -= OnHealthChanged;
        }

        private void Start()
        {
            if (score != null)
            {
                displayedScore = score.Current;
                SetHighScore(score.HighScore);
            }

            if (health != null) OnHealthChanged(health.Current, health.Max);
        }

        private void Update()
        {
            if (score == null) return;

            // The count-up flourish only belongs to live play; a paused or finished run must show
            // the exact number, otherwise the HUD disagrees with the panel drawn on top of it.
            var step = game != null && game.Phase == GamePhase.Playing
                ? Mathf.Max(12f, score.Current * 0.08f) * Time.unscaledDeltaTime
                : float.MaxValue;
            displayedScore = Mathf.MoveTowards(displayedScore, score.Current, step);
            if (scoreLabel != null) scoreLabel.text = Mathf.RoundToInt(displayedScore).ToString("N0");

            if (pickupFade > 0f) pickupFade -= Time.deltaTime;

            if (pickupLabel != null)
            {
                var color = pickupLabel.color;
                color.a = Mathf.Clamp01(pickupFade / 0.6f);
                pickupLabel.color = color;
            }
        }

        private void LateUpdate()
        {
            if (group == null || game == null) return;

            var target = game.Phase == GamePhase.Playing || game.Phase == GamePhase.GameOver ? 1f : 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime * fadeSpeed);
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        private void OnScored(int delta)
        {
            if (delta < 5 || pickupLabel == null) return;
            pickupLabel.text = $"+{delta}";
            pickupFade = 0.9f;
        }

        private void OnHighScoreChanged(int value) => SetHighScore(value);

        private void SetHighScore(int value)
        {
            if (highScoreLabel != null) highScoreLabel.text = $"BEST  {value:N0}";
        }

        private void OnHealthChanged(int current, int max)
        {
            var ratio = max > 0 ? (float)current / max : 0f;

            if (healthFill != null)
            {
                healthFill.fillAmount = ratio;
                healthFill.color = Color.Lerp(dangerColor, healthyColor, ratio);
            }

            for (var i = 0; i < healthPips.Count; i++)
            {
                if (healthPips[i] == null) continue;
                // The heart sprite carries its own colour, so pips are only ever tinted white or dimmed.
                healthPips[i].color = i < current ? Color.white : Palette.WithAlpha(Color.white, 0.22f);
            }
        }
    }
}
