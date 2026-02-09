using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class EnemyTooltipUI : MonoBehaviour
{
    public static EnemyTooltipUI instance;

    [Header("Refs")]
    public TextMeshProUGUI nameText;
    public Image hpFill;
    public Image heartIcon;

    [Header("Heart States")]
    public Sprite heartFull;
    public Sprite heartThreeQuarters;
    public Sprite heartHalf;
    public Sprite heartOneQuarter;
    [Space]
    [SerializeField] private Vector2 cursorOffset = new Vector2(20f, -20f);
    [SerializeField] private float fadeDuration = 0.15f;

    private BattleActor boundActor;
    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(true);

        canvasGroup = GetComponent<CanvasGroup>();  
        canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (boundActor == null)
            return;

        hpFill.fillAmount = boundActor.currentHP / (float)boundActor.maxHP;
        UpdateHeartIcon();
    }

    private void LateUpdate()
    {
        if (Mouse.current == null)
            return;

        transform.position = Mouse.current.position.ReadValue() + cursorOffset;
    }
    public void Show(EnemyBattleActor enemy)
    {
        boundActor = enemy;
        nameText.text = enemy.enemyData.characterName.ToDisplayName();
        StartFade(1f);
    }

    public void Hide()
    {
        boundActor = null;
        StartFade(0f);
    }

    private void UpdateHeartIcon()
    {
        float hpPercent = boundActor.currentHP / (float)boundActor.maxHP;

        if (hpPercent >= 0.75f)
            heartIcon.sprite = heartFull;
        else if (hpPercent >= 0.5f)
            heartIcon.sprite = heartThreeQuarters;
        else if (hpPercent >= 0.25f)
            heartIcon.sprite = heartHalf;
        else
            heartIcon.sprite = heartOneQuarter;
    }

    private void StartFade(float target)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);  // stop any ongoing fade

        fadeRoutine = StartCoroutine(FadeRoutine(target));
    }

    private IEnumerator FadeRoutine(float target)
    {
        float start = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = target;
    }
}
