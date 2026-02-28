using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class UIDigitGroup : MonoBehaviour
{
    [Header("Prefab Refs")]
    [SerializeField] private UIDigit digitPrefab;
    [SerializeField] private RectTransform linePrefab;
    
    [Header("Animation Settings")]
    [SerializeField] private float holdDuration = 1.2f;
    [SerializeField] private float fadeDuration = 0.3f;

    private List<UIDigit> allDigits = new();

    public void AddNumber(int amount, Sprite[] spriteSet)
    {
        RectTransform line = Instantiate(linePrefab, transform);
        string number = amount.ToString();  // convert # to string to iterate through digits

        foreach (char c in number)
        {
            int digit = c - '0';  // convert char back to int (e.g. '5' - '0' = 5)

            // instantiate digit prefab, set sprite based on digit, and add to list
            UIDigit newDigit = Instantiate(digitPrefab, line);
            newDigit.SetSprite(spriteSet[digit]);

            allDigits.Add(newDigit);
        }
    }

    public void Play() => StartCoroutine(AnimateDigits());
    private IEnumerator AnimateDigits()
    {
        float timer = 0f;

        // hold digits in place for hold duration
        yield return new WaitForSeconds(holdDuration);

        // fade out digits' alpha over fade duration
        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - (timer / fadeDuration);

            foreach (var digit in allDigits)
                digit.SetAlpha(alpha);

            yield return null;
        }

        Destroy(gameObject);
    }
}