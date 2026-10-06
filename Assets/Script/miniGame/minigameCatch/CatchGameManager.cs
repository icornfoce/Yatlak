using System;
using UnityEngine;
using UnityEngine.Events;

namespace MiniGame.Catch
{
    [System.Serializable]
    public class PlayerStats
    {
        public int score = 0;
        public int combo = 0;
        public int maxCombo = 0;
        public int missedCount = 0;
        public int caughtCount = 0;
        public int bombsHit = 0;
    }

    /// <summary>
    /// ตัวจัดการเกม osu!catch แข่งขัน 2 ผู้เล่น (แบบจับเวลา 60 วินาที + ผลไม้พิเศษ & ระเบิด)
    /// </summary>
    public class CatchGameManager : MonoBehaviour
    {
        public static CatchGameManager Instance { get; private set; }

        [Header("Game Timer")]
        [Tooltip("ระยะเวลาการแข่งขันต่อรอบ (วินาที) เช่น 60 วินาที")]
        public float matchDuration = 60f;
        public float RemainingTime { get; private set; }
        public bool IsPlaying { get; private set; } = false;

        [Header("Player Stats")]
        public PlayerStats player1 = new PlayerStats();
        public PlayerStats player2 = new PlayerStats();

        [Header("Events")]
        public UnityEvent<PlayerID, int, int> OnScoreOrComboChanged; // PlayerID, Score, Combo
        public UnityEvent<PlayerID, FruitType> OnFruitCaught;        // PlayerID, FruitType
        public UnityEvent<PlayerID> OnMiss;                          // PlayerID
        public UnityEvent<float> OnTimerUpdated;                     // RemainingTime in seconds
        public UnityEvent<string> OnGameOver;                        // Winner announcement text

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
            StartGame();
        }

        private void Update()
        {
            if (!IsPlaying) return;

            RemainingTime -= Time.deltaTime;
            if (RemainingTime < 0f)
            {
                RemainingTime = 0f;
            }

            OnTimerUpdated?.Invoke(RemainingTime);

            if (RemainingTime <= 0f)
            {
                EndGame();
            }
        }

        /// <summary>
        /// เริ่มต้นเกมใหม่
        /// </summary>
        public void StartGame()
        {
            RemainingTime = matchDuration;
            IsPlaying = true;

            player1 = new PlayerStats();
            player2 = new PlayerStats();

            OnScoreOrComboChanged?.Invoke(PlayerID.Player1, 0, 0);
            OnScoreOrComboChanged?.Invoke(PlayerID.Player2, 0, 0);
            OnTimerUpdated?.Invoke(RemainingTime);
        }

        /// <summary>
        /// จบเกมเมื่อหมดเวลา
        /// </summary>
        public void EndGame()
        {
            if (!IsPlaying) return;
            IsPlaying = false;

            string resultMessage;
            if (player1.score > player2.score)
            {
                resultMessage = "PLAYER 1 WINS!";
            }
            else if (player2.score > player1.score)
            {
                resultMessage = "PLAYER 2 WINS!";
            }
            else
            {
                resultMessage = "IT'S A DRAW!";
            }

            Debug.Log($"[CatchGameManager] Game Over! {resultMessage} (P1: {player1.score} vs P2: {player2.score})");
            OnGameOver?.Invoke(resultMessage);
        }

        /// <summary>
        /// เมื่อผู้เล่นเก็บผลไม้ได้ (คิดคะแนนตามชนิดผลไม้)
        /// </summary>
        public void RegisterCatch(PlayerID playerID, FruitType fruitType, int basePoints)
        {
            if (!IsPlaying) return;

            PlayerStats stats = GetStats(playerID);

            switch (fruitType)
            {
                case FruitType.Normal:
                    stats.combo++;
                    if (stats.combo > stats.maxCombo) stats.maxCombo = stats.combo;
                    int comboBonus = Mathf.Max(0, (stats.combo - 1) * 10);
                    stats.score += basePoints + comboBonus;
                    stats.caughtCount++;
                    break;


                case FruitType.Bomb:
                    // ระเบิด/ผลไม้พิษ: หักคะแนน และคอมโบแตกเป็น 0 ทันที!
                    stats.combo = 0;
                    stats.score = Mathf.Max(0, stats.score - Mathf.Abs(basePoints));
                    stats.bombsHit++;
                    break;
            }

            OnFruitCaught?.Invoke(playerID, fruitType);
            OnScoreOrComboChanged?.Invoke(playerID, stats.score, stats.combo);
        }

        /// <summary>
        /// เมื่อผลไม้ตกพื้น
        /// </summary>
        public void RegisterMiss(PlayerID playerID, FruitType fruitType)
        {
            if (!IsPlaying) return;

            // ถ้าระเบิดตกพื้น = ดีแล้วที่หลบได้ ไม่นับเป็น Miss
            if (fruitType == FruitType.Bomb) return;

            PlayerStats stats = GetStats(playerID);
            stats.combo = 0;
            stats.missedCount++;

            OnMiss?.Invoke(playerID);
            OnScoreOrComboChanged?.Invoke(playerID, stats.score, stats.combo);
        }

        public PlayerStats GetStats(PlayerID playerID)
        {
            return (playerID == PlayerID.Player1) ? player1 : player2;
        }
    }
}
