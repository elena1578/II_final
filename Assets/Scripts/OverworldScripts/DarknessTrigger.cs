using UnityEngine;


public class DarknessTrigger : MonoBehaviour
{
    public int triggerNumber;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Debug.Log("Player entered darkness trigger " + triggerNumber);
            DarknessUI.instance.SetDarknessLevel(triggerNumber);
        }
    }
}