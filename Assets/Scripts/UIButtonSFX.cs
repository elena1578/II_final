using UnityEngine;
using UnityEngine.EventSystems;


public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("SFX Settings")]
    public bool playHover = true;
    public bool playClick = true;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHover)
            AudioManager.instance?.OnButtonHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClick)
            AudioManager.instance?.PlaySelectSFX();
    }
}