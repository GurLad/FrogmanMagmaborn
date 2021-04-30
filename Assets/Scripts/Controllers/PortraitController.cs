using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PortraitController : MonoBehaviour
{
    public static PortraitController Current;
    [Header("Portraits")]
    public List<Portrait> Portraits;
    public List<GenericPortrait> GenericPortraits;
    // In the future, may add other way to identify name groups (ex. names for guards, names for males/females, names for Tormen etc.)
    public List<GenericCharacterVoice> GenericVoicesAndNames;
    public List<Palette> GenericPossibleBackgroundColors = new List<Palette>();
    public Portrait ErrorPortrait;
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
            //Debug.Log("Tags are " + tags + ", found " + string.Join(", ", genericPortraits));
        }
        else
        {
            genericPortraits = GenericPortraits;
        }
        // Select portrait
        return genericPortraits[Random.Range(0, genericPortraits.Count)].ToPortrait();
    }
    #if UNITY_EDITOR
    public void AutoLoad()
    {
        // Load jsons
        string json = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/Portraits.json").text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("Portraits"), this);
        json = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/GenericPortraits.json").text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("GenericPortraits"), this);
        json = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/GenericPortraitsGlobalData.json").text;
        JsonUtility.FromJsonOverwrite(json, this);
        // Load sprites
        for (int i = 0; i < Portraits.Count; i++)
        {
            Portraits[i].Foreground = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Data/Images/Portraits/" + Portraits[i].Name + "/F.png");
            Portraits[i].Background = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Data/Images/Portraits/" + Portraits[i].Name + "/B.png");
        }
        for (int i = 0; i < GenericPortraits.Count; i++)
        {
            GenericPortraits[i].Foreground = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Data/Images/GenericPortraits/" + GenericPortraits[i].Name + "/F.png");
            GenericPortraits[i].Background = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Data/Images/GenericPortraits/" + GenericPortraits[i].Name + "/B.png");
        }
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
    #endif
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
    public CharacterVoice Voice;
    [HideInInspector]
    public bool Assigned;
    public Portrait()
    {
        BackgroundColor = new Palette();
        for (int i = 0; i < 4; i++)
        {
            BackgroundColor.Colors[i] = Color.black;
        }
    }
    public override string ToString()
    {
        return '"' + Name + '"';
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
    // For FrogForge
    public string Name;
    [HideInInspector]
    public List<string> Tags;

    public void Init()
    {
        Tags = new List<string>(tags.Split(','));
    }

    public Portrait ToPortrait()
    {
        Portrait portrait = new Portrait();
        NamedVoice voice = PortraitController.Current.GenericVoicesAndNames[NameValuesID].ToVoice();
        portrait.Voice = voice;
        portrait.Name = voice.Name;
        portrait.Background = Background;
        portrait.Foreground = Foreground;
        portrait.BackgroundColor = PortraitController.Current.GenericPossibleBackgroundColors[Random.Range(0, PortraitController.Current.GenericPossibleBackgroundColors.Count)];
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
    public VoiceType VoiceType;
    [Range(0, 2)]
    public float Pitch = 1;
}

[System.Serializable]
public class NamedVoice : CharacterVoice
{
    public string Name;
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

    public NamedVoice ToVoice()
    {
        NamedVoice voice = new NamedVoice();
        voice.Name = Names[Random.Range(0, Names.Length)];
        voice.VoiceType = AvailableVoiceTypes[Random.Range(0, AvailableVoiceTypes.Count)];
        voice.Pitch = Random.Range(PitchRange.x, PitchRange.y);
        return voice;
    }
}