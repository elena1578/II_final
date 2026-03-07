using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;


public class BattleDialogManager : MonoBehaviour
{
    public static BattleDialogManager instance;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float typeSpeed = 0.04f;

    private Coroutine typingRoutine;
    public bool typing { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public void Show(string message)
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeText(message));
    }

    public void Show(BattleActionData action, BattleActionResult result)
    {
        string parsed = Parse(action, result);
        typing = true;
        Show(parsed);
    }

    private IEnumerator TypeText(string message)
    {
        text.text = "";

        foreach (char c in message)
        {
            text.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        typing = false;
    }

    public void Clear()
    {
        text.text = "";
    }

    /// <summary>
    /// used for when an actor starts their turn/is in process of choosing an action
    /// </summary>
    /// <param name="actor"></param>
    public void ShowPlanningPrompt(BattleActor actor) => Show($"What will {actor.name.ToDisplayName()} do?");


    #region Parsing
    private string Parse(BattleActionData action, BattleActionResult result)
    {
        string text = action.battleLogText;

        text = text.Replace("{actor}", GetActorName(result.actor));
        text = text.Replace("{target}", GetActorName(result.targets));

        text = text.Replace("{damage}", result.damage.ToString());
        text = text.Replace("{heal}", result.heal.ToString());

        text += GetAutoResultText(action, result);  // auto-add damage, heal, etc. text depending on action type

        if (result.didCrit)
            text = "IT HIT RIGHT IN THE HEART!\n" + text;

        // prob also need to add a parse for damage actions 
        // (e.g., "{actor} did {damage} damage to {target}!") 
        // and healing actions (e.g., "{actor} healed {target} for {heal} HP!")

        return text;
    }

    private string GetActorName(BattleActor actor)
    {
        return actor != null ? actor.name.ToDisplayName() : "?????";
    }

    private string GetActorName(List<BattleActor> actors)
    {
        if (actors == null || actors.Count == 0)
            return "?????";

        if (actors.Count == 1)
            return GetActorName(actors[0]);

        return string.Join(", ", actors.ConvertAll(a => GetActorName(a)));
    }

    private string GetAutoResultText(BattleActionData action, BattleActionResult result)
    {
        switch (action.actionType)
        {
            case BattleActionData.ActionType.Attack:
                if (result.damage > 0)
                    return $"\n{GetActorName(result.targets)} takes {result.damage} damage!";

                return "";

            case BattleActionData.ActionType.Heal:
                if (result.heal > 0)
                    return $"\n{GetActorName(result.targets)} recovers {result.heal} HP!";

                return "";

            case BattleActionData.ActionType.Emotion:
                if (action.emotionEffect != EmotionType.Neutral)
                    return $"\n{GetActorName(result.targets)} became {action.emotionEffect}.";

                return "";

            default:
                return "";
        }
    }
    #endregion
}
