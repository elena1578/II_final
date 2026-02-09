using UnityEngine;
using TMPro;


public class BattleActionButton : MonoBehaviour
{
    public TextMeshProUGUI buttonTextChild;
    [SerializeField] private BattleActionData actionData;
    private BattleUIManager battleUIManager;
    private MainCommandButtons mainCommandButtons;


    private void Start()
    {
        battleUIManager = GetComponentInParent<BattleUIManager>();
        mainCommandButtons = GetComponentInParent<MainCommandButtons>();
        gameObject.SetActive(true);
    }

    public void Initialize(BattleActionData data)
    {
        actionData = data;
        buttonTextChild.text = data.actionName.ToDisplayName();
        gameObject.SetActive(true);
    }

    public void Clear()
    {
        actionData = null;
        gameObject.SetActive(false);
    }

    public void OnButtonPressed()
    {
        if (actionData == null)
            return;

        BattleManager.instance.OnPlayerSelectedAction(actionData);
        mainCommandButtons.PostSkillSelection();  // hide skill UI -> next actor turn
    }
}
