using UnityEngine;

public class GameFlow : MonoBehaviour
{
    [SerializeField] int _ballAmount;
    [SerializeField] GameObject _ballPrefab;
    [SerializeField] Transform _spawnpoint;
    bool _isBallActive = false;
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
        _isBallActive = true;
    }


}
