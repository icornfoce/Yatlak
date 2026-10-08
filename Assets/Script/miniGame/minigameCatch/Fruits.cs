using UnityEngine;

public class Fruits : MonoBehaviour
{
    public minigameCatch controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(this.gameObject, 2f);
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
        controller.addScore(playerNumber);
    }
}
