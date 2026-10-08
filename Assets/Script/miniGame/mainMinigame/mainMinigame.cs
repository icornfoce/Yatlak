using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum DirectionKey
{
    Up,
    Down,
    Left,
    Right
}

public class mainMinigame : MonoBehaviour
{
    private List<List<DirectionKey>> masterRounds = new List<List<DirectionKey>>();

    public List<DirectionKey> p1TargetKeys = new List<DirectionKey>();
    public List<DirectionKey> p2TargetKeys = new List<DirectionKey>();

    public Slider p1Slider;
    public Slider p2Slider;

    public TextMeshProUGUI p1Text; 
    public TextMeshProUGUI p2Text;

    public TextMeshProUGUI p1ResultText;
    public TextMeshProUGUI p2ResultText;

    private int p1Round = 0;
    private int p2Round = 0;

    private int p1Index = 0;
    private int p2Index = 0;

    private bool isGameOver = false;

    public float buffTime = 0f;

    [Header("SFX")]
    public AudioClip correctKeySound;
    public AudioClip wrongKeySound;
    public AudioClip roundSuccessSound;
    public AudioClip winSound;
    private AudioSource audioSource2D;

    [Header("VFX")]
    public GameObject roundSuccessVfxPrefab;
    public GameObject winVfxPrefab;

    [Header("Animation")]
    public Animator p1Animator;
    public Animator p2Animator;
    public string eatTrigger = "Eating";

    private Vector2 p1TextInitialPos;
    private Vector2 p2TextInitialPos;
    private Vector3 p1TextInitialScale = Vector3.one;
    private Vector3 p2TextInitialScale = Vector3.one;
    private Vector3 p1SliderInitialScale = Vector3.one;
    private Vector3 p2SliderInitialScale = Vector3.one;

    private float p1WrongTimer = 0f;
    private float p2WrongTimer = 0f;

    void Awake()
    {
        p1TargetKeys = GetRoundKeys(p1Round);
        p2TargetKeys = GetRoundKeys(p2Round);

        audioSource2D = gameObject.AddComponent<AudioSource>();
        audioSource2D.spatialBlend = 0f;

        if (p1Text != null)
        {
            p1TextInitialPos = p1Text.rectTransform.anchoredPosition;
            p1TextInitialScale = p1Text.transform.localScale;
        }
        if (p2Text != null)
        {
            p2TextInitialPos = p2Text.rectTransform.anchoredPosition;
            p2TextInitialScale = p2Text.transform.localScale;
        }
        if (p1Slider != null) p1SliderInitialScale = p1Slider.transform.localScale;
        if (p2Slider != null) p2SliderInitialScale = p2Slider.transform.localScale;
    }

    void OnEnable()
    {
        if (MinigameManager.Instance != null && (MinigameManager.Instance.P1buff > 0f || MinigameManager.Instance.P2buff > 0f))
        {
            buffTime = 10f;
        }
    }

    void Update()
    {
        if (p1WrongTimer > 0f) p1WrongTimer -= Time.deltaTime;
        if (p2WrongTimer > 0f) p2WrongTimer -= Time.deltaTime;

        if (isGameOver) return;

        if (buffTime > 0f)
        {
            buffTime -= Time.deltaTime;
            if (buffTime <= 0f && MinigameManager.Instance != null)
            {
                MinigameManager.Instance.P1buff = 0f;
                MinigameManager.Instance.P2buff = 0f;
            }
        }

        player1Key();
        player2Key();
        UI();
    }

    List<DirectionKey> GetRoundKeys(int roundIndex)
    {
        while (masterRounds.Count <= roundIndex)
        {
            List<DirectionKey> newRound = new List<DirectionKey>();
            int count = Random.Range(4, 7);

            for (int i = 0; i < count; i++)
            {
                newRound.Add((DirectionKey)Random.Range(0, 4));
            }

            masterRounds.Add(newRound);
        }

        return masterRounds[roundIndex];
    }

