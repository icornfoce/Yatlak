using UnityEngine;
using TMPro;

namespace MiniGame.Catch
{
    /// <summary>
    /// แสดงผล UI มินิเกม osu!catch: คะแนน, คอมโบ, นาฬิกาจับเวลา 60 วินาที, และข้อความผลแพ้ชนะ (TextMeshPro)
    /// </summary>
    public class CatchUI : MonoBehaviour
    {
        [Header("Mode")]
        [Tooltip("ติ๊กถูกถ้าคอมโพเนนต์นี้ใช้แสดงผลเวลานับถอยหลัง")]
        public bool isMatchManagerUI = false;

        [Header("Player Target")]
        public PlayerID targetPlayer = PlayerID.Player1;
        [Tooltip("Text แสดงผลแพ้ชนะตอนจบเกม (เช่น WIN / LOSE)")]
        public TextMeshProUGUI resultTextTMP;

        [Header("Score & Combo UI (TextMeshPro)")]
        public TextMeshProUGUI scoreTMP;
        public TextMeshProUGUI comboTMP;

        [Header("Timer UI (TextMeshPro)")]
        public TextMeshProUGUI timerTMP;

        private void Start()
        {
            if (CatchGameManager.Instance != null)
            {
                CatchGameManager.Instance.OnScoreOrComboChanged.AddListener(OnScoreChanged);
                CatchGameManager.Instance.OnTimerUpdated.AddListener(OnTimerUpdated);
                CatchGameManager.Instance.OnGameOver.AddListener(OnGameOver);

                // ซ่อนข้อความผลการแข่งขันตอนเริ่มเกม
                if (resultTextTMP != null)
                {
                    resultTextTMP.text = "";
                }

                UpdateDisplay(0, 0);
            }
        }

        private void OnDestroy()
        {
            if (CatchGameManager.Instance != null)
            {
                CatchGameManager.Instance.OnScoreOrComboChanged.RemoveListener(OnScoreChanged);
                CatchGameManager.Instance.OnTimerUpdated.RemoveListener(OnTimerUpdated);
                CatchGameManager.Instance.OnGameOver.RemoveListener(OnGameOver);
            }
        }

        private void OnScoreChanged(PlayerID player, int score, int combo)
        {
            if (player == targetPlayer && !isMatchManagerUI)
            {
                UpdateDisplay(score, combo);
            }
        }

        private void OnTimerUpdated(float remainingTime)
        {
            int seconds = Mathf.CeilToInt(remainingTime);
            int mins = seconds / 60;
            int secs = seconds % 60;
            string timeStr = $"{mins:D2}:{secs:D2}";

            if (timerTMP != null) timerTMP.text = timeStr;
        }

        private void OnGameOver(string winnerMessage)
        {
            if (resultTextTMP == null) return;

            if (isMatchManagerUI)
            {
                resultTextTMP.text = winnerMessage;
            }
            else if (CatchGameManager.Instance != null)
            {
                // ตรวจสอบผลคะแนนเทียบกับอีกฝ่าย
                int myScore = CatchGameManager.Instance.GetStats(targetPlayer).score;
                PlayerID otherPlayer = (targetPlayer == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;
                int otherScore = CatchGameManager.Instance.GetStats(otherPlayer).score;

                if (myScore > otherScore)
                {
                    resultTextTMP.text = "WIN";
                }
                else if (myScore < otherScore)
                {
                    resultTextTMP.text = "LOSE";
                }
                else
                {
                    resultTextTMP.text = "DRAW";
                }
            }
        }

        private void UpdateDisplay(int score, int combo)
        {
            string scoreStr = $"{score:N0}";
            string comboStr = combo > 1 ? $"{combo}x" : "";

            if (scoreTMP != null) scoreTMP.text = scoreStr;
            if (comboTMP != null) comboTMP.text = comboStr;
        }
    }
}
