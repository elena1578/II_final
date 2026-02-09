using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade instance;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (fadeCanvasGroup == null)
            fadeCanvasGroup = GetComponent<CanvasGroup>();
    }

    public IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer <= fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    public IEnumerator FadeOut()
    {
        float timer = 0f;
        while (timer <= fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
    }
}