    void player1Key()
    {
        if (p1Index >= p1TargetKeys.Count) return;

        DirectionKey current = p1TargetKeys[p1Index];

        bool pressedW = Input.GetKeyDown(KeyCode.W);
        bool pressedS = Input.GetKeyDown(KeyCode.S);
        bool pressedA = Input.GetKeyDown(KeyCode.A);
        bool pressedD = Input.GetKeyDown(KeyCode.D);

        if (pressedW || pressedS || pressedA || pressedD)
        {
            bool isCorrect = (pressedW && current == DirectionKey.Up) ||
                             (pressedS && current == DirectionKey.Down) ||
                             (pressedA && current == DirectionKey.Left) ||
                             (pressedD && current == DirectionKey.Right);

            if (isCorrect)
            {
                p1Index++;
                PlaySound(correctKeySound);
                if (p1Text != null) StartCoroutine(PunchScaleCoroutine(p1Text.transform, p1TextInitialScale, new Vector3(1.12f, 1.12f, 1f), 0.08f));

                if (p1Index >= p1TargetKeys.Count)
                {
                    addScore(1);
                }
            }
            else
            {
                p1Index = 0; 
                p1WrongTimer = 0.22f;
                PlaySound(wrongKeySound);
                if (p1Text != null) StartCoroutine(ShakeRectCoroutine(p1Text.rectTransform, p1TextInitialPos, 0.2f, 14f));
            }
        }
    }

    void player2Key()
    {
        if (p2Index >= p2TargetKeys.Count) return;

        DirectionKey current = p2TargetKeys[p2Index];

        bool pressedUp = Input.GetKeyDown(KeyCode.UpArrow);
        bool pressedDown = Input.GetKeyDown(KeyCode.DownArrow);
        bool pressedLeft = Input.GetKeyDown(KeyCode.LeftArrow);
        bool pressedRight = Input.GetKeyDown(KeyCode.RightArrow);

        if (pressedUp || pressedDown || pressedLeft || pressedRight)
        {
            bool isCorrect = (pressedUp && current == DirectionKey.Up) ||
                             (pressedDown && current == DirectionKey.Down) ||
                             (pressedLeft && current == DirectionKey.Left) ||
                             (pressedRight && current == DirectionKey.Right);

            if (isCorrect)
            {
                p2Index++;
                PlaySound(correctKeySound);
                if (p2Text != null) StartCoroutine(PunchScaleCoroutine(p2Text.transform, p2TextInitialScale, new Vector3(1.12f, 1.12f, 1f), 0.08f));

                if (p2Index >= p2TargetKeys.Count)
                {
                    addScore(2);
                }
            }
            else
            {
                p2Index = 0; 
                p2WrongTimer = 0.22f;
                PlaySound(wrongKeySound);
                if (p2Text != null) StartCoroutine(ShakeRectCoroutine(p2Text.rectTransform, p2TextInitialPos, 0.2f, 14f));
            }
        }
    }

    void addScore(int player)
    {
        if (player == 1)
        {
            float bonus = (MinigameManager.Instance != null) ? MinigameManager.Instance.P1buff : 0f;
            if (p1Slider != null)
            {
                p1Slider.value += 5f + bonus;
                StartCoroutine(PunchScaleCoroutine(p1Slider.transform, p1SliderInitialScale, new Vector3(1.06f, 1.15f, 1f), 0.16f));
            }
            
            p1Round++;
            p1Index = 0;
            p1TargetKeys = GetRoundKeys(p1Round);
            TriggerRoundSuccessVFX(1);
            PlaySound(roundSuccessSound);
            PlayEatAnimation(1);
        }
        else if (player == 2)
        {
            float bonus = (MinigameManager.Instance != null) ? MinigameManager.Instance.P2buff : 0f;
            if (p2Slider != null)
            {
                p2Slider.value += 5f + bonus;
                StartCoroutine(PunchScaleCoroutine(p2Slider.transform, p2SliderInitialScale, new Vector3(1.06f, 1.15f, 1f), 0.16f));
            }

            p2Round++;
            p2Index = 0;
            p2TargetKeys = GetRoundKeys(p2Round);
            TriggerRoundSuccessVFX(2);
            PlaySound(roundSuccessSound);
            PlayEatAnimation(2);
        }

        checkWin();
    }

    void UI()
    {
        string p1DisplayText = "";

        for (int i = 0; i < p1TargetKeys.Count; i++)
        {
            string keyChar = GetKeyLetter(p1TargetKeys[i]);
            if (p1WrongTimer > 0f)
            {
                p1DisplayText += $"<color=#FF4444>{keyChar}</color> ";
            }
            else if (i < p1Index)
            {
                p1DisplayText += $"<color=#00FF00>{keyChar}</color> ";
            }
            else
            {  
                p1DisplayText += $"{keyChar} ";
            }
        }
        if (p1Text != null) p1Text.text = p1DisplayText.TrimEnd();

        string p2DisplayText = "";

        for (int i = 0; i < p2TargetKeys.Count; i++)
        {
            string keyChar = GetKeyArrow(p2TargetKeys[i]);
            if (p2WrongTimer > 0f)
            {
                p2DisplayText += $"<color=#FF4444>{keyChar}</color> ";
            }
            else if (i < p2Index)
            {
                p2DisplayText += $"<color=#00FF00>{keyChar}</color> ";
            }
            else
            {
                p2DisplayText += $"{keyChar} ";
            }
        }
        if (p2Text != null) p2Text.text = p2DisplayText.TrimEnd();
    }

