using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MiniGame.MainMinigame
{
    public class SequencePlayerController : MonoBehaviour
    {
        [Header("Player Settings")]
        public Catch.PlayerID playerID = Catch.PlayerID.Player1;

        [Header("State")]
        public List<DirectionKey> currentSequence = new List<DirectionKey>();
        public int currentIndex = 0;
        public int completedSequenceCount = 0;

        [Header("Audio")]
        public AudioClip correctSound;
        public AudioClip completeSequenceSound;
        public AudioClip wrongSound;

        [Header("Events")]
        public UnityEvent<List<DirectionKey>, int> OnSequenceUpdated;
        public UnityEvent OnWrongKey;
        public UnityEvent OnSequenceCompleted;

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

            DirectionKey expectedDir = currentSequence[currentIndex];

            if (pressedDir.Value == expectedDir)
            {
                currentIndex++;
                PlaySound(correctSound);

                if (currentIndex >= currentSequence.Count)
                {
                    PlaySound(completeSequenceSound);
                    OnSequenceCompleted?.Invoke();

                    if (SequenceGameManager.Instance != null)
                    {
                        SequenceGameManager.Instance.RegisterSequenceCompleted(playerID, currentSequence.Count);
                    }

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
                if (Input.GetKeyDown(KeyCode.W)) return DirectionKey.Up;
                if (Input.GetKeyDown(KeyCode.S)) return DirectionKey.Down;
                if (Input.GetKeyDown(KeyCode.A)) return DirectionKey.Left;
                if (Input.GetKeyDown(KeyCode.D)) return DirectionKey.Right;
            }
            else
            {
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
