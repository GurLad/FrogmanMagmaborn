using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    private static SoundController soundController;
    public float Volume
    {
        get
        {
            return volume;
        }
        set
        {
            volume = value * baseVolume;
            for (int i = 0; i < audioSources.Count; i++)
            {
                audioSources[i].volume = Volume;
            }
            fixedPitchSource.volume = Volume;
        }
    }
    [SerializeField]
    [Range(0, 1)]
    private float volume;
    private List<AudioSource> audioSources = new List<AudioSource>();
    private AudioSource fixedPitchSource;
    private float baseVolume;

    public void Init()
    {
        for (int i = 0; i < 3; i++)
        {
            audioSources.Add(gameObject.AddComponent<AudioSource>());
        }
        fixedPitchSource = gameObject.AddComponent<AudioSource>();
        fixedPitchSource.pitch = 1;
        soundController = this;
        baseVolume = Volume;
        Volume = SavedData.Load("SFXOn", 1, SaveMode.Global) * baseVolume;
    }

    public static void PlaySound(AudioClip audioClip, bool stop = false)
    {
        if (audioClip == null)
        {
            return;
        }
        if (stop)
        {
            soundController.fixedPitchSource.Stop();
        }
        soundController.fixedPitchSource.PlayOneShot(audioClip);
    }

    public static void PlaySound(AudioClip audioClip, float pitch)
    {
        if (audioClip == null)
        {
            return;
        }
        AudioSource audioSource = null;
        for (int i = 0; i < 3; i++)
        {
            if (!soundController.audioSources[i].isPlaying)
            {
                audioSource = soundController.audioSources[i];
                break;
            }
        }
        if (audioSource == null)
        {
            audioSource = soundController.gameObject.AddComponent<AudioSource>();
            audioSource.volume = soundController.Volume;
            soundController.audioSources.Add(audioSource);
        }
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(audioClip);
    }

    public static void SetVolume(float value)
    {
        soundController.Volume = soundController.baseVolume * value;
    }

    public void EditorPlaySound(AudioClip audioClip, float pitch)
    {
        fixedPitchSource.pitch = pitch;
        fixedPitchSource.PlayOneShot(audioClip);
    }

    private void Update()
    {
        for (int i = 3; i < audioSources.Count; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                DestroyImmediate(audioSources[i]);
                audioSources.RemoveAt(i);
            }
        }
    }
}
