using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CrossfadeMusicPlayerObject
{
    public string Name;
    public AudioClip AudioClip;
}

public class CrossfadeMusicPlayer : MonoBehaviour
{
    public static CrossfadeMusicPlayer Current;
    public List<CrossfadeMusicPlayerObject> Tracks;
    public float FadeSpeed;
    [Range(0,1)]
    public float Volume = 1;
    public bool PlayOnStart;
    public bool KeepTimestamp;
    public string Playing;
    private AudioSource mainAudioSource;
    private AudioSource seconderyAudioSource;
    private float count;
    private bool playingIntro;
    private bool playingBattle;
    private void Awake()
    {
        if (Current != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        else
        {
            Current = this;
        }
        DontDestroyOnLoad(gameObject);
        mainAudioSource = gameObject.AddComponent<AudioSource>();
        seconderyAudioSource = gameObject.AddComponent<AudioSource>();
        mainAudioSource.loop = seconderyAudioSource.loop = true;
        mainAudioSource.volume = Volume;
        seconderyAudioSource.volume = 0;
        if (PlayOnStart)
        {
            mainAudioSource.clip = Tracks[0].AudioClip;
            mainAudioSource.Play();
            Playing = Tracks[0].Name;
        }
        GetComponent<SoundController>().Init();
    }
    public void Play(string name, bool? keepTimestamp = null)
    {
        CrossfadeMusicPlayerObject target = Tracks.Find(a => a.Name == name);
        if (target == null)
        {
            throw Bugger.Error("No matching audio clip! (" + name + ")");
        }
        if (Playing == name)
        {
            return;
        }
        seconderyAudioSource.loop = true;
        playingIntro = false;
        playingBattle = false;
        seconderyAudioSource.clip = target.AudioClip;
        Playing = name;
        mainAudioSource.volume = Volume;
        seconderyAudioSource.volume = 0;
        count = 0;
        if (keepTimestamp ?? KeepTimestamp)
        {
            seconderyAudioSource.time = mainAudioSource.time * (seconderyAudioSource.clip.length / mainAudioSource.clip.length);
        }
        else
        {
            seconderyAudioSource.time = 0;
        }
        seconderyAudioSource.Play();
    }
    public void PlayIntro(string name, bool? keepTimestamp = null)
    {
        Play(name + "Intro", keepTimestamp);
        Playing = Playing.Replace("Intro", "");
        playingIntro = true;
        seconderyAudioSource.loop = false;
    }
    public void SwitchBattleMode(bool on)
    {
        if (on)
        {
            Play(Playing + "_Battle");
        }
        else
        {
            Play(Playing.Replace("_Battle", ""));
        }
        playingBattle = true;
    }
    private void Update()
    {
        if (seconderyAudioSource.clip != null)
        {
            count += Time.unscaledDeltaTime * FadeSpeed * (playingBattle ? 1 : 1);
            if (count >= 1)
            {
                AudioSource temp = mainAudioSource;
                mainAudioSource = seconderyAudioSource;
                seconderyAudioSource = temp;
                mainAudioSource.volume = Volume;
                seconderyAudioSource.volume = 0;
                seconderyAudioSource.clip = null;
                seconderyAudioSource.Stop();
                count = 0;
            }
            else
            {
                mainAudioSource.volume = Volume * (1 - count);
                seconderyAudioSource.volume = Volume * count;
            }
        }
        if (playingIntro && !mainAudioSource.isPlaying)
        {
            CrossfadeMusicPlayerObject target = Tracks.Find(a => a.Name == Playing);
            if (target == null)
            {
                throw Bugger.Error("No main track for the intro! (" + Playing + ")");
            }
            mainAudioSource.clip = target.AudioClip;
            mainAudioSource.Play();
            playingIntro = false;
            mainAudioSource.loop = true;
        }
    }
}
