using UnityEngine;


public class UIDigitSpawner : MonoBehaviour
{
    public UIDigit uiDigitPrefab;
    public UIDigitGroup digitGroupPrefab;

    [Header("Digit Sprites (0-9)")]
    public Sprite[] damageDigitSprites;  // 0-9 **in order** to ensure that #s match correctly
    public Sprite[] healDigitSprites; 
    public Sprite[] juiceLossDigitSprites;

    public void SpawnDamage(BattleActionResult result, RectTransform target)
    {
        UIDigitGroup group = Instantiate(digitGroupPrefab, transform);
        group.transform.position = target.position;

        if (result.damage > 0)
            group.AddNumber(result.damage, damageDigitSprites);

        if (result.heal > 0)
            group.AddNumber(result.heal, healDigitSprites);

        group.Play();
    }

    public void SpawnHeal(BattleActionResult result, RectTransform target)
    {
        UIDigitGroup group = Instantiate(digitGroupPrefab, transform);
        group.transform.position = target.position;

        if (result.heal > 0)
            group.AddNumber(result.heal, healDigitSprites);

        group.Play();
    }
}
