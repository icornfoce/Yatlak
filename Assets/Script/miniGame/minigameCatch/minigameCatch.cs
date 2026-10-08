using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class minigameCatch : MinigameBase
{
    [SerializeField]
    public GameObject mainMinigame;

    public GameObject Catcher1;
    public GameObject Catcher2;

    public GameObject camera1;
    public GameObject camera2;

    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

    public TextMeshProUGUI p1ComboText;
    public TextMeshProUGUI p2ComboText;

    public TextMeshProUGUI p1ResultText;
    public TextMeshProUGUI p2ResultText;

    public TextMeshProUGUI TimerText;

    public GameObject[] Fruits;

    private int player1Score = 0;
    private int player2Score = 0;

    public float moveSpeed = 8f; 

    private bool isGameOver = false;
    public float timeLeft = 60f;

    private float minX1;
    private float maxX1;
    private float minX2;
    private float maxX2;

    private float time = 0f;
    public Slider volumeSlider;
    [Header("SFX")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    public AudioClip catchSound;
    public AudioClip gameOverSound;
    private AudioSource audioSource2D;

    [Header("VFX")]
    public GameObject catchVfxPrefab;
    public GameObject gameOverVfxPrefab;

    private Vector3 catcher1InitialScale = Vector3.one;
    private Vector3 catcher2InitialScale = Vector3.one;
    private Vector3 p1ScoreInitialScale = Vector3.one;
    private Vector3 p2ScoreInitialScale = Vector3.one;
    private Vector3 timerInitialScale = Vector3.one;
    private int lastTimerSecond = -1;

    void Awake()
    {
        audioSource2D = gameObject.AddComponent<AudioSource>();
        audioSource2D.spatialBlend = 0f;
    }

    public void Start()
    {
        minX1 = camera1.transform.position.x - 3.5f;
        maxX1 = camera1.transform.position.x + 3.5f;

        minX2 = camera2.transform.position.x - 3.5f;
        maxX2 = camera2.transform.position.x + 3.5f;

        if (Catcher1 != null) catcher1InitialScale = Catcher1.transform.localScale;
        if (Catcher2 != null) catcher2InitialScale = Catcher2.transform.localScale;
        if (p1ScoreText != null) p1ScoreInitialScale = p1ScoreText.transform.localScale;
        if (p2ScoreText != null) p2ScoreInitialScale = p2ScoreText.transform.localScale;
        if (TimerText != null) timerInitialScale = TimerText.transform.localScale;

        if (audioSource2D != null)
        {
            audioSource2D.volume = sfxVolume;
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = sfxVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    void Update()
    {
        spawnFruitBothSides();
        player1();
        player2();
        UI();
        checkGameOver();
    }

    void spawnFruitBothSides()
    {
        time += Time.deltaTime;

        if (time >= 0.2f)
        {
            GameObject randomFruit = Fruits[Random.Range(0, Fruits.Length)];
            float randomOffset = Random.Range(-3.5f, 3.5f);

            SpawnFruit(randomFruit, camera1.transform.position.x + randomOffset);
            SpawnFruit(randomFruit, camera2.transform.position.x + randomOffset);
            time = 0f;
        }
    }

    void SpawnFruit(GameObject fruit, float posX)
    {
        Vector3 randomPosition = new Vector3(posX, 6f, 0f);
        GameObject spawnedObject = Instantiate(fruit, randomPosition, Quaternion.identity);
        Fruits fruitComponent = spawnedObject.GetComponent<Fruits>();
        if (fruitComponent != null)
        {
            fruitComponent.controller = this;
        }
    }

    public void addScore(int playerNumber)
    {
        addScore(playerNumber, default);
    }

    public void addScore(int playerNumber, Vector3 fruitPos)
    {
        if (playerNumber == 1)
        {
            player1Score += 5;
            TriggerCatchVFX(1, fruitPos);
        }
        else if (playerNumber == 2)
        {
            player2Score += 5;
            TriggerCatchVFX(2, fruitPos);
        }

        PlaySound(catchSound);
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource2D != null && clip != null)
        {
            audioSource2D.PlayOneShot(clip sfxVolume);
        }
    }

    public void SetVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (audioSource2D != null)
        {
            audioSource2D.volume = sfxVolume;
    }

    void TriggerCatchVFX(int playerNumber, Vector3 fruitPos)
    {
        GameObject catcher = (playerNumber == 1) ? Catcher1 : Catcher2;
        TextMeshProUGUI scoreText = (playerNumber == 1) ? p1ScoreText : p2ScoreText;
        Vector3 scoreInitScale = (playerNumber == 1) ? p1ScoreInitialScale : p2ScoreInitialScale;
        Vector3 basketInitScale = (playerNumber == 1) ? catcher1InitialScale : catcher2InitialScale;

        if (catchVfxPrefab != null)
        {
            Vector3 spawnPoint = (fruitPos != default) ? fruitPos : (catcher != null ? catcher.transform.position : Vector3.zero);
            GameObject vfx = Instantiate(catchVfxPrefab, spawnPoint, Quaternion.identity);
            Destroy(vfx, 1.5f);
        }

        if (catcher != null)
        {
            catcher.transform.DOKill();
            catcher.transform.localScale = basketInitScale;
            catcher.transform.DOPunchScale(new Vector3(basketInitScale.x * 0.35f, -basketInitScale.y * 0.25f, 0f), 0.18f, 5, 0.5f);
        }

        if (scoreText != null)
        {
            scoreText.transform.DOKill();
            scoreText.transform.localScale = scoreInitScale;
            scoreText.transform.DOPunchScale(Vector3.one * 0.35f, 0.18f, 5, 0.5f);
        }
    }

    void player1()
    {
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        Vector3 currentPos = Catcher1.transform.position;
        currentPos.x += moveInput * moveSpeed * Time.deltaTime;

        currentPos.x = Mathf.Clamp(currentPos.x, minX1, maxX1);

        Catcher1.transform.position = currentPos;
    }

    void player2()
    {
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveInput = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveInput = 1f;

        Vector3 currentPos = Catcher2.transform.position;
        currentPos.x += moveInput * moveSpeed * Time.deltaTime;

        currentPos.x = Mathf.Clamp(currentPos.x, minX2, maxX2);

        Catcher2.transform.position = currentPos;
    }

    void UI()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0f) timeLeft = 0f;
        TimerText.text = timeLeft.ToString("F0");

        p1ScoreText.text = player1Score.ToString();
        p2ScoreText.text = player2Score.ToString();

        int currentSec = Mathf.CeilToInt(timeLeft);
        if (currentSec != lastTimerSecond && currentSec <= 5 && currentSec > 0 && !isGameOver)
        {
            lastTimerSecond = currentSec;
            if (TimerText != null)
            {
                TimerText.transform.DOKill();
                TimerText.transform.localScale = timerInitialScale;
                TimerText.transform.DOPunchScale(Vector3.one * 0.35f, 0.25f, 5, 0.5f);
                TimerText.color = Color.red;
            }
        }
    }

    void checkGameOver()
    {
        if (timeLeft <= 0f && !isGameOver)
        {
            isGameOver = true;
            PlaySound(gameOverSound);

            if (gameOverVfxPrefab != null)
            {
                GameObject vfx = Instantiate(gameOverVfxPrefab, Vector3.zero, Quaternion.identity);
                Destroy(vfx, 2f);
            }

            Result();
        }
    }

    public override void OverwritedResult()
    {
        if (isGameOver)
        {
            if (player1Score > player2Score)
            {
                AnimateResult(p1ResultText, "WIN", true, false);
                AnimateResult(p2ResultText, "LOSE", false, false);
                if (MinigameManager.Instance != null)
                {
                    MinigameManager.Instance.P1buff = 5f;
                    MinigameManager.Instance.P2buff = 0f;
                }
            }
            else if (player2Score > player1Score)
            {
                AnimateResult(p1ResultText, "LOSE", false, false);
                AnimateResult(p2ResultText, "WIN", true, false);
                if (MinigameManager.Instance != null)
                {
                    MinigameManager.Instance.P1buff = 0f;
                    MinigameManager.Instance.P2buff = 5f;
                }
            }
            else
            {
                AnimateResult(p1ResultText, "DRAW", false, true);
                AnimateResult(p2ResultText, "DRAW", false, true);
                if (MinigameManager.Instance != null)
                {
                    MinigameManager.Instance.P1buff = 0f;
                    MinigameManager.Instance.P2buff = 0f;
                }
            }
        }
    }

    void AnimateResult(TextMeshProUGUI textComp, string message, bool isWinner, bool isDraw)
    {
        if (textComp == null) return;
        textComp.gameObject.SetActive(true);
        textComp.text = message;
        textComp.transform.DOKill();
        textComp.transform.localScale = Vector3.zero;

        if (isDraw)
        {
            textComp.transform.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBounce);
        }
        else if (isWinner)
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

    void OnDestroy()
    {
        if (Catcher1 != null) Catcher1.transform.DOKill();
        if (Catcher2 != null) Catcher2.transform.DOKill();
        if (p1ScoreText != null) p1ScoreText.transform.DOKill();
        if (p2ScoreText != null) p2ScoreText.transform.DOKill();
        if (TimerText != null) TimerText.transform.DOKill();
        if (p1ResultText != null) p1ResultText.transform.DOKill();
        if (p2ResultText != null) p2ResultText.transform.DOKill();
    }
}
