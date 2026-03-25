using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;


public class BattleActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI buttonTextChild;
    [SerializeField] private BattleActionData actionData;
    private BattleUIManager battleUIManager;
    private MainCommandButtons mainCommandButtons;
    
    // internal refs pulled from BattleUIManager
    private TextMeshProUGUI juiceCostText;
    private GameObject juiceImage;
    private GameObject skillDescriptionBox;
    private TextMeshProUGUI skillNameText;
    private TextMeshProUGUI skillDescriptionText;
    private CanvasGroup skillDescriptionCanvasGroup;
    private CanvasGroup juiceCostCanvasGroup;
    private Coroutine juiceFadeRoutine;
    private Coroutine descriptionFadeRoutine;


    private void Start()
    {
        battleUIManager = GetComponentInParent<BattleUIManager>();
        mainCommandButtons = GetComponentInParent<MainCommandButtons>();

        // cache refs from BattleUIManager
        // juice cost
        juiceCostText = battleUIManager.juiceCostText;  
        juiceImage = battleUIManager.juiceCostIcon;

        // skill description
        skillDescriptionBox = battleUIManager.skillDescriptionBox;
        skillNameText = battleUIManager.skillNameText;
        skillDescriptionText = battleUIManager.skillDescriptionText;

        // canvas groups
        skillDescriptionCanvasGroup = battleUIManager.skillDescriptionCanvasGroup;
        juiceCostCanvasGroup = battleUIManager.juiceCostCanvasGroup;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (actionData == null)
        {
            Debug.LogWarning("[BattleActionButton] No action data assigned to this button!");
            return;
        }

        // juice cost
        juiceCostText.text = $"Cost: {actionData.juiceCost}";
        StartFade(1f, 0.12f, juiceCostCanvasGroup, ref juiceFadeRoutine);

        // skill description
        skillNameText.text = actionData.actionName.ToDisplayName();
        skillDescriptionText.text = actionData.descriptionText;
        StartFade(1f, 0.06f, skillDescriptionCanvasGroup, ref descriptionFadeRoutine);
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        // juice cost
        // juiceCostText.text = "";
        // juiceImage.SetActive(false);
        StartFade(0f, 0.12f, juiceCostCanvasGroup, ref juiceFadeRoutine);

        // skill description
        // skillDescriptionBox.SetActive(false);
        // skillNameText.text = "";
        // skillDescriptionText.text = "";
        StartFade(0f, 0.06f, skillDescriptionCanvasGroup, ref descriptionFadeRoutine);
    }


    #region Helpers
    private void StartFade(float target, float fadeDuration, CanvasGroup group, ref Coroutine routine)
    {
        if (routine != null)
            StopCoroutine(routine);  // stop any ongoing fade

        routine = StartCoroutine(FadeRoutine(target, fadeDuration, group));
    }

    private IEnumerator FadeRoutine(float target, float fadeDuration, CanvasGroup group)
    {
        float start = group.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, time / fadeDuration);
            yield return null;
        }

        group.alpha = target;
    }
    #endregion
}
