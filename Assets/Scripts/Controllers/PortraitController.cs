using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VoiceType { Square50, Square25, Square12, Triangle }
public class PortraitController : MonoBehaviour
{
    public static PortraitController Current;
    [Header("Portraits")]
    public List<Portrait> Portraits;
    public List<GenericPortrait> GenericPortraits;
    [SerializeField]
    private List<string> genericNames;
    public Portrait ErrorPortrait;
    [Header("Voices")]
    public List<CharacterVoice> CharacterVoices;
    [Header("Error voice")]
    public CharacterVoice ErrorVoice;
    [Header("Debug only")]
    public List<AudioClip> DebugVoices;
    public AudioSource DebugSource;
    // I just realized how many problems thus far could've been solved with a dictionary...
    [HideInInspector]
    public Dictionary<string, Portrait> TempPortraits = new Dictionary<string, Portrait>(); 
    // In the future, may add other way to identify name groups (ex. names for guards, names for males/females, names for Tormen etc.)
    [HideInInspector]
    public Dictionary<int, string[]> GenericNames = new Dictionary<int, string[]>();
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
        // Process generic names
        for (int i = 0; i < genericNames.Count; i++)
        {
            GenericNames.Add(i, genericNames[i].Split(','));
        }
    }
    public Portrait FindPortrait(string name)
    {
        return Portraits.Find(a => a.Name == name) ?? (TempPortraits.ContainsKey(name) ? TempPortraits[name] :  ErrorPortrait);
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
public class GenericPortrait
{
    public string Tags;
    public int NameValuesID;
    public Sprite Background;
    public Sprite Foreground;
    public List<Palette> PossibleBackgroundColors = new List<Palette>();
    // Currently, can always use every foreground palette. Replace when adding Tormen
    public GenericPortrait()
    {
        Palette first = new Palette();
        PossibleBackgroundColors.Add(first);
        for (int i = 0; i < 4; i++)
        {
            first.Colors[i] = Color.black;
        }
    }

    public Portrait ToPortrait()
    {
        Portrait portrait = new Portrait();
        string[] genericNamesOptions = PortraitController.Current.GenericNames[NameValuesID];
        portrait.Name = genericNamesOptions[Random.Range(0, genericNamesOptions.Length)];
        portrait.Background = Background;
        portrait.Foreground = Foreground;
        portrait.BackgroundColor = PossibleBackgroundColors[Random.Range(0, PossibleBackgroundColors.Count)];
        portrait.ForegroundColorID = Random.Range(0, 4);
        return portrait;
    }
}

[System.Serializable]
public class CharacterVoice
{
    public string Name;
    public VoiceType VoiceType;
    public float Pitch = 1;
}