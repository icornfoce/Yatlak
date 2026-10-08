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
 
    public SliderController _SlideValue;

    [Header("Audio")]
    [SerializeField] AudioClip _throwSfx;

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
        rb.useGravity = false;
        startPos = transform.position;

        if (playerNumber == 2)
        {
            _keyShoot = KeyCode.UpArrow;
            _keyLeft = KeyCode.LeftArrow;
            _keyRight = KeyCode.RightArrow;
        }
        else
        {
            _keyShoot = KeyCode.W;
            _keyLeft = KeyCode.A;
            _keyRight = KeyCode.D;
        }

        if (playerNumber == 1)
        {
            _SlideValue = GameFlow.Instance._slider1;
        }
        else if (playerNumber == 2)
        {
            _SlideValue = GameFlow.Instance._slider2;
        }

        _SlideValue.UnEnable();
    }

    void Update()
    {
        freezepos();

        if (_clickCount == 0)
        {
            transform.position += Vector3.right * moveDir * _ballSpeed * Time.deltaTime;
            if (transform.position.x > startPos.x + 1f)
            {
                moveDir = -1f;
            }
            else if (transform.position.x < startPos.x - 1f)
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
            float horizontal = 0f;
            if (Input.GetKey(_keyLeft)) horizontal -= 1f;
            if (Input.GetKey(_keyRight)) horizontal += 1f;

            transform.Rotate(Vector3.up, horizontal * _turnspeed * Time.deltaTime, Space.World);

            _SlideValue.Enable();

            if (Input.GetKeyDown(_keyShoot))
            {
                lockpos = false;
                rb.constraints = RigidbodyConstraints.None;
                rb.useGravity = true;
                Shoot();
                _SlideValue.setzero();
                _SlideValue.UnEnable();
                _clickCount++;
            }
        }
    }

    private void freezepos()
    {
        if (lockpos == true)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }
        else if (lockpos == false)
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }


    [HideInInspector] public bool hasScored = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("cup") && !hasScored)
        {
            hasScored = true;
            glass g = other.GetComponentInParent<glass>();
            g.AddScore(playerNumber);
            Destroy(gameObject);
        }
    }

    void Shoot()
    {
        Rigidbody ballRb = _ball.GetComponent<Rigidbody>();
        float upPower = _SlideValue._powershootcurrent;
        Vector3 forceToAdd = (transform.forward * _powershoot) + (Vector3.up * upPower);
        ballRb.AddForce(forceToAdd, ForceMode.Impulse);
        if (_throwSfx != null)
        {
            AudioSource.PlayClipAtPoint(_throwSfx, transform.position);
        }
        Destroy(gameObject, 5);
        Debug.Log("hit");
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