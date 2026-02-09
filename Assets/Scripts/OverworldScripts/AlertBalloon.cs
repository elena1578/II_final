using UnityEngine;


[RequireComponent(typeof(Animator))]
public class AlertBalloon : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // called as an animation event at the end of the alert balloon animation
    public void OnAnimationFinished()
    {
        Destroy(gameObject);
    }
}
