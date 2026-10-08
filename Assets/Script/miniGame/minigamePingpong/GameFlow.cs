using UnityEngine;
using TMPro;

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance { get; private set; }

    [Header("Player 1")]
    [SerializeField] int _ballAmount = 5;
    [SerializeField] GameObject _ballPrefab;
    [SerializeField] Transform _spawnpoint;
    [SerializeField] SliderController _slider1;
    public bool _isBallActive = false;

    [Header("Player 2")]
    [SerializeField] int _ballAmount2 = 5;
    [SerializeField] GameObject _ballPrefab2;
    [SerializeField] Transform _spawnpoint2;
    [SerializeField] SliderController _slider2;
    public bool _isBallActive2 = false;

    [Header("Scores")]
    public int score1;
    public int score2;

    [Header("UI Texts")]
    [SerializeField] TextMeshProUGUI _scoreText1;
    [SerializeField] TextMeshProUGUI _scoreText2;
    [SerializeField] TextMeshProUGUI _ballLeftText1;
    [SerializeField] TextMeshProUGUI _ballLeftText2;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _ballPrefab2 = _ballPrefab;
        _spawnpoint2 = GameObject.Find("Spawn point P1 (1)").transform;
        _slider1 = GameObject.Find("Player slider 1").GetComponent<SliderController>();
        _slider2 = GameObject.Find("Slider_P2").GetComponent<SliderController>();
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
    }

    private void InstantiateBall1()
    {
        GameObject ball = Instantiate(_ballPrefab, _spawnpoint.position, Quaternion.identity);
        BallController bc = ball.GetComponent<BallController>();
        bc.playerNumber = 1;
        bc._SlideValue = _slider1;
        _ballAmount--;
        _isBallActive = true;
    }

    private void InstantiateBall2()
    {
        GameObject ball = Instantiate(_ballPrefab2, _spawnpoint2.position, Quaternion.identity);
        BallController bc = ball.GetComponent<BallController>();
        bc.playerNumber = 2;
        bc._SlideValue = _slider2;
        _ballAmount2--;
        _isBallActive2 = true;
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
}