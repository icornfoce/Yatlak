using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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
    public GameObject positionP1;
    public GameObject positionP2;

    [Header("SFX")]
    public AudioClip correctKeySound;
    public AudioClip wrongKeySound;
    public AudioClip roundSuccessSound;
    public AudioClip winSound;
    private AudioSource audioSource2D;
    public AudioClip buffSound;

    [Header("VFX")]
    public GameObject roundSuccessVfxPrefab;
    public GameObject winVfxPrefab;
    public GameObject buffVfx;

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

    private float p1SliderTarget = 0f;
    private float p2SliderTarget = 0f;

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
        if (p1Slider != null)
        {
            p1SliderInitialScale = p1Slider.transform.localScale;
            p1SliderTarget = p1Slider.value;
        }
        if (p2Slider != null)
        {
            p2SliderInitialScale = p2Slider.transform.localScale;
            p2SliderTarget = p2Slider.value;
        }

        if (p1ResultText != null) p1ResultText.gameObject.SetActive(false);
        if (p2ResultText != null) p2ResultText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (p1Slider != null) p1SliderTarget = p1Slider.value;
        if (p2Slider != null) p2SliderTarget = p2Slider.value;

        if (MinigameManager.Instance != null && (MinigameManager.Instance.P1buff > 0f || MinigameManager.Instance.P2buff > 0f))
        {
            buffTime = 5f;
            if (MinigameManager.Instance.P1buff > 0f) TriggerBuffVFX(1);
            if (MinigameManager.Instance.P2buff > 0f) TriggerBuffVFX(2);
        }

        if (p1Slider != null && p2Slider != null)
        {
            if (p1Slider.value >= 75f || p2Slider.value >= 75f)
            {
                if (p1Slider.value < p2Slider.value)
                {
                    if (p1Slider.value < 25f)
                    {
                        if (MinigameManager.Instance != null) MinigameManager.Instance.P1buff = 10f;
                        buffTime = 5f;
                        TriggerBuffVFX(1);
                    }
                    else if (p1Slider.value < 50f)
                    {
                        if (MinigameManager.Instance != null) MinigameManager.Instance.P1buff = 5f;
                        buffTime = 5f;
                        TriggerBuffVFX(1);
                    }
                }
                else if (p2Slider.value < p1Slider.value)
                {
                    if (p2Slider.value < 25f)
                    {
                        if (MinigameManager.Instance != null) MinigameManager.Instance.P2buff = 10f;
                        buffTime = 5f;
                        TriggerBuffVFX(2);
                    }
                    else if (p2Slider.value < 50f)
                    {
                        if (MinigameManager.Instance != null) MinigameManager.Instance.P2buff = 5f;
                        buffTime = 5f;
                        TriggerBuffVFX(2);
                    }
                }
            }
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
                if (p1Text != null)
                {
                    p1Text.transform.DOKill();
                    p1Text.transform.localScale = p1TextInitialScale;
                    p1Text.transform.DOPunchScale(Vector3.one * 0.12f, 0.08f, 4, 0.5f);
                }

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
                if (p1Text != null)
                {
                    p1Text.rectTransform.DOKill();
                    p1Text.rectTransform.anchoredPosition = p1TextInitialPos;
                    p1Text.rectTransform.DOShakeAnchorPos(0.22f, new Vector2(16f, 0f), 18, 90f, false, true);
                }
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
                if (p2Text != null)
                {
                    p2Text.transform.DOKill();
                    p2Text.transform.localScale = p2TextInitialScale;
                    p2Text.transform.DOPunchScale(Vector3.one * 0.12f, 0.08f, 4, 0.5f);
                }

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
                if (p2Text != null)
                {
                    p2Text.rectTransform.DOKill();
                    p2Text.rectTransform.anchoredPosition = p2TextInitialPos;
                    p2Text.rectTransform.DOShakeAnchorPos(0.22f, new Vector2(16f, 0f), 18, 90f, false, true);
                }
            }
        }
    }

    void addScore(int player)
    {
        if (player == 1)
        {
            float bonus = (MinigameManager.Instance != null) ? MinigameManager.Instance.P1buff : 0f;
            p1SliderTarget = Mathf.Min(p1SliderTarget + 5f + bonus, 100f);
            if (p1Slider != null)
            {
                p1Slider.DOKill();
                p1Slider.DOValue(p1SliderTarget, 0.2f).SetEase(Ease.OutQuad).OnUpdate(checkWin);
                p1Slider.transform.DOKill();
                p1Slider.transform.localScale = p1SliderInitialScale;
                p1Slider.transform.DOPunchScale(new Vector3(0.08f, 0.15f, 0f), 0.2f, 5, 0.5f);
            }
            if (p1Text != null)
            {
                p1Text.transform.DOKill();
                p1Text.transform.localScale = p1TextInitialScale;
                p1Text.transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, 5, 0.5f);
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
            p2SliderTarget = Mathf.Min(p2SliderTarget + 5f + bonus, 100f);
            if (p2Slider != null)
            {
                p2Slider.DOKill();
                p2Slider.DOValue(p2SliderTarget, 0.2f).SetEase(Ease.OutQuad).OnUpdate(checkWin);
                p2Slider.transform.DOKill();
                p2Slider.transform.localScale = p2SliderInitialScale;
                p2Slider.transform.DOPunchScale(new Vector3(0.08f, 0.15f, 0f), 0.2f, 5, 0.5f);
            }
            if (p2Text != null)
            {
                p2Text.transform.DOKill();
                p2Text.transform.localScale = p2TextInitialScale;
                p2Text.transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, 5, 0.5f);
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

        if (!isGameOver)
        {
            if (p1ResultText != null)
            {
                if (MinigameManager.Instance != null && MinigameManager.Instance.P1buff > 0f && buffTime > 0f)
                {
                    p1ResultText.gameObject.SetActive(true);
                    string mult = (MinigameManager.Instance.P1buff >= 10f) ? "x3" : "x2";
                    p1ResultText.text = $"BUFF {mult}";
                }
                else if (p1ResultText.text.StartsWith("BUFF"))
                {
                    p1ResultText.gameObject.SetActive(false);
                    p1ResultText.text = "";
                }
            }

            if (p2ResultText != null)
            {
                if (MinigameManager.Instance != null && MinigameManager.Instance.P2buff > 0f && buffTime > 0f)
                {
                    p2ResultText.gameObject.SetActive(true);
                    string mult = (MinigameManager.Instance.P2buff >= 10f) ? "x3" : "x2";
                    p2ResultText.text = $"BUFF {mult}";
                }
                else if (p2ResultText.text.StartsWith("BUFF"))
                {
                    p2ResultText.gameObject.SetActive(false);
                    p2ResultText.text = "";
                }
            }
        }
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
        if (isGameOver) return;

        if (p1Slider != null && (p1Slider.value >= 100f || p1SliderTarget >= 100f))
        {
            isGameOver = true;
            AnimateResult(p1ResultText, "WIN", true);
            AnimateResult(p2ResultText, "LOSE", false);
            PlaySound(winSound);
            TriggerWinVFX(1);
        }
        else if (p2Slider != null && (p2Slider.value >= 100f || p2SliderTarget >= 100f))
        {
            isGameOver = true;
            AnimateResult(p2ResultText, "WIN", true);
            AnimateResult(p1ResultText, "LOSE", false);
            PlaySound(winSound);
            TriggerWinVFX(2);
        }
    }

    void AnimateResult(TextMeshProUGUI textComp, string message, bool isWinner)
    {
        if (textComp == null) return;
        textComp.gameObject.SetActive(true);
        textComp.text = message;
        textComp.transform.DOKill();
        textComp.transform.localScale = Vector3.zero;

        if (isWinner)
        {
            textComp.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            textComp.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 4, 0.5f).SetDelay(0.5f);
        }
        else
        {
            textComp.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutQuad);
            textComp.rectTransform.DOShakeAnchorPos(0.4f, new Vector2(10f, 0f), 15).SetDelay(0.4f);
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
            GameObject targetPos = (playerNumber == 1) ? positionP1 : positionP2;
            Vector3 spawnPos = (targetPos != null) ? targetPos.transform.position : 
                (((playerNumber == 1) ? p1Slider : p2Slider) != null ? ((playerNumber == 1) ? p1Slider : p2Slider).transform.position : Vector3.zero);
            GameObject vfx = Instantiate(roundSuccessVfxPrefab, spawnPos, Quaternion.identity);
            Destroy(vfx, 1.5f);
        }
    }

    void TriggerBuffVFX(int playerNumber)
    {
        if (buffVfx != null)
        {
            GameObject targetPos = (playerNumber == 1) ? positionP1 : positionP2;
            Vector3 spawnPos = (targetPos != null) ? targetPos.transform.position : 
                (((playerNumber == 1) ? p1Slider : p2Slider) != null ? ((playerNumber == 1) ? p1Slider : p2Slider).transform.position : Vector3.zero);
            GameObject vfx = Instantiate(buffVfx, spawnPos, Quaternion.identity);
            Destroy(vfx, 2f);
        }
        PlaySound(buffSound);

        TextMeshProUGUI resultText = (playerNumber == 1) ? p1ResultText : p2ResultText;
        if (resultText != null && !isGameOver)
        {
            resultText.gameObject.SetActive(true);
            float buffVal = (playerNumber == 1 && MinigameManager.Instance != null) ? MinigameManager.Instance.P1buff : 
                            (playerNumber == 2 && MinigameManager.Instance != null) ? MinigameManager.Instance.P2buff : 0f;
            string mult = (buffVal >= 10f) ? "x3" : "x2";
            resultText.text = $"BUFF {mult}";
            resultText.transform.DOKill();
            resultText.transform.localScale = Vector3.zero;
            resultText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
            resultText.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 4, 0.5f).SetDelay(0.4f);
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

    void OnDestroy()
    {
        if (p1Slider != null) p1Slider.DOKill();
        if (p2Slider != null) p2Slider.DOKill();
        if (p1Text != null) p1Text.transform.DOKill();
        if (p2Text != null) p2Text.transform.DOKill();
        if (p1ResultText != null) p1ResultText.transform.DOKill();
        if (p2ResultText != null) p2ResultText.transform.DOKill();
    }
}