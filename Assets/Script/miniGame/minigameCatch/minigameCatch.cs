using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class minigameCatch : MonoBehaviour
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
    public float minX;
    public float maxX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        spawnFruit();
        player1();
        player2();
    }

    void spawnFruit()
    {
        
    }

    void player1()
    {
        minX = camera1.transform.position.x - 4f;
        maxX = camera1.transform.position.x + 4f;

        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        Vector3 currentPos = Catcher1.transform.position;
        currentPos.x += moveInput * moveSpeed * Time.deltaTime;

        currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);

        Catcher1.transform.position = currentPos;
    }

    void player2()
    {
        minX = camera2.transform.position.x - 4f;
        maxX = camera2.transform.position.x + 4f;

        float moveInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveInput = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveInput = 1f;

        Vector3 currentPos = Catcher2.transform.position;
        currentPos.x += moveInput * moveSpeed * Time.deltaTime;

        currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);

        Catcher2.transform.position = currentPos;
    }

    void UI()
    {
        
    }

    void result()
    {

    }
}
