using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class randomMinigame : MonoBehaviour
{
    public GameObject[] minigame;
    public bool[] doesMinigame;
    public Slider p1Slider;
    public Slider p2Slider;

    private List<int> minigamePool = new List<int>();

    private bool hasTriggered25 = false;
    private bool hasTriggered50 = false;
    private bool hasTriggered75 = false;

    [SerializeField] 
    private GameObject mainMinigame;

    void Start()
    {
        InitializePool();
    }

    void InitializePool()
    {
        minigamePool.Clear();
        if (minigame == null) return;

        for (int i = 0; i < minigame.Length; i++)
        {
            if (minigame[i] != null)
            {
                minigamePool.Add(i);
            }
        }
    }

    void Update()
    {
        if (p1Slider == null || p2Slider == null) return;

        if (mainMinigame != null && !mainMinigame.activeInHierarchy) return;

        float p1 = p1Slider.value;
        float p2 = p2Slider.value;

        if (!hasTriggered25 && (p1 >= 25 || p2 >= 25))
        {
            hasTriggered25 = true;
            if (StartRandomMinigame() && mainMinigame != null)
            {
                mainMinigame.SetActive(false);
            }
        }
        else if (!hasTriggered50 && (p1 >= 50 || p2 >= 50))
        {
            hasTriggered50 = true;
            if (StartRandomMinigame() && mainMinigame != null)
            {
                mainMinigame.SetActive(false);
            }
        }
        else if (!hasTriggered75 && (p1 >= 75 || p2 >= 75))
        {
            hasTriggered75 = true;
            if (StartRandomMinigame() && mainMinigame != null)
            {
                mainMinigame.SetActive(false);
            }
        }
    }

    public bool StartRandomMinigame()
    {
        if (minigame == null || minigame.Length == 0)
        {
            return false;
        }

        if (minigamePool.Count == 0)
        {
            InitializePool();
            if (minigamePool.Count == 0) return false;
        }

        int poolIndex = Random.Range(0, minigamePool.Count);
        int gameIndex = minigamePool[poolIndex];
        minigamePool.RemoveAt(poolIndex);

        GameObject selectedMinigame = minigame[gameIndex];
        if (selectedMinigame == null) return false;

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.setCurrentMiniGame(selectedMinigame);
        }

        selectedMinigame.SetActive(true);
        return true;
    }

    public void RandomMinigame()
    {
        StartRandomMinigame();
    }

    public void ResetTriggers()
    {
        hasTriggered25 = false;
        hasTriggered50 = false;
        hasTriggered75 = false;
        InitializePool();
    }
}

