using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Player Controls")]
    public int playerNumber = 1;
    public KeyCode _keyShoot = KeyCode.W;
    public KeyCode _keyLeft = KeyCode.A;
    public KeyCode _keyRight = KeyCode.D;

    [Header("setting")]
    [SerializeField] float _ballSpeed = 3f;
    [SerializeField] float _turnspeed = 3f;
    [SerializeField] float _powershoot = 3f;

    [Header("obj set up")]
    [SerializeField] Transform _shootPoint;
    public GameObject _ball;
    public GameObject _wallLeft;
    public GameObject _wallRight;

    [Header("UI set up")]
    public SliderController _SlideValue;

    private int _clickCount = 0;
    private Rigidbody rb;
    private bool lockpos;
    private Vector3 startPos;
    private float moveDir = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _ball = gameObject;
        if (playerNumber == 2)
        {
            _keyShoot = KeyCode.UpArrow;
            _keyLeft = KeyCode.LeftArrow;
            _keyRight = KeyCode.RightArrow;
        }
    }

    void Start()
    {
        lockpos = true;
        startPos = transform.position;
        if (playerNumber == 2)
        {
            _keyShoot = KeyCode.UpArrow;
            _keyLeft = KeyCode.LeftArrow;
            _keyRight = KeyCode.RightArrow;
        }
        _SlideValue.UnEnable();
    }

    void Update()
    {
        freezepos();
        freezerota();

        if (_clickCount == 0)
        {
            transform.position += Vector3.right * moveDir * _ballSpeed * Time.deltaTime;
            if (transform.position.x > startPos.x + 2.2f)
            {
                moveDir = -1f;
            }
            else if (transform.position.x < startPos.x - 2.2f)
            {
                moveDir = 1f;
            }

            if (Input.GetKeyDown(_keyShoot))
            {
                _clickCount++;
            }
        }
        else if (_clickCount == 1)
        {
            rb.useGravity = false;
            float horizontal = 0f;
            if (Input.GetKey(_keyLeft)) horizontal -= 1f;
            if (Input.GetKey(_keyRight)) horizontal += 1f;

            transform.Rotate(Vector3.up, horizontal * _turnspeed * Time.deltaTime, Space.World);

            _SlideValue.Enable();

            if (Input.GetKeyDown(_keyShoot))
            {
                lockpos = false;
                rb.useGravity = true;
                Shoot();
                _SlideValue.setzero();
                _clickCount++;
            }
        }
    }

    private void freezepos()
    {
        if (lockpos == true)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX;
        }
        else if (lockpos == false)
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    private void freezerota()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("cup"))
        {
            Destroy(gameObject);
        }
    }

    void Shoot()
    {
        Rigidbody ballRb = _ball.GetComponent<Rigidbody>();
        Vector3 forceToAdd = (transform.forward * _powershoot) + (Vector3.up * _SlideValue._powershootcurrent);
        ballRb.AddForce(forceToAdd, ForceMode.Impulse);
        Destroy(gameObject, 3);
        if (playerNumber == 1)
        {
            GameFlow.Instance.Ball1Deactive();
        }
        else
        {
            GameFlow.Instance.Ball2Deactive();
        }
    }
}