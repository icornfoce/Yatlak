using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("SFX")]
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
        GameObject randomFruit = Fruits[Random.Range(0, Fruits.Length)];

        if (time >= 0.2f)
        {
            SpawnFruit(randomFruit, minX1, maxX1);
            SpawnFruit(randomFruit, minX2, maxX2);
            time = 0f;
        }
    }

    void SpawnFruit(GameObject fruit, float minX, float maxX)
    {
        Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), 6f, 0f);
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
            audioSource2D.PlayOneShot(clip);
        }
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
            StartCoroutine(BounceBasketCoroutine(catcher.transform, basketInitScale, 0.16f));
        }

        if (scoreText != null)
        {
            StartCoroutine(PunchScaleCoroutine(scoreText.transform, scoreInitScale, new Vector3(1.35f, 1.35f, 1f), 0.15f));
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
        TimerText.text = timeLeft.ToString("F0");

        p1ScoreText.text = player1Score.ToString();
        p2ScoreText.text = player2Score.ToString();
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
                p1ResultText.gameObject.SetActive(true);
                p2ResultText.gameObject.SetActive(true);
                p1ResultText.text = "WIN";
                p2ResultText.text = "LOSE";
                if (MinigameManager.Instance != null)
                {
                    MinigameManager.Instance.P1buff = 5f;
                    MinigameManager.Instance.P2buff = 0f;
                }
            }
            else if (player2Score > player1Score)
            {
                p1ResultText.gameObject.SetActive(true);
                p2ResultText.gameObject.SetActive(true);
                p1ResultText.text = "LOSE";
                p2ResultText.text = "WIN";
                if (MinigameManager.Instance != null)
                {
                    MinigameManager.Instance.P1buff = 0f;
                    MinigameManager.Instance.P2buff = 5f;
                }
            }
            else
            {
                p1ResultText.gameObject.SetActive(true);
                p2ResultText.gameObject.SetActive(true);
                p1ResultText.text = "DRAW";
                p2ResultText.text = "DRAW";
                if (MinigameManager.Instance != null)
                {
                    MinigameManager.Instance.P1buff = 0f;
                    MinigameManager.Instance.P2buff = 0f;
                }
            }

            if (p1ResultText != null) StartCoroutine(PunchScaleCoroutine(p1ResultText.transform, Vector3.one, new Vector3(1.3f, 1.3f, 1f), 0.3f));
            if (p2ResultText != null) StartCoroutine(PunchScaleCoroutine(p2ResultText.transform, Vector3.one, new Vector3(1.3f, 1.3f, 1f), 0.3f));
        }
    }

    IEnumerator BounceBasketCoroutine(Transform target, Vector3 baseScale, float duration)
    {
        if (target == null) yield break;
        Vector3 squashed = new Vector3(baseScale.x * 1.3f, baseScale.y * 0.75f, baseScale.z);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curve = Mathf.Sin(t * Mathf.PI);
            target.localScale = Vector3.Lerp(baseScale, squashed, curve);
            yield return null;
        }
        if (target != null) target.localScale = baseScale;
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
