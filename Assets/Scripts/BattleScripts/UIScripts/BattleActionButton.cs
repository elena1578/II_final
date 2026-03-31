using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;


public class BattleActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI buttonTextChild;
    [HideInInspector] public BattleActionData actionData;
    [HideInInspector] public BattleUIManager battleUIManager;
    private MainCommandButtons mainCommandButtons;
    private PlayerBattleActor actor;
    private Button button;
    
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


    private void Awake()
    {
        battleUIManager = GetComponentInParent<BattleUIManager>();
        mainCommandButtons = GetComponentInParent<MainCommandButtons>();
        button = GetComponent<Button>();

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

    public void SetActor(PlayerBattleActor actor) => this.actor = actor;
    public void Initialize(BattleActionData data)
    {
        if (data == null || actor == null)
        {
            Clear();
            return;
        }

        // cache internal refs if they happen to be null
        battleUIManager ??= GetComponentInParent<BattleUIManager>();
        mainCommandButtons ??= GetComponentInParent<MainCommandButtons>();
        button ??= GetComponent<Button>();

        // cache refs again if they happen to be null
        juiceCostText ??= battleUIManager.juiceCostText;
        juiceImage ??= battleUIManager.juiceCostIcon;
        skillDescriptionBox ??= battleUIManager.skillDescriptionBox;
        skillNameText ??= battleUIManager.skillNameText;
        skillDescriptionText ??= battleUIManager.skillDescriptionText;
        skillDescriptionCanvasGroup ??= battleUIManager.skillDescriptionCanvasGroup;
        juiceCostCanvasGroup ??= battleUIManager.juiceCostCanvasGroup;

        // set data & text
        actionData = data;
        buttonTextChild.text = data.actionName.ToDisplayName();

        // check if actor has enough juice for this action & set interactable state
        bool canUse = battleUIManager.CanUseAction(actor, actionData);
        button.interactable = canUse;
        buttonTextChild.color = canUse ? Color.white : Color.gray;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// ensures elements are hidden even when coroutines are still running
    /// </summary>
    private void OnDisable()
    {
        // stop fades
        if (juiceFadeRoutine != null)
            StopCoroutine(juiceFadeRoutine);

        if (descriptionFadeRoutine != null)
            StopCoroutine(descriptionFadeRoutine);

        // force hidden state
        if (juiceCostCanvasGroup != null)
            juiceCostCanvasGroup.alpha = 0f;

        if (skillDescriptionCanvasGroup != null)
            skillDescriptionCanvasGroup.alpha = 0f;
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
        StartCoroutine(HideUI());  // hide juice cost and skill description since now selecting target
    }

    private IEnumerator HideUI()
    {
        // fade out juice cost and skill description
        StartFade(0f, 0.12f, juiceCostCanvasGroup, ref juiceFadeRoutine);
        StartFade(0f, 0.06f, skillDescriptionCanvasGroup, ref descriptionFadeRoutine);

        yield return new WaitForSecondsRealtime(0.12f);  // wait for fade out to finish
        mainCommandButtons.HideSkillSelection();  // proceeds to target selection, so hide
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
        StartFade(0f, 0.12f, juiceCostCanvasGroup, ref juiceFadeRoutine);
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
