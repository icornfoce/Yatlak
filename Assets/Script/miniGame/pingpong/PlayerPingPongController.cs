using UnityEngine;

namespace Yatlak.MiniGame.PingPong
{
    public class PlayerPingPongController : MonoBehaviour
    {
        public enum ShotState
        {
            Position,   // State 1: Ball auto-slides X-axis. Bounces on Tag "wall". Press key to lock X pos.
            Aim,        // State 2: Aim indicator sweeps 180° front arc (-90° to +90°, ball as pivot). Press key to lock aim angle.
            Power,      // State 3: Vertical power bar oscillates min-max. Press key to lock power and launch ball forward.
            InAir,      // Ball launched, waiting for result.
            Finished    // All balls shot and resolved.
        }

        [Header("Player Identity & Input")]
        [SerializeField] private string playerName = "Player 1";
        [SerializeField] private KeyCode inputKey = KeyCode.S;
        [SerializeField] private int totalBalls = 5;

        [Header("State 1: Position Sliding")]
        [Tooltip("Sliding speed along X-axis")]
        [SerializeField] private float slideSpeed = 2.5f;
        [Tooltip("Half-width of sliding track (used as safety bounds)")]
        [SerializeField] private float slideHalfWidth = 1.3f;
        [SerializeField] private Transform ballSpawnPoint;

        [Header("State 2: Aim Sweeping")]
        [Tooltip("Sweep speed in degrees per second (sweeping 180 deg front arc: -90 to +90)")]
        [SerializeField] private float aimSweepSpeed = 120f;
        [SerializeField] private AimIndicator aimIndicator;

        [Header("State 3: Power & Launch")]
        [Tooltip("Oscillation speed of power bar")]
        [SerializeField] private float powerOscillateSpeed = 1.8f;
        [Tooltip("Minimum launch speed")]
        [SerializeField] private float minLaunchSpeed = 6.5f;
        [Tooltip("Maximum launch speed")]
        [SerializeField] private float maxLaunchSpeed = 12.5f;
        [Tooltip("องศาการยิงในแนวดิ่ง (ถ้าค่าเป็นลบ เช่น -15 ถึง -30 จะยิงกดลงพื้นโต๊ะเพื่อให้เด้งก่อนเข้าถ้วย ถ้าเป็นบวกจะโด่งขึ้นฟ้า)")]
        [SerializeField] private float launchPitchAngle = -20.0f;

        [Header("Ball & Table References")]
        [Tooltip("ลาก Prefab ลูกปิงปองมาใส่ที่นี่")]
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private Transform tableTransform;
        [SerializeField] private PingPongPlayerUI playerUI;

        // Runtime variables
        private ShotState currentState = ShotState.Position;
        private int remainingBalls;
        private int currentScore = 0;
        private PingPongBall activeBall;

        // State 1
        private float slideDirection = 1.0f;
        private float currentLocalX = 0.0f;

        // State 2
        private float aimTimer = 0.0f;
        private float lockedAimAngle = 0.0f;

        // State 3
        private float powerTimer = 0.0f;
        private float lockedPower = 0.5f;

        public string PlayerName => playerName;
        public KeyCode InputKey => inputKey;
        public int CurrentScore => currentScore;
        public int RemainingBalls => remainingBalls;
        public ShotState CurrentState => currentState;
        public bool IsFinished => currentState == ShotState.Finished;

        private void Start()
        {
            remainingBalls = totalBalls;
            currentScore = 0;

            if (playerUI != null)
            {
                playerUI.Initialize(playerName, inputKey.ToString(), remainingBalls);
            }

            if (aimIndicator != null)
            {
                aimIndicator.Show(false);
            }

            SpawnNewBall();
        }

        private void Update()
        {
            if (currentState == ShotState.Finished) return;

            bool keyPressed = CheckInputPressed();

            switch (currentState)
            {
                case ShotState.Position:
                    UpdatePositionState(keyPressed);
                    break;

                case ShotState.Aim:
                    UpdateAimState(keyPressed);
                    break;

                case ShotState.Power:
                    UpdatePowerState(keyPressed);
                    break;

                case ShotState.InAir:
                    // Waiting for ball to resolve
                    break;
            }
        }

        #region State 1: Position Sliding
        private void UpdatePositionState(bool keyPressed)
        {
            if (activeBall == null) return;

            // Move ball along local X axis
            currentLocalX += slideDirection * slideSpeed * Time.deltaTime;

            // Safety check: if outside half-width, bounce
            if (currentLocalX > slideHalfWidth)
            {
                currentLocalX = slideHalfWidth;
                slideDirection = -1.0f;
            }
            else if (currentLocalX < -slideHalfWidth)
            {
                currentLocalX = -slideHalfWidth;
                slideDirection = 1.0f;
            }

            Vector3 basePos = ballSpawnPoint != null ? ballSpawnPoint.position : transform.position;
            Vector3 worldOffset = transform.right * currentLocalX;
            activeBall.transform.position = basePos + worldOffset;

            if (keyPressed)
            {
                // Lock X position and advance to State 2 (Aim)
                currentState = ShotState.Aim;
                aimTimer = 0.0f;

                if (aimIndicator != null)
                {
                    aimIndicator.Show(true);
                }

                if (playerUI != null)
                {
                    playerUI.SetPrompt($"Press [{inputKey}] to Lock Aim");
                }
            }
        }

        public void OnWallBounce()
        {
            // Ball bounced on tag "wall"
            if (currentState == ShotState.Position)
            {
                slideDirection = -slideDirection;
            }
        }
        #endregion

