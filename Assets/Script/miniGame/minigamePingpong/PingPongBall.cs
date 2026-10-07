using System.Collections;
using UnityEngine;

namespace Yatlak.MiniGame.PingPong
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class PingPongBall : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private SphereCollider sphereCollider;

        [Header("Miss / Bounds Rules")]
        [Tooltip("Minimum Y height before ball is considered fallen off the table/missed")]
        [SerializeField] private float fallThresholdY = -1.0f;
        [Tooltip("Maximum flight duration in seconds before auto-resolving as miss")]
        [SerializeField] private float maxFlightDuration = 4.0f;
        [Tooltip("Wait time in seconds after result before spawning next ball (spec: 1s)")]
        [SerializeField] private float waitTimeAfterResult = 1.0f;

        private PlayerPingPongController ownerPlayer;
        private bool isLaunched = false;
        private bool isResolved = false;
        private float launchTime = 0f;
        private bool hasTouchedTable = false;
        private float restTimer = 0f;

        public Rigidbody Rigidbody => rb;
        public bool IsLaunched => isLaunched;
        public bool IsResolved => isResolved;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (sphereCollider == null) sphereCollider = GetComponent<SphereCollider>();

            // Ensure ball has smooth collision detection
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        public void Initialize(PlayerPingPongController player, float tableHeight)
        {
            ownerPlayer = player;
            isLaunched = false;
            isResolved = false;
            hasTouchedTable = false;
            restTimer = 0f;
            fallThresholdY = tableHeight - 0.8f;

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector3.zero;
#else
                rb.velocity = Vector3.zero;
#endif
                rb.angularVelocity = Vector3.zero;
            }
        }

        public void Launch(Vector3 velocity)
        {
            isLaunched = true;
            launchTime = Time.time;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = velocity;
#else
                rb.velocity = velocity;
#endif
            }
        }

        private void Update()
        {
            if (!isLaunched || isResolved) return;

            // 1. Check if fallen below threshold
            if (transform.position.y < fallThresholdY)
            {
                ResolveMiss("Fell below bounds");
                return;
            }

            // 2. Check max flight timeout
            if (Time.time - launchTime > maxFlightDuration)
            {
                ResolveMiss("Max flight timeout reached");
                return;
            }

            // 3. Check if ball has settled to a near stop on the table
            if (hasTouchedTable && rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                float speed = rb.linearVelocity.magnitude;
#else
                float speed = rb.velocity.magnitude;
#endif
                if (speed < 0.15f)
                {
                    restTimer += Time.deltaTime;
                    if (restTimer >= 0.75f)
                    {
                        ResolveMiss("Ball stopped on table without entering cup");
                    }
                }
                else
                {
                    restTimer = 0f;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // During State 1 (Position sliding): check if ball hits a wall
            if (!isLaunched)
            {
                if (collision.gameObject.CompareTag("wall"))
                {
                    if (ownerPlayer != null)
                    {
                        ownerPlayer.OnWallBounce();
                    }
                }
                return;
            }

            // During flight:
            if (collision.gameObject.CompareTag("wall"))
            {
                // Ball hit side boundary wall during flight
            }
            else if (collision.gameObject.name.ToLower().Contains("table") || collision.gameObject.CompareTag("Untagged"))
            {
                hasTouchedTable = true;
            }
            else if (collision.gameObject.CompareTag("Floor") || collision.gameObject.name.ToLower().Contains("floor"))
            {
                ResolveMiss("Hit floor");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // In State 1 (sliding): check trigger wall
            if (!isLaunched)
            {
                if (other.CompareTag("wall"))
                {
                    if (ownerPlayer != null)
                    {
                        ownerPlayer.OnWallBounce();
                    }
                }
                return;
            }

            if (other.CompareTag("wall"))
            {
                // Handled
            }
            else if (other.CompareTag("Floor") || other.name.ToLower().Contains("floor"))
            {
                ResolveMiss("Hit floor trigger");
            }
            else
            {
                PingPongCup cup = other.GetComponent<PingPongCup>();
                if (cup == null) cup = other.GetComponentInParent<PingPongCup>();
                if (cup != null)
                {
                    OnCupEntered(cup);
                }
            }
        }

        public void OnCupEntered(PingPongCup cup)
        {
            if (isResolved || !isLaunched) return;

            isResolved = true;

            // Dampen ball motion so it stays comfortably inside the cup
            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = rb.linearVelocity * 0.1f + Vector3.down * 0.5f;
#else
                rb.velocity = rb.velocity * 0.1f + Vector3.down * 0.5f;
#endif
                rb.angularVelocity = Vector3.zero;
            }

            cup.OnBallScored(this);

            if (ownerPlayer != null)
            {
                ownerPlayer.OnBallScored(cup.ScoreValue);
            }

            StartCoroutine(WaitAndNotifyRoutine(true));
        }

        public void ResolveMiss(string reason = "")
        {
            if (isResolved) return;
            isResolved = true;

            if (ownerPlayer != null)
            {
                ownerPlayer.OnBallMissed();
            }

            StartCoroutine(WaitAndNotifyRoutine(false));
        }

        private IEnumerator WaitAndNotifyRoutine(bool scored)
        {
            yield return new WaitForSeconds(waitTimeAfterResult);

            if (ownerPlayer != null)
            {
                ownerPlayer.OnBallCycleCompleted(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
