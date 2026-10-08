using UnityEngine;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class DartsPlayer : MonoBehaviour
{
    public int playerNumber = 1;

    public KeyCode throwKey = KeyCode.S;

    public string keyText = "";


    public DartsManager manager;

    public Transform target;

    public Transform crosshair;

    public GameObject dartPrefab;

    public TextMeshProUGUI statusText;


    public float aimRangeX = 2.5f;

    public float aimRangeY = 2.5f;

    public float moveSpeedX = 11f;

    public float moveSpeedY = 12.5f;


    public AudioClip lockSound;

    public AudioClip hitSound;

    private AudioSource audioSource2D;

    private List<GameObject> spawnedDarts = new List<GameObject>();


    int step = 0;//0 = ปาครบแล้ว 1 = เล็งซ้ายขวา 2 = เล็งขึ้นลง 3 = ปา

    float aimX = 0;

    float aimY = 0;

    int direction = 1;

    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        StartAiming();
    }

    void StartAiming()
    {
        aimX = -aimRangeX;
        aimY = 0;
        direction = 1;
        crosshair.gameObject.SetActive(true);
        step = 1;
        statusText.text = "Lock X " + keyText;
        PunchStatusText(0.15f);
    }

    void Update()
    {
        if (manager.isPlaying == false)
        {
            return;
        }

        MoveCrosshair();
        CheckButton();
    }

    void MoveCrosshair()
    {
        if (step == 1)
        {
            aimX = aimX + moveSpeedX * direction * Time.deltaTime;

            if (aimX > aimRangeX) 
            {
                aimX = aimRangeX;
                direction = -1;
            }
            if (aimX < -aimRangeX) 
            {
                aimX = -aimRangeX;
                direction = 1;
            }
        }

        if (step == 2)
        {
            aimY = aimY + moveSpeedY * direction * Time.deltaTime;

            if (aimY > aimRangeY)
            {
                aimY = aimRangeY;
                direction = -1;
            }
            if (aimY < -aimRangeY) 
            {
                aimY = -aimRangeY;
                direction = 1;
            }
        }
        Vector3 aimPoint = new Vector3(target.position.x + aimX, target.position.y + aimY, 0);
        crosshair.position = cam.WorldToScreenPoint(aimPoint);
    }

    void CheckButton()
    {
        if (Input.GetKeyDown(throwKey))
        {
            if (step == 1)
            {
                PlaySound(lockSound);
                aimY = -aimRangeY;
                direction = 1;
                step = 2;
                statusText.text = "Lock Y " + keyText;
                PunchStatusText(0.2f);
            }
            else if (step == 2)
            {
                PlaySound(lockSound);
                step = 3;
                Invoke("DartHit", 0.2f);
            }
        }
    }


    void DartHit()
    {
        GameObject dart = Instantiate(dartPrefab, new Vector3(target.position.x + aimX, target.position.y + aimY, 0), Quaternion.identity, transform);
        if (dart != null)
        {
            spawnedDarts.Add(dart);
        }
        PlaySound(hitSound);

        float distance = new Vector2(aimX, aimY).magnitude;

        int points = 0;
        if (distance <= 0.5f)
        {
            points = 100;
            statusText.text = "+100 BULLSEYE!";
        }
        else if (distance <= 0.9f)
        {
            points = 60;
            statusText.text = "+60 INNER RING!";
        }
        else if (distance <= 1.6f)
        {
            points = 40;
            statusText.text = "+40 MIDDLE RING!";
        }
        else if (distance <= 2.1f) 
        {
            points = 20;
            statusText.text = "+20 OUTER RING!";
        }
        else if (distance <= 2.8f) 
        {
            points = 10;
            statusText.text = "+10 SUPER OUTER RING!";
        }
        else
        {
            statusText.text = "MISS!";
        }
        PunchStatusText(0.35f);
        manager.AddScore(playerNumber, points);
        Invoke("NextDart", 1f); 
    }

    public void ClearDarts()
    {
        foreach (GameObject dart in spawnedDarts)
        {
            if (dart != null)
            {
                Destroy(dart);
            }
        }
        spawnedDarts.Clear();
    }

    private void OnDisable()
    {
        ClearDarts();
    }

    private void OnDestroy()
    {
        ClearDarts();
        if (statusText != null) statusText.transform.DOKill();
    }

    void NextDart()
    {
        if (manager.GetDartsLeft(playerNumber) <= 0)
        {
            step = 0;
            statusText.text = "Done!";
            PunchStatusText(0.2f);
            crosshair.gameObject.SetActive(false); 
            return;
        }
        StartAiming();
    }

    void PunchStatusText(float intensity)
    {
        if (statusText != null)
        {
            statusText.transform.DOKill();
            statusText.transform.localScale = Vector3.one;
            statusText.transform.DOPunchScale(Vector3.one * intensity, 0.2f, 5, 0.5f);
        }
    }

    void PlaySound(AudioClip sound)
    {
        if (sound != null)
        {
            audioSource2D.PlayOneShot(sound);
        }
    }
    void Awake()
    {
        audioSource2D = gameObject.AddComponent<AudioSource>();
        audioSource2D.spatialBlend = 0f;
    }
}
