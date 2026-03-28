using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;


public class BattleActorUI : MonoBehaviour
{
    [Header("Portrait Refs")]
    public Image portraitImage;
    public Animator portraitAnimator;
    public Image portraitBackgroundImage;
    public Image emotionLabelImage;
    public Image hpFill;
    public TextMeshProUGUI hpText;
    public Image juiceFill;
    public TextMeshProUGUI juiceText;
    public GameObject selectFrame;
    public EmotionUIVisuals emotionUIVisuals;

    [Header("Action Overlay")]
    public GameObject actionOverlayRoot;
    public Animator actionAnimator;

    [Header("Click Region")]
    public ClickBattleActorUI clickRegion;
    [SerializeField] private Image highlightImage;
    [SerializeField] private float highlightFadeSpeed = 8f;

    private BattleActor boundActor;
    private bool targetable;
    private Coroutine highlightRoutine;
    private Color normalColor = Color.white;
    private Color hoverColor = new Color(1.1f, 1.05f, 0.6f);  // light yellow for highlight
    private bool toast = false;


    public void Bind(BattleActor actor, Sprite portrait)
    {
        boundActor = actor;

        // portrait = visual only
        if (portraitImage != null)
            portraitImage.raycastTarget = false;

        // bind click region
        if (clickRegion != null)
            clickRegion.Bind(actor);

        if (portraitAnimator != null)
        {
            portraitAnimator.Rebind();
            portraitAnimator.Update(0f);
            SetEmotionAnimation(actor.currentEmotion);
        }

        UpdateAll();
    }

    public void UpdateAll()
    {
        if (boundActor == null)
            return;

        // HP
        if (hpFill != null)
            hpFill.fillAmount = (float)boundActor.currentHP / boundActor.maxHP;

        if (hpText != null)
            hpText.text = $"{boundActor.currentHP}/{boundActor.maxHP}";


        // juice
        if (juiceFill != null)
            juiceFill.fillAmount = (float)boundActor.currentJuice / boundActor.maxJuice;

        if (juiceText != null)
            juiceText.text = $"{boundActor.currentJuice}/{boundActor.maxJuice}";
    }

    public void SetTurnActive(bool active)
    {
        if (selectFrame != null)
            selectFrame.SetActive(active);
    }


    #region Targeting
    public void SetTargetable(bool value)
    {
        targetable = value;

        if (clickRegion != null)
            clickRegion.SetTargetable(value);
    }

    public void SetHoverHighlight(bool state)
    {
        if (!targetable || highlightImage == null)
            return;

        Color target = state ? hoverColor : normalColor;
        // Debug.Log($"[BattleActorUI] Setting hover highlight for {boundActor} to {state} | target color: {target}");

        if (highlightRoutine != null)
            StopCoroutine(highlightRoutine);

        highlightRoutine = StartCoroutine(FadeHighlight(target));
    }

    private IEnumerator FadeHighlight(Color target)
    {
        Color start = highlightImage.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * highlightFadeSpeed;
            highlightImage.color = Color.Lerp(start, target, t);
            yield return null;
        }

        highlightImage.color = target;
    }

    public void ResetHighlight()
    {
        if (highlightImage == null)
            return;

        if (highlightRoutine != null)
            StopCoroutine(highlightRoutine);

        highlightImage.color = normalColor;
    }
    #endregion


    #region Animations
    public void SetEmotionAnimation(EmotionType emotion)
    {
        if (portraitAnimator == null)
            return;

        // face animation
        portraitAnimator.SetInteger("emotion", (int)emotion);

        // background and label sprites
        if (emotionUIVisuals != null)
        {
            emotionUIVisuals.GetSprites(emotion, out var bg, out var label);

            portraitBackgroundImage.sprite = bg;
            emotionLabelImage.sprite = label;
        }
    }

    public void PlayHurtAnimation()
    {
        if (portraitAnimator == null || toast)
            return;  // skip hurt if toast-ed

        
        // Debug.Log($"[BattleActorUI] Playing hurt animation for {boundActor}");
        portraitAnimator.ResetTrigger("hurt");  // safety reset
        portraitAnimator.SetTrigger("hurt");
    }

    public void PlayActionAnimation(string trigger)
    {
        if (actionAnimator == null || string.IsNullOrEmpty(trigger))
            return;

        actionOverlayRoot.SetActive(true);
        // actionAnimator.Rebind();
        // actionAnimator.Update(0f);

        actionAnimator.ResetTrigger(trigger);  // safety reset  
        actionAnimator.SetTrigger(trigger);
    }

    public void HideActionOverlay()
    {
        if (actionOverlayRoot != null)
            actionOverlayRoot.SetActive(false);
    }

    public void SetToastAnimation()
    {
        if (portraitAnimator == null)
            return;

        toast = true;
        portraitAnimator.ResetTrigger("hurt");
        portraitAnimator.SetBool("toast", true);
    }

    public void SetSuccumbAnimation()
    {
        if (portraitAnimator == null)
            return;

        portraitAnimator.SetBool("succumb", true);
        // set up screen dark pulsing here later
        // also not sure if succumb animation is reset back to neutral when healed? need to check
    }

    public void SlideOffScreen() => StartCoroutine(SlideOffScreenRoutine(transform));

    /// <summary>
    /// slides actor down off screen. used for when an enemy is toast
    /// </summary>
    /// <param name="enemyTransform"></param>
    /// <returns></returns>
    private IEnumerator SlideOffScreenRoutine(Transform enemyTransform)
    {
        Vector3 start = enemyTransform.localPosition;
        Vector3 end = start + Vector3.down * 500f;

        float time = 0f;
        float duration = 0.8f;

        while (time < duration)
        {
            time += Time.deltaTime;
            enemyTransform.localPosition = Vector3.Lerp(start, end, time / duration);
            yield return null;
        }
    }
    #endregion
}
