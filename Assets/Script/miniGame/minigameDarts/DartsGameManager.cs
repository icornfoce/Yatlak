using UnityEngine;
using UnityEngine.Events;

namespace MiniGame.Darts
{
    public enum PlayerID
    {
        Player1, // ฝั่งซ้าย
        Player2  // ฝั่งขวา
    }

    [System.Serializable]
    public class DartsPlayerStats
    {
        public int score = 0;
        public int dartsLeft = 3;
        public int lastThrowScore = 0;
        public bool hasFinishedAllDarts => dartsLeft <= 0;
    }

    /// <summary>
    /// ผู้จัดการมินิเกมปาลูกดอก (Darts 1v1):
    /// - แต่ละคนมี 3 ลูกดอก
    /// - จับเวลาแข่งขัน 60 วินาที
    /// - ใครแต้มเยอะที่สุดจาก 3 ดอกเป็นผู้ชนะ (หรือหมดเวลา)
    /// </summary>
    public class DartsGameManager : MonoBehaviour
    {
        public static DartsGameManager Instance { get; private set; }

        [Header("Match Settings")]
        [Tooltip("จำนวนลูกดอกต่อคน (3 ดอก)")]
        public int maxDartsPerPlayer = 3;

        [Tooltip("เวลาการแข่งขันต่อรอบ (60 วินาที)")]
        public float matchDuration = 60f;

        public float RemainingTime { get; private set; }
        public bool IsPlaying { get; private set; } = false;

        [Header("Player Stats")]
        public DartsPlayerStats player1 = new DartsPlayerStats();
        public DartsPlayerStats player2 = new DartsPlayerStats();

        [Header("Events")]
        public UnityEvent<PlayerID, int, int, int> OnScoreUpdated; // PlayerID, TotalScore, DartsLeft, LastThrowScore
        public UnityEvent<float> OnTimerUpdated;                   // Remaining seconds
        public UnityEvent<string> OnGameOver;                      // Winner text (PLAYER 1 WINS / PLAYER 2 WINS)

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartNewGame();
        }

        private void Update()
        {
            if (!IsPlaying) return;

            RemainingTime -= Time.deltaTime;
            if (RemainingTime < 0f) RemainingTime = 0f;

            OnTimerUpdated?.Invoke(RemainingTime);

            // หมดเวลา
            if (RemainingTime <= 0f)
            {
                EndGame();
            }
        }

        public void StartNewGame()
        {
            RemainingTime = matchDuration;
            IsPlaying = true;

            player1 = new DartsPlayerStats { dartsLeft = maxDartsPerPlayer };
            player2 = new DartsPlayerStats { dartsLeft = maxDartsPerPlayer };

            OnScoreUpdated?.Invoke(PlayerID.Player1, 0, player1.dartsLeft, 0);
            OnScoreUpdated?.Invoke(PlayerID.Player2, 0, player2.dartsLeft, 0);
            OnTimerUpdated?.Invoke(RemainingTime);
        }

        /// <summary>
        /// บันทึกคะแนนเมื่อปาลูกดอกเข้าเป้า
        /// </summary>
        public void RegisterDartHit(PlayerID playerID, int points)
        {
            if (!IsPlaying) return;

            DartsPlayerStats stats = GetStats(playerID);
            if (stats.dartsLeft <= 0) return;

            stats.dartsLeft--;
            stats.lastThrowScore = points;
            stats.score += points;

            OnScoreUpdated?.Invoke(playerID, stats.score, stats.dartsLeft, points);

            // ถ้าทั้ง 2 คนปาครบ 3 ดอกแล้ว ให้จบเกมทันทีโดยไม่ต้องรอให้หมดเวลา
            if (player1.hasFinishedAllDarts && player2.hasFinishedAllDarts)
            {
                EndGame();
            }
        }

        public void EndGame()
        {
            if (!IsPlaying) return;
            IsPlaying = false;

            string winnerMessage;
            if (player1.score > player2.score)
            {
                winnerMessage = "PLAYER 1 WINS!";
            }
            else if (player2.score > player1.score)
            {
                winnerMessage = "PLAYER 2 WINS!";
            }
            else
            {
                winnerMessage = "IT'S A DRAW!";
            }

            Debug.Log($"[DartsGameManager] Game Over! {winnerMessage} (P1: {player1.score} vs P2: {player2.score})");
            OnGameOver?.Invoke(winnerMessage);
        }

        public DartsPlayerStats GetStats(PlayerID playerID)
        {
            return (playerID == PlayerID.Player1) ? player1 : player2;
        }
    }
}
