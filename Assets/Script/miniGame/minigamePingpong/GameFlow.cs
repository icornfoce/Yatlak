using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;   

public class GameFlow : MinigameBase
{
    public static GameFlow Instance;

    [Header("Player 1")]
    [SerializeField] int _ballAmount = 5;
    [SerializeField] GameObject _ballPrefab;
    [SerializeField] Transform _spawnpoint;
    public SliderController _slider1;
    public bool _isBallActive = false;

    [Header("Player 2")]
    [SerializeField] int _ballAmount2 = 5;
    [SerializeField] GameObject _ballPrefab2;
    [SerializeField] Transform _spawnpoint2;
    public SliderController _slider2;
    public bool _isBallActive2 = false;

    [Header("Scores")]
    public int score1;
    public int score2;
    private bool isGameOver = false;
    public TextMeshProUGUI timerText;
    



    public void AddScore(int player, int amount)
    {
        if (player == 1)
        {
            score1 += amount;
            if (_scoreText1 != null)
            {
                _scoreText1.transform.DOKill();
                _scoreText1.transform.localScale = Vector3.one;
                _scoreText1.transform.DOPunchScale(Vector3.one * 0.35f, 0.2f, 5, 0.5f);
            }
        }
        else if (player == 2)
        {
            score2 += amount;
            if (_scoreText2 != null)
            {
                _scoreText2.transform.DOKill();
                _scoreText2.transform.localScale = Vector3.one;
                _scoreText2.transform.DOPunchScale(Vector3.one * 0.35f, 0.2f, 5, 0.5f);
            }
        }
    }

    [Header("UI Texts")]
    [SerializeField] TextMeshProUGUI _scoreText1;
    [SerializeField] TextMeshProUGUI _scoreText2;
    [SerializeField] TextMeshProUGUI _ballLeftText1;
    [SerializeField] TextMeshProUGUI _ballLeftText2;
    public TextMeshProUGUI p1ResultText;
    public TextMeshProUGUI p2ResultText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
        _isBallActive = false;
        _isBallActive2 = false;

        if (_ballPrefab2 == null) _ballPrefab2 = _ballPrefab;
        if (_spawnpoint2 == null) _spawnpoint2 = GameObject.Find("Spawn point P1 (1)").transform;

        _scoreText1 = GameObject.Find("Score 1").GetComponent<TextMeshProUGUI>();
        _scoreText2 = GameObject.Find("Score 2").GetComponent<TextMeshProUGUI>();
        _ballLeftText1 = GameObject.Find("Ball left 1").GetComponent<TextMeshProUGUI>();
        _ballLeftText2 = GameObject.Find("Ball left 2").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (_ballAmount > 0 && _isBallActive == false)
        {
            InstantiateBall1();
        }

        if (_ballAmount2 > 0 && _isBallActive2 == false)
        {
            InstantiateBall2();
        }

        _scoreText1.text = score1.ToString();
        _scoreText2.text = score2.ToString();
        _ballLeftText1.text = _ballAmount.ToString();
        _ballLeftText2.text = _ballAmount2.ToString();

        checkGameOver();
    }

    
    void checkGameOver()
    {
        if (!isGameOver && _ballAmount <= 0 && !_isBallActive && _ballAmount2 <= 0 && !_isBallActive2)
        {
            isGameOver = true;
            Result();
        }
    }

    public override void OverwritedResult()
    {
        if (score1 > score2)
        {
            AnimateResult(p1ResultText, "WIN", true, false);
            AnimateResult(p2ResultText, "LOSE", false, false);
            if (MinigameManager.Instance != null)
            {
                MinigameManager.Instance.P1buff = 5f;
                MinigameManager.Instance.P2buff = 0f;
            }
        }
        else if (score2 > score1)
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

    private void InstantiateBall1()
    {
        GameObject ball = Instantiate(_ballPrefab, _spawnpoint.position, Quaternion.identity);
        BallController bc = ball.GetComponent<BallController>();
        bc.playerNumber = 1;
        bc._SlideValue = _slider1;
        _ballAmount--;
        _isBallActive = true;

        if (_ballLeftText1 != null)
        {
            _ballLeftText1.transform.DOKill();
            _ballLeftText1.transform.localScale = Vector3.one;
            _ballLeftText1.transform.DOPunchScale(Vector3.one * 0.2f, 0.15f, 4, 0.5f);
        }
    }

    private void InstantiateBall2()
    {
        GameObject ball = Instantiate(_ballPrefab2, _spawnpoint2.position, Quaternion.identity);
        BallController bc = ball.GetComponent<BallController>();
        bc.playerNumber = 2;
        bc._SlideValue = _slider2;
        _ballAmount2--;
        _isBallActive2 = true;

        if (_ballLeftText2 != null)
        {
            _ballLeftText2.transform.DOKill();
            _ballLeftText2.transform.localScale = Vector3.one;
            _ballLeftText2.transform.DOPunchScale(Vector3.one * 0.2f, 0.15f, 4, 0.5f);
        }
    }

    public void Ball1Deactive()
    {
        _isBallActive = false;
    }

    public void Ball2Deactive()
    {
        _isBallActive2 = false;
    }

    public void BallActive()
    {
        _isBallActive = true;
    }

    public void BallDective()
    {
        _isBallActive = false;
    }

    void OnDestroy()
    {
        if (_scoreText1 != null) _scoreText1.transform.DOKill();
        if (_scoreText2 != null) _scoreText2.transform.DOKill();
        if (_ballLeftText1 != null) _ballLeftText1.transform.DOKill();
        if (_ballLeftText2 != null) _ballLeftText2.transform.DOKill();
        if (p1ResultText != null) p1ResultText.transform.DOKill();
        if (p2ResultText != null) p2ResultText.transform.DOKill();
    }
}
