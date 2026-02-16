using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class BattleActorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

    private BattleActor boundActor;


    public void Bind(BattleActor actor, Sprite portrait)
    {
        boundActor = actor;
        // portraitImage.sprite = portrait;

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


    #region Tooltip
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (boundActor is EnemyBattleActor enemy)
            EnemyTooltipUI.instance.Show(enemy);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (boundActor is EnemyBattleActor)
            EnemyTooltipUI.instance.Hide();
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
        if (portraitAnimator == null)
            return;

        Debug.Log($"Playing hurt animation for {boundActor}");
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

        // toast animation currently isn't playing, need to fix
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

    public void SlideOffScreen()
    {
        StartCoroutine(SlideOffScreenRoutine(transform));
    }

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
