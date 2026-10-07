using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniGame.MainMinigame
{
    public class SequenceUI : MonoBehaviour
    {
        [Header("Player Settings")]
        public Catch.PlayerID playerID = Catch.PlayerID.Player1;
        public SequencePlayerController playerController;

        [Header("Sequence Display")]
        public TextMeshProUGUI sequenceTextTMP;

        [Header("Progress Bar")]
        public RawImage fillRawImage;
        public RectTransform bgBarRect;
        public float smoothSpeed = 6f;
        public TextMeshProUGUI progressPercentTMP;

        [Header("Result Display")]
        public TextMeshProUGUI resultTextTMP;

        [Header("Colors")]
        public Color normalColor = Color.white;
        public Color passedColor = new Color(0.3f, 1f, 0.4f);
        public Color wrongColor = new Color(1f, 0.3f, 0.3f);

        private Coroutine flashRoutine;
        private float currentHeight = 0f;
        private float targetPercent = 0f;

        private void Start()
        {
            if (playerController == null)
            {
                SequencePlayerController[] controllers = FindObjectsByType<SequencePlayerController>(FindObjectsSortMode.None);
                foreach (var ctrl in controllers)
                {
                    if (ctrl.playerID == this.playerID)
                    {
                        playerController = ctrl;
                        break;
                    }
                }
            }

            if (playerController != null)
            {
                playerController.OnSequenceUpdated.AddListener(OnSequenceUpdated);
                playerController.OnWrongKey.AddListener(OnWrongKey);

                if (playerController.currentSequence != null && playerController.currentSequence.Count > 0)
                {
                    OnSequenceUpdated(playerController.currentSequence, playerController.currentIndex);
                }
            }

            if (SequenceGameManager.Instance != null)
            {
                SequenceGameManager.Instance.OnProgressUpdated.AddListener(OnProgressUpdated);
                SequenceGameManager.Instance.OnGameOver.AddListener(OnGameOver);
            }

            if (fillRawImage != null)
            {
                RectTransform rt = fillRawImage.rectTransform;
                rt.pivot = new Vector2(rt.pivot.x, 0f);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
            }

            currentHeight = 0f;
            targetPercent = 0f;

            if (resultTextTMP != null)
            {
                resultTextTMP.text = "";
            }

            if (progressPercentTMP != null)
            {
                progressPercentTMP.text = "0%";
            }
        }

        private void OnDestroy()
        {
            if (playerController != null)
            {
                playerController.OnSequenceUpdated.RemoveListener(OnSequenceUpdated);
                playerController.OnWrongKey.RemoveListener(OnWrongKey);
            }

            if (SequenceGameManager.Instance != null)
            {
                SequenceGameManager.Instance.OnProgressUpdated.RemoveListener(OnProgressUpdated);
                SequenceGameManager.Instance.OnGameOver.RemoveListener(OnGameOver);
            }
        }

        private void Update()
        {
            SmoothUpdateBar();
        }

        private void SmoothUpdateBar()
        {
            if (fillRawImage == null) return;

            float maxHeight = (bgBarRect != null) ? bgBarRect.rect.height : 100f;
            float targetHeight = maxHeight * Mathf.Clamp01(targetPercent / 100f);

            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * smoothSpeed);

            if (Mathf.Abs(targetHeight - currentHeight) < 0.2f)
            {
                currentHeight = targetHeight;
            }

            fillRawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentHeight);

            if (progressPercentTMP != null && maxHeight > 0f)
            {
                float currentPercent = (currentHeight / maxHeight) * 100f;
                progressPercentTMP.text = $"{Mathf.FloorToInt(currentPercent)}%";
            }
        }

        private void OnSequenceUpdated(List<DirectionKey> sequence, int currentIndex)
        {
            if (sequenceTextTMP == null || sequence == null) return;

            string passedHex = ColorUtility.ToHtmlStringRGB(passedColor);
            string normalHex = ColorUtility.ToHtmlStringRGB(normalColor);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < sequence.Count; i++)
            {
                string keySymbol = GetKeySymbol(sequence[i]);
                string colorHex = (i < currentIndex) ? passedHex : normalHex;

                sb.Append($"<color=#{colorHex}>{keySymbol}</color> ");
            }

            sequenceTextTMP.text = sb.ToString().TrimEnd();
        }

        private string GetKeySymbol(DirectionKey dir)
        {
            if (playerID == Catch.PlayerID.Player1)
            {
                switch (dir)
                {
                    case DirectionKey.Up: return "W";
                    case DirectionKey.Down: return "S";
                    case DirectionKey.Left: return "A";
                    case DirectionKey.Right: return "D";
                }
            }
            else
            {
                switch (dir)
                {
                    case DirectionKey.Up: return "↑";
                    case DirectionKey.Down: return "↓";
                    case DirectionKey.Left: return "←";
                    case DirectionKey.Right: return "→";
                }
            }
            return "?";
        }

        private void OnWrongKey()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }
            flashRoutine = StartCoroutine(FlashWrongEffect());
        }

        private IEnumerator FlashWrongEffect()
        {
            if (sequenceTextTMP != null)
            {
                Color originalColor = sequenceTextTMP.color;
                sequenceTextTMP.color = wrongColor;
                yield return new WaitForSeconds(0.18f);
                sequenceTextTMP.color = originalColor;
            }
        }

        private void OnProgressUpdated(float p1Prog, float p2Prog)
        {
            targetPercent = (playerID == Catch.PlayerID.Player1) ? p1Prog : p2Prog;
        }

        private void OnGameOver(string winnerMessage)
        {
            if (resultTextTMP == null || SequenceGameManager.Instance == null) return;

            float p1 = SequenceGameManager.Instance.p1Progress;
            float p2 = SequenceGameManager.Instance.p2Progress;

            if (p1 >= 100f && p2 >= 100f)
            {
                resultTextTMP.text = "DRAW";
            }
            else if (playerID == Catch.PlayerID.Player1)
            {
                resultTextTMP.text = (p1 >= 100f) ? "WIN" : "LOSE";
            }
            else
            {
                resultTextTMP.text = (p2 >= 100f) ? "WIN" : "LOSE";
            }
        }
    }
}
