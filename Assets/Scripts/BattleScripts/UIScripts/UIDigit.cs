using UnityEngine;
using UnityEngine.UI;

public class UIDigit : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetSprite(Sprite sprite) => image.sprite = sprite;
    public void SetAlpha(float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}