using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenShake : MonoBehaviour
{
    [Header("Default Shake Settings")]
    [SerializeField] private float defaultDuration = 0.25f;
    [SerializeField] private float defaultMagnitude = 15f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;

    private float shakeTimeRemaining;
    private float shakeDuration;
    private float shakeMagnitude;

#if UNITY_EDITOR
    private InputAction shakeAction;
    private float testDuration = 0.25f;
    private float testMagnitude = 15f;
#endif

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        if (shakeAction == null)
        {
            shakeAction = new InputAction("shake", binding: "<Keyboard>/p");
            shakeAction.performed += ctx =>
            {
                Debug.Log("[ScreenShake] P pressed");
                Shake(testDuration, testMagnitude);
            };
        }

        shakeAction.Enable();
        Debug.Log("[ScreenShake] Shake action enabled");
    }

    private void OnDisable()
    {
        shakeAction?.Disable();
    }
#endif

    private void Update()
    {
        if (shakeTimeRemaining <= 0f)
        {
            rectTransform.anchoredPosition = originalPosition;  // ensure original pos is reset
            return;
        }

        shakeTimeRemaining -= Time.deltaTime;

        float progress = 1f - (shakeTimeRemaining / shakeDuration);
        float damper = 1f - Mathf.Clamp01(progress);  // damps/fades the shake over time

        // consistent oscillation shake vs. random 
        float frequency = 40f; // how fast it oscillates
        float x = Mathf.Sin(Time.time * frequency) * shakeMagnitude * damper;  // side-to-side shake
        Vector2 offset = new Vector2(x, 0f);

        rectTransform.anchoredPosition = originalPosition + offset;

        if (shakeTimeRemaining <= 0f)
            rectTransform.anchoredPosition = originalPosition;
    }

    /// <summary>
    /// call to trigger screen shake [for UI] w/ optional custom duration and magnitude.
    /// default vals are 0.25f, 15f respectively
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="magnitude"></param>
    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        shakeDuration = duration > 0 ? duration : defaultDuration;
        shakeMagnitude = magnitude > 0 ? magnitude : defaultMagnitude;

        shakeTimeRemaining = shakeDuration;
    }
}



