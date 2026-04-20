using UnityEngine;


public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen instance;
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        Hide();
    }

    public void Show()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }
}