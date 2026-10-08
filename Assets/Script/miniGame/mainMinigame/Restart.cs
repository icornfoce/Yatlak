using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Restart : MonoBehaviour
{
    [SerializeField]
    public GameObject uiPanel;

    private Vector3 panelInitialScale = Vector3.one;
    private CanvasGroup canvasGroup;
    private bool isTransitioning = false;

    void Awake()
    {
        if (uiPanel != null)
        {
            panelInitialScale = uiPanel.transform.localScale;
            canvasGroup = uiPanel.GetComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanel();
        }

        if (uiPanel != null && uiPanel.activeSelf && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void TogglePanel()
    {
        if (uiPanel == null || isTransitioning) return;

        if (uiPanel.activeSelf)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }

    public void ShowPanel()
    {
        if (uiPanel == null) return;

        isTransitioning = true;
        uiPanel.SetActive(true);
        uiPanel.transform.DOKill();
        uiPanel.transform.localScale = Vector3.zero;

        uiPanel.transform.DOScale(panelInitialScale, 0.3f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() => isTransitioning = false);

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
        }
    }

    public void HidePanel()
    {
        if (uiPanel == null) return;

        isTransitioning = true;
        uiPanel.transform.DOKill();

        uiPanel.transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                uiPanel.SetActive(false);
                isTransitioning = false;
            });

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, 0.18f).SetUpdate(true);
        }
    }

    public void RestartGame()
    {
        if (uiPanel == null) return;

        isTransitioning = true;
        uiPanel.transform.DOKill();
        uiPanel.transform.DOPunchScale(Vector3.one * 0.15f, 0.18f, 5, 0.5f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
    }

    void OnDestroy()
    {
        if (uiPanel != null)
        {
            uiPanel.transform.DOKill();
        }
    }
}