        #region State 2: Aim Sweeping
        private void UpdateAimState(bool keyPressed)
        {
            if (activeBall == null) return;

            aimTimer += Time.deltaTime;

            // Sweep 180° front arc (-90° to +90°, ball as pivot)
            // PingPong oscillates between 0 and 180, minus 90 gives range [-90, +90]
            float sweepAngle = Mathf.PingPong(aimTimer * aimSweepSpeed, 180f) - 90f;

            if (aimIndicator != null)
            {
                aimIndicator.SetAim(activeBall.transform.position, sweepAngle, transform.forward);
            }

            if (keyPressed)
            {
                // Lock aim angle and advance to State 3 (Power)
                lockedAimAngle = sweepAngle;
                currentState = ShotState.Power;
                powerTimer = 0.0f;

                if (playerUI != null)
                {
                    playerUI.SetPrompt($"Press [{inputKey}] to Shoot!");
                }
            }
        }
        #endregion

        #region State 3: Power & Launch
        private void UpdatePowerState(bool keyPressed)
        {
            powerTimer += Time.deltaTime;

            // Oscillate vertical power bar min-max (0 to 1)
            float powerNormalized = Mathf.PingPong(powerTimer * powerOscillateSpeed, 1.0f);

            if (playerUI != null)
            {
                playerUI.SetPower(powerNormalized);
            }

            if (keyPressed)
            {
                // Lock power and launch ball forward
                lockedPower = powerNormalized;
                LaunchBall();
            }
        }

        private void LaunchBall()
        {
            if (activeBall == null) return;

            remainingBalls--;
            if (playerUI != null)
            {
                playerUI.SetRemainingBalls(remainingBalls);
                playerUI.SetPrompt("BALL IN FLIGHT...");
            }

            if (aimIndicator != null)
            {
                aimIndicator.Show(false);
            }

            // Calculate 3D launch vector from locked aim angle and pitch angle
            Vector3 horizontalDir = Quaternion.AngleAxis(lockedAimAngle, Vector3.up) * transform.forward;
            Vector3 pitchAxis = Vector3.Cross(Vector3.up, horizontalDir);
            // ถ้า launchPitchAngle เป็นลบ จะหมุนทิ่มลงพื้นโต๊ะ (downward pitch เพื่อให้ลูกเด้งโต๊ะก่อน)
            Vector3 launchDir = Quaternion.AngleAxis(-launchPitchAngle, pitchAxis) * horizontalDir;
            launchDir.Normalize();

            float launchSpeed = Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, lockedPower);
            Vector3 launchVelocity = launchDir * launchSpeed;

            activeBall.Launch(launchVelocity);
            currentState = ShotState.InAir;
        }
        #endregion

        #region Scoring & Ball Lifecycle
        public void OnBallScored(int points)
        {
            currentScore += points;
            if (playerUI != null)
            {
                playerUI.SetScore(currentScore);
                playerUI.SetPrompt($"GOAL! +{points} PTS");
            }
        }

        public void OnBallMissed()
        {
            if (playerUI != null)
            {
                playerUI.SetPrompt("MISSED!");
            }
        }

        public void OnBallCycleCompleted(PingPongBall finishedBall)
        {
            if (finishedBall != null)
            {
                Destroy(finishedBall.gameObject);
            }

            activeBall = null;

            if (playerUI != null)
            {
                playerUI.SetPower(0f);
            }

            if (remainingBalls > 0)
            {
                SpawnNewBall();
            }
            else
            {
                currentState = ShotState.Finished;
                if (playerUI != null)
                {
                    playerUI.SetPrompt("FINISHED!");
                }

                if (PingPongGameManager.Instance != null)
                {
                    PingPongGameManager.Instance.CheckGameOver();
                }
            }
        }

        private void SpawnNewBall()
        {
            Vector3 spawnPos = ballSpawnPoint != null ? ballSpawnPoint.position : transform.position;
            GameObject ballObj = null;

            if (ballPrefab != null)
            {
                ballObj = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"[{playerName}] ยังไม่ได้ลาก Ball Prefab ใส่ในช่อง Inspector! กรุณาลาก Prefab ลูกปิงปองมาใส่", this);
                return;
            }

            activeBall = ballObj.GetComponent<PingPongBall>();
            if (activeBall == null)
            {
                activeBall = ballObj.AddComponent<PingPongBall>();
            }

            float tableHeight = tableTransform != null ? tableTransform.position.y : transform.position.y;
            activeBall.Initialize(this, tableHeight);

            // Reset state to State 1: Position
            currentState = ShotState.Position;
            currentLocalX = 0.0f;
            slideDirection = 1.0f;

            if (aimIndicator != null)
            {
                aimIndicator.Show(false);
            }

            if (playerUI != null)
            {
                playerUI.SetPower(0f);
                playerUI.SetPrompt($"Press [{inputKey}] to Lock Position");
            }
        }
        #endregion

        private bool CheckInputPressed()
        {
            // Legacy Input
            try
            {
                if (Input.GetKeyDown(inputKey)) return true;
            }
            catch { }

            // New Input System fallback
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (inputKey == KeyCode.S && UnityEngine.InputSystem.Keyboard.current.sKey.wasPressedThisFrame) return true;
                if (inputKey == KeyCode.L && UnityEngine.InputSystem.Keyboard.current.lKey.wasPressedThisFrame) return true;
            }
#endif
            return false;
        }

        public void RestartGame()
        {
            remainingBalls = totalBalls;
            currentScore = 0;

            if (activeBall != null)
            {
                Destroy(activeBall.gameObject);
                activeBall = null;
            }

            if (playerUI != null)
            {
                playerUI.Initialize(playerName, inputKey.ToString(), remainingBalls);
            }

            SpawnNewBall();
        }
    }
}
