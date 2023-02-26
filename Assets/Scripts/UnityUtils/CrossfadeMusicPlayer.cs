using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class CrossfadeMusicPlayerObject
{
    public string Name;
    public AudioClip AudioClip;

    public override string ToString()
    {
        return Name;
    }
}

public class CrossfadeMusicPlayer : MonoBehaviour
{
    public static CrossfadeMusicPlayer Current;
    public List<CrossfadeMusicPlayerObject> Tracks;
    public float FadeSpeed;
    public bool PlayOnStart;
    public bool KeepTimestamp;
    [SerializeField]
    [Range(0, 1)]
    private float volume;
    public float Volume
    {
        get
        {
            return volume;
        }
        set
        {
            volume = value * baseVolume;
            mainAudioSource.volume = Volume;
        }
    }
    [HideInInspector]
    public string Playing;
    private AudioSource mainAudioSource;
    private AudioSource seconderyAudioSource;
    private float count;
    private bool playingIntro;
    private bool playingBattle;
    private float baseVolume;

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
        baseVolume = Volume;
        Volume = SavedData.Load("MusicOn", 1, SaveMode.Global);
        mainAudioSource.volume = Volume;
        seconderyAudioSource.volume = 0;
        if (PlayOnStart)
        {
            mainAudioSource.clip = Tracks[0].AudioClip;
            mainAudioSource.Play();
            Playing = Tracks[0].Name;
        }
        GetComponent<SoundController>().Init();
        GetComponent<SystemSFXController>().Init();
    }

    public void Play(string name, bool? keepTimestamp = null)
    {
        if (Playing == name)
        {
            return;
        }
        CrossfadeMusicPlayerObject target = Tracks.Find(a => a.Name == name);
        if (target == null)
        {
            throw Bugger.Error("No matching audio clip! (" + name + ")");
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
            Bugger.Info("PLaying battle! " + Playing);
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

#if UNITY_EDITOR || MODDABLE_BUILD
    public void AutoLoad()
    {
        // Load musics json
        MusicDataHolder dataHolder = new MusicDataHolder();
        string json = FrogForgeImporter.LoadTextFile("Musics.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("Musics"), dataHolder);
#if UNITY_EDITOR
        Tracks.Clear();
#endif
        // Load music files (additive)
        foreach (MusicDataHolder.MusicData music in dataHolder.Musics)
        {
            //Bugger.Info("Begin music " + music.Name);
            CrossfadeMusicPlayerObject musicObject = Tracks.Find(a => a.Name == music.Name);
            if (musicObject == null)
            {
                musicObject = new CrossfadeMusicPlayerObject();
                musicObject.Name = music.Name;
                Tracks.Add(musicObject);
            }
            musicObject.AudioClip = FrogForgeImporter.LoadAudioFile("Musics/" + music.FullFileName);
        }
        //Bugger.Info("Finished loading! Musics: " + string.Join(", ", Tracks));
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }
#endif

    private class MusicDataHolder
    {
        public List<MusicData> Musics;

        [System.Serializable]
        public class MusicData
        {
            public string Name;
            public string FullFileName;
        }
    }
}
