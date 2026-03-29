using UnityEngine;

[CreateAssetMenu(fileName = "EmotionUIVisuals", menuName = "Scriptable Objects/Battle/Emotion UI Visuals")]
public class EmotionUIVisuals : ScriptableObject
{
    [Header("Portrait Backgrounds")]
    public Sprite neutralBackground;
    public Sprite happyBackground;
    public Sprite sadBackground;
    public Sprite angryBackground;
    public Sprite afraidBackground;
    public Sprite ecstaticBackground;
    public Sprite depressedBackground;
    public Sprite enragedBackground;
    public Sprite manicBackground;
    public Sprite miserableBackground;
    public Sprite furiousBackground;

    [Header("Portrait Emotion Labels")]
    public Sprite neutralLabel;
    public Sprite happyLabel;
    public Sprite sadLabel;
    public Sprite angryLabel;
    public Sprite afraidLabel;
    public Sprite ecstaticLabel;
    public Sprite depressedLabel;
    public Sprite enragedLabel;
    public Sprite manicLabel;
    public Sprite miserableLabel;
    public Sprite furiousLabel;

    public void GetSprites(EmotionType emotion, int tier, out Sprite background, out Sprite label)
    {
        switch (emotion)
        {
            case EmotionType.Happy:
                background = tier == 2 ? ecstaticBackground : tier == 3 ? manicBackground : happyBackground;
                label = tier == 2 ? ecstaticLabel : tier == 3 ? manicLabel : happyLabel;
                break;

            case EmotionType.Sad:
                background = tier == 2 ? depressedBackground : tier == 3 ? miserableBackground : sadBackground;
                label = tier == 2 ? depressedLabel : tier == 3 ? miserableLabel : sadLabel;
                break;

            case EmotionType.Angry:
                background = tier == 2 ? enragedBackground : tier == 3 ? furiousBackground : angryBackground;
                label = tier == 2 ? enragedLabel : tier == 3 ? furiousLabel : angryLabel;
                break;

            case EmotionType.Afraid:
                background = afraidBackground;
                label = afraidLabel;
                break;

            default:
                background = neutralBackground;
                label = neutralLabel;
                break;
        }
    }
}
