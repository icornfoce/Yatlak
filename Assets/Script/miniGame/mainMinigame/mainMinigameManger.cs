using UnityEngine;

namespace MiniGame.MainMinigame
{
    public enum DirectionKey
    {
        Up, 
        Down,
        Left,
        Right
    }
    public class Manager : MonoBehaviour
    {
        public DirectionKey DirectionKey;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            RandomDirection();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RandomDirection();
            }
        }
        void RandomDirection()
        {
            DirectionKey = (DirectionKey)Random.Range(0, 4);
            Debug.Log(DirectionKey.ToString());
        }
    }
}