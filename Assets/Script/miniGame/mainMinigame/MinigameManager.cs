using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    public GameObject[] minigamePrefab;

    public GameObject primaryMiniGame;
    GameObject currentMinigame;

    public float P1buff;
    public float P2buff;

    void Awake()
    {
        Instance = this;
    }

    public void setCurrentMiniGame(GameObject minigame)
    {
        currentMinigame = minigame;
    }

    public void UpdateGameState()
    {
        if (currentMinigame != null)
        {
            currentMinigame.SetActive(false);
        }
        if (primaryMiniGame != null)
        {
            primaryMiniGame.SetActive(true);
        }
    }
}
