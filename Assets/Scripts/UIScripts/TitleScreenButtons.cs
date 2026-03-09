using UnityEngine;


public class TitleScreenButtons : MonoBehaviour
{
    public void OnNewGameButtonPressed() =>
        RoomChangeManager.instance.InitializeAndGoToRoom(RoomData.RoomID.TitleScreen_99, RoomData.RoomID.Entrance153_01);

    public void OnContinueButtonPressed()
    {
        // no save/load system (yet) (probably not happening) so just play error sfx & ensure button is disabled
        AudioManager.instance.PlayErrorSFX();
    }

    public void OnOptionsButtonPressed()
    {
        // placeholder for options menu functionality. for now, just play error
        AudioManager.instance.PlayErrorSFX();
    }
}
