using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        minX1 = camera1.transform.position.x - 3.5f;
        maxX1 = camera1.transform.position.x + 3.5f;

        minX2 = camera2.transform.position.x - 3.5f;
        maxX2 = camera2.transform.position.x + 3.5f;
    }

    // Update is called once per frame
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
        if (playerNumber == 1)
        {
            player1Score += 5;
        }
        else if (playerNumber == 2)
        {
            player2Score += 5;
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
        if (timeLeft <= 0f)
        {
            isGameOver = true;
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
            }
            else if (player2Score > player1Score)
            {
                p1ResultText.gameObject.SetActive(true);
                p2ResultText.gameObject.SetActive(true);
                p1ResultText.text = "LOSE";
                p2ResultText.text = "WIN";
            }
            else
            {
                p1ResultText.gameObject.SetActive(true);
                p2ResultText.gameObject.SetActive(true);
                p1ResultText.text = "DRAW";
                p2ResultText.text = "DRAW";
            }
        }
    }
}
