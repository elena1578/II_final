using UnityEngine;
using System.Collections.Generic;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Battle SFX")]
    public AudioClip basicAttackSFX;

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
}
