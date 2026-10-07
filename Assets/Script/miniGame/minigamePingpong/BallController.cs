using System;
using Unity.VisualScripting;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] float _ballSpeed;
    [SerializeField] float _shootspeed;
    [SerializeField] Transform _shootPoint;
    [SerializeField] GameObject _ball;
    [SerializeField] GameObject _wallLeft;
    [SerializeField] GameObject _wallRight;
    private int _clickCount = 0;
    private Transform _target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _target = _wallLeft.transform;
       
    }

    // Update is called once per frame
    void Update()
    {

        _ball.transform.position = Vector3.MoveTowards(_ball.transform.position, _target.transform.position, _ballSpeed * Time.deltaTime);
        Debug.Log(_ballSpeed);

        if (Input.GetKey(KeyCode.W))
            {
            _ballSpeed = 0f;
            shoot();

            }

    }

    private void OnTriggerEnter(Collider other)
    {
            if (other.CompareTag("wall-R"))
            {
                _target = _wallLeft.transform;
            }
            else if (other.CompareTag("wall-L"))
            {
                _target = _wallRight.transform;
        }

    }

    private void shoot()
    {

    }


}
