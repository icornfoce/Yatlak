using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MiniGame.MainMinigame
{
    public enum DirectionKey
    {
        Up,
        Down,
        Left,
        Right
    }

    public class SequenceGameManager : MonoBehaviour
    {
        public static SequenceGameManager Instance { get; private set; }

        [Header("Sequence Settings")]
        public int minSequenceLength = 3;
        public int maxSequenceLength = 6;

        [Header("Progress Settings")]
        public float baseProgressPercent = 2.0f;
        public float lengthBonusMultiplier = 0.2f;
        public float targetProgress = 100f;

        [Header("Player Progress")]
        [Range(0f, 100f)] public float p1Progress = 0f;
        [Range(0f, 100f)] public float p2Progress = 0f;
        public bool IsGameOver { get; private set; } = false;

        [Header("Events")]
        public UnityEvent<float, float> OnProgressUpdated;
        public UnityEvent<string> OnGameOver;

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

        public List<DirectionKey> GetSequence(int sequenceIndex)
        {
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

        public void RegisterSequenceCompleted(Catch.PlayerID player, int sequenceLength)
        {
            if (IsGameOver) return;

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
