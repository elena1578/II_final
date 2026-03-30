using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Collider2D))]
public class PicnicBasket : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float sfxWaitTime = 1f;  // time to wait for heal SFX to play before fading out
    private PlayerOverworldController currentPlayer;  // ref to player currently in range to interact w/ picnic basket

    private void Start()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerOverworldController player = other.GetComponent<PlayerOverworldController>();
        if (player != null)
        {
            currentPlayer = player;
            player.interactAction.performed += OnInteract;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerOverworldController player = other.GetComponent<PlayerOverworldController>();
        if (player != null && player == currentPlayer)
        {
            player.interactAction.performed -= OnInteract;
            currentPlayer = null;
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (currentPlayer != null)
            StartCoroutine(HealRoutine(currentPlayer));
    }

    private IEnumerator HealRoutine(PlayerOverworldController player)
    {
        player.DisablePlayerMovement();

        if (ScreenFade.instance != null)
        {
            // make sure fade image is white
            ScreenFade.instance.fadeCanvasGroup.GetComponentInChildren<UnityEngine.UI.Image>().color = Color.white;
            yield return ScreenFade.instance.FadeIn();
        }

        BattlePartyDataManager.HealEntireParty();
        AudioManager.instance.PlayHealSFX();
        yield return new WaitForSeconds(sfxWaitTime);

        // fade out & set color back to black once done
        if (ScreenFade.instance != null)
        {
            yield return ScreenFade.instance.FadeOut();
            ScreenFade.instance.fadeCanvasGroup.GetComponentInChildren<UnityEngine.UI.Image>().color = Color.black;
        }

        player.EnablePlayerMovement();
    }
}