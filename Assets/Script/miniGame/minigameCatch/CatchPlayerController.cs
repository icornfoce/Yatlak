using UnityEngine;

namespace MiniGame.Catch
{
    public enum PlayerID
    {
        Player1, // ฝั่งซ้าย
        Player2  // ฝั่งขวา
    }

    /// <summary>
    /// ควบคุมการเคลื่อนที่ของ Player ในมินิเกม osu!catch
    /// - P1: A/D เดินซ้าย-ขวา, W+A / W+D เพื่อเร่งความเร็ว (Dash)
    /// - P2: ลูกศรซ้าย/ขวา เดิน, Up+Left / Up+Right เพื่อเร่งความเร็ว (Dash)
    /// </summary>
    public class CatchPlayerController : MonoBehaviour
    {
        [Header("Player Settings")]
        [Tooltip("เลือกผู้เล่นว่าเป็น Player 1 หรือ Player 2")]
        public PlayerID playerID = PlayerID.Player1;

        [Header("Movement")]
        [Tooltip("ความเร็วเดินปกติ")]
        public float moveSpeed = 8f;

        [Tooltip("ตัวคูณความเร็วเมื่อกดปุ่มเร่ง (Dash) เช่น 2 เท่า")]
        public float dashMultiplier = 2f;

        [Header("Boundary Limits")]
        [Tooltip("จุดกึ่งกลางของหน้าจอผู้เล่น (P1 = 0, P2 = 50 เป็นต้น)")]
        public float centerOffsetX = 0f;

        [Tooltip("ระยะทางซ้าย-ขวาสูงสุดที่เดินได้จากจุดกึ่งกลาง")]
        public float moveRangeX = 4f;

        [Header("Visual Effects")]
        [Tooltip("TrailRenderer สำหรับเอฟเฟกต์ตอนพุ่ง/เร่งความเร็ว")]
        public TrailRenderer dashTrail;

        [Tooltip("SpriteRenderer ตัวละคร (ใช้เปลี่ยนสีตอน Dash)")]
        public SpriteRenderer characterSprite;
        public Color normalColor = Color.white;
        public Color dashColor = new Color(1f, 0.4f, 0.4f, 1f); // สีแดงสไตล์ osu!catch
        public Color stunColor = new Color(0.3f, 0.3f, 0.3f, 1f); // สีตอนโดนระเบิดสตั๊น

        // State
        public bool IsDashing { get; private set; }
        public float CurrentDirection { get; private set; }
        public bool IsStunned => stunTimer > 0f;
        private float stunTimer = 0f;

        private void Awake()
        {
            if (characterSprite == null)
            {
                characterSprite = GetComponentInChildren<SpriteRenderer>();
            }
            if (dashTrail == null)
            {
                dashTrail = GetComponentInChildren<TrailRenderer>();
            }

            if (dashTrail != null)
            {
                dashTrail.emitting = false;
            }
        }

        private void Update()
        {
            // ตรวจสอบสถานะการเล่นจาก GameManager (ถ้าหมดเวลาหรือยังไม่เริ่ม ห้ามขยับ)
            if (CatchGameManager.Instance != null && !CatchGameManager.Instance.IsPlaying)
            {
                IsDashing = false;
                UpdateVisuals();
                return;
            }

            // จัดการเวลาติดสตั๊น (เมื่อโดนระเบิด)
            if (stunTimer > 0f)
            {
                stunTimer -= Time.deltaTime;
                IsDashing = false;
                UpdateVisuals();
                return; // ขยับไม่ได้ขณะติดสตั๊น
            }

            HandleInput();
        }

        public void ApplyStun(float duration)
        {
            stunTimer = duration;
            if (dashTrail != null)
            {
                dashTrail.emitting = false;
            }
        }

        private void HandleInput()
        {
            float horizontal = 0f;
            bool dashPressed = false;

            // ตรวจสอบการกดปุ่มตาม PlayerID
            if (playerID == PlayerID.Player1)
            {
                // P1: ใช้ได้เฉพาะ W A S D
                if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
                if (Input.GetKey(KeyCode.D)) horizontal += 1f;

                // W ใช้เป็นปุ่มเร่งความเร็ว (กด W + A หรือ W + D)
                dashPressed = Input.GetKey(KeyCode.W);
            }
            else
            {
                // P2: ใช้ได้เฉพาะ Arrow Keys
                if (Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
                if (Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;

                // ลูกศรขึ้น (Up Arrow) ใช้เป็นปุ่มเร่งความเร็ว (กด Up + Left หรือ Up + Right)
                dashPressed = Input.GetKey(KeyCode.UpArrow);
            }

            CurrentDirection = horizontal;
            IsDashing = dashPressed && horizontal != 0;

            // คำนวณความเร็ว
            float speed = moveSpeed;
            if (IsDashing)
            {
                speed *= dashMultiplier;
            }

            // เคลื่อนที่แนวนอน
            Vector3 pos = transform.position;
            pos.x += horizontal * speed * Time.deltaTime;

            // จำกัดขอบเขตไม่ให้ออกนอกจอ (Clamp)
            float minX = centerOffsetX - moveRangeX;
            float maxX = centerOffsetX + moveRangeX;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);

            transform.position = pos;

            // อัปเดตเอฟเฟกต์การ Dash
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // เปิด/ปิด Trail ตอน Dash
            if (dashTrail != null)
            {
                dashTrail.emitting = IsDashing && !IsStunned;
            }

            // เปลี่ยนสีตัวละคร
            if (characterSprite != null)
            {
                if (IsStunned)
                {
                    characterSprite.color = stunColor;
                }
                else if (IsDashing)
                {
                    characterSprite.color = dashColor;
                }
                else
                {
                    characterSprite.color = normalColor;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = (playerID == PlayerID.Player1) ? Color.cyan : Color.magenta;
            Vector3 left = new Vector3(centerOffsetX - moveRangeX, transform.position.y, 0);
            Vector3 right = new Vector3(centerOffsetX + moveRangeX, transform.position.y, 0);
            Gizmos.DrawLine(left, right);
            Gizmos.DrawWireSphere(left, 0.3f);
            Gizmos.DrawWireSphere(right, 0.3f);
        }
    }
}
