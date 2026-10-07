using UnityEngine;
using TMPro;

// ตัวคุมเกมปาลูกดอก (ใส่ไว้ที่ MinigameDarts_Manager มีตัวเดียวในฉาก)
// หน้าที่:
// 1. นับเวลาถอยหลัง
// 2. เก็บคะแนน กับ ลูกดอกที่เหลือ ของผู้เล่น 1 และ ผู้เล่น 2
// 3. เปลี่ยนตัวหนังสือบนจอ (คะแนน / ลูกดอก / เวลา / ชนะ-แพ้)
// 4. จบเกม เมื่อหมดเวลา หรือ ทั้ง 2 คนปาครบแล้ว
public class DartsManager : MonoBehaviour
{
    [Header("ตั้งค่าเกม")]
    public int maxDarts = 3;        // ลูกดอกคนละกี่ลูก
    public float gameTime = 60f;    // เวลาเล่น (วินาที)

    [Header("ตัวหนังสือ ผู้เล่น 1")]
    public TextMeshProUGUI scoreTextP1;
    public TextMeshProUGUI dartsTextP1;
    public TextMeshProUGUI resultTextP1;

    [Header("ตัวหนังสือ ผู้เล่น 2")]
    public TextMeshProUGUI scoreTextP2;
    public TextMeshProUGUI dartsTextP2;
    public TextMeshProUGUI resultTextP2;

    [Header("ตัวหนังสือ เวลา")]
    public TextMeshProUGUI timerText;

    [Header("ค่าตอนเล่น (ไว้ดูเฉยๆ ไม่ต้องแก้)")]
    public int p1Score = 0;
    public int p2Score = 0;
    public int p1DartsLeft = 3;
    public int p2DartsLeft = 3;
    public float timeLeft = 60f;
    public bool isPlaying = false;

    void Start()
    {
        // เริ่มเกมใหม่
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
            return; // เกมจบแล้ว
        }

        // เวลาลดลงเรื่อยๆ
        timeLeft = timeLeft - Time.deltaTime;
        if (timeLeft < 0)
        {
            timeLeft = 0;
        }
        ShowTime();

        // หมดเวลา = จบเกม
        if (timeLeft <= 0)
        {
            EndGame();
        }
    }

    // DartsPlayer เรียกตอนลูกดอกปักเป้าแล้ว
    // playerNumber = 1 หรือ 2, points = แต้มที่ได้
    public void AddScore(int playerNumber, int points)
    {
        if (isPlaying == false)
        {
            return; // เกมจบแล้ว ไม่นับแต้ม
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

        // ทั้ง 2 คนปาครบแล้ว = จบเกมเลย ไม่ต้องรอเวลาหมด
        if (p1DartsLeft <= 0 && p2DartsLeft <= 0)
        {
            EndGame();
        }
    }

    // DartsPlayer ถามว่าตัวเองเหลือลูกดอกกี่ลูก
    public int GetDartsLeft(int playerNumber)
    {
        if (playerNumber == 1)
        {
            return p1DartsLeft;
        }
        return p2DartsLeft;
    }

    void EndGame()
    {
        isPlaying = false;

        // ใครแต้มเยอะกว่าชนะ
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

        Debug.Log("Darts Game Over! P1: " + p1Score + " vs P2: " + p2Score);
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
        int allSeconds = Mathf.CeilToInt(timeLeft); // ปัดขึ้น เช่น 59.3 -> 60
        int minutes = allSeconds / 60;
        int seconds = allSeconds % 60;
        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00"); // เช่น 01:00
    }
}
