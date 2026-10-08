using UnityEngine;



public class glass : MonoBehaviour
{
    public int score = 1;

    [Header("VFX & SFX")]
    [SerializeField] GameObject _scoreVfx;
    [SerializeField] AudioClip _scoreSfx;

    public void AddScore(int playerNumber)
    {
        GameFlow.Instance.AddScore(playerNumber, score);

        if (_scoreVfx != null)
        {
            GameObject vfx = Instantiate(_scoreVfx, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        if (_scoreSfx != null)
        {
            AudioSource.PlayClipAtPoint(_scoreSfx, transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (!ball.hasScored)
        {
            ball.hasScored = true;
            AddScore(ball.playerNumber);
            Destroy(other.gameObject);
        }
    }
}
