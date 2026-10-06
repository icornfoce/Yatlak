using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniGame.MainMinigame
{
    /// <summary>
    /// แสดงผล UI มินิเกมแข่งกดปุ่มตามลำดับ (QTE Sequence):
    /// - แสดงลำดับปุ่ม (P1 เป็น WASD, P2 เป็นลูกศร ↑ ↓ ← →)
    /// - ไฮไลต์สีเขียวตัวที่กดถูกต้องแล้ว และกะพริบสีแดงเมื่อกดผิด
    /// - หลอด Progress Bar แบบ RawImage ปรับความสูงค่อยๆ เพิ่มขึ้นเทียบกับ BG Bar
    /// - ข้อความผลแพ้ชนะ (WIN / LOSE)
    /// </summary>
    public class SequenceUI : MonoBehaviour
    {
        [Header("Target Player")]
        public Catch.PlayerID playerID = Catch.PlayerID.Player1;
        public SequencePlayerController playerController;

        [Header("Sequence Display (TextMeshPro)")]
        [Tooltip("Text แสดงลำดับปุ่ม (เช่น W A S D หรือ ↑ ← ↓ →)")]
        public TextMeshProUGUI sequenceTextTMP;

        [Header("Progress Bar Display (RawImage Vertical Bar)")]
        [Tooltip("RawImage ของหลอดที่จะขยายความสูงขึ้นมา")]
        public RawImage fillRawImage;

        [Tooltip("RectTransform พื้นหลังของหลอด Bar (ใช้เพื่อเทียบความสูงสูงสุด 100%)")]
        public RectTransform bgBarRect;

        [Tooltip("ความเร็วในการค่อยๆ ขยับความสูงขึ้นแบบ Smooth")]
        public float smoothSpeed = 6f;

        [Tooltip("Text แสดงตัวเลขเปอร์เซ็นต์ (เช่น 25%)")]
        public TextMeshProUGUI progressPercentTMP;

        [Header("Result Display")]
        [Tooltip("Text แสดงผลแพ้ชนะตอนจบเกม (WIN / LOSE)")]
        public TextMeshProUGUI resultTextTMP;

        [Header("Colors (สีไฮไลต์)")]
        public Color normalColor = Color.white;
        public Color passedColor = new Color(0.3f, 1f, 0.4f); // สีเขียวสว่างสำหรับตัวที่กดผ่านแล้ว
        public Color wrongColor = new Color(1f, 0.3f, 0.3f);   // สีแดงเวลากดผิด

        private Coroutine flashRoutine;
        private float currentHeight = 0f;
        private float targetPercent = 0f;

        private void Start()
        {
            // ค้นหา PlayerController ที่มี PlayerID ตรงกับ UI นี้โดยเฉพาะ
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

                // ดึงโจทย์ชุดปัจจุบันมาแสดงผลบน UI ทันที
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

            // ตั้งค่า Pivot ของ RawImage ให้อยู่ด้านล่าง เพื่อให้ขยายความสูงขึ้นด้านบน
            if (fillRawImage != null)
            {
                RectTransform rt = fillRawImage.rectTransform;
                rt.pivot = new Vector2(rt.pivot.x, 0f);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
            }

            currentHeight = 0f;
            targetPercent = 0f;

            // ซ่อนข้อความผลการแข่งขันตอนเริ่มเกม
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

        /// <summary>
        /// ค่อยๆ เพิ่มความสูงของ RawImage ให้สูงขึ้นตามสัดส่วนความสูงของ BG Bar แบบ Smooth
        /// </summary>
        private void SmoothUpdateBar()
        {
            if (fillRawImage == null) return;

            // คำนวณความสูงเป้าหมายเทียบกับ BG Bar
            float maxHeight = (bgBarRect != null) ? bgBarRect.rect.height : 100f;
            float targetHeight = maxHeight * Mathf.Clamp01(targetPercent / 100f);

            // ค่อยๆ Lerp ความสูงปัจจุบันเข้าหาเป้าหมาย
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * smoothSpeed);

            // เมื่อเข้าใกล้เป้าหมายมากๆ ให้ปัดเท่ากัน
            if (Mathf.Abs(targetHeight - currentHeight) < 0.2f)
            {
                currentHeight = targetHeight;
            }

            // กำหนดความสูงให้กับ RawImage
            fillRawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentHeight);

            // อัปเดตตัวเลข % บนหน้าจอ
            if (progressPercentTMP != null && maxHeight > 0f)
            {
                float currentPercent = (currentHeight / maxHeight) * 100f;
                progressPercentTMP.text = $"{Mathf.FloorToInt(currentPercent)}%";
            }
        }

        /// <summary>
        /// อัปเดตข้อความลำดับปุ่ม และไฮไลต์ตัวที่กดผ่านแล้วเป็นสีเขียว
        /// </summary>
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
                // P1: แสดง W A S D
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
                // P2: แสดงลูกศร
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

        /// <summary>
        /// เมื่อกดผิด กะพริบสีแดงเตือนผู้เล่น
        /// </summary>
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

        /// <summary>
        /// รับค่าความคืบหน้าจาก GameManager
        /// </summary>
        private void OnProgressUpdated(float p1Prog, float p2Prog)
        {
            targetPercent = (playerID == Catch.PlayerID.Player1) ? p1Prog : p2Prog;
        }

        /// <summary>
        /// เมื่อมีคนถึง 100% ประกาศผล WIN / LOSE
        /// </summary>
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