    string GetKeyLetter(DirectionKey key)
    {
        switch (key)
        {
            case DirectionKey.Up: return "W";
            case DirectionKey.Down: return "S";
            case DirectionKey.Left: return "A";
            case DirectionKey.Right: return "D";
            default: return "";
        }
    }

    string GetKeyArrow(DirectionKey key)
    {
        switch (key)
        {
            case DirectionKey.Up: return "↑";
            case DirectionKey.Down: return "↓";
            case DirectionKey.Left: return "←";
            case DirectionKey.Right: return "→";
            default: return "";
        }
    }

    void checkWin()
    {
        if (p1Slider != null && p1Slider.value >= 100f)
        {
            if (p1ResultText != null)
            {
                p1ResultText.gameObject.SetActive(true);
                p1ResultText.text = "WIN";
                StartCoroutine(PunchScaleCoroutine(p1ResultText.transform, Vector3.one, new Vector3(1.4f, 1.4f, 1f), 0.35f));
            }
            if (p2ResultText != null)
            {
                p2ResultText.gameObject.SetActive(true);
                p2ResultText.text = "LOSE";
            }
            isGameOver = true;
            PlaySound(winSound);
            TriggerWinVFX(1);
        }
        else if (p2Slider != null && p2Slider.value >= 100f)
        {
            if (p2ResultText != null)
            {
                p2ResultText.gameObject.SetActive(true);
                p2ResultText.text = "WIN";
                StartCoroutine(PunchScaleCoroutine(p2ResultText.transform, Vector3.one, new Vector3(1.4f, 1.4f, 1f), 0.35f));
            }
            if (p1ResultText != null)
            {
                p1ResultText.gameObject.SetActive(true);
                p1ResultText.text = "LOSE";
            }
            isGameOver = true;
            PlaySound(winSound);
            TriggerWinVFX(2);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource2D != null && clip != null)
        {
            audioSource2D.PlayOneShot(clip);
        }
    }

    void PlayEatAnimation(int player)
    {
        Animator anim = (player == 1) ? p1Animator : p2Animator;
        if (anim != null)
        {
            bool hasTrigger = false;
            foreach (var param in anim.parameters)
            {
                if (param.name == eatTrigger)
                {
                    hasTrigger = true;
                    break;
                }
            }

            if (hasTrigger)
            {
                anim.SetTrigger(eatTrigger);
            }
            else
            {
                anim.Play(eatTrigger, 0, 0f);
            }
        }
    }

    void TriggerRoundSuccessVFX(int playerNumber)
    {
        if (roundSuccessVfxPrefab != null)
        {
            Slider slider = (playerNumber == 1) ? p1Slider : p2Slider;
            Vector3 spawnPos = (slider != null) ? slider.transform.position : Vector3.zero;
            GameObject vfx = Instantiate(roundSuccessVfxPrefab, spawnPos, Quaternion.identity);
            Destroy(vfx, 1.5f);
        }
    }

    void TriggerWinVFX(int playerNumber)
    {
        if (winVfxPrefab != null)
        {
            GameObject vfx = Instantiate(winVfxPrefab, Vector3.zero, Quaternion.identity);
            Destroy(vfx, 2.5f);
        }
    }

    IEnumerator ShakeRectCoroutine(RectTransform target, Vector2 basePos, float duration, float magnitude)
    {
        if (target == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.deltaTime;
            float xOffset = Random.Range(-1f, 1f) * magnitude;
            target.anchoredPosition = basePos + new Vector2(xOffset, 0f);
            yield return null;
        }
        if (target != null) target.anchoredPosition = basePos;
    }

    IEnumerator PunchScaleCoroutine(Transform target, Vector3 baseScale, Vector3 targetScale, float duration)
    {
        if (target == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curve = Mathf.Sin(t * Mathf.PI);
            target.localScale = Vector3.Lerp(baseScale, targetScale, curve);
            yield return null;
        }
        if (target != null) target.localScale = baseScale;
    }
}