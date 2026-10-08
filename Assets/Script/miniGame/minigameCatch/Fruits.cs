using UnityEngine;

public class Fruits : MonoBehaviour
{
    public minigameCatch controller;

    void Start()
    {
        Destroy(this.gameObject, 4f);
    }

    void Update()
    {
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "Player1")
        {
            addScore(1);
            Destroy(this.gameObject);
        }
        else if (LayerMask.LayerToName(other.gameObject.layer) == "Player2")
        {
            addScore(2);
            Destroy(this.gameObject);
        }
    }

    void addScore(int playerNumber)
    {
        if (controller != null)
        {
            controller.addScore(playerNumber, transform.position);
        }
    }
}
