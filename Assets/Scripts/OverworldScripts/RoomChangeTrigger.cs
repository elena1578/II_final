using UnityEngine;


public class RoomChangeTrigger : MonoBehaviour
{
    [Tooltip("Select the RoomID that this trigger should load when the player enters it.")]
    public RoomData.RoomID exitingTo;

    private PlayerOverworldController player;
    private bool currentlyTransitioning = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentlyTransitioning) return;
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player now exiting to: " + exitingTo);
            currentlyTransitioning = true;

            player = other.GetComponent<PlayerOverworldController>();
            if (player != null)
                player.DisablePlayerMovement();

            RoomData currentRoom = RoomManager.GetRoomFromActiveScene();
            if (currentRoom == null)
            {
                Debug.LogError("[RoomChangeTrigger] Could not determine current room from active scene");
                return;
            }

            RoomChangeManager.instance.InitializeAndGoToRoom(currentRoom.roomID, exitingTo);
        }
    }
}
