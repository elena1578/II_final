using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private List<AudioSource> sfxSources = new List<AudioSource>();
    private List<AudioSource> musicSources = new List<AudioSource>();
    private int nextSFXIndex = 0;

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
        // set order for audiosources
        // 0 = music, 1-3 = sfx
        AudioSource[] sources = GetComponents<AudioSource>();
        foreach (var src in sources)
        {
            if (src.outputAudioMixerGroup.name == "Music")
                musicSources.Add(src);
            else
                sfxSources.Add(src);
        }
    }

    // -----------------------------------------------------
    // music
    public void PlayMusic(AudioClip clip) => MusicFadeInOut.instance?.CheckMusic(clip, 1f);
    public void StopMusic() => MusicFadeInOut.instance?.StopMusic();
    public void SetMusicVolume(float volume)
    {
        if (MusicFadeInOut.instance?.musicSource != null)
            MusicFadeInOut.instance.musicSource.volume = Mathf.Clamp01(volume);
    }

    // -----------------------------------------------------
    // sfx
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        AudioSource src = sfxSources[nextSFXIndex];
        nextSFXIndex = (nextSFXIndex + 1) % sfxSources.Count;

        src.Stop(); // prevent leftover state
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
}
