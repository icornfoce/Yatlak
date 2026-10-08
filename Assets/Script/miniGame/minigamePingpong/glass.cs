using UnityEngine;



public class glass : MonoBehaviour


{

enum player
    {
        player1,
        player2
    }
    [SerializeField] int score;
    [SerializeField] player _player;

    private void OnTriggerEnter(Collider other)
    {
      if(other.gameObject.CompareTag("ball"))
        {
            if(_player == player.player1)
            {
                GameFlow.Instance.score2+= score;
            }
            else
            {
                GameFlow.Instance.score1+= score;
            }
        }
    }
}
