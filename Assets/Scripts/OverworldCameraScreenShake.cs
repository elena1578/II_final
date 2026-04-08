using UnityEngine;
using UnityEngine.InputSystem;


public class OverworldCameraScreenShake : MonoBehaviour
{
    [Header("Default Shake Settings")]
    [SerializeField] private float defaultDuration = 0.25f;
    [SerializeField] private float defaultMagnitude = 15f;

    private Camera cam;
    private OverworldCameraFollow cameraFollow;
    private Vector3 originalPosition;

    private float shakeTimeRemaining;
    private float shakeDuration;
    private float shakeMagnitude;
    private float shakeFrequency = 40f;  // how fast screen shakes, higher = faster

#if UNITY_EDITOR
    private InputAction shakeAction;
    private float testDuration = 0.25f;
    private float testMagnitude = 3f;
#endif

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cameraFollow = GetComponent<OverworldCameraFollow>();
        originalPosition = cam.transform.localPosition;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        if (shakeAction == null)
        {
            shakeAction = new InputAction("shake", binding: "<Keyboard>/t");
            shakeAction.performed += ctx =>
            {
                Debug.Log("[ScreenShake] T pressed");
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

    private void LateUpdate()
    {
        if (shakeTimeRemaining <= 0f)
        {
            cam.transform.localPosition = originalPosition;  // ensure original pos is reset
            return;
        }

        shakeTimeRemaining -= Time.deltaTime;

        // consistent oscillation shake side-to-side (vs. random)
        float progress = 1f - (shakeTimeRemaining / shakeDuration);
        float damper = 1f - Mathf.Clamp01(progress); 
        float x = Mathf.Sin(Time.time * shakeFrequency) * shakeMagnitude * damper;

        // apply shake offset to original pos
        Vector3 offset = new Vector3(x, 0f, 0f);
        cameraFollow.SetShakeOffset(offset);  // apply to OverworldCameraFollow to be added on top of normal follow position

        if (shakeTimeRemaining <= 0f)
            cameraFollow.SetShakeOffset(Vector3.zero);
    }

    /// <summary>
    /// call to trigger screen shake [for UI] w/ optional custom duration and magnitude.
    /// default vals are 0.45f, 0.15f respectively
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



