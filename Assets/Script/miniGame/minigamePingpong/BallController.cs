using TMPro;
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
    [SerializeField] GameObject guideshoot;

    public SliderController _SlideValue;

    [Header("Audio")]
    [SerializeField] AudioClip _throwSfx;

    [Header("UI")]
    public TextMeshProUGUI statusText;
    

    private int _clickCount = 0;
    private Rigidbody rb;
    private bool lockpos;
    private Vector3 startPos;
    private float moveDir = 1f;
    

    void Awake()
    {
        if (guideshoot == null || guideshoot == gameObject)
        {
            Transform arrow = transform.Find("3D RightArrow");
            if (arrow == null) arrow = transform.Find("3DRightArrow");
            if (arrow != null) guideshoot = arrow.gameObject;
            
        }

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
        if (statusText == null)
        {
            GameObject statObj = GameObject.Find("Stat" + playerNumber);
            if (statObj != null) statusText = statObj.GetComponent<TextMeshProUGUI>();
        }

        lockpos = true;
        rb.useGravity = false;
        startPos = transform.position;
        if (guideshoot != null) guideshoot.SetActive(false);

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
            if (statusText != null)
            {
                if (playerNumber == 1)
                {
                    statusText.text = " [Press W] ";
                }
                else if (playerNumber == 2)
                {
                    statusText.text = " [Press ↑] ";
                }
            }
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
            if (playerNumber == 1)
            {
                statusText.text = " [A] [D] to move [W] to shoot ";
            }
            else if (playerNumber == 2)
            {
                statusText.text = " [←] [→] to move [↑] to shoot ";
            }
            if (guideshoot != null) guideshoot.SetActive(true);
            float horizontal = 0f;
            if (Input.GetKey(_keyLeft)) horizontal -= 1f;
            if (Input.GetKey(_keyRight)) horizontal += 1f;

            transform.Rotate(Vector3.up, horizontal * _turnspeed * Time.deltaTime, Space.World);

            _SlideValue.Enable();

            if (Input.GetKeyDown(_keyShoot))
            {
                if (guideshoot != null) guideshoot.SetActive(false);
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