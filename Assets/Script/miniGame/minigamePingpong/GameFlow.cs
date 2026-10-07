using UnityEngine;

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance { get; private set; }
    [SerializeField] int _ballAmount;
    [SerializeField] GameObject _ballPrefab;
    [SerializeField] Transform _spawnpoint;
    public bool _isBallActive = false;
    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_ballAmount <= 0)
        {
            return;
        }

        if (_isBallActive == false)
        {
            InstantiateBall();
        }
    }

    private void InstantiateBall()
    {
        Instantiate(_ballPrefab, _spawnpoint.position, Quaternion.identity);
        _ballAmount--;
        BallActive();
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