using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConversationData : System.IComparable<ConversationData>
{
    public List<string> Requirements { get; } = new List<string>(); // Can add a class for that as well, but seems a bit of an overkill.
    public List<string> Demands { get; private set; } // See above.
    public List<string> Lines { get; private set; } // See above.
    public List<string> PostBattleLines { get; private set; }
    public Dictionary<string, List<string>> Functions;
    public bool Done { get; private set; }
    public string ID { get; private set; } = null;
    public string DisplayName { get; private set; } = null;
    // For suspend data
    [SerializeField]
    private string sourceText;
    [SerializeField]
    private string sourceID;
    private int priority;
    private bool unique;

    public ConversationData(TextFile sourceFile) : this(sourceFile.Text, sourceFile.Name) { }

    public ConversationData(ConversationData data) : this(data.sourceText, data.sourceID) { }

    public ConversationData(string text, string altID = "Temp")
    {
        sourceText = text;
        sourceID = altID;
        // I know that using JSON is techincally better, but I want to be able to create events using a simple text editor, so splits are simple.
        string source = text.Replace("\r", "").Replace('~', '\a').Replace(@"\w", "~");
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
                    ID = lineParts[1];
                    break;
                case "displayName":
                    DisplayName = lineParts[1];
                    break;
                default:
                    break;
            }
        }
        // Check unique & id
        ID ??= altID;
        DisplayName ??= ID;
        UpdateDone();
        // Requirements, demands, text and everything else
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
            if (parts.Length > 5)
            {
                PostBattleLines.RemoveAt(PostBattleLines.Count - 1);
                Functions = new Dictionary<string, List<string>>();
                for (int i = 5; i < parts.Length; i++)
                {
                    List<string> functionParts = new List<string>(parts[i].Split('\n'));
                    string functionName = functionParts[0].Trim();
                    functionParts.RemoveAt(0);
                    functionParts.RemoveAt(functionParts.Count - 1);
                    Functions.Add(functionName, functionParts);
                }
            }
        }
        else
        {
            PostBattleLines = new List<string>();
        }
    }

    // When I originally wrote this class, I commented on everything. Too bad future me isn't as patient.
    public bool MeetsRequirements()
    {
        if (Done) // Failsafe
        {
            Bugger.Warning("Checking requirements of a done conversation? (" + ID + ")");
            return false;
        }
        foreach (var requirement in Requirements)
        {
            if (!MeetsRequirement(requirement))
            {
                return false;
            }
        }
        return true;
    }

    public bool MeetsRequirement(string requirement)
    {
        if (requirement.Length < 1)
        {
            return true;
        }
        if (requirement[0] == '!')
        {
            return !CheckRequirements(requirement.Substring(1));
        }
        else
        {
            return CheckRequirements(requirement);
        }
    }

    private bool CheckRequirements(string requirement)
    {
        string[] parts = requirement.Split(':');
        switch (parts[0])
        {
            // Save file requirements

            case "hasKnowledge":
                // Return if has knowledge upgrade, based on internal name (ex. InclinationFrogman)
                return GameCalculations.HasKnowledge(parts[1]);
            case "foundKnowledge":
                // Return if found knowledge upgrade, based on internal name (ex. InclinationFrogman)
                return GameCalculations.FoundKnowledge(parts[1]);
            case "hasFlag":
                // Return whether a conversation flag is turned on
                return SavedData.Load("ConversationData", "Flag" + parts[1], 0) == 1;
            case "compareCounter":
                // Return whether a conversation counter matches the given value
                return MeetsComparisonRequirement(parts[2][0], SavedData.Load("ConversationData", "Counter" + parts[1], 0), int.Parse(parts[2].Substring(1)));
            case "numRuns":
                // Return whether a certain number of runs was reached.
                return MeetsComparisonRequirement(parts[1][0], SavedData.Load<int>("NumRuns"), int.Parse(parts[1].Substring(1)));
            case "furthestLevel":
                // Return whether a the fursthest level was reached.
                return MeetsComparisonRequirement(parts[1][0], SavedData.Load<int>("FurthestLevel"), int.Parse(parts[1].Substring(1)));
            case "firstTime":
                // Return whether the pre-battle part of the conversation was played
                return SavedData.Load<int>("ConversationData", "ID" + ID) == 0;
            case "finishedConversation":
                // Return whether the whole conversation was played
                return SavedData.Load<int>("ConversationData", "ID" + ID) == 2;

            // Current run requirements

            case "hasCharacter":
                // Check, return false if false.
                return GameController.Current.PlayerUnits.Find(a => a.Name == parts[1]) != null;
            case "hadCharacter":
                // Check whether the given character died
                return GameController.Current.DeadPlayerUnits.Contains(parts[1]);
            case "charactersAlive":
                // Format: charactersAlive:?X, ex. charactersAlive:>2
                return MeetsComparisonRequirement(parts[1][0], GameController.Current.PlayerUnits.FindAll(a => a.Name != StaticGlobals.MainCharacterName).Count, int.Parse(parts[1].Substring(1)));
            case "levelNumber":
                // Will also have a X-Y format, for specific areas/specific part of the game (1-3,2-7 etc.)
                return int.Parse(parts[1]) == GameController.Current.LevelNumber;

            // Mid-battle requirements

            case "turn":
                // Return whether a certain turn has passed. Obviously only works in "wait" commands.
                return MeetsComparisonRequirement(parts[1][0], GameController.Current.Turn, int.Parse(parts[1].Substring(1)));
            case "unitAlive":
                // Return whether a certain unit is alive.
                return GameController.Current.CheckUnitAlive(parts[1]);
            case "countUnits":
                // Compares the amount of units alive with the given name to the given value.
                return MeetsComparisonRequirement(parts[2][0], GameController.Current.GetNamedUnits(parts[1]).Count, int.Parse(parts[2].Substring(1)));
            case "teamUnitsAlive":
                // Compares the amount of living units in the given team to the given number.
                // Params: team:?value
                return MeetsComparisonRequirement(parts[2][0], GameController.Current.CountUnitsAlive(parts[1].ToTeam()), int.Parse(parts[2].Substring(1)));
            case "chose":
                // Return whether the last choice matches the given id.
                return SavedData.Load<int>("ConversationData", "ChoiceResult") == int.Parse(parts[1]);
            case "hasTempFlag":
                // Return whether a temp conversation flag is turned on
                return GameController.Current.TempFlags.Contains(parts[1]);
            case "teamMaxPos":
                // Compaers the max pos of a unit in the given team to the given value (x or y)
                // Params: x/y:team:?value
                return MeetsComparisonRequirement(parts[3][0], GameController.Current.FindMinMaxPosUnit(parts[2].ToTeam(), parts[1] == "x", true), int.Parse(parts[3].Substring(1)));
            case "teamMinPos":
                // Compaers the min pos of a unit in the given team to the given value (x or y)
                // Params: x/y:team:?value
                return MeetsComparisonRequirement(parts[3][0], GameController.Current.FindMinMaxPosUnit(parts[2].ToTeam(), parts[1] == "x", false), int.Parse(parts[3].Substring(1)));

            // Misc

            case "stringEquals":
                // Return the two given strings are equal
                return parts[1] == parts[2];

            // Endgame

            case "endgameCompareLastSummonMode":
                // Compares the given var with the last endgame summon mode (Magmaborn = 0, DeadBoss = 1, Generic = 2, Monster = 3
                return MeetsComparisonRequirement(parts[1][0], EndgameSummoner.Current.LastSummonMode, int.Parse(parts[1].Substring(1)));
            default:
                break;
        }
        return true;
    }

    private bool MeetsComparisonRequirement(char comparisonType, int valueL, int valueR)
    {
        switch (comparisonType)
        {
            case '>':
                return valueL > valueR;
            case '<':
                return valueL < valueR;
            case '=':
                return valueL == valueR;
            default:
                break;
        }
        throw Bugger.Error("No/wrong sign! " + comparisonType + " should be >/</=");
    }

    public int CompareTo(ConversationData other)
    {
        return -priority.CompareTo(other.priority);
    }

    public bool Choose(bool postBattle, bool affectNonUnique = false)
    {
        if (unique || affectNonUnique)
        {
            SavedData.Save("ConversationData", "ID" + ID, Mathf.Max(SavedData.Load<int>("ConversationData", "ID" + ID), postBattle ? 2 : 1));
            return true;
        }
        return false;
    }

    public void UpdateDone()
    {
        Done = unique && SavedData.Load<int>("ConversationData", "ID" + ID) == 2;
    }

    public override string ToString()
    {
        return ID;
    }
}