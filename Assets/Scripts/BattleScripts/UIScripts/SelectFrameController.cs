using UnityEngine;
using UnityEngine.UI;

public class SelectFrameController : MonoBehaviour
{
    [Header("Frame Positions")]
    public Vector3 topLeftPosition;
    public Vector3 topRightPosition;
    public Vector3 bottomLeftPosition;
    public Vector3 bottomRightPosition;

    [Header("Blink Settings")]
    public float blinkSpeed = 5f;
    public float minAlpha = 0.5f;
    public float maxAlpha = 1f;

    private Image selectFrameImage;

    private void Start()
    {
        selectFrameImage = GetComponent<Image>();
    }
    
    private void Update()
    {
        Blink();
    }

    private void Blink()
    {
        float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
        Color color = selectFrameImage.color;
        color.a = Mathf.Lerp(minAlpha, maxAlpha, alpha);
        selectFrameImage.color = color;
    }
}
