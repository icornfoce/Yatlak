using UnityEngine;
using TMPro;

namespace MiniGame.Darts
{
    /// <summary>
    /// แสดงผล UI มินิเกมปาลูกดอก (Darts):
    /// - คะแนนรวมของแต่ละคน
    /// - จำนวนลูกดอกที่เหลือ (3 ดอก)
    /// - สถานะคำแนะนำการกด (LOCK X -> LOCK Y) และคะแนนที่ได้ต่อดอก
    /// - นาฬิกาจับเวลา 60 วิ
    /// - ผลแพ้ชนะตอนจบเกม (WIN / LOSE)
    /// </summary>
    public class DartsUI : MonoBehaviour
    {
        [Header("Player Target")]
        public PlayerID targetPlayer = PlayerID.Player1;
        public DartsPlayerController playerController;

        [Header("Player UI (TextMeshPro)")]
        [Tooltip("Text แสดงคะแนนรวม")]
        public TextMeshProUGUI scoreTMP;

        [Tooltip("Text แสดงจำนวนลูกดอกที่เหลือ (เช่น Darts: 3)")]
        public TextMeshProUGUI dartsLeftTMP;

        [Tooltip("Text แสดงสถานะคำแนะนำ (เช่น LOCK X -> LOCK Y) หรือผลคะแนนล่าสุด")]
        public TextMeshProUGUI statusTMP;

        [Tooltip("Text แสดงผลแพ้ชนะตอนจบเกม (WIN / LOSE)")]
        public TextMeshProUGUI resultTextTMP;

        [Header("Match UI (ทางเลือก - ถ้าต้องการแสดงเวลาตรงกลาง)")]
        [Tooltip("Text แสดงเวลานับถอยหลัง 60 วิ")]
        public TextMeshProUGUI timerTMP;

        private void Start()
        {
            if (playerController == null)
            {
                DartsPlayerController[] controllers = FindObjectsByType<DartsPlayerController>(FindObjectsSortMode.None);
                foreach (var ctrl in controllers)
                {
                    if (ctrl.playerID == this.targetPlayer)
                    {
                        playerController = ctrl;
                        break;
                    }
                }
            }

            if (playerController != null)
            {
                playerController.OnAimStateChanged.AddListener(OnAimStateChanged);
                playerController.OnDartHit.AddListener(OnDartHit);
            }

            if (DartsGameManager.Instance != null)
            {
                DartsGameManager.Instance.OnScoreUpdated.AddListener(OnScoreUpdated);
                DartsGameManager.Instance.OnTimerUpdated.AddListener(OnTimerUpdated);
                DartsGameManager.Instance.OnGameOver.AddListener(OnGameOver);

                UpdateDisplay(0, DartsGameManager.Instance.maxDartsPerPlayer);
            }

            // ซ่อนผลลัพธ์ตอนเริ่ม
            if (resultTextTMP != null)
            {
                resultTextTMP.text = "";
            }
        }

        private void OnDestroy()
        {
            if (playerController != null)
            {
                playerController.OnAimStateChanged.RemoveListener(OnAimStateChanged);
                playerController.OnDartHit.RemoveListener(OnDartHit);
            }

            if (DartsGameManager.Instance != null)
            {
                DartsGameManager.Instance.OnScoreUpdated.RemoveListener(OnScoreUpdated);
                DartsGameManager.Instance.OnTimerUpdated.RemoveListener(OnTimerUpdated);
                DartsGameManager.Instance.OnGameOver.RemoveListener(OnGameOver);
            }
        }

        private void OnAimStateChanged(AimState state)
        {
            if (statusTMP == null) return;

            string keyName = (targetPlayer == PlayerID.Player1) ? "[S]" : "[↓]";

            switch (state)
            {
                case AimState.AimingX:
                    statusTMP.text = $"Lock X {keyName}";
                    break;
                case AimState.AimingY:
                    statusTMP.text = $"Lock Y {keyName}";
                    break;
                case AimState.Throwing:
                    statusTMP.text = "Throwing...";
                    break;
                case AimState.Idle:
                    statusTMP.text = "Done!";
                    break;
            }
        }

        private void OnDartHit(Vector2 hitPos, int points, string ringName)
        {
            if (statusTMP != null)
            {
                if (points > 0)
                {
                    statusTMP.text = $"+{points} {ringName}!";
                }
                else
                {
                    statusTMP.text = "MISS!";
                }
            }
        }

        private void OnScoreUpdated(PlayerID player, int totalScore, int dartsLeft, int lastHitPoints)
        {
            if (player == targetPlayer)
            {
                UpdateDisplay(totalScore, dartsLeft);
            }
        }

        private void UpdateDisplay(int score, int dartsLeft)
        {
            if (scoreTMP != null)
            {
                scoreTMP.text = $"Score: {score}";
            }

            if (dartsLeftTMP != null)
            {
                dartsLeftTMP.text = $"Darts: {dartsLeft}";
            }
        }

        private void OnTimerUpdated(float remainingTime)
        {
            if (timerTMP == null) return;

            int seconds = Mathf.CeilToInt(remainingTime);
            int mins = seconds / 60;
            int secs = seconds % 60;
            timerTMP.text = $"{mins:D2}:{secs:D2}";
        }

        private void OnGameOver(string winnerMessage)
        {
            if (resultTextTMP == null || DartsGameManager.Instance == null) return;

            int p1 = DartsGameManager.Instance.player1.score;
            int p2 = DartsGameManager.Instance.player2.score;

            if (p1 == p2)
            {
                resultTextTMP.text = "DRAW";
            }
            else if (targetPlayer == PlayerID.Player1)
            {
                resultTextTMP.text = (p1 > p2) ? "WIN" : "LOSE";
            }
            else
            {
                resultTextTMP.text = (p2 > p1) ? "WIN" : "LOSE";
            }
        }
    }
}
