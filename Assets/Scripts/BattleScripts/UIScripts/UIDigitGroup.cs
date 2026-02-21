using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIDigitGroup : MonoBehaviour
{
    [SerializeField] private UIDigit digitPrefab;
    [SerializeField] private RectTransform linePrefab;
    
    [Header("Animation Settings")]
    // [SerializeField] private float floatSpeed = 60f;
    // [SerializeField] private float lifetime = 1f;
    // [SerializeField] private float dropDistance = 40f;
    // [SerializeField] private float dropDuration = 0.25f;
    [SerializeField] private float holdDuration = 1.2f;
    [SerializeField] private float fadeDuration = 0.3f;

    private List<UIDigit> allDigits = new();

    public void AddNumber(int amount, Sprite[] spriteSet)
    {
        RectTransform line = Instantiate(linePrefab, transform);

        string number = amount.ToString();

        foreach (char c in number)
        {
            int digit = c - '0';

            UIDigit newDigit = Instantiate(digitPrefab, line);
            newDigit.SetSprite(spriteSet[digit]);

            allDigits.Add(newDigit);
        }
    }

    public void Play() => StartCoroutine(AnimateDigits());

    // private IEnumerator AnimateDigits()
    // {
    //     float timer = 0f;

    //     // cache starting positions for drop animation
    //     Dictionary<UIDigit, Vector3> startPositions = new();

    //     foreach (var digit in allDigits)
    //     {
    //         Vector3 start = digit.transform.localPosition + Vector3.up * dropDistance;
    //         digit.transform.localPosition = start;
    //         startPositions[digit] = start;
    //     }

    //     // 1. drop digits w/ slight per-digit variation 
    //     while (timer < dropDuration)
    //     {
    //         timer += Time.deltaTime;
    //         float t = Mathf.Clamp01(timer / dropDuration);

    //         foreach (var digit in allDigits)
    //         {
    //             float randomOffset = Mathf.Sin((t + digit.GetInstanceID()) * 10f) * 2f;

    //             Vector3 start = startPositions[digit];
    //             Vector3 end = start - Vector3.up * dropDistance;

    //             digit.transform.localPosition = Vector3.Lerp(start, end, t) + Vector3.up * randomOffset;
    //         }

    //         yield return null;
    //     }

    //     // ensure final position is set after drop
    //     foreach (var digit in allDigits)
    //     {
    //         digit.transform.localPosition = startPositions[digit] - Vector3.up * dropDistance;
    //     }

    //     // 2. hold digits in place for hold duration
    //     yield return new WaitForSeconds(holdDuration);

    //     // 3. fade out digits' alpha over fade duration
    //     timer = 0f;

    //     while (timer < fadeDuration)
    //     {
    //         timer += Time.deltaTime;
    //         float alpha = 1f - (timer / fadeDuration);

    //         foreach (var digit in allDigits)
    //             digit.SetAlpha(alpha);

    //         yield return null;
    //     }

    //     Destroy(gameObject);
    // }

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