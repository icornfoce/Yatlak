using UnityEngine;
using TMPro;


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
        // crosshair เป็น UI จึงต้องแปลงตำแหน่งในฉากเป็นตำแหน่งบนหน้าจอ
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
        Instantiate(dartPrefab, new Vector3(target.position.x + aimX, target.position.y + aimY, 0), Quaternion.identity);
        PlaySound(hitSound);

        float distance = new Vector2(aimX, aimY).magnitude;

        int points = 0;
        if (distance <= 0.35f)
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
        else if (distance <= 2.4f) 
        {
            points = 20;
            statusText.text = "+20 OUTER RING!";
        }
        else
        {
            statusText.text = "MISS!";
        }

        manager.AddScore(playerNumber, points);

        Invoke("NextDart", 1f); 
    }

    void NextDart()
    {
        if (manager.GetDartsLeft(playerNumber) <= 0)
        {
            step = 0;
            statusText.text = "Done!";
            crosshair.gameObject.SetActive(false); 
            return;
        }

        StartAiming();
    }

    void PlaySound(AudioClip sound)
    {
        if (sound != null)
        {
            AudioSource.PlayClipAtPoint(sound, transform.position);
        }
    }
}
