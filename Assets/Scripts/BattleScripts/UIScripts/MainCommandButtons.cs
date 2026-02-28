using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class MainCommandButtons : MonoBehaviour
{
    [Header("Command Buttons in Scene")]
    public GameObject fightButton;
    public GameObject runButton;

    [Header("Canvas Groups")]
    public CanvasGroup mainCommandCanvasGroup;
    public CanvasGroup secondaryCommandCanvasGroup;

    [Header("Objects")]
    public GameObject skillSelectionGroup;

    private InputAction backInputAction;

    
    private void OnEnable()
    {
        backInputAction = InputSystemManager.instance.actions["Back"];
        backInputAction.performed += OnBackInput;
        backInputAction.Enable();
    }

    private void OnDisable()
    {
        backInputAction.performed -= OnBackInput;
        backInputAction.Disable();
    }

    private void OnBackInput(InputAction.CallbackContext context)
    {
        if (secondaryCommandCanvasGroup.interactable)
        {
            HideSecondaryCommands();
            ShowMainCommands();
        }
        else if (skillSelectionGroup.activeSelf)
        {
            HideSkillSelection();
            ShowSecondaryCommands();
        }
    }

    public void OnFightButtonPressed()
    {
        HideMainCommands();
        ShowSecondaryCommands();
    }

    public void OnRunButtonPressed() => BattleManager.instance.AttemptFlee();

    public void OnAttackButtonPressed()
    {
        BattleManager.instance.SelectDefaultAttack();
        HideSecondaryCommands();
    }

    public void OnSkillButtonPressed()
    {
        ShowSkillSelection();
        HideSecondaryCommands();
    }   

    public void OnSnackButtonPressed()
    {
        // won't do anything for now
        // AudioManager.instance.PlaySFX("Error");
    }

    public void OnToyButtonPressed()
    {
        // also won't do anything for now
        // AudioManager.instance.PlaySFX("Error");
    }


    #region Show/Hide
    public void ShowMainCommands()
    {
        mainCommandCanvasGroup.alpha = 1;
        mainCommandCanvasGroup.interactable = true;
        mainCommandCanvasGroup.blocksRaycasts = true;
    }

    private void HideMainCommands()
    {
        mainCommandCanvasGroup.alpha = 0;
        mainCommandCanvasGroup.interactable = false;
        mainCommandCanvasGroup.blocksRaycasts = false;
    }

    private void ShowSecondaryCommands()
    {
        secondaryCommandCanvasGroup.alpha = 1;
        secondaryCommandCanvasGroup.interactable = true;
        secondaryCommandCanvasGroup.blocksRaycasts = true;
    }

    private void HideSecondaryCommands()
    {
        secondaryCommandCanvasGroup.alpha = 0;
        secondaryCommandCanvasGroup.interactable = false;
        secondaryCommandCanvasGroup.blocksRaycasts = false;
    }

    private void ShowSkillSelection() => skillSelectionGroup.SetActive(true);
    private void HideSkillSelection() => skillSelectionGroup.SetActive(false);

    public void PostSkillSelection()
    {
        HideSkillSelection();
        HideSecondaryCommands();
        ShowMainCommands();
    }

    public void HideAllCommands()
    {
        HideMainCommands();
        HideSecondaryCommands();
    }
    #endregion
}
