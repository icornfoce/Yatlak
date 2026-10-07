using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Yatlak.MiniGame.PingPong
{
    public class PingPongGameManager : MonoBehaviour
    {
        public static PingPongGameManager Instance { get; private set; }

        [Header("Players")]
        [SerializeField] private PlayerPingPongController player1;
        [SerializeField] private PlayerPingPongController player2;

        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text winnerText;
        [SerializeField] private TMP_Text finalScoresText;
        [SerializeField] private Button restartButton;

        private bool isGameOver = false;
        public bool IsGameOver => isGameOver;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            AutoFindPlayersIfNull();
        }

        private void Start()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartMatch);
            }
        }

        private void AutoFindPlayersIfNull()
        {
            if (player1 == null || player2 == null)
            {
                var controllers = FindObjectsByType<PlayerPingPongController>(FindObjectsSortMode.None);
                foreach (var c in controllers)
                {
                    if (c.InputKey == KeyCode.S && player1 == null)
                    {
                        player1 = c;
                    }
                    else if (c.InputKey == KeyCode.L && player2 == null)
                    {
                        player2 = c;
                    }
                }

                if (controllers.Length >= 2)
                {
                    if (player1 == null) player1 = controllers[0];
                    if (player2 == null) player2 = controllers[1];
                }
            }
        }

        private void Update()
        {
            if (isGameOver)
            {
                // Allow pressing 'R' or Space to restart
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
                {
                    RestartMatch();
                }
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Keyboard.current != null)
                {
                    if (UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame ||
                        UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
                    {
                        RestartMatch();
                    }
                }
#endif
            }
        }

        public void CheckGameOver()
        {
            AutoFindPlayersIfNull();

            if (player1 == null || player2 == null) return;

            // Game over only when BOTH players have finished all balls
            if (player1.IsFinished && player2.IsFinished)
            {
                TriggerGameOver();
            }
        }

        private void TriggerGameOver()
        {
            isGameOver = true;

            int p1Score = player1.CurrentScore;
            int p2Score = player2.CurrentScore;

            string winnerMsg;
            if (p1Score > p2Score)
            {
                winnerMsg = "🏆 PLAYER 1 WINS! 🏆";
            }
            else if (p2Score > p1Score)
            {
                winnerMsg = "🏆 PLAYER 2 WINS! 🏆";
            }
            else
            {
                winnerMsg = "🤝 IT'S A TIE! 🤝";
            }

            if (winnerText != null)
            {
                winnerText.text = winnerMsg;
            }

            if (finalScoresText != null)
            {
                finalScoresText.text = $"P1 Score: {p1Score}  |  P2 Score: {p2Score}";
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            Debug.Log($"[PingPongGameManager] Game Over! {winnerMsg} (P1: {p1Score}, P2: {p2Score})");
        }

        public void RestartMatch()
        {
            isGameOver = false;

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            // Either reload active scene or reset player states
            if (player1 != null && player2 != null)
            {
                player1.RestartGame();
                player2.RestartGame();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
