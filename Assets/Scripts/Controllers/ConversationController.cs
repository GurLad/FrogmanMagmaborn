using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConversationController : MonoBehaviour
{
    public static ConversationController Current;
    public List<TextFile> Conversations;
    public bool IgnoreDoneConversations = true;
    public bool SetAsMain = true;
    [Header("Editor loader")]
    public string Path = "Conversations";
    private List<ConversationData> options;

    private void Awake()
    {
        if (SetAsMain)
        {
            Current = this;
        }
        options = new List<ConversationData>();
        foreach (TextFile conversation in Conversations)
        {
            options.Add(new ConversationData(conversation));
        }
        if (IgnoreDoneConversations)
        {
            options = options.FindAll(a => !a.Done);
        }
    }

    public List<ConversationData> GetAllOptions(bool includeAllPriorities = false)
    {
        if (!IgnoreDoneConversations)
        {
            options.ForEach(a => a.UpdateDone());
        }
        List<ConversationData> currentOptions = options.FindAll(a => a.MeetsRequirements());
        currentOptions.Sort();
        if (!includeAllPriorities)
        {
            currentOptions = currentOptions.FindAll(a => a.CompareTo(currentOptions[0]) == 0); // 0 is max, so can't be bigger anyway
        }
        Bugger.Info("GetAllOptions - they are: " + string.Join(", ", currentOptions));
        if (currentOptions.Count <= 0)
        {
            throw Bugger.Crash("Zero possible conversations!");
        }
        return currentOptions;
    }

    public ConversationData SelectConversation()
    {
        List<ConversationData> currentOptions = GetAllOptions();
        ConversationData chosen = currentOptions.RandomItemInList();
        return chosen;
    }

    public ConversationData SelectConversationByID(string id)
    {
        List<ConversationData> currentOptions = options.FindAll(a => a.ID == id);
        if (currentOptions.Count <= 0)
        {
            return null;
        }
        currentOptions.Sort();
        currentOptions = currentOptions.FindAll(a => a.CompareTo(currentOptions[0]) == 0); // 0 is max, so can't be bigger anyway
        Bugger.Info("SelectConversationByID - they are: " + string.Join(", ", currentOptions));
        ConversationData chosen = currentOptions.RandomItemInList();
        return chosen;
    }

#if UNITY_EDITOR || MODDABLE_BUILD
    public void AutoLoad()
    {
        Bugger.Info("Loading conversations from " + Path);
        Conversations.Clear();
        string[] fileNames = FrogForgeImporter.GetAllFilesAtPath(Path); // UnityEditor.AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/Data/" + Path });
        Bugger.Info("They are " + string.Join(", ", fileNames));
        foreach (string fileName in fileNames)
        {
            TextFile file = FrogForgeImporter.LoadTextFile(fileName, true);
            Conversations.Add(file);
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }
#endif
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
