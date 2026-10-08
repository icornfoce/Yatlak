using UnityEngine;
using TMPro;
using DG.Tweening;

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

    private Vector3 timerInitialScale = Vector3.one;
    private int lastTimerSecond = -1;

    void Start()
    {
        ClearAllDarts();
        p1Score = 0;
        p2Score = 0;
        p1DartsLeft = maxDarts;
        p2DartsLeft = maxDarts;
        timeLeft = gameTime;
        isPlaying = true;

        resultTextP1.text = "";
        resultTextP2.text = "";
        if (timerText != null) timerInitialScale = timerText.transform.localScale;
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

            if (scoreTextP1 != null)
            {
                scoreTextP1.transform.DOKill();
                scoreTextP1.transform.localScale = Vector3.one;
                scoreTextP1.transform.DOPunchScale(Vector3.one * 0.35f, 0.2f, 5, 0.5f);
            }
            if (dartsTextP1 != null)
            {
                dartsTextP1.transform.DOKill();
                dartsTextP1.transform.localScale = Vector3.one;
                dartsTextP1.transform.DOPunchScale(Vector3.one * 0.2f, 0.15f, 4, 0.5f);
            }
        }
        else
        {
            if (p2DartsLeft <= 0)
            {
                return;
            }
            p2DartsLeft = p2DartsLeft - 1;
            p2Score = p2Score + points;

            if (scoreTextP2 != null)
            {
                scoreTextP2.transform.DOKill();
                scoreTextP2.transform.localScale = Vector3.one;
                scoreTextP2.transform.DOPunchScale(Vector3.one * 0.35f, 0.2f, 5, 0.5f);
            }
            if (dartsTextP2 != null)
            {
                dartsTextP2.transform.DOKill();
                dartsTextP2.transform.localScale = Vector3.one;
                dartsTextP2.transform.DOPunchScale(Vector3.one * 0.2f, 0.15f, 4, 0.5f);
            }
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

    public override void OverwritedResult()
    {
        isPlaying = false;
        ClearAllDarts();

        if (p1Score > p2Score)
        {
            AnimateResult(resultTextP1, "WIN", true, false);
            AnimateResult(resultTextP2, "LOSE", false, false);
            if (MinigameManager.Instance != null)
            {
                MinigameManager.Instance.P1buff = 5f;
                MinigameManager.Instance.P2buff = 0f;
            }
        }
        else if (p2Score > p1Score)
        {
            AnimateResult(resultTextP1, "LOSE", false, false);
            AnimateResult(resultTextP2, "WIN", true, false);
            if (MinigameManager.Instance != null)
            {
                MinigameManager.Instance.P1buff = 0f;
                MinigameManager.Instance.P2buff = 5f;
            }
        }
        else
        {
            AnimateResult(resultTextP1, "DRAW", false, true);
            AnimateResult(resultTextP2, "DRAW", false, true);
            if (MinigameManager.Instance != null)
            {
                MinigameManager.Instance.P1buff = 0f;
                MinigameManager.Instance.P2buff = 0f;
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

    public void ClearAllDarts()
    {
        DartsPlayer[] players = GetComponentsInChildren<DartsPlayer>(true);
        foreach (DartsPlayer player in players)
        {
            if (player != null)
            {
                player.ClearDarts();
            }
        }
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
        if (timerText != null)
        {
            timerText.text = seconds.ToString("00");
            if (allSeconds != lastTimerSecond && allSeconds <= 5 && allSeconds > 0 && isPlaying)
            {
                lastTimerSecond = allSeconds;
                timerText.transform.DOKill();
                timerText.transform.localScale = timerInitialScale;
                timerText.transform.DOPunchScale(Vector3.one * 0.35f, 0.25f, 5, 0.5f);
                timerText.color = Color.red;
            }
        }
    }

    void OnDestroy()
    {
        if (scoreTextP1 != null) scoreTextP1.transform.DOKill();
        if (scoreTextP2 != null) scoreTextP2.transform.DOKill();
        if (dartsTextP1 != null) dartsTextP1.transform.DOKill();
        if (dartsTextP2 != null) dartsTextP2.transform.DOKill();
        if (timerText != null) timerText.transform.DOKill();
        if (resultTextP1 != null) resultTextP1.transform.DOKill();
        if (resultTextP2 != null) resultTextP2.transform.DOKill();
    }
}

