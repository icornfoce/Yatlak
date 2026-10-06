using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MiniGame.MainMinigame
{
    /// <summary>
    /// ตัวควบคุมการกดปุ่มของผู้เล่นแต่ละคน (P1 ใช้ WASD, P2 ใช้ลูกศร)
    /// ตรวจสอบการกดตามลำดับ: ถ้ากดถูกตัวถัดไป ถ้ากดผิดล้างชุดเริ่มใหม่จากตัวแรก
    /// </summary>
    public class SequencePlayerController : MonoBehaviour
    {
        [Header("Player Target")]
        public Catch.PlayerID playerID = Catch.PlayerID.Player1;

        [Header("State")]
        public List<DirectionKey> currentSequence = new List<DirectionKey>();
        public int currentIndex = 0;
        public int completedSequenceCount = 0;

        [Header("Audio (ทางเลือก)")]
        public AudioClip correctSound;
        public AudioClip completeSequenceSound;
        public AudioClip wrongSound;

        [Header("Events")]
        public UnityEvent<List<DirectionKey>, int> OnSequenceUpdated; // currentSequence, currentIndex
        public UnityEvent OnWrongKey;                                 // เมื่อกดผิด
        public UnityEvent OnSequenceCompleted;                        // เมื่อกดครบชุด

        private void Start()
        {
            LoadNewSequence();
        }

        private void Update()
        {
            if (SequenceGameManager.Instance != null && SequenceGameManager.Instance.IsGameOver)
            {
                return;
            }

            CheckInput();
        }

        private void LoadNewSequence()
        {
            if (SequenceGameManager.Instance != null)
            {
                currentSequence = SequenceGameManager.Instance.GetSequence(completedSequenceCount);
                currentIndex = 0;
                OnSequenceUpdated?.Invoke(currentSequence, currentIndex);
            }
        }

        private void CheckInput()
        {
            if (currentSequence == null || currentSequence.Count == 0) return;

            DirectionKey? pressedDir = GetPressedDirection();
            if (!pressedDir.HasValue) return;

            // ตรวจสอบว่าปุ่มที่กดตรงกับตำแหน่งปัจจุบันหรือไม่
            DirectionKey expectedDir = currentSequence[currentIndex];

            if (pressedDir.Value == expectedDir)
            {
                // กดถูกต้อง!
                currentIndex++;
                PlaySound(correctSound);

                // ถ้ากดครบทั้งชุด (3-6 ตัว)
                if (currentIndex >= currentSequence.Count)
                {
                    PlaySound(completeSequenceSound);
                    OnSequenceCompleted?.Invoke();

                    if (SequenceGameManager.Instance != null)
                    {
                        SequenceGameManager.Instance.RegisterSequenceCompleted(playerID, currentSequence.Count);
                    }

                    // ข้ามไปยังโจทย์ชุดถัดไป
                    completedSequenceCount++;
                    LoadNewSequence();
                }
                else
                {
                    OnSequenceUpdated?.Invoke(currentSequence, currentIndex);
                }
            }
            else
            {
                // กดผิดปุ่ม! -> ล้างชุดนั้นทันที ต้องเริ่มกดตั้งแต่ตัวแรกใหม่
                PlaySound(wrongSound);
                currentIndex = 0;
                OnWrongKey?.Invoke();
                OnSequenceUpdated?.Invoke(currentSequence, currentIndex);
            }
        }

        private DirectionKey? GetPressedDirection()
        {
            if (playerID == Catch.PlayerID.Player1)
            {
                // P1: ใช้ W A S D
                if (Input.GetKeyDown(KeyCode.W)) return DirectionKey.Up;
                if (Input.GetKeyDown(KeyCode.S)) return DirectionKey.Down;
                if (Input.GetKeyDown(KeyCode.A)) return DirectionKey.Left;
                if (Input.GetKeyDown(KeyCode.D)) return DirectionKey.Right;
            }
            else
            {
                // P2: ใช้ลูกศร (Arrow Keys)
                if (Input.GetKeyDown(KeyCode.UpArrow)) return DirectionKey.Up;
                if (Input.GetKeyDown(KeyCode.DownArrow)) return DirectionKey.Down;
                if (Input.GetKeyDown(KeyCode.LeftArrow)) return DirectionKey.Left;
                if (Input.GetKeyDown(KeyCode.RightArrow)) return DirectionKey.Right;
            }

            return null;
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
    }
}
