using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class DarknessUI : MonoBehaviour
{
    public static DarknessUI instance;
    public Image darknessOverlayImage;
    public Image vignetteImage;
    public float transitionSpeed = 2f;
    public AudioClip darknessAmb;

    private Coroutine transitionRoutine;
    private float targetDarkness;
    private float targetVignette;
    private AudioSource darknessAudioSource;
    private float audioTimer;
    private float minPitch = 1f;
    private float maxPitch = 1f;


    private void Awake()
    {
        instance = this;
        darknessAudioSource = GetComponent<AudioSource>();
    }

    private void Update() => UpdateDarknessAudio();

    public void SetDarknessLevel(int level)
    {
        switch (level)
        {
            case 0:
                SetDarkness(level, 0f, 0f);
                break;

            case 1:
                SetDarkness(level, 0.3f, 0f);
                break;

            case 2:
                SetDarkness(level, 0.5f, 0.3f);
                break;

            case 3:
                SetDarkness(level, 0.65f, 0.6f);
                break;

            case 4:
                SetDarkness(level, 0.75f, 1f);
                break;
        }
    }

    public void GetDarknessMusicVolumeAndPitch(int level, out float volume, out float minPitch, out float maxPitch)
    {
        switch (level)
        {
            case 0:
                volume = 0f;
                minPitch = 1f;
                maxPitch = 1f;
                break;

            case 1:
                volume = 0.09f;
                minPitch = 0.95f;
                maxPitch = 1.05f;
                break;

            case 2:
                volume = 0.13f;
                minPitch = 0.8f;
                maxPitch = 0.9f;
                break;

            case 3:
                volume = 0.17f;
                minPitch = 0.7f;
                maxPitch = 0.8f;
                break;

            case 4:
                volume = 0.21f;
                minPitch = 0.6f;
                maxPitch = 0.7f;
                break;

            default:
                volume = 0f;
                minPitch = 0.5f;
                maxPitch = 0.6f;
                break;
        }
    }

    private void SetDarkness(int level, float darkness, float vignette)
    {
        // visuals
        targetDarkness = darkness;
        targetVignette = vignette;

        // audio
        SetDarknessAudioSettings(level);

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        float startDarkness = darknessOverlayImage.color.a;
        float startVignette = vignetteImage.color.a;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;

            float darknessAlpha = Mathf.Lerp(startDarkness, targetDarkness, t);
            float vignetteAlpha = Mathf.Lerp(startVignette, targetVignette, t);

            if (darknessOverlayImage != null)
                darknessOverlayImage.color = new Color(0, 0, 0, darknessAlpha);

            if (vignetteImage != null)
                vignetteImage.color = new Color(1, 1, 1, vignetteAlpha);

            yield return null;
        }

        darknessOverlayImage.color = new Color(0, 0, 0, targetDarkness);
        vignetteImage.color = new Color(1, 1, 1, targetVignette);
        transitionRoutine = null;
    }

    private void SetDarknessAudioSettings(int level)
    {
        // set clip & play
        if (darknessAudioSource == null || darknessAmb == null) return;
        if (darknessAudioSource.clip != darknessAmb)
        {
            darknessAudioSource.clip = darknessAmb;
            darknessAudioSource.loop = true;
            darknessAudioSource.Play();
            audioTimer = 0f;
        }

        // assign vol/pitch depending on darkness int
        GetDarknessMusicVolumeAndPitch(level, out float volume, out float minP, out float maxP);
        darknessAudioSource.volume = volume;

        // store pitch range from level for oscillation
        minPitch = minP;
        maxPitch = maxP;
    }

    private void UpdateDarknessAudio()
    {
        if (darknessAudioSource == null || darknessAudioSource.clip == null) return;

        audioTimer += Time.deltaTime;

        // panning
        float pan = Mathf.Sin(audioTimer * 0.5f) * 0.5f;
        if (darknessAudioSource.volume != 0f)
            darknessAudioSource.panStereo = pan;

        // pitch oscillation
        float pitchRange = maxPitch - minPitch;
        darknessAudioSource.pitch = minPitch + Mathf.Abs(Mathf.Sin(audioTimer * 0.5f)) * pitchRange;
    }
}