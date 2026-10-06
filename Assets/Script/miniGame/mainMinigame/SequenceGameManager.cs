using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MiniGame.MainMinigame
{
    public enum DirectionKey
    {
        Up,    // W หรือ ↑
        Down,  // S หรือ ↓
        Left,  // A หรือ ←
        Right  // D หรือ →
    }

    /// <summary>
    /// ผู้จัดการมินิเกมแข่งกดปุ่มตามลำดับ (QTE Sequence)
    /// สุ่มปุ่ม 3-6 ตัว แข่งกันกดเพิ่มหลอด Bar ถึง 100% ก่อนชนะ
    /// </summary>
    public class SequenceGameManager : MonoBehaviour
    {
        public static SequenceGameManager Instance { get; private set; }

        [Header("Sequence Settings (การสุ่มโจทย์)")]
        [Tooltip("จำนวนปุ่มขั้นต่ำต่อ 1 ชุด (3 ตัว)")]
        public int minSequenceLength = 3;

        [Tooltip("จำนวนปุ่มสูงสุดต่อ 1 ชุด (6 ตัว)")]
        public int maxSequenceLength = 6;

        [Header("Progress Bar Settings (ความเร็วการเพิ่มหลอด)")]
        [Tooltip("เปอร์เซ็นต์พื้นฐานที่เพิ่มขึ้นเมื่อกดจบ 1 ชุด (แนะนำ 2.0% - 2.5% เพื่อให้ 25% ใช้เวลาประมาณ 15-45 วิ)")]
        public float baseProgressPercent = 2.0f;

        [Tooltip("โบนัสเปอร์เซ็นต์ตามความยาวของชุด (เช่น +0.2% ต่อความยาว 1 ตัว)")]
        public float lengthBonusMultiplier = 0.2f;

        [Tooltip("เป้าหมายเปอร์เซ็นต์เพื่อชนะ (100%)")]
        public float targetProgress = 100f;

        [Header("Player Progress")]
        [Range(0f, 100f)] public float p1Progress = 0f;
        [Range(0f, 100f)] public float p2Progress = 0f;
        public bool IsGameOver { get; private set; } = false;

        [Header("Events")]
        public UnityEvent<float, float> OnProgressUpdated; // P1 Progress, P2 Progress
        public UnityEvent<string> OnGameOver;             // Winner announcement (PLAYER 1 WINS / PLAYER 2 WINS)

        // คิวของชุดลำดับปุ่มที่สุ่มไว้ล่วงหน้าเพื่อให้ทั้ง 2 คนได้โจทย์เดียวกันเป๊ะ
        private readonly List<List<DirectionKey>> preGeneratedSequences = new List<List<DirectionKey>>();
        private const int PRE_GEN_COUNT = 150;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (preGeneratedSequences.Count == 0)
                {
                    GenerateSequencePool();
                }
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

        public void StartNewGame()
        {
            p1Progress = 0f;
            p2Progress = 0f;
            IsGameOver = false;

            if (preGeneratedSequences.Count == 0)
            {
                GenerateSequencePool();
            }

            OnProgressUpdated?.Invoke(p1Progress, p2Progress);
        }

        /// <summary>
        /// สุ่มสร้างชุดลำดับล่วงหน้า เพื่อให้ P1 และ P2 ได้รับโจทย์ชุดที่ 1, 2, 3... เหมือนกันเป๊ะ
        /// </summary>
        private void GenerateSequencePool()
        {
            preGeneratedSequences.Clear();
            for (int i = 0; i < PRE_GEN_COUNT; i++)
            {
                int length = Random.Range(minSequenceLength, maxSequenceLength + 1);
                List<DirectionKey> sequence = new List<DirectionKey>();
                for (int j = 0; j < length; j++)
                {
                    DirectionKey randomDir = (DirectionKey)Random.Range(0, 4);
                    sequence.Add(randomDir);
                }
                preGeneratedSequences.Add(sequence);
            }
        }

        /// <summary>
        /// ขอรับโจทย์ชุดที่ sequenceIndex
        /// </summary>
        public List<DirectionKey> GetSequence(int sequenceIndex)
        {
            // ถ้าเล่นไปไกลจนเกิน Pool ให้สุ่มเพิ่มต่อท้าย
            while (sequenceIndex >= preGeneratedSequences.Count)
            {
                int length = Random.Range(minSequenceLength, maxSequenceLength + 1);
                List<DirectionKey> seq = new List<DirectionKey>();
                for (int j = 0; j < length; j++)
                {
                    seq.Add((DirectionKey)Random.Range(0, 4));
                }
                preGeneratedSequences.Add(seq);
            }

            return new List<DirectionKey>(preGeneratedSequences[sequenceIndex]);
        }

        /// <summary>
        /// เมื่อผู้เล่นกดชุดนั้นสำเร็จ
        /// </summary>
        public void RegisterSequenceCompleted(Catch.PlayerID player, int sequenceLength)
        {
            if (IsGameOver) return;

            // คำนวณเปอร์เซ็นต์ที่เพิ่ม: ค่าพื้นฐาน + โบนัสความยาว
            float gainedPercent = baseProgressPercent + (sequenceLength * lengthBonusMultiplier);

            if (player == Catch.PlayerID.Player1)
            {
                p1Progress = Mathf.Min(targetProgress, p1Progress + gainedPercent);
            }
            else
            {
                p2Progress = Mathf.Min(targetProgress, p2Progress + gainedPercent);
            }

            OnProgressUpdated?.Invoke(p1Progress, p2Progress);

            // ตรวจสอบเงื่อนไขชนะ (ถึง 100%)
            if (p1Progress >= targetProgress || p2Progress >= targetProgress)
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            IsGameOver = true;
            string winnerMessage;

            if (p1Progress >= targetProgress && p2Progress >= targetProgress)
            {
                winnerMessage = "IT'S A DRAW!";
            }
            else if (p1Progress >= targetProgress)
            {
                winnerMessage = "PLAYER 1 WINS!";
            }
            else
            {
                winnerMessage = "PLAYER 2 WINS!";
            }

            Debug.Log($"[SequenceGameManager] Game Over! {winnerMessage}");
            OnGameOver?.Invoke(winnerMessage);
        }
    }
}
