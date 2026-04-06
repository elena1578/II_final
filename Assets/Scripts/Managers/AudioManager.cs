using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private AudioSource musicSource;
    private AudioSource[] sfxSources;
    private int nextSFXIndex = 0;

    [Header("System")]
    public AudioClip mouseHover;
    public AudioClip mouseClick;
    public AudioClip back;
    public AudioClip error;

    [Header("Battle")]
    public AudioClip alert;
    public AudioClip battleStart;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;
    public AudioClip picnicHeal;
    public AudioClip healthHeal;
    public AudioClip statUp;
    public AudioClip statDown;
    public AudioClip statDownAlt;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length < 2)
        {
            Debug.LogError("AudioManager needs at least 1 music source and 1 SFX source to function properly");
            return;
        }

        musicSource = sources[0];  // source 0 = music
        bool musicInitialized = true;

        // 1+ = sfx sources
        sfxSources = new AudioSource[sources.Length - 1];
        for (int i = 1; i < sources.Length; i++)
        {
            sfxSources[i - 1] = sources[i];
        }

        Debug.Log($"[AudioManager] {sfxSources.Length} SFX sources initialized, music source initialized: {musicInitialized}");
    }

    
    #region Music
    public void PlayMusic(AudioClip clip) => MusicFadeInOut.instance?.CheckMusic(clip, 1f);
    public void StopMusic() => MusicFadeInOut.instance?.StopMusic();
    public void SetMusicVolume(float volume)
    {
        if (MusicFadeInOut.instance?.musicSource != null)
            MusicFadeInOut.instance.musicSource.volume = Mathf.Clamp01(volume);
    }
    #endregion

    
    #region SFX
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || sfxSources.Length == 0)
            return;

        AudioSource src = sfxSources[nextSFXIndex];
        nextSFXIndex = (nextSFXIndex + 1) % sfxSources.Length;  // cycle through sources to allow overlapping SFX

        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.pitch = Mathf.Clamp(pitch, 0.1f, 3f); 
        src.Play();
    }

    public void StopAllSFX()
    {
        foreach (var src in sfxSources)
            src.Stop();
    }

    public IEnumerator ResetSFXAfterClip(float delay, AudioSource src)
    {
        yield return new WaitForSeconds(delay);

        if (src != null)
        {
            src.volume = 1f;
            src.pitch = 1f;
        }
    }
    #endregion


    #region Specific Methods
    // a bit redundant but keeps calls consistent + allows for easy future adjustments
    public void OnButtonHover() => PlaySFX(mouseHover);
    public void PlaySelectSFX() => PlaySFX(mouseClick);
    public void PlayBackSFX() => PlaySFX(back);
    public void PlayErrorSFX() => PlaySFX(error);
    public void PlayHealSFX() => PlaySFX(picnicHeal);
    public void PlayBattleHealSFX() => PlaySFX(healthHeal);
    public void PlayStatUpSFX() => PlaySFX(statUp); 
    public void PlayStatDownSFX()
    {
        // randomly choose btwn 2 stat down SFX (equal chance for either)
        AudioClip chosenClip = Random.value < 0.5f ? statDown : statDownAlt;
        PlaySFX(chosenClip);
    }
    #endregion
}
