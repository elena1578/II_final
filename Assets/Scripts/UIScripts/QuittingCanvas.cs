using UnityEngine;
using UnityEngine.UI;
using System;


public class QuittingCanvas : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public static QuittingCanvas instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        GameManager.OnStartingQuitHold += ShowQuittingCanvas;
        GameManager.OnCancelingQuitHold += HideQuittingCanvas;
    }
    
    void Start()
    {
        // pause animation
        Animator animator = GetComponent<Animator>();
        if (animator != null)
            animator.speed = 0f;
    }

    private void ShowQuittingCanvas()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        // restart animation from beginning
        Animator animator = GetComponent<Animator>();
        if (animator != null)  
            animator.Play("quitting", -1, 0f);
    }

    private void HideQuittingCanvas()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }
}
