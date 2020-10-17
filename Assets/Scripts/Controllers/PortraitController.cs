using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VoiceType { Square50, Square25, Square12, Triangle }
public class PortraitController : MonoBehaviour
{
    public static PortraitController Current;
    public List<Portrait> Portraits;
    public List<CharacterVoice> CharacterVoices;
    public Portrait ErrorPortrait;
    [Header("Error voice")]
    public CharacterVoice ErrorVoice;
    public List<AudioClip> DebugVoices;
    public AudioSource DebugSource;
    private void Reset()
    {
        Portraits.Add(new Portrait());
        DebugSource = GetComponent<AudioSource>();
    }
    private void Awake()
    {
        Current = this;
        Destroy(DebugSource);
        DebugVoices.Clear();
    }
    public Portrait FindPortrait(string name)
    {
        return Portraits.Find(a => a.Name == name) ?? ErrorPortrait;
    }
    public CharacterVoice FindVoice(string name)
    {
        return CharacterVoices.Find(a => a.Name == name) ?? ErrorVoice;
    }
}

[System.Serializable]
public class Portrait
{
    public string Name;
    public Sprite Background;
    public Sprite Foreground;
    public Palette BackgroundColor = new Palette();
    [Range(0, 3)]
    public int ForegroundColorID;
    public Portrait()
    {
        BackgroundColor = new Palette();
        for (int i = 0; i < 4; i++)
        {
            BackgroundColor.Colors[i] = Color.black;
        }
    }
}

[System.Serializable]
public class CharacterVoice
{
    public string Name;
    public VoiceType VoiceType;
    public float Pitch = 1;
}