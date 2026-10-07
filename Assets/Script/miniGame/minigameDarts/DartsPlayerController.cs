using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MiniGame.Darts
{
    public enum AimState
    {
        Idle,       // ยังไม่เริ่ม หรือหมดลูกดอกแล้ว
        AimingX,    // กำลังเล็งแกน X (ขยับซ้าย-ขวา)
        AimingY,    // กำลังเล็งแกน Y (ขยับบน-ล่าง)
        Throwing    // กำลังปา / แสดงผล
    }

    /// <summary>
    /// ควบคุมการเล็งและปาลูกดอก:
    /// - P1: กดปุ่ม S เพื่อล็อกแกน X แล้วกด S อีกครั้งเพื่อล็อกแกน Y และปา
    /// - P2: กดปุ่ม Down Arrow เพื่อล็อกแกน X แล้วกด Down Arrow อีกครั้งเพื่อล็อกแกน Y และปา
    /// </summary>
    public class DartsPlayerController : MonoBehaviour
    {
        [Header("Player Settings")]
        public PlayerID playerID = PlayerID.Player1;

        [Header("Target Reference")]
        [Tooltip("เป้าปาลูกดอกของฝั่งนี้")]
        public DartsTarget target;

        [Header("Aiming Limits (ขอบเขตการเล็ง)")]
        [Tooltip("ระยะการแกว่งแกน X ซ้าย-ขวา เทียบจากจุดศูนย์กลางเป้า")]
        public float aimRangeX = 2.5f;

        [Tooltip("ระยะการแกว่งแกน Y บน-ล่าง เทียบจากจุดศูนย์กลางเป้า")]
        public float aimRangeY = 2.5f;

        [Header("Aiming Speed (ความเร็วการแกว่ง)")]
        public float aimSpeedX = 2.2f;
        public float aimSpeedY = 2.5f;

        [Header("Visual Indicators (ตัวเล็ง)")]
        [Tooltip("GameObject ตัวเล็งเป้า (Crosshair) ที่จะขยับไปตามตำแหน่งที่เล็ง")]
        public Transform aimCrosshair;

        [Tooltip("Prefab ลูกดอกที่จะปักคาไว้บนเป้า")]
        public GameObject pinnedDartPrefab;

        [Header("Audio (ทางเลือก)")]
        public AudioClip lockSound;
        public AudioClip hitSound;

        [Header("Events")]
        public UnityEvent<AimState> OnAimStateChanged;
        public UnityEvent<Vector2, int, string> OnDartHit; // HitPos, Points, RingName

        public AimState CurrentState { get; private set; } = AimState.Idle;

        private float lockedX;
        private float lockedY;
        private float aimTimerX;
        private float aimTimerY;
        private List<GameObject> spawnedDarts = new List<GameObject>();

        private void Start()
        {
            if (target == null)
            {
                target = GetComponentInChildren<DartsTarget>();
            }

            StartNextDart();
        }

        private void Update()
        {
            if (DartsGameManager.Instance != null && !DartsGameManager.Instance.IsPlaying)
            {
                return;
            }

            HandleAimingMovement();
            HandleInput();
        }

        public void StartNextDart()
        {
            if (DartsGameManager.Instance != null)
            {
                var stats = DartsGameManager.Instance.GetStats(playerID);
                if (stats.dartsLeft <= 0)
                {
                    SetState(AimState.Idle);
                    if (aimCrosshair != null) aimCrosshair.gameObject.SetActive(false);
                    return;
                }
            }

            aimTimerX = 0f;
            aimTimerY = 0f;

            if (aimCrosshair != null)
            {
                aimCrosshair.gameObject.SetActive(true);
            }

            SetState(AimState.AimingX);
        }

        private void HandleAimingMovement()
        {
            if (target == null) return;

            Vector2 center = target.CenterPosition;

            if (CurrentState == AimState.AimingX)
            {
                aimTimerX += Time.deltaTime * aimSpeedX;
                float t = Mathf.PingPong(aimTimerX, 1f);
                float x = Mathf.Lerp(center.x - aimRangeX, center.x + aimRangeX, t);

                if (aimCrosshair != null)
                {
                    aimCrosshair.position = new Vector3(x, center.y, aimCrosshair.position.z);
                }
            }
            else if (CurrentState == AimState.AimingY)
            {
                aimTimerY += Time.deltaTime * aimSpeedY;
                float t = Mathf.PingPong(aimTimerY, 1f);
                float y = Mathf.Lerp(center.y - aimRangeY, center.y + aimRangeY, t);

                if (aimCrosshair != null)
                {
                    aimCrosshair.position = new Vector3(lockedX, y, aimCrosshair.position.z);
                }
            }
        }

        private void HandleInput()
        {
            bool confirmPressed = false;

            if (playerID == PlayerID.Player1)
            {
                // P1: ใช้ปุ่ม S
                confirmPressed = Input.GetKeyDown(KeyCode.S);
            }
            else
            {
                // P2: ใช้ปุ่มลูกศรลง (Down Arrow)
                confirmPressed = Input.GetKeyDown(KeyCode.DownArrow);
            }

            if (!confirmPressed) return;

            if (CurrentState == AimState.AimingX)
            {
                // ล็อกแกน X -> เปลี่ยนไปเล็งแกน Y
                lockedX = (aimCrosshair != null) ? aimCrosshair.position.x : target.CenterPosition.x;
                PlaySound(lockSound);
                SetState(AimState.AimingY);
            }
            else if (CurrentState == AimState.AimingY)
            {
                // ล็อกแกน Y -> ปาลูกดอกทันที
                lockedY = (aimCrosshair != null) ? aimCrosshair.position.y : target.CenterPosition.y;
                PlaySound(lockSound);
                StartCoroutine(ExecuteThrowRoutine());
            }
        }

        private IEnumerator ExecuteThrowRoutine()
        {
            SetState(AimState.Throwing);

            Vector2 hitPoint = new Vector2(lockedX, lockedY);

            // จำลองจังหวะลูกดอกพุ่งไปปัก (0.2 วินาที)
            yield return new WaitForSeconds(0.2f);

            // สร้างลูกดอกปักคาไว้บนเป้า
            if (pinnedDartPrefab != null)
            {
                GameObject dart = Instantiate(pinnedDartPrefab, new Vector3(hitPoint.x, hitPoint.y, 0), Quaternion.identity);
                spawnedDarts.Add(dart);
            }

            PlaySound(hitSound);

            // คำนวณคะแนน
            int score = 0;
            string ringName = "MISS";
            if (target != null)
            {
                var result = target.CalculateScore(hitPoint);
                score = result.points;
                ringName = result.ringName;
            }

            OnDartHit?.Invoke(hitPoint, score, ringName);

            if (DartsGameManager.Instance != null)
            {
                DartsGameManager.Instance.RegisterDartHit(playerID, score);
            }

            // รอ 1 วินาทีให้ผู้เล่นดูจุดปักและคะแนน
            yield return new WaitForSeconds(1.0f);

            // เตรียมปาดอกถัดไป (ถ้ายังมีลูกดอกเหลือ)
            StartNextDart();
        }

        private void SetState(AimState newState)
        {
            CurrentState = newState;
            OnAimStateChanged?.Invoke(CurrentState);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }

        private void OnDestroy()
        {
            // ล้างลูกดอกที่ปักไว้เมื่อเปลี่ยนฉาก
            foreach (var dart in spawnedDarts)
            {
                if (dart != null) Destroy(dart);
            }
            spawnedDarts.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = (target != null) ? (Vector3)target.CenterPosition : transform.position;
            Gizmos.color = (playerID == PlayerID.Player1) ? Color.cyan : Color.yellow;
            Gizmos.DrawWireCube(center, new Vector3(aimRangeX * 2, aimRangeY * 2, 0.1f));
        }
    }
}
