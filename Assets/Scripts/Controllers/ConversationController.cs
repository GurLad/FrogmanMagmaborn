using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConversationController : MonoBehaviour
{
    public static ConversationController Current;
    public List<TextAsset> Conversations;
    [Header("Editor loader")]
    public string Path = "Conversations";
    private List<ConversationData> options;
    private void Awake()
    {
        Current = this;
        options = new List<ConversationData>();
        foreach (TextAsset conversation in Conversations)
        {
            options.Add(new ConversationData(conversation));
        }
        options = options.FindAll(a => !a.Done);
    }
    public ConversationData SelectConversation()
    {
        List<ConversationData> currentOptions = options.FindAll(a => a.MeetsRequirements());
        currentOptions.Sort();
        Debug.Log("Options: " + string.Join(", ", currentOptions));
        ConversationData chosen = currentOptions[0];
        //if (chosen.Choose())
        //{
        //    options.Remove(chosen);
        //}
        return chosen;
    }
    public void AutoLoad()
    {
        Conversations.Clear();
        string[] fileNames = UnityEditor.AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/Data/" + Path });
        foreach (string fileName in fileNames)
        {
            TextAsset file = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(UnityEditor.AssetDatabase.GUIDToAssetPath(fileName));
            Debug.Log(fileName + ", " + file.name);
            Conversations.Add(file);
        }
    }
}

public class ConversationData : IComparable<ConversationData>
{
    public List<string> Requirements { get; } = new List<string>(); // Can add a class for that as well, but seems a bit of an overkill.
    public List<string> Demands { get; private set; } // See above.
    public List<string> Lines { get; private set; } // See above.
    public List<string> PostBattleLines { get; private set; }
    public bool Done { get; private set; }
    private int priority;
    private bool unique;
    private string id = null;

    public ConversationData(TextAsset sourceFile)
    {
        // I know that using JSON is techincally better, but I want to be able to create events using a simple text editor, so splits are simple.
        string source = sourceFile.text.Replace("\r", "").Replace('~', '\a').Replace(@"\w", "~");
        string[] parts = source.Split('\a');
        string[] lines = parts[0].Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string[] lineParts = lines[i].Split(':');
            switch (lineParts[0])
            {
                case "priority":
                    priority = int.Parse(lineParts[1]);
                    break;
                case "unique":
                    unique = lineParts[1] == "T";
                    break;
                case "id":
                    id = lineParts[1];
                    break;
                default:
                    break;
            }
        }
        // Check unique & id
        id = id ?? sourceFile.name;
        if (unique && SavedData.Load<int>(id) == 1)
        {
            Done = true;
        }
        // Current approach, Requirments is a list of strings
        Requirements = new List<string>(parts[1].Split('\n'));
        Requirements.RemoveAt(0);
        Requirements.RemoveAt(Requirements.Count - 1);
        Demands = new List<string>(parts[2].Split('\n'));
        Demands.RemoveAt(0);
        Demands.RemoveAt(Demands.Count - 1);
        Lines = new List<string>(parts[3].Split('\n'));
        Lines.RemoveAt(0);
        if (parts.Length > 4)
        {
            Lines.RemoveAt(Lines.Count - 1);
            PostBattleLines = new List<string>(parts[4].Split('\n'));
            PostBattleLines.RemoveAt(0);
        }
        else
        {
            PostBattleLines = new List<string>();
        }
        // Alternate approach with custom classes. Currently theoretical.
        //lines = parts[1].Split('\n');
        //for (int i = 0; i < lines.Length; i++)
        //{
        //    string[] lineParts = lines[i].Split(':');
        //    Requirements.Add(new RequirementClass(lineParts[0], lineParts[1]));
        //}
    }
    // When I originally wrote this class, I commented on everything. Too bad future me isn't as patient.
    public bool MeetsRequirements()
    {
        if (Done) // Failsafe
        {
            Debug.LogWarning("Checking requirements of a done conversation? (" + id + ")");
            return false;
        }
        foreach (var requirement in Requirements)
        {
            if (requirement[0] == '!')
            {
                if (MeetsRequirement(requirement.Substring(1)))
                {
                    return false;
                }
            }
            else
            {
                if (!MeetsRequirement(requirement))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool MeetsRequirement(string requirement)
    {
        string[] parts = requirement.Split(':');
        switch (parts[0])
        {
            case "hasCharacter":
                // Check, return false if false.
                return GameController.Current.PlayerUnits.Find(a => a.Name == parts[1]) != null;
            case "charactersAlive":
                // Format: charactersAlive:?X, ex. charactersAlive:>2
                int targetNumber = int.Parse(parts[1].Substring(1));
                switch (parts[1][0])
                {
                    case '>':
                        return GameController.Current.PlayerUnits.FindAll(a => a.Name != "Frogman").Count > targetNumber;
                    case '<':
                        return GameController.Current.PlayerUnits.FindAll(a => a.Name != "Frogman").Count < targetNumber;
                    case '=':
                        return GameController.Current.PlayerUnits.FindAll(a => a.Name != "Frogman").Count == targetNumber;
                    default:
                        break;
                }
                break;
            case "roomNumber":
                // Will also have a X-Y format, for specific areas/specific part of the game (1-3,2-7 etc.)
                return int.Parse(parts[1]) == GameController.Current.LevelNumber;
            case "hasKnowledge":
                // Return if has knowledge upgrade, based on internal name (ex. InclinationFrogman)
                return KnowledgeController.HasKnowledge(parts[1]);
            case "hasFlag":
                // Return whether a conversation flag is turned on
                return SavedData.Load("Flag" + parts[1], 0) == 1;
            default:
                break;
        }
        return true;
    }

    public int CompareTo(ConversationData other)
    {
        if (priority.CompareTo(other.priority) != 0)
        {
            return -priority.CompareTo(other.priority);
        }
        else
        {
            return UnityEngine.Random.Range(-1, 2);
        }
    }

    public bool Choose()
    {
        if (unique)
        {
            SavedData.Save(id, 1);
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return id;
    }
}

/*
 * Concept:
 *  Each room (out of the 9 per run, plus maybe the BFOT ones) starts with a conversation, out of a pool of available ones for that specific room.
 *  Some events are global (can appear at any room), local (only appear in the Xth room of a run) or specific (only appear in a specific map).
 *  Those use the requirements of "roomNumber:X" and "map:ID".
 *  Read text files with the conversation information.
 *  Starts with priority and unique flag, then requirements (ex. hasCharacter, foughtCharacter...), then the event itself.
 * Priority & Unique:
 *  Unique events only appear once per save.
 *  If multiple events are possible, choose a random one from among the highest priority available ones.
 *  High priority should be tutorials/story-based events. Unique.
 *  Medium priority should be interactions between specific character/reactions to rare events. Mostly unique.
 *  Low priority should be filler conversation, in case no higher priority events are available/all of them appeared already. Not unique.
 *  Format (temporary?):
 *  "
 *      priority:int (higher number means higher priority)
 *      unique:T/F
 *      ~
 *  "
 * Requirements:
 *  The requirements for the event to appear.
 *  Mainly having participating characters, but could also be about whether the play defeated X, has a certain weapon, etc.
 *  Format (temporary?):
 *  "
 *      requirement:info
 *      requirement:info
 *      ...
 *      requirement:info
 *      ~
 *  "
 * Demands (new!):
 *  Some events only make sense in a specific type of maps. The demands is a list of requirments for the sort of maps this event can appear in.
 *  Most demands will be about the number of characters alive (ex. the third level of each area may allow you to recruit another character if you have less than X).
 *  Format (temporary?):
 *  "
 *      demand:info
 *      demand:info
 *      ...
 *      demand:info
 *      ~
 *  "
 * Event:
 *  Read line-by-line.
 *  When a character name appears ("name: text"), load its image until another name appears.
 *  Show a line until the player presses A, then show the next one (change image if necessary) and so on.
 *  Format (temporary?):
 *  "
 *      Name: Text.
 *      More text.
 *      Name 2: Even more text.
 *      ...
 *      Some text.
 *  "
 * Example:
 * "
 *  priority:3
 *  unique:T
 *  ~
 *  hasCharacter:Frogman
 *  ~
 *  charactersAlive:=0
 *  ~
 *  Frogman: Hello world!
 *  This is text!
 *  Enemy: Die!!!
 * "
 */
