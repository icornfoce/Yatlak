using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

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

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            for(int i = 0; i < minigame.Length; i++ )
            {
            minigamePool.Add(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float p1 = p1Slider.value;
        float p2 = p2Slider.value;

        if (!hasTriggered25 && (p1 >= 25 || p2 >= 25))
        {
            RandomMinigame();
            mainMinigame.SetActive(false);
            hasTriggered25 = true;
        }
        else if (!hasTriggered50 && (p1 >= 50 || p2 >= 50))
        {
            RandomMinigame();
            mainMinigame.SetActive(false);
            hasTriggered50 = true;
        }
        else if (!hasTriggered75 && (p1 >= 75 || p2 >= 75))
        {
            RandomMinigame();
            mainMinigame.SetActive(false);
            hasTriggered75 = true;
        }
    }

    void RandomMinigame()
    {
        if(minigamePool.Count == 0)
        {

            //game end
            return;
        }
      
         int   randomIndex = Random.Range(0, minigamePool.Count);



        minigamePool.Remove(randomIndex);

MinigameManager.Instance.setCurrentMiniGame(minigame[randomIndex]);
        minigame[randomIndex].SetActive(true);
    }
}
