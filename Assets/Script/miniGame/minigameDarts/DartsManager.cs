using UnityEngine;
using TMPro;

public class DartsManager : MinigameBase
{
    public int maxDarts = 3;     
    public float gameTime = 60f; 

    public TextMeshProUGUI scoreTextP1;
    public TextMeshProUGUI dartsTextP1;
    public TextMeshProUGUI resultTextP1;

    public TextMeshProUGUI scoreTextP2;
    public TextMeshProUGUI dartsTextP2;
    public TextMeshProUGUI resultTextP2;

    public TextMeshProUGUI timerText;

    public int p1Score = 0;
    public int p2Score = 0;
    public int p1DartsLeft = 3;
    public int p2DartsLeft = 3;
    public float timeLeft = 60f;
    public bool isPlaying = false;

    void Start()
    {
        p1Score = 0;
        p2Score = 0;
        p1DartsLeft = maxDarts;
        p2DartsLeft = maxDarts;
        timeLeft = gameTime;
        isPlaying = true;

        resultTextP1.text = "";
        resultTextP2.text = "";
        ShowScore();
        ShowTime();
    }

    void Update()
    {
        if (isPlaying == false)
        {
            return;
        }

        timeLeft = timeLeft - Time.deltaTime;
        if (timeLeft < 0)
        {
            timeLeft = 0;
        }
        ShowTime();

        if (timeLeft <= 0)
        {
            Result();
        }
    }


    public void AddScore(int playerNumber , int points)
    {
        if (isPlaying == false)
        {
            return;
        }

        if (playerNumber == 1)
        {
            if (p1DartsLeft <= 0)
            {
                return;
            }
            p1DartsLeft = p1DartsLeft - 1;
            p1Score = p1Score + points;
        }
        else
        {
            if (p2DartsLeft <= 0)
            {
                return;
            }
            p2DartsLeft = p2DartsLeft - 1;
            p2Score = p2Score + points;
        }

        ShowScore();
        if (p1DartsLeft <= 0 && p2DartsLeft <= 0)
        {
            Result();
        }
    }

    public int GetDartsLeft(int playerNumber)
    {
        if (playerNumber == 1)
        {
            return p1DartsLeft;
        }
        return p2DartsLeft;
    }

    public override void  OverwritedResult()
    {
        isPlaying = false;

        if (p1Score > p2Score)
        {
            resultTextP1.text = "WIN";
            resultTextP2.text = "LOSE";
        }
        else if (p2Score > p1Score)
        {
            resultTextP1.text = "LOSE";
            resultTextP2.text = "WIN";
        }
        else
        {
            resultTextP1.text = "DRAW";
            resultTextP2.text = "DRAW";
        }
        MinigameManager.Instance.UpdateGameState(); 
    }

    void ShowScore()
    {
        scoreTextP1.text = "Score: " + p1Score;
        dartsTextP1.text = "Darts: " + p1DartsLeft;

        scoreTextP2.text = "Score: " + p2Score;
        dartsTextP2.text = "Darts: " + p2DartsLeft;
    }

    void ShowTime()
    {
        int allSeconds = Mathf.CeilToInt(timeLeft);
        int seconds = allSeconds % 60;
        timerText.text = seconds.ToString("00"); 
    }
}
