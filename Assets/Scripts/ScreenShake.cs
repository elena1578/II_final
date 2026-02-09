using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [HideInInspector] public ScreenShake instance;

    [SerializeField] private float defaultDuration = 0f;
    [SerializeField] private float defaultMagnitude = 0.1f;

    private Vector3 initialPosition;
    private float shakeTimeRemaining = 0f;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

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

        initialPosition = FindFirstObjectByType<Camera>().transform.localPosition;
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

        if (Input.GetKeyDown(KeyCode.T))
            TestShake();
    }

    /// <summary>
    /// Shake screen w/ optional custom duration and magnitude
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="magnitude"></param>
    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        shakeDuration = duration > 0 ? duration : defaultDuration;
        shakeMagnitude = magnitude > 0 ? magnitude : defaultMagnitude;

        shakeTimeRemaining = shakeDuration;
    }

    public void TestShake()
    {
        Shake(0.5f, 0.2f);
    }
}
