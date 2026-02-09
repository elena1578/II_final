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

    public void Show(string template, BattleActionResult result)
    {
        string parsed = Parse(template, result);
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

    #region Parsing
    private string Parse(string template, BattleActionResult result)
    {
        string text = template;

        text = text.Replace("{actor}", GetActorName(result.actor));
        text = text.Replace("{target}", GetActorName(result.targets));

        text = text.Replace("{damage}", result.damage.ToString());
        text = text.Replace("{heal}", result.heal.ToString());

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
    #endregion
}
