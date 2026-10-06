using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.Catch
{
    public enum SpawnerMode
    {
        SynchronizedBothPlayers, // สปอว์นผลไม้แบบเดียวกันพร้อมกันทั้ง 2 คน (ยุติธรรมสำหรับแข่ง 1v1)
        SinglePlayerOnly         // สปอว์นเฉพาะคนใดคนหนึ่ง
    }

    /// <summary>
    /// ระบบสปอว์นผลไม้สำหรับ osu!catch 2 ผู้เล่น
    /// </summary>
    public class CatchFruitSpawner : MonoBehaviour
    {
        [Header("Spawner Mode")]
        public SpawnerMode spawnerMode = SpawnerMode.SynchronizedBothPlayers;

        [Tooltip("กรณีเลือก SinglePlayerOnly ให้เลือกว่าสปอว์นให้ใคร")]
        public PlayerID targetPlayer = PlayerID.Player1;

        [Header("Fruit Prefabs")]
        [Tooltip("รายการ Prefab ผลไม้และระเบิดทั้งหมด (สุ่มหยิบมาปล่อย)")]
        public List<GameObject> fruitPrefabs = new List<GameObject>();

        [Header("Spawn Timing & Area")]
        [Tooltip("ความถี่ตอนเริ่มเกม")]
        public float startSpawnInterval = 0.85f;

        [Tooltip("ความถี่ตอนท้ายเกมใกล้หมดเวลา")]
        public float endSpawnInterval = 0.55f;

        [Tooltip("ระดับความสูงแกน Y ที่ปล่อยผลไม้ลงมา")]
        public float spawnY = 6f;

        [Tooltip("ระยะกระจาย X สุ่มซ้าย-ขวา จากจุดกึ่งกลาง")]
        public float spawnRangeX = 3.5f;

        [Header("Linear Fall Speed")]
        [Tooltip("ความเร็วตกตอนเริ่มเกม (ช้า ควบคุมง่าย)")]
        public float startFallSpeed = 2.5f;

        [Tooltip("ความเร็วตกตอนท้ายเกมใกล้หมดเวลา (เร็ว ท้าทาย)")]
        public float endFallSpeed = 5.0f;

        [Tooltip("สุ่มความเร็วแกว่งเล็กน้อย (+/-) สลับช้า-เร็ว เพื่อความสนุก")]
        public bool randomizeSpeed = true;

        [Tooltip("ระยะแกว่งความเร็วแบบสุ่ม เช่น 0.3")]
        public float speedVariance = 0.3f;

        [Header("Player Screen Offsets")]
        [Tooltip("จุดศูนย์กลาง X ของ P1 (ปกติ = 0)")]
        public float p1CenterX = 0f;

        [Tooltip("จุดศูนย์กลาง X ของ P2 (ปกติ = 50)")]
        public float p2CenterX = 50f;

        [Header("Auto Start")]
        public bool autoStart = true;

        private Coroutine spawnRoutine;

        private void Start()
        {
            if (autoStart)
            {
                StartSpawning();
            }
        }

        public void StartSpawning()
        {
            if (spawnRoutine == null)
            {
                spawnRoutine = StartCoroutine(SpawnLoop());
            }
        }

        public void StopSpawning()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                // คำนวณความคืบหน้าของเกม (0.0 = เริ่มเกม, 1.0 = ท้ายเกมใกล้หมดเวลา)
                float progress = 0f;
                if (CatchGameManager.Instance != null && CatchGameManager.Instance.matchDuration > 0f)
                {
                    progress = Mathf.Clamp01(1f - (CatchGameManager.Instance.RemainingTime / CatchGameManager.Instance.matchDuration));
                }

                // คำนวณช่วงเวลาปล่อยผลไม้แบบ Linear (ค่อยๆ ถี่ขึ้นตามเวลา)
                float currentInterval = Mathf.Lerp(startSpawnInterval, endSpawnInterval, progress);
                yield return new WaitForSeconds(currentInterval);

                // ตรวจสอบว่าเกมกำลังเล่นอยู่หรือไม่ ถ้าหมดเวลาแล้วจะไม่ปล่อยผลไม้เพิ่ม
                if (CatchGameManager.Instance != null && !CatchGameManager.Instance.IsPlaying)
                {
                    continue;
                }

                GameObject prefabToSpawn = PickPrefabToSpawn();
                if (prefabToSpawn == null) continue;

                // สุ่มตำแหน่ง X ภายในเลน (-spawnRangeX ถึง +spawnRangeX)
                float localX = Random.Range(-spawnRangeX, spawnRangeX);

                // คำนวณความเร็วตกแบบ Linear (เริ่มจาก startFallSpeed แล้วค่อยๆ เร่งขึ้นไปถึง endFallSpeed)
                float speed = Mathf.Lerp(startFallSpeed, endFallSpeed, progress);
                if (randomizeSpeed)
                {
                    speed += Random.Range(-speedVariance, speedVariance);
                }

                if (spawnerMode == SpawnerMode.SynchronizedBothPlayers)
                {
                    // สปอว์นให้ทั้ง P1 และ P2 ในตำแหน่ง, ชนิด, และความเร็วเดียวกันเป๊ะ
                    SpawnFruitAt(prefabToSpawn, p1CenterX + localX, spawnY, PlayerID.Player1, speed);
                    SpawnFruitAt(prefabToSpawn, p2CenterX + localX, spawnY, PlayerID.Player2, speed);
                }
                else
                {
                    float centerX = (targetPlayer == PlayerID.Player1) ? p1CenterX : p2CenterX;
                    SpawnFruitAt(prefabToSpawn, centerX + localX, spawnY, targetPlayer, speed);
                }
            }
        }

        private GameObject PickPrefabToSpawn()
        {
            if (fruitPrefabs == null || fruitPrefabs.Count == 0)
            {
                return null;
            }

            int index = Random.Range(0, fruitPrefabs.Count);
            return fruitPrefabs[index];
        }

        private void SpawnFruitAt(GameObject prefab, float x, float y, PlayerID player, float speed)
        {
            Vector3 spawnPos = new Vector3(x, y, 0);
            GameObject fruitObj = Instantiate(prefab, spawnPos, Quaternion.identity);

            // กำหนดเป้าหมายผู้เล่นและความเร็วให้กับผลไม้
            CatchFruit fruit = fruitObj.GetComponent<CatchFruit>();
            if (fruit != null)
            {
                fruit.targetPlayer = player;
                fruit.fallSpeed = speed;
            }
        }

        /// <summary>
        /// ล้างผลไม้ทั้งหมดที่ยังค้างอยู่ในฉาก (เช่น ตอนเริ่มเกมใหม่)
        /// </summary>
        public void ClearAllActiveFruits()
        {
            CatchFruit[] activeFruits = FindObjectsByType<CatchFruit>(FindObjectsSortMode.None);
            foreach (var fruit in activeFruits)
            {
                Destroy(fruit.gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            // วาดจุดเกิดผลไม้ P1
            Gizmos.DrawLine(new Vector3(p1CenterX - spawnRangeX, spawnY, 0), new Vector3(p1CenterX + spawnRangeX, spawnY, 0));
            // วาดจุดเกิดผลไม้ P2
            Gizmos.DrawLine(new Vector3(p2CenterX - spawnRangeX, spawnY, 0), new Vector3(p2CenterX + spawnRangeX, spawnY, 0));
        }
    }
}
