using UnityEngine;

namespace MiniGame.Catch
{
    public enum FruitType
    {
        Normal, // ผลไม้ทั่วไป (+100 แต้ม, +1 Combo)
        Bomb    // ระเบิด/ผลไม้พิษ (-100 แต้ม, ล้าง Combo เป็น 0, สตั๊นผู้เล่นชั่วคราว)
    }

    /// <summary>
    /// ผลไม้ที่ตกลงมาในเกม osu!catch (ผลไม้ทั่วไป และ ระเบิด)
    /// </summary>
    public class CatchFruit : MonoBehaviour
    {
        [Header("Fruit Configuration")]
        [Tooltip("ชนิดของผลไม้")]
        public FruitType fruitType = FruitType.Normal;

        [Tooltip("ความเร็วในการตกลงมา (ปรับให้ช้าลงตามที่ต้องการ)")]
        public float fallSpeed = 3f;

        [Tooltip("คะแนนที่ได้ (หรือโดนหักถ้าเป็นระเบิด)")]
        public int scorePoints = 100;

        [Tooltip("ระยะเวลาสตั๊นเมื่อชนระเบิด (วินาที)")]
        public float stunDurationOnBomb = 0.8f;

        [Tooltip("ผู้เล่นเป้าหมายของผลไม้นี้ (P1 หรือ P2)")]
        public PlayerID targetPlayer = PlayerID.Player1;

        [Header("Boundary")]
        [Tooltip("ระดับแกน Y ที่ถือว่าหลุดจอ/รับไม่ทัน")]
        public float missY = -5.5f;

        [Header("Visual & Effects")]
        public GameObject catchEffectPrefab;
        public AudioClip catchSound;

        private bool hasHandled = false;

        private void Update()
        {
            // เคลื่อนที่ตกลงมาตามแกน Y
            transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

            // ถ้าตกต่ำกว่าขอบจอ
            if (!hasHandled && transform.position.y <= missY)
            {
                OnMissed();
            }
        }

        private void OnMissed()
        {
            hasHandled = true;

            if (CatchGameManager.Instance != null)
            {
                CatchGameManager.Instance.RegisterMiss(targetPlayer, fruitType);
            }

            Destroy(gameObject);
        }

        private void OnCaught(CatchPlayerController player)
        {
            if (hasHandled) return;

            // เช็คว่าเป็นผลไม้ของเลนผู้เล่นคนนี้หรือไม่
            if (player.playerID != targetPlayer) return;

            hasHandled = true;

            // ส่งคะแนนให้ GameManager
            if (CatchGameManager.Instance != null)
            {
                CatchGameManager.Instance.RegisterCatch(player.playerID, fruitType, scorePoints);
            }

            // ถ้ารับโดนระเบิด ให้ผู้เล่นสตั๊นขยับไม่ได้ชั่วคราว
            if (fruitType == FruitType.Bomb)
            {
                player.ApplyStun(stunDurationOnBomb);
            }

            // เอฟเฟกต์และเสียง
            if (catchEffectPrefab != null)
            {
                Instantiate(catchEffectPrefab, transform.position, Quaternion.identity);
            }
            if (catchSound != null)
            {
                AudioSource.PlayClipAtPoint(catchSound, transform.position);
            }

            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            CatchPlayerController player = collision.GetComponent<CatchPlayerController>();
            if (player == null) player = collision.GetComponentInParent<CatchPlayerController>();

            if (player != null)
            {
                OnCaught(player);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CatchPlayerController player = other.GetComponent<CatchPlayerController>();
            if (player == null) player = other.GetComponentInParent<CatchPlayerController>();

            if (player != null)
            {
                OnCaught(player);
            }
        }
    }
}
