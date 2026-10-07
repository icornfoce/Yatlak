using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public enum DirectionKey
{
    Up,
    Down,
    Left,
    Right
}

public class mainMinigame : MonoBehaviour
{
    private List<List<DirectionKey>> masterRounds = new List<List<DirectionKey>>();

    public List<DirectionKey> p1TargetKeys = new List<DirectionKey>();
    public List<DirectionKey> p2TargetKeys = new List<DirectionKey>();

    public Slider p1Slider;
    public Slider p2Slider;

    public TextMeshProUGUI p1Text; 
    public TextMeshProUGUI p2Text;

    public TextMeshProUGUI p1ResultText;
    public TextMeshProUGUI p2ResultText;

    private int p1Round = 0;
    private int p2Round = 0;

    private int p1Index = 0;
    private int p2Index = 0;

    private bool isGameOver = false;

    void Awake()
    {
        p1TargetKeys = GetRoundKeys(p1Round);
        p2TargetKeys = GetRoundKeys(p2Round);
    }

    void Update()
    {
        if (isGameOver) return;

        player1Key();
        player2Key();
        UI();
    }

    List<DirectionKey> GetRoundKeys(int roundIndex)
    {
        while (masterRounds.Count <= roundIndex)
        {
            List<DirectionKey> newRound = new List<DirectionKey>();
            int count = Random.Range(4, 7);

            for (int i = 0; i < count; i++)
            {
                newRound.Add((DirectionKey)Random.Range(0, 4));
            }

            masterRounds.Add(newRound);
        }

        return masterRounds[roundIndex];
    }

    void player1Key()
    {
        if (p1Index >= p1TargetKeys.Count) return;

            DirectionKey current = p1TargetKeys[p1Index];

            bool pressedW = Input.GetKeyDown(KeyCode.W);
            bool pressedS = Input.GetKeyDown(KeyCode.S);
            bool pressedA = Input.GetKeyDown(KeyCode.A);
            bool pressedD = Input.GetKeyDown(KeyCode.D);

            if (pressedW || pressedS || pressedA || pressedD)
            {
                bool isCorrect = (pressedW && current == DirectionKey.Up) ||
                                (pressedS && current == DirectionKey.Down) ||
                                (pressedA && current == DirectionKey.Left) ||
                                (pressedD && current == DirectionKey.Right);

                if (isCorrect)
                {
                    p1Index++;
                    if (p1Index >= p1TargetKeys.Count)
                    {
                        addScore(1);
                    }
                }
                else
                {
                    p1Index = 0; 
                }
            }
    }

    void player2Key()
    {
        if (p2Index >= p2TargetKeys.Count) return;

            DirectionKey current = p2TargetKeys[p2Index];

            bool pressedUp = Input.GetKeyDown(KeyCode.UpArrow);
            bool pressedDown = Input.GetKeyDown(KeyCode.DownArrow);
            bool pressedLeft = Input.GetKeyDown(KeyCode.LeftArrow);
            bool pressedRight = Input.GetKeyDown(KeyCode.RightArrow);

            if (pressedUp || pressedDown || pressedLeft || pressedRight)
            {
                bool isCorrect = (pressedUp && current == DirectionKey.Up) ||
                                (pressedDown && current == DirectionKey.Down) ||
                                (pressedLeft && current == DirectionKey.Left) ||
                                (pressedRight && current == DirectionKey.Right);

                if (isCorrect)
                {
                    p2Index++;
                    if (p2Index >= p2TargetKeys.Count)
                    {
                        addScore(2);
                    }
                }
                else
                {
                    p2Index = 0; 
                }
            }
    }

    void addScore(int player)
    {
        if (player == 1)
        {
            if (p1Slider != null) p1Slider.value += 5f;
            
            p1Round++;
            p1Index = 0;
            p1TargetKeys = GetRoundKeys(p1Round);
        }
        else if (player == 2)
        {
            if (p2Slider != null) p2Slider.value += 5f;

            p2Round++;
            p2Index = 0;
            p2TargetKeys = GetRoundKeys(p2Round);
        }

        checkWin();
    }

    void UI()
    {
        string p1DisplayText = "";

        for (int i = 0; i < p1TargetKeys.Count; i++)
        {
            string keyChar = GetKeyLetter(p1TargetKeys[i]);
            if (i < p1Index)
            {
                p1DisplayText += $"<color=#00FF00>{keyChar}</color> ";
            }
            else
            {  
                p1DisplayText += $"{keyChar} ";
            }
            
            p1Text.text = p1DisplayText.TrimEnd();
        }

        string p2DisplayText = "";

        for (int i = 0; i < p2TargetKeys.Count; i++)
        {
            string keyChar = GetKeyArrow(p2TargetKeys[i]);
            if (i < p2Index)
            {
                p2DisplayText += $"<color=#00FF00>{keyChar}</color> ";
            }
            else
            {
                p2DisplayText += $"{keyChar} ";
            }

            p2Text.text = p2DisplayText.TrimEnd();
        }
    }

    string GetKeyLetter(DirectionKey key)
    {
        switch (key)
        {
            case DirectionKey.Up: return "W";
            case DirectionKey.Down: return "S";
            case DirectionKey.Left: return "A";
            case DirectionKey.Right: return "D";
            default: return "";
        }
    }

    string GetKeyArrow(DirectionKey key)
    {
        switch (key)
        {
            case DirectionKey.Up: return "↑";
            case DirectionKey.Down: return "↓";
            case DirectionKey.Left: return "←";
            case DirectionKey.Right: return "→";
            default: return "";
        }
    }

    void checkWin()
    {
        if (p1Slider != null && p1Slider.value >= 100f)
        {
            p1ResultText.gameObject.SetActive(true);
            isGameOver = true;
            if (p1ResultText != null) p1ResultText.text = "WIN";
            if (p2ResultText != null) p2ResultText.text = "LOSE";
        }
        else if (p2Slider != null && p2Slider.value >= 100f)
        {
            p2ResultText.gameObject.SetActive(true);
            isGameOver = true;
            if (p2ResultText != null) p2ResultText.text = "WIN";
            if (p1ResultText != null) p1ResultText.text = "LOSE";
        }
    }
}