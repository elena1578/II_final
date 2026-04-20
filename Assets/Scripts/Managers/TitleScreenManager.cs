using UnityEngine;


public class TitleScreenManager : MonoBehaviour
{
    public CanvasGroup optionsCanvasGroup;

    private void Start()
    {
        HideOptions();
    }
    
    public void OnOptionsButtonPressed() => ShowOptions();  
    public void OnBackButtonPressed() => HideOptions();
    public void OnFullscreenButtonPressed() => Screen.fullScreen = !Screen.fullScreen;

    private void ShowOptions()
    {
        optionsCanvasGroup.alpha = 1f;
        optionsCanvasGroup.interactable = true;
        optionsCanvasGroup.blocksRaycasts = true;
    }

    private void HideOptions()
    {
        optionsCanvasGroup.alpha = 0f;
        optionsCanvasGroup.interactable = false;
        optionsCanvasGroup.blocksRaycasts = false;
    }
}
