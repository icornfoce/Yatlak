using UnityEngine;
using UnityEngine.UI;

public class SequencePlayerController : MonoBehaviour
{
    private SequenceUI sequenceUI;

    public Slider p1Slider;
    public Slider p2Slider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            p2Slider.value += 10f;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log("Down Arrow Pressed");
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("Left Arrow Pressed");
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("Right Arrow Pressed");
        }
    }
}
