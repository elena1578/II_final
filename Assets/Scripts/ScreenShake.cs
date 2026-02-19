using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ScreenShake : MonoBehaviour
{
    [HideInInspector] public ScreenShake instance;

    [SerializeField] private float defaultDuration = 0f;
    [SerializeField] private float defaultMagnitude = 0.1f;

    private Vector3 initialPosition;
    private float shakeTimeRemaining = 0f;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

#if UNITY_EDITOR    
    private InputAction shakeAction;
#endif

    private void Awake()
    {
        instance = this;
        
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        RebindCamera();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RebindCamera();
    private void OnEnable() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
#if UNITY_EDITOR
        shakeAction = new InputAction("shake", binding: "<Keyboard>/p");
        shakeAction.performed += _ => Shake();
        shakeAction.Enable();
        Debug.Log("[ScreenShake] Shake action bound to 'P' key");
#endif
    }
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void RebindCamera() 
    {
        initialPosition = FindFirstObjectByType<Camera>().transform.localPosition;
        Debug.Log("[ScreenShake] Camera bound");
    }

    private void Update()
    {
        if (shakeTimeRemaining > 0)
        {
            Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude;
            GetComponent<Camera>().transform.localPosition = initialPosition + (Vector3)shakeOffset;

            shakeTimeRemaining -= Time.deltaTime;
            if (shakeTimeRemaining <= 0f)
                GetComponent<Camera>().transform.localPosition = initialPosition;  // reset
        }
    }

    /// <summary>
    /// shake screen w/ optional custom duration and magnitude.
    /// default is 1f, 1f
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
