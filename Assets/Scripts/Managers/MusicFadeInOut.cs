using UnityEngine;
using System.Collections;


public class MusicFadeInOut : MonoBehaviour
{
    public static MusicFadeInOut instance;
    [HideInInspector] public AudioSource musicSource;
    private AudioClip currentMusic;
    private Coroutine musicFadeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            AudioSource source = GetComponent<AudioSource>();
            if (source != null)
                musicSource = source;
            else
                musicSource = gameObject.AddComponent<AudioSource>();
        }
        else
            Destroy(gameObject);
    }

    public void CheckMusic(AudioClip newMusic, float targetVolume)
    {
        if (newMusic != currentMusic)
        {
            currentMusic = newMusic;
            musicSource.clip = newMusic;

            if (musicFadeCoroutine != null)
                StopCoroutine(musicFadeCoroutine);

            musicFadeCoroutine = StartCoroutine(MusicFadeIn(musicSource, targetVolume, 1f));
        }
        else
        {
            if (musicFadeCoroutine != null)
                StopCoroutine(musicFadeCoroutine);

            musicFadeCoroutine = StartCoroutine(AdjustMusicVolume(targetVolume, 0.5f));
        }
    }

    public void PreTransitionCheckMusic(AudioClip newMusic)
    {
        if (newMusic != currentMusic)
            StartCoroutine(MusicFadeOut(musicSource, 1f));
    }

    public void StopMusic()
    {
        musicSource.Stop();
        currentMusic = null;
    }

    public IEnumerator MusicFadeOut(AudioSource musicSource, float fadeDuration)
    {
        float startVolume = musicSource.volume;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
    }

    public IEnumerator MusicFadeIn(AudioSource musicSource, float targetVolume, float fadeDuration)
    {
        musicSource.volume = 0f;
        musicSource.Play();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }


    /// <summary>
    /// adjusts music volume to target volume over fadeDuration w/o changing or restarting music clip
    /// </summary>
    /// <param name="targetVolume"></param>
    /// <param name="fadeDuration"></param>
    /// <returns></returns>
    private IEnumerator AdjustMusicVolume(float targetVolume, float fadeDuration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }
}

