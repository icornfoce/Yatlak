using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class randomMinigame : MonoBehaviour
{
    public GameObject[] minigame;
    public bool[] doesMinigame;
    public Slider p1Slider;
    public Slider p2Slider;

    private List<int> playedIndices = new List<int>();

    private bool hasTriggered25 = false;
    private bool hasTriggered50 = false;
    private bool hasTriggered75 = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        float p1 = p1Slider.value;
        float p2 = p2Slider.value;

        if (!hasTriggered25 && (p1 >= 25 || p2 >= 25))
        {
            RandomMinigame();
            hasTriggered25 = true;
        }
        else if (!hasTriggered50 && (p1 >= 50 || p2 >= 50))
        {
            RandomMinigame();
            hasTriggered50 = true;
        }
        else if (!hasTriggered75 && (p1 >= 75 || p2 >= 75))
        {
            RandomMinigame();
            hasTriggered75 = true;
        }
    }

    void RandomMinigame()
    {
        int randomIndex;
        
        do
        {
            randomIndex = Random.Range(0, minigame.Length);
        } 
        while (playedIndices.Contains(randomIndex));

        playedIndices.Add(randomIndex);
        minigame[randomIndex].SetActive(true);
    }
}
