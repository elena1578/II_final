using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class DarknessUI : MonoBehaviour
{
    public static DarknessUI instance;
    public Image darknessOverlayImage;
    public Image vignetteImage;
    public float transitionSpeed = 2f;

    private Coroutine transitionRoutine;
    private float targetDarkness;
    private float targetVignette;

    private void Awake()
    {
        instance = this;
    }

    public void SetDarknessLevel(int level)
    {
        switch (level)
        {
            case 0:
                SetDarkness(0f, 0f);
                break;

            case 1:
                SetDarkness(0.3f, 0f);
                break;

            case 2:
                SetDarkness(0.5f, 0.3f);
                break;

            case 3:
                SetDarkness(0.65f, 0.6f);
                break;

            case 4:
                SetDarkness(0.75f, 1f);
                break;
        }
    }

    private void SetDarkness(float darkness, float vignette)
    {
        targetDarkness = darkness;
        targetVignette = vignette;

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        float startDarkness = darknessOverlayImage.color.a;
        float startVignette = vignetteImage.color.a;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;

            float darknessAlpha = Mathf.Lerp(startDarkness, targetDarkness, t);
            float vignetteAlpha = Mathf.Lerp(startVignette, targetVignette, t);

            if (darknessOverlayImage != null)
                darknessOverlayImage.color = new Color(0, 0, 0, darknessAlpha);

            if (vignetteImage != null)
                vignetteImage.color = new Color(1, 1, 1, vignetteAlpha);

            yield return null;
        }

        darknessOverlayImage.color = new Color(0, 0, 0, targetDarkness);
        vignetteImage.color = new Color(1, 1, 1, targetVignette);
        transitionRoutine = null;
    }
}