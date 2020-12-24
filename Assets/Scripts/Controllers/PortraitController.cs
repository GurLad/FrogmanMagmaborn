using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum VoiceType { Square50, Square25, Square12, Triangle }
public class PortraitController : MonoBehaviour
{
    public static PortraitController Current;
    [Header("Portraits")]
    public List<Portrait> Portraits;
    public List<GenericPortrait> GenericPortraits;
    // In the future, may add other way to identify name groups (ex. names for guards, names for males/females, names for Tormen etc.)
    public List<GenericCharacterVoice> GenericVoicesAndNames;
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
        // Process portraits
        for (int i = 0; i < Portraits.Count; i++)
        {
            Portraits[i].Voice = CharacterVoices.Find(a => a.Name == Portraits[i].Name) ?? ErrorVoice;
        }
        // Process generic names
        for (int i = 0; i < GenericVoicesAndNames.Count; i++)
        {
            GenericVoicesAndNames[i].Init();
        }
        // Process generic portraits
        for (int i = 0; i < GenericPortraits.Count; i++)
        {
            GenericPortraits[i].Init();
        }
    }
    public Portrait FindPortrait(string name)
    {
        return Portraits.Find(a => a.Name == name) ?? (TempPortraits.ContainsKey(name) ? TempPortraits[name] :  ErrorPortrait);
    }
    public Portrait FindGenericPortrait(string tags = "")
    {
        // Find portraits with matching tags
        List<GenericPortrait> genericPortraits;
        if (tags != "")
        {
            List<string> splitTags = new List<string>(tags.Split(','));
            genericPortraits = GenericPortraits.FindAll(a => splitTags.All(b => a.Tags.Contains(b)));
            Debug.Log("Tags are " + tags + ", found " + string.Join(", ", genericPortraits));
        }
        else
        {
            genericPortraits = GenericPortraits;
        }
        // Select portrait
        return genericPortraits[Random.Range(0, genericPortraits.Count)].ToPortrait();
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
    [HideInInspector]
    public CharacterVoice Voice;
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
    [SerializeField]
    private string tags;
    public int NameValuesID;
    public Sprite Background;
    public Sprite Foreground;
    public List<Palette> PossibleBackgroundColors = new List<Palette>();
    [HideInInspector]
    public List<string> Tags;
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

    public void Init()
    {
        Tags = new List<string>(tags.Split(','));
    }

    public Portrait ToPortrait()
    {
        Portrait portrait = new Portrait();
        portrait.Voice = PortraitController.Current.GenericVoicesAndNames[NameValuesID].ToVoice();
        portrait.Name = portrait.Voice.Name;
        portrait.Background = Background;
        portrait.Foreground = Foreground;
        portrait.BackgroundColor = PossibleBackgroundColors[Random.Range(0, PossibleBackgroundColors.Count)];
        portrait.ForegroundColorID = Random.Range(0, 4);
        return portrait;
    }

    public override string ToString()
    {
        return tags + ": " + Background.name;
    }
}

[System.Serializable]
public class CharacterVoice
{
    public string Name;
    public VoiceType VoiceType;
    [Range(0, 2)]
    public float Pitch = 1;
}

[System.Serializable]
public class GenericCharacterVoice
{
    [TextArea]
    [SerializeField]
    private string names;
    public List<VoiceType> AvailableVoiceTypes;
    public Vector2 PitchRange;
    [HideInInspector]
    public string[] Names;

    public void Init()
    {
        Names = names.Split(',');
    }

    public CharacterVoice ToVoice()
    {
        CharacterVoice voice = new CharacterVoice();
        voice.Name = Names[Random.Range(0, Names.Length)];
        voice.VoiceType = AvailableVoiceTypes[Random.Range(0, AvailableVoiceTypes.Count)];
        voice.Pitch = Random.Range(PitchRange.x, PitchRange.y);
        return voice;
    }
}