using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Yatlak.MiniGame.PingPong
{
    public class PingPongPlayerUI : MonoBehaviour
    {
        [Header("Player Labels (UI Text)")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text remainingBallsText;
        [SerializeField] private TMP_Text statePromptText;

        [Header("Vertical Power Bar")]
        [Tooltip("Image หลอดพลัง ที่ตั้งค่า Image Type เป็น Filled และ Fill Method เป็น Vertical")]
        [SerializeField] private Image powerFillImage;
        [SerializeField] private Slider powerSlider;

        [Header("Power Bar Color Gradient (ไล่สี เขียว -> เหลือง -> แดง)")]
        [SerializeField] private Gradient powerGradient;

        private void Awake()
        {
            SetupGradientDefault();
        }

        private void SetupGradientDefault()
        {
            if (powerGradient == null || powerGradient.colorKeys.Length <= 2)
            {
                powerGradient = new Gradient();

                // ไล่สีตามภาพ: เขียวอมฟ้า -> เขียว -> เหลือง -> ส้ม -> แดงสด
                GradientColorKey[] colors = new GradientColorKey[5];
                colors[0] = new GradientColorKey(new Color(0.18f, 0.76f, 0.50f), 0.0f);   // 0%: เขียวอมฟ้า (เบาสุด)
                colors[1] = new GradientColorKey(new Color(0.40f, 0.85f, 0.20f), 0.25f);  // 25%: เขียวสว่าง
                colors[2] = new GradientColorKey(new Color(1.00f, 0.90f, 0.00f), 0.50f);  // 50%: เหลือง
                colors[3] = new GradientColorKey(new Color(1.00f, 0.45f, 0.05f), 0.75f);  // 75%: ส้ม
                colors[4] = new GradientColorKey(new Color(0.95f, 0.05f, 0.10f), 1.0f);   // 100%: แดงสด (แรงสุด)

                GradientAlphaKey[] alphas = new GradientAlphaKey[2];
                alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
                alphas[1] = new GradientAlphaKey(1.0f, 1.0f);

                powerGradient.SetKeys(colors, alphas);
            }
        }

        public void Initialize(string playerName, string keyName, int initialBalls)
        {
            if (playerNameText != null) playerNameText.text = $"{playerName} [{keyName}]";
            SetScore(0);
            SetRemainingBalls(initialBalls);
            SetPower(0f);
            SetPrompt($"Press [{keyName}] to Lock Position");
        }

        public void SetScore(int score)
        {
            if (scoreText != null) scoreText.text = $"SCORE: {score}";
        }

        public void SetRemainingBalls(int remaining)
        {
            if (remainingBallsText != null) remainingBallsText.text = $"BALLS: {remaining}";
        }

        public void SetPrompt(string prompt)
        {
            if (statePromptText != null) statePromptText.text = prompt;
        }

        public void SetPower(float normalizedPower)
        {
            normalizedPower = Mathf.Clamp01(normalizedPower);

            if (powerSlider != null)
            {
                powerSlider.value = normalizedPower;

                // ถ้านำ Slider มาใส่ จะอัปเดตสีของ Fill ด้านใน Slider ตาม Gradient ให้ทันที
                if (powerSlider.fillRect != null)
                {
                    Image sliderFill = powerSlider.fillRect.GetComponent<Image>();
                    if (sliderFill != null && powerGradient != null)
                    {
                        sliderFill.color = powerGradient.Evaluate(normalizedPower);
                    }
                }
            }

            if (powerFillImage != null)
            {
                // เติมหลอดจากล่างขึ้นบน
                powerFillImage.fillAmount = normalizedPower;

                // เปลี่ยนสีตามแรงที่ชาร์จ: เขียว -> เหลือง -> แดง
                if (powerGradient != null)
                {
                    powerFillImage.color = powerGradient.Evaluate(normalizedPower);
                }
            }
        }
    }
}
