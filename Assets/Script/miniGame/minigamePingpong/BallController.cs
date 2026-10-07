using System;
using Unity.VisualScripting;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("setting")]
    [SerializeField] float _ballSpeed;
    [SerializeField] float _powershoot;
    [SerializeField] float _powershootup;
    [SerializeField] float _height;
    [SerializeField] float _turnspeed;

    //[SerializeField] float _bounce;

    [Header("obj set up")]
    [SerializeField] Transform _shootPoint;
    [SerializeField] GameObject _ball;
    [SerializeField] GameObject _wallLeft;
    [SerializeField] GameObject _wallRight;

    private int _clickCount = 0;
    private Transform _target;
    private Quaternion _originalRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _originalRotation = transform.rotation;
        _target = _wallLeft.transform;
       
    }

    // Update is called once per frame
    void Update()
    {
        if (_clickCount == 0)
        {
            _ball.transform.position = Vector3.MoveTowards(_ball.transform.position, _target.transform.position, _ballSpeed * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.W))
            {
                transform.rotation = _originalRotation;
                _clickCount++;
            }
        } 
        else if (_clickCount == 1)
        {
            float horizontal = 0f;
            if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D)) horizontal += 1f;
            transform.Rotate(Vector3.up, horizontal * _turnspeed * Time.deltaTime, Space.World);

            if (Input.GetKeyDown(KeyCode.W) )
            {
                Shoot();
                _clickCount++;
            }
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

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("wall-R"))
        {
            _target = _wallLeft.transform;
        }
        else if (other.gameObject.CompareTag("wall-L"))
        {
            _target = _wallRight.transform;
        }

        else if (other.gameObject.CompareTag("cup"))
        {
            Destroy(gameObject);
        }
    }

    void Shoot()
    {
        Rigidbody ballRb = _ball.GetComponent<Rigidbody>();
        Vector3 forceToAdd = (transform.forward * _powershoot) + (transform.up * _powershootup);
        ballRb.AddForce(forceToAdd, ForceMode.Impulse);
        Destroy(gameObject, 3);
        GameFlow.Instance.BallDective();
    }

 
}