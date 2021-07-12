using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationData : System.IComparable<ConversationData>
{
    public List<string> Requirements { get; } = new List<string>(); // Can add a class for that as well, but seems a bit of an overkill.
    public List<string> Demands { get; private set; } // See above.
    public List<string> Lines { get; private set; } // See above.
    public List<string> PostBattleLines { get; private set; }
    public Dictionary<string, List<string>> Functions;
    public bool Done { get; private set; }
    public string ID { get; private set; } = null;
    private int priority;
    private bool unique;

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
                    ID = lineParts[1];
                    break;
                default:
                    break;
            }
        }
        // Check unique & id
        ID = ID ?? sourceFile.name;
        if (unique && SavedData.Load<int>("ConversationData", "ID" + ID) == 2)
        {
            Done = true;
        }
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
            Debug.LogWarning("Checking requirements of a done conversation? (" + ID + ")");
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
            case "hasCharacter":
                // Check, return false if false.
                return GameController.Current.PlayerUnits.Find(a => a.Name == parts[1]) != null;
            case "charactersAlive":
                // Format: charactersAlive:?X, ex. charactersAlive:>2
                return MeetsComparisonRequirement(parts[1][0], GameController.Current.PlayerUnits.FindAll(a => a.Name != StaticGlobals.MAIN_CHARACTER_NAME).Count, int.Parse(parts[1].Substring(1)));
            case "roomNumber":
                // Will also have a X-Y format, for specific areas/specific part of the game (1-3,2-7 etc.)
                return int.Parse(parts[1]) == GameController.Current.LevelNumber;
            case "hasKnowledge":
                // Return if has knowledge upgrade, based on internal name (ex. InclinationFrogman)
                return GameCalculations.HasKnowledge(parts[1]);
            case "foundKnowledge":
                // Return if found knowledge upgrade, based on internal name (ex. InclinationFrogman)
                return GameCalculations.FoundKnowledge(parts[1]);
            case "hasFlag":
                // Return whether a conversation flag is turned on
                return SavedData.Load("ConversationData", "Flag" + parts[1], 0) == 1;
            case "numRuns":
                // Return whether a certain number of runs was reached.
                return MeetsComparisonRequirement(parts[1][0], SavedData.Load<int>("NumRuns"), int.Parse(parts[1].Substring(1)));
            case "firstTime":
                // Return whether the pre-battle part of the conversation was played
                return SavedData.Load<int>("ConversationData", "ID" + ID) == 0;
            case "finishedConversation":
                // Return whether the whole conversation was played
                return SavedData.Load<int>("ConversationData", "ID" + ID) == 2;

            // Mid-battle requirements

            case "turn":
                // Return whether a certain turn has passed. Obviously only works in "wait" commands.
                return MeetsComparisonRequirement(parts[1][0], GameController.Current.Turn, int.Parse(parts[1].Substring(1)));
            case "unitAlive":
                // Return whether a certain unit is alive.
                return GameController.Current.CheckUnitAlive(parts[1]);
            case "chose":
                // Return whether the last choice matches the given id.
                return SavedData.Load<int>("ConversationData", "ChoiceResult") == int.Parse(parts[1]);
            case "attacking":
                // Return whether a certain unit is being attacked.
                return GameController.Current.InteractState == InteractState.Attack && GameController.Current.Target.ToString() == parts[1];
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
        throw new System.Exception("No/wrong sign!");
    }

    public int CompareTo(ConversationData other)
    {
        return -priority.CompareTo(other.priority);
    }

    public bool Choose(bool postBattle)
    {
        if (unique)
        {
            SavedData.Save("ConversationData", "ID" + ID, postBattle ? 2 : 1);
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return ID;
    }
}