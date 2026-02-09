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

    [Header("Portrait Emotion Labels")]
    public Sprite neutralLabel;
    public Sprite happyLabel;
    public Sprite sadLabel;
    public Sprite angryLabel;
    public Sprite afraidLabel;

    public void GetSprites(EmotionType emotion, out Sprite background, out Sprite label)
    {
        switch (emotion)
        {
            case EmotionType.Happy:
                background = happyBackground;
                label = happyLabel;
                break;

            case EmotionType.Sad:
                background = sadBackground;
                label = sadLabel;
                break;

            case EmotionType.Angry:
                background = angryBackground;
                label = angryLabel;
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
