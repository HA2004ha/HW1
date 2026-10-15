using System;
using Runner.World;
using UnityEngine;

namespace Runner.Core
{
    /// <summary>
    /// Turns distance travelled and collected chips into a score, and keeps the record alive
    /// between sessions through PlayerPrefs.
    /// </summary>
    public sealed class ScoreManager : MonoBehaviour
    {
        [SerializeField] private WorldController world;
        [SerializeField] private int pointsPerWorldUnit = 2;
        [SerializeField] private string highScoreKey = "Runner.HighScore";
        [SerializeField] private string runCountKey = "Runner.Runs";

        public int Current { get; private set; }
        public int HighScore { get; private set; }
        public bool SetNewRecord { get; private set; }

        public event Action<int> Scored;
        public event Action<int> CurrentChanged;
        public event Action<int> HighScoreChanged;

        private float distanceCredit;

        private void Awake()
        {
            HighScore = Mathf.Max(0, PlayerPrefs.GetInt(highScoreKey, 0));
            HighScoreChanged?.Invoke(HighScore);
        }

        private void Update()
        {
            if (world == null || !world.Scrolling) return;

            distanceCredit += world.Speed * Time.deltaTime * pointsPerWorldUnit;
            var whole = Mathf.FloorToInt(distanceCredit);
            if (whole <= 0) return;

            distanceCredit -= whole;
            AddScore(whole);
        }

        public void AddScore(int amount)
        {
            if (amount <= 0) return;

            Current += amount;
            Scored?.Invoke(amount);
            CurrentChanged?.Invoke(Current);

            if (Current <= HighScore) return;

            HighScore = Current;
            SetNewRecord = true;
            PlayerPrefs.SetInt(highScoreKey, HighScore);
            PlayerPrefs.Save();
            HighScoreChanged?.Invoke(HighScore);
        }

        public void BeginRun()
        {
            Current = 0;
            distanceCredit = 0f;
            SetNewRecord = false;
            PlayerPrefs.SetInt(runCountKey, PlayerPrefs.GetInt(runCountKey, 0) + 1);
            CurrentChanged?.Invoke(Current);
            HighScoreChanged?.Invoke(HighScore);
        }

        /// <summary>Called once the death sequence finishes so the record is banked exactly once.</summary>
        public void CommitRun()
        {
            PlayerPrefs.SetInt(highScoreKey, HighScore);
            PlayerPrefs.Save();
        }
    }
}
