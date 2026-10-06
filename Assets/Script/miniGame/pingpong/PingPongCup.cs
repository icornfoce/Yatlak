using System.Collections;
using UnityEngine;
using TMPro;

namespace Yatlak.MiniGame.PingPong
{
    public class PingPongCup : MonoBehaviour
    {
        [Header("Cup Settings")]
        [Tooltip("Points awarded when ball lands in this cup")]
        [SerializeField] private int scoreValue = 10;
        public int ScoreValue => scoreValue;

        [Header("Visual Feedback")]
        [SerializeField] private MeshRenderer cupRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color scoredColor = new Color(0.2f, 1f, 0.3f, 1f);
        [SerializeField] private TMP_Text scoreLabelText;

        [Header("Cup Trigger")]
        [SerializeField] private Collider triggerCollider;

        private Material cupMaterialInstance;
        private bool isScored = false;
        public bool IsScored => isScored;

        private void Awake()
        {
            if (cupRenderer == null)
            {
                cupRenderer = GetComponent<MeshRenderer>();
                if (cupRenderer == null) cupRenderer = GetComponentInChildren<MeshRenderer>();
            }

            if (cupRenderer != null && cupRenderer.material != null)
            {
                cupMaterialInstance = cupRenderer.material;
                cupMaterialInstance.color = normalColor;
            }

            if (triggerCollider == null)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>();
                foreach (var c in colliders)
                {
                    if (c.isTrigger)
                    {
                        triggerCollider = c;
                        break;
                    }
                }
            }

            UpdateLabelText();
        }

        public void SetScoreValue(int value)
        {
            scoreValue = value;
            UpdateLabelText();
        }

        private void UpdateLabelText()
        {
            if (scoreLabelText != null)
            {
                scoreLabelText.text = scoreValue.ToString();
            }
        }

        public void OnBallScored(PingPongBall ball)
        {
            isScored = true;

            // Visual feedback: color flash
            if (cupMaterialInstance != null)
            {
                StartCoroutine(FlashColorRoutine());
            }
        }

        private IEnumerator FlashColorRoutine()
        {
            if (cupMaterialInstance == null) yield break;

            cupMaterialInstance.color = scoredColor;
            yield return new WaitForSeconds(0.4f);
            cupMaterialInstance.color = normalColor;
        }

        public void ResetCup()
        {
            isScored = false;
            if (cupMaterialInstance != null)
            {
                cupMaterialInstance.color = normalColor;
            }
        }

        public void HandleBallTrigger(Collider other)
        {
            PingPongBall ball = other.GetComponent<PingPongBall>();
            if (ball == null)
            {
                ball = other.GetComponentInParent<PingPongBall>();
            }

            if (ball != null)
            {
                ball.OnCupEntered(this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleBallTrigger(other);
        }

        private void OnDestroy()
        {
            if (cupMaterialInstance != null)
            {
                Destroy(cupMaterialInstance);
            }
        }
    }

    public class CupTriggerZone : MonoBehaviour
    {
        [SerializeField] private PingPongCup parentCup;

        public void SetParentCup(PingPongCup cup)
        {
            parentCup = cup;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (parentCup != null)
            {
                parentCup.HandleBallTrigger(other);
            }
            else
            {
                PingPongCup cup = GetComponentInParent<PingPongCup>();
                if (cup != null) cup.HandleBallTrigger(other);
            }
        }
    }
}
