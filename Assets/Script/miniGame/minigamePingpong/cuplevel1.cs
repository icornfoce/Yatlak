using UnityEngine;

public class cuplevel1 : MonoBehaviour
{
    public static cuplevel1 c1 { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        c1 = this;
    }

    // Update is called once per frame
    void scoreup()
    {
        
    }
}
