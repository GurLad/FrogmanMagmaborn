using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CAT = ConversationPlayer.CommandArgumentType;

public class ConversationPlayer : MidBattleScreen, ISuspendable<SuspendDataConversationPlayer>
{
    public enum CommandArgumentType { String, Int, Float, Bool, Team, AIType, OpString = 10, OpInt, OpFloat, OpBool, OpTeam, OpAIType } // Assume there aren't more than 10 types
    private enum CurrentState { Writing, Waiting, Sleep, Hold }
    private enum StartLineResult { None = 0, LoadMap = 1, LoadUnits = 2, Fade = 4, MidBattleScreen = 8, FinishLevel = 16, FinishConversation = 32, Wait = 64 }
    private enum PlayMode { PreBattle, MidBattle, PostBattle }

    public static ConversationPlayer Current;
    [Header("Stats")]
    public float LettersPerSecond;
    public int LineWidth = 22;
    public List<AudioClip> VoiceTypes;
    public float VoiceMod;
    public float PunctuationDelay;
    [Header("Objects")]
    public GameObject AllObjectsHolder;
    public RectTransform NameHolder;
    public Text Name;
    public Text Text;
    public PortraitHolder PortraitL;
    public PortraitHolder PortraitR;
    public GameObject Arrow;
    public MenuController ChoiceMenu;
    public MenuController InfoDialogue;
    public MenuController SkipDialogue;
    public CGController CGController;
    [Header("Palette stuff")]
    public PalettedSprite TextHolderPalette;
    public PalettedSprite NameHolderPalette;
    public PalettedSprite PortraitLHolderPalette;
    public PalettedSprite PortraitRHolderPalette;
    [Header("Main menu only")]
    public GameObject Knowledge;
    public MenuController Tutorial;
    public Intro Intro;
    [HideInInspector]
    public System.Action OnFinishConversation;
    [SerializeField]
    private bool startActive = true;
    private float speed;
    private CurrentState state;
    private ConversationData origin;
    private CharacterVoice voice;
    private bool playingVoice;
    private bool currentSpeakerIsLeft = false;
    private string speakerL = "";
    private string speakerR = "";
    private int currentLine;
    private int currentChar;
    private float count;
    private PlayMode playMode = PlayMode.PreBattle;
    private string targetLine;
    private string waitRequirement = "";
    private Stack<FunctionStackObject> functionStack = new Stack<FunctionStackObject>();
    private List<string> lines;
    private string[] previousLineParts;
    private bool skipping;
    private void Awake()
    {
        Current = this;
        gameObject.SetActive(startActive);
        PortraitR.Awake();
        PortraitR.gameObject.SetActive(false);
        PortraitL.Awake();
        PortraitL.gameObject.SetActive(false);
        CGController.Init();
    }
    private void Update()
    {
        if (origin != null)
        {
            if (skipping && state != CurrentState.Hold)
            {
                if (++currentLine >= lines.Count)
                {
                    FinishConversation();
                }
                else
                {
                    StartLine(currentLine);
                }
                return;
            }
            switch (state)
            {
                case CurrentState.Writing:
                    if (Control.GetButtonDown(Control.CB.A))
                    {
                        PlayLetter('m');
                        int aIndex = targetLine.IndexOf('\a', currentChar + 1);
                        string trueLine = aIndex > 0 ? targetLine.Substring(0, aIndex) : targetLine;
                        while (trueLine.Count(a => a == '\n') > 1)
                        {
                            int lengthReduce = trueLine.IndexOf('\n');
                            trueLine = trueLine.Substring(lengthReduce + 1);
                        }
                        Text.text = trueLine;
                        targetLine = (aIndex < targetLine.Length - 1 && aIndex > 0) ? targetLine : "";
                        currentChar = aIndex;
                        Arrow.SetActive(true);
                        state = CurrentState.Waiting;
                    }
                    else
                    {
                        count += Time.deltaTime * speed;
                        if (count >= 1)
                        {
                            if (++currentChar < targetLine.Length)
                            {
                                count -= 1;
                                if (targetLine[currentChar] == '\a')
                                {
                                    int aIndex = targetLine.IndexOf('\a');
                                    targetLine = (aIndex < targetLine.Length - 1 && aIndex > 0) ? targetLine : "";
                                    Arrow.SetActive(true);
                                    state = CurrentState.Waiting;
                                    return;
                                }
                                if (targetLine[currentChar] == '\n' && Text.text.Count(a => a == '\n') > 0)
                                {
                                    Text.text = Text.text.Split('\n')[1];
                                    int lengthReduce = targetLine.IndexOf('\n');
                                    targetLine = targetLine.Substring(lengthReduce);
                                    currentChar -= lengthReduce;
                                    count -= PunctuationDelay;
                                }
                                Text.text += targetLine[currentChar];
                                if (targetLine[currentChar] == ' ')
                                {
                                    char letter = targetLine[currentChar - 1];
                                    if (letter == '.' || letter == ',' || letter == '!' || letter == '?')
                                    {
                                        count -= PunctuationDelay * (letter == ',' ? 0.5f : 1);
                                    }
                                }
                                if (currentChar + 1 < targetLine.Length && targetLine[currentChar + 1] == '\n' && Text.text.Count(a => a == '\n') > 0)
                                {
                                    count -= PunctuationDelay;
                                }
                                PlayLetter(targetLine[currentChar]);
                            }
                            else
                            {
                                targetLine = "";
                                Arrow.SetActive(true);
                                state = CurrentState.Waiting;
                            }
                        }
                    }
                    break;
                case CurrentState.Waiting:
                    if (Control.GetButtonDown(Control.CB.A))
                    {
                        if (targetLine != "")
                        {
                            Arrow.SetActive(false);
                            state = CurrentState.Writing;
                        }
                        else if (++currentLine >= lines.Count)
                        {
                            FinishConversation();
                        }
                        else
                        {
                            StartLine(currentLine);
                        }
                    }
                    else if (Control.GetButtonDown(Control.CB.Start) && SkipDialogue != null)
                    {
                        Pause(false);
                        SkipDialogue.Begin();
                        SkipDialogue.SelectItem(1);
                    }
                    break;
                case CurrentState.Hold:
                    count -= Time.deltaTime;
                    if (count <= 0)
                    {
                        AllObjectsHolder.SetActive(true);
                        StartLine(currentLine + 1);
                    }
                    break;
                default:
                    break;
            }
        }
    }
    public void Play(ConversationData conversation, bool shouldFadeIn = true)
    {
        playMode = PlayMode.PreBattle;
        gameObject.SetActive(true);
        MidBattleScreen.Set(this, true);
        origin = conversation;
        lines = origin.Lines;
        speed = LettersPerSecond * (SavedData.Load("TextSpeed", 0, SaveMode.Global) + 1);
        StartLine(0, true, shouldFadeIn);
    }
    public void PlayOneShot(string text)
    {
        // Store current lines & position
        if (currentLine < (lines?.Count ?? 0))
        {
            functionStack.Push(new FunctionStackObject(currentLine, lines));
        }
        // Load new lines
        playMode = PlayMode.MidBattle;
        lines = new List<string>(text.Split('\n'));
        speed = LettersPerSecond * (SavedData.Load("TextSpeed", 0, SaveMode.Global) + 1);
        gameObject.SetActive(true);
        MidBattleScreen.Set(this, true);
        StartLine(0);
    }
    public void PlayPostBattle()
    {
        if (origin.PostBattleLines.Count <= 0)
        {
            GameController.Current.Win();
            return;
        }
        playMode = PlayMode.PostBattle;
        gameObject.SetActive(true);
        MidBattleScreen.Set(this, true);
        lines = origin.PostBattleLines;
        speed = LettersPerSecond * (SavedData.Load("TextSpeed", 0, SaveMode.Global) + 1);
        StartLine(0);
    }
    public void Pause(bool resetSpeakers = true)
    {
        gameObject.SetActive(false);
        MidBattleScreen.Set(this, false);
        if (resetSpeakers)
        {
            SetNoPortrait();
            currentSpeakerIsLeft = false;
        }
    }
    public void Wait(float seconds)
    {
        SoftResume();
        AllObjectsHolder.SetActive(false);
        state = CurrentState.Hold;
        count = seconds;
    }
    /// <summary>
    /// Resumes the conversations & plays the next line
    /// </summary>
    /// <param name="mod">Skip mod lines (0 for repeat last line, 1 for play next, 2 or more for skip lines)</param>
    /// <param name="fadeIn">If true, fades in the conversation before resuming, unless it's already over - in which case, finish it instead</param>
    public void Resume(int mod = 1, bool fadeIn = false)
    {
        SkipEmptyLines(currentLine + mod);
        if (currentLine >= lines.Count)
        {
            FinishConversation(fadeIn);
            return;
        }
        if (fadeIn)
        {
            FadeThisIn(() => Resume(0), false);
        }
        else
        {
            SoftResume();
            StartLine(currentLine);
        }
    }
    /// <summary>
    /// Resumes the conversation without modifying its state (aka after menus)
    /// </summary>
    public void SoftResume()
    {
        MidBattleScreen.Set(this, true);
        gameObject.SetActive(true);
        enabled = true;
    }
    public void Skip()
    {
        skipping = true;
        Resume();
    }
    /// <summary>
    /// Checks whether the wait requiremenet was met.
    /// </summary>
    /// <returns>True if it was (aka resumes conversation), false if it wasn't.</returns>
    public bool CheckWait()
    {
        if (!string.IsNullOrEmpty(waitRequirement) && origin.MeetsRequirement(waitRequirement))
        {
            waitRequirement = "";
            Resume();
            return true;
        }
        return false;
    }
    private void SkipEmptyLines(int startAt = -1)
    {
        currentLine = startAt >= 0 ? startAt : currentLine;
        string line;
        while (currentLine < lines.Count && ((line = lines[currentLine]).Length <= 0 || line[0] == '#' || line[0] == '}'))
        {
            currentLine++;
        }
    }
    private StartLineResult StartLine(int num, bool beforeBattleStart = false, bool shouldFadeIn = true)
    {
        System.Action<StartLineResult> delayedAction = null;

        StartLineResult StartLineTrue(int num)
        {
            StartLineResult result = StartLineResult.None;
            if (num >= lines.Count)
            {
                return result | FinishConversation();
            }
            currentLine = num;
            string line = lines[currentLine];
            if (line.Length <= 0) // Empty line
            {
                return result | StartLineTrue(num + 1);
            }
            int index;
            while ((index = line.IndexOf("[Name:")) >= 0) // Character display names (ex. "[Name:FrogmanMan]" would convert to "Frogman". For attacker, generic etc.)
            {
                //Bugger.Info(line);
                int lastIndex = line.IndexOf(']');
                if (lastIndex < 0 || lastIndex <= index)
                {
                    throw Bugger.Error("Bad [Name:] syntax: " + line);
                }
                string target = line.Substring(index + 6, lastIndex - index - 6);
                line = line.Replace("[Name:" + target + "]", PortraitController.Current.FindPortrait(target).TheDisplayName);
            }
            if (line[0] == ':') // Command
            {
                string[] parts = line.Split(':');
                // I need to add an empty "" arg at the end, for both ":loadMap" and ":loadMap:" to work
                string[] args = parts.Length > 2 ? (parts[parts.Length - 1] == "" ? new string[parts.Length - 2] : new string[parts.Length - 1]) : new string[1];
                if (args.Length > 0)
                {
                    // Array copy doesn't work for some reason?
                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = (i + 2 < parts.Length) ? parts[i + 2] : "";
                    }
                }
                switch (parts[1])
                {
                    // Level (gameplay) commands

                    case "addUnit":
                        // Params: string name
                        AssertCommand("addUnit", args, CAT.String);
                        GameController.Current.PlayerUnits.Add(GameController.Current.CreatePlayerUnit(args[0]));
                        break;
                    case "loadUnits":
                        // Params: string mapName = chosenMap, Team team = allTeams, bool keepPrevious = false
                        AssertCommand("loadUnits", args, CAT.OpString, CAT.OpTeam, CAT.OpBool);
                        if (parts.Length < 4)
                        {
                            GameController.Current.LoadLevelUnits(args[0]);
                        }
                        else if (parts.Length < 5)
                        {
                            GameController.Current.LoadLevelUnits(args[0], args[1].ToTeam());
                        }
                        else
                        {
                            GameController.Current.LoadLevelUnits(args[0], args[1].ToTeam(), args[2] == "T");
                        }
                        result |= StartLineResult.LoadUnits;
                        break;
                    case "loadMap":
                        // Params: string mapName = chosenMap
                        AssertCommand("loadMap", args, CAT.OpString);
                        GameController.Current.LoadMap(args[0]);
                        result |= StartLineResult.LoadMap;
                        break;
                    case "setTeam":
                        // Params: string unitName, Team changeTo
                        // Changes a unit's team
                        AssertCommand("setTeam", args, CAT.String, CAT.Team);
                        Unit target = GameController.Current.GetNamedUnit(args[0]);
                        if (target != null)
                        {
                            target.TheTeam = args[1].ToTeam() ?? target.TheTeam;
                            target.Moved = target.Moved;
                        }
                        else
                        {
                            throw Bugger.Error("No matching unit! (" + args[0] + ")");
                        }
                        break;
                    case "setBattleQuote":
                        // Params: string unitName, string functionName
                        // Set a unit's battle quote (aka add boss battle quote). Must use a function.
                        AssertCommand("setBattleQuote", args, CAT.String, CAT.String);
                        Unit target1 = GameController.Current.GetNamedUnit(args[0]);
                        if (target1 != null)
                        {
                            if (origin.Functions.ContainsKey(args[1]))
                            {
                                target1.BattleQuote = string.Join("\n", origin.Functions[args[1]]);
                                Bugger.Info(target1.BattleQuote);
                            }
                            else
                            {
                                throw Bugger.Error("No matching function! (" + args[1] + ")");
                            }
                        }
                        else
                        {
                            throw Bugger.Error("No matching unit! (" + args[0] + ")");
                        }
                        break;
                    case "setDeathQuote":
                        // Params: string unitName, string functionName
                        // Set a unit's death quote (retains bewtween chapters). Must use a function.
                        AssertCommand("setDeathQuote", args, CAT.String, CAT.String);
                        Unit target2 = GameController.Current.GetNamedUnit(args[0]);
                        if (target2 != null)
                        {
                            if (origin.Functions.ContainsKey(args[1]))
                            {
                                target2.DeathQuote = string.Join("\n", origin.Functions[args[1]]);
                            }
                            else
                            {
                                throw Bugger.Error("No matching function! (" + args[1] + ")");
                            }
                        }
                        else
                        {
                            throw Bugger.Error("No matching unit! (" + args[0] + ")");
                        }
                        break;
                    case "addSkill":
                        // Params: string unitName, string skillName
                        // Adds a skill to a unit.
                        AssertCommand("addSkill", args, CAT.String, CAT.String);
                        Unit target3 = GameController.Current.GetNamedUnit(args[0]);
                        if (target3 != null)
                        {
                            target3.AddSkill(args[1].ToSkill() ?? throw Bugger.Error("No matching skill! (" + args[1] + ")"));
                        }
                        else
                        {
                            throw Bugger.Error("No matching unit! (" + args[0] + ")");
                        }
                        break;
                    case "killUnit":
                        // Params: string unitName, bool showDeathQuote = true
                        // Kills a unit.
                        AssertCommand("killUnit", args, CAT.String, CAT.OpBool);
                        Unit target4 = GameController.Current.GetNamedUnit(args[0]);
                        if (target4 != null)
                        {
                            if (args.Length > 1 && args[1] == "F")
                            {
                                // Remove death quote before killing
                                target4.DeathQuote = "";
                            }
                            GameController.Current.KillUnit(target4);
                        }
                        else
                        {
                            throw Bugger.Error("No matching unit! (" + args[0] + ")");
                        }
                        break;
                    case "hideUnit":
                        // Params: string unitName
                        // Hides a unit - equivalent to pseudo-kill (aka when units die with permadeath off).
                        AssertCommand("hideUnit", args, CAT.String);
                        Unit target5 = GameController.Current.GetNamedUnit(args[0]);
                        if (target5 != null)
                        {
                            GameController.Current.PseudoKillUnit(target5);
                        }
                        else
                        {
                            throw Bugger.Error("No matching unit! (" + args[0] + ")");
                        }
                        break;
                    case "replaceUnit":
                        // Params: string oldUnit, string newUnit, bool keepHealth = false
                        // Kills oldUnit and spawns newUnit in its place
                        AssertCommand("replaceUnit", args, CAT.String, CAT.String);
                        Unit target6 = GameController.Current.GetNamedUnit(args[0]);
                        if (target6 != null)
                        {
                            Unit newUnit = GameController.Current.CreateUnit(args[1], target6.Level, target6.TheTeam, false);
                            newUnit.Pos = target6.Pos;
                            if (args.Length > 2 && args[2] == "T")
                            {
                                newUnit.Health = target6.Health;
                            }
                            target6.DeathQuote = ""; // Doesn't actually die, after all
                            GameController.Current.KillUnit(target6);
                        }
                        else
                        {
                            throw Bugger.Error("No matching unit! (" + args[0] + ")");
                        }
                        break;
                    case "killTeam":
                        // Params: string teamName
                        // Kills all units in a team.
                        AssertCommand("killTeam", args, CAT.Team);
                        GameController.Current.KillTeam(args[0].ToTeam() ?? throw Bugger.Error("No team!"));
                        break;
                    case "setTeamAI":
                        // Params: Team team, AIType ai
                        AssertCommand("setTeamAI", args, CAT.Team, CAT.AIType);
                        Team team = args[0].ToTeam() ?? throw Bugger.Error("No team!");
                        GameController.Current.AssignAIToTeam(team, args[1].ToAIType() ?? throw Bugger.Error("Impossible - I just validated..."));
                        break;
                    case "lose":
                        // Params: none
                        AssertCommand("lose", args);
                        if (GameController.Current != null)
                        {
                            GameController.Current.Lose();
                        }
                        else
                        {
                            SavedData.SaveAll(SaveMode.Slot);
                            SceneController.LoadScene("GameOver");
                        }
                        return result | StartLineResult.FinishLevel;
                    case "win":
                        // Params: none
                        AssertCommand("win", args);
                        GameController.Current.Win();
                        return result | StartLineResult.FinishLevel;

                    // Conversation (graphics, music etc.) commands

                    case "play":
                        // Params: string name, bool keepTimestamp = false
                        AssertCommand("play", args, CAT.String, CAT.OpBool);
                        CrossfadeMusicPlayer.Current.Play(args[0], parts.Length > 3 ? (args[1] == "T") : false);
                        break;
                    case "playIntro":
                        // Params: string name
                        AssertCommand("playIntro", args, CAT.String);
                        CrossfadeMusicPlayer.Current.PlayIntro(args[0], false);
                        break;
                    case "setMapTheme":
                        // Params: string name
                        AssertCommand("setMapTheme", args, CAT.String);
                        GameController.Current.LevelMetadata.MusicName = args[0]; // I really hope this doesn't break anything...
                        break;
                    case "addGenericCharacter":
                        // Params: string internalName, string forceTags = none
                        // Add to TempPortraits with args[0] internal name and args[1] tags
                        AssertCommand("addGenericCharacter", args, CAT.String, CAT.OpString);
                        PortraitController.Current.GeneratedGenericPortraits.Add(args[0], PortraitController.Current.FindGenericPortrait(args[1]));
                        break;
                    case "getGenericCharacter":
                        // Params: string internalName, Team fromTeam = null
                        AssertCommand("getGenericCharacter", args, CAT.String, CAT.OpTeam);
                        PortraitController.Current.GeneratedGenericPortraits.Add(args[0], GameController.Current.GetGenericPortrait(parts.Length > 3 ? args[1].ToTeam() : null));
                        break;
                    case "setSingleSpeaker":
                        // Params: bool left = true
                        // Removes all speakers and makes sure the next is on the left/right
                        AssertCommand("setSingleSpeaker", args, CAT.OpBool);
                        bool left = parts.Length > 2 ? args[0] == "L" : true;
                        SetSinglePortrait(left);
                        currentSpeakerIsLeft = !left;
                        break;
                    case "setSpeaker":
                        // Params: string speaker
                        // Displays the speaker without pausing (equivelant to "name|display|L/R: bla", without the text/pause)
                        AssertCommand("setSpeaker", args, CAT.String);
                        SetSpeakerFromText(args[0]);
                        break;
                    case "showCG":
                        // Params: string name
                        // Removes the previous CG (if any), then shows the requested CG until manually removed
                        AssertCommand("showCG", args, CAT.String);
                        Pause();
                        delayedAction = (result) =>
                        {
                            if (CGController.Active)
                            {
                                CGController.FadeOutCG(() => CGController.FadeInCG(args[0]));
                            }
                            else if (beforeBattleStart && shouldFadeIn)
                            {
                                PaletteController.PaletteControllerState currentPaletteState = PaletteController.Current.SaveState();
                                CGController.FadeInCG(args[0], currentPaletteState);
                            }
                            else
                            {
                                PaletteController.PaletteControllerState currentPaletteState = PaletteController.Current.SaveState();
                                PaletteController.Current.FadeOut(() => CGController.FadeInCG(args[0], currentPaletteState));
                            }
                        };
                        return result | StartLineResult.Fade;
                    case "hideCG":
                        // Params: none
                        // Removes the previous CG (if any)
                        AssertCommand("hideCG", args);
                        if (CGController.Active)
                        {
                            Pause();
                            CGController.FadeOutCG(() => Resume(1, true));
                        }
                        return result | StartLineResult.Fade;
                    case "screenShake":
                        // Params: float strength = 1, float duration = 1
                        // Shakes the screen for duartion time with strength amount
                        AssertCommand("screenShake", args, CAT.OpFloat, CAT.OpFloat);
                        float strength = parts.Length > 2 ? float.Parse(args[0] != "" ? args[0] : "0.5") : 0.5f;
                        float duration = parts.Length > 3 ? float.Parse(args[1] != "" ? args[1] : "0.5") : 0.5f;
                        CameraController.Current.ScreenShake(strength, duration);
                        break;
                    case "darkenScreen":
                        // Params: bool fixDoubleWhite = false
                        // Darkens all palettes by one stage. If fixDoubleWhite is on, darkens true white (0) twice.
                        AssertCommand("darkenScreen", args, CAT.OpBool);
                        PaletteController.Current.DarkenScreen(args.Length > 0 ? args[0] == "T" : false);
                        break;

                    // Show other screens (MidBattleScreens)

                    case "showInfoDialogue":
                        // Args: title
                        AssertCommand("showInfoDialogue", args, CAT.String);
                        Pause();
                        InfoDialogue.Text.text = args[0];
                        InfoDialogue.Begin();
                        return result | StartLineResult.MidBattleScreen;
                    case "showPartTitle":
                        // Args: subtitle, title
                        AssertCommand("showPartTitle", args, CAT.String, CAT.String);
                        Pause();
                        PartTitleAnimation partTitle = Instantiate(GameController.Current.PartTitle).GetComponentInChildren<PartTitleAnimation>();
                        partTitle.Begin(new List<string>(new string[] { args[0], args[1] }));
                        partTitle.transform.parent.gameObject.SetActive(false);
                        FadeThisOut(() => { partTitle.InitPalette(); partTitle.FadeThisIn(); }, null, false);
                        return result | StartLineResult.MidBattleScreen;
                    case "showChoice":
                        // Args: choosingCharacterName, option1, option2
                        AssertCommand("showChoice", args, CAT.String, CAT.String, CAT.String);
                        Pause();
                        gameObject.SetActive(true);
                        enabled = false;
                        Text.text = "";
                        Arrow.SetActive(false);
                        PortraitL.Portrait = PortraitController.Current.FindPortrait(Name.text = args[0]);
                        SetSinglePortrait(true);
                        SetSpeaker(true);
                        if (parts.Length != 5)
                        {
                            throw Bugger.Error("Currently, choices of more than 2 options aren't supported.");
                        }
                        ChoiceMenu.MenuItems[0].Text = args[1];
                        ChoiceMenu.MenuItems[1].Text = args[2];
                        // To prevent people from thinking a choice is "correct"
                        ChoiceMenu.Begin();
                        ChoiceMenu.SelectItem(Random.Range(0, 2));
                        return result | StartLineResult.MidBattleScreen;
                    case "showBase":
                        // Args: none
                        AssertCommand("showBase", args);
                        Pause();
                        BaseController baseController = Instantiate(GameController.Current.BaseMenu, GameController.Current.Canvas.transform).GetComponentInChildren<BaseController>();
                        baseController.Show(GameController.Current.PlayerUnits);
                        return result | StartLineResult.MidBattleScreen;

                    // Global commands

                    case "unlockKnowledge":
                        AssertCommand("unlockKnowledge", args, CAT.String);
                        GameCalculations.UnlockKnowledge(args[0]);
                        break;
                    case "setFlag":
                        AssertCommand("setFlag", args, CAT.String);
                        SavedData.Save("ConversationData", "Flag" + args[0], 1);
                        break;
                    case "setTempFlag":
                        // Params: name
                        AssertCommand("setTempFlag", args, CAT.String);
                        GameController.Current.TempFlags.Add(args[0]);
                        break;
                    case "markDone":
                        // Params: none
                        AssertCommand("markDone", args);
                        origin.Choose(true);
                        break;
                    case "setCounter":
                        // Params: string name, int amount
                        AssertCommand("setCounter", args, CAT.String, CAT.Int);
                        SavedData.Save("ConversationData", "Counter" + args[0], int.Parse(args[1]));
                        break;
                    case "addCounter":
                        // Params: string name, int amount
                        AssertCommand("addCounter", args, CAT.String, CAT.Int);
                        SavedData.Append("ConversationData", "Counter" + args[0], int.Parse(args[1]));
                        break;

                    // Syntax commands (ifs, functions...)

                    case "if":
                        /* Syntax:
                         * :if:hasFlag:bla{
                         * Firbell: Will happen if hasFlag (requirement)
                         * }
                         * :else:{
                         * Firbell: Will happen if !hasFlag
                         * }
                         * Firbell: Will anyway happen
                         */
                        // A bit too complex to assert for now
                        string requirement = line.Substring(line.IndexOf(':', 1) + 1);
                        requirement = requirement.Substring(0, requirement.IndexOf('{'));
                        if (!origin.MeetsRequirement(requirement))
                        {
                            num = SkipBlock(num);
                            // If found an else, do that content.
                            if (lines[num + 1].Contains(":else:"))
                            {
                                return result | StartLineTrue(num + 2);
                            }
                        }
                        break;
                    case "else":
                        // Reaching an else outside an if means that it wasn't taken, so just skip the block.
                        // A bit too complex to assert for now
                        num = SkipBlock(num);
                        break;
                    case "call":
                        AssertCommand("call", args, CAT.String);
                        if (origin.Functions.ContainsKey(args[0]))
                        {
                            // Store current lines & position
                            functionStack.Push(new FunctionStackObject(num, lines));
                            // Load new lines
                            lines = origin.Functions[args[0]];
                            return result | StartLineTrue(0);
                        }
                        throw Bugger.Error("No matching function! (" + args[0] + ")");
                    case "callOther":
                        AssertCommand("callOther", args, CAT.String);
                        // Store current lines & position
                        functionStack.Push(new FunctionStackObject(num, lines));
                        // Load new conversation
                        ConversationData conversation = ConversationController.Current.SelectConversationByID(args[0]);
                        if (conversation != null)
                        {
                            lines = playMode == PlayMode.PostBattle ? conversation.PostBattleLines : conversation.Lines;
                            return result | StartLineTrue(0);
                        }
                        throw Bugger.Error("No matching conversation! (" + args[0] + ")");
                    case "wait":
                        // Params: string[] requirement
                        // A bit too complex to assert for now
                        waitRequirement = line.Substring(line.IndexOf(':', 1) + 1);
                        Pause();
                        if (playMode != PlayMode.PostBattle)
                        {
                            CrossfadeMusicPlayer.Current.Play(GameController.Current.LevelMetadata.MusicName, false);
                        }
                        return result | StartLineResult.Wait;
                    case "return":
                        AssertCommand("return", args);
                        if (functionStack.Count == 0)
                        {
                            throw Bugger.Error("Nothing to return from!");
                        }
                        FunctionStackObject function = functionStack.Pop();
                        lines = function.Lines;
                        return result | StartLineTrue(function.LineNumber + 1);
                    case "finishConversation":
                        // Params: none
                        AssertCommand("finishConversation", args);
                        FinishConversation();
                        return result | StartLineResult.FinishConversation;

                    // Tutorial commands

                    case "tutorialForceButton":
                        // Not asserting tutorials for now
                        if (TutorialGameController.Current == null)
                        {
                            throw Bugger.Error("Don't use tutorial commands outside the tutorial");
                        }
                        TutorialGameController.ForceButton forceButton = new TutorialGameController.ForceButton();
                        forceButton.Move = System.Enum.TryParse(args[0], out forceButton.Button);
                        if (parts.Length > 3)
                        {
                            string[] pos = args[1].Split(',');
                            forceButton.Pos = new Vector2Int(int.Parse(pos[0]), int.Parse(pos[1]));
                            if (parts.Length > 4)
                            {
                                forceButton.WrongLine = int.Parse(args[2]);
                            }
                        }
                        TutorialGameController.Current.CurrentForceButton = forceButton;
                        TutorialGameController.Current.WaitingForForceButton = true;
                        Pause();
                        return result | StartLineResult.Wait;
                    case "tutorialShowMarker":
                        // Not asserting tutorials for now
                        if (TutorialGameController.Current == null)
                        {
                            throw Bugger.Error("Don't use tutorial commands outside the tutorial");
                        }
                        string[] markerPos = args[0].Split(',');
                        TutorialGameController.Current.ShowMarkerCursor(new Vector2Int(int.Parse(markerPos[0]), int.Parse(markerPos[1])));
                        break;
                    case "tutorialFinish":
                        // Not asserting tutorials for now
                        if (TutorialGameController.Current == null)
                        {
                            throw Bugger.Error("Don't use tutorial commands outside the tutorial");
                        }
                        SavedData.SaveAll(SaveMode.Slot);
                        SceneController.LoadScene("Map");
                        return result | StartLineResult.FinishLevel;
                    default:
                        break;

                    // Main menu commands

                    case "introShowCutscene":
                        // Params: none
                        if (Intro == null)
                        {
                            throw Bugger.Error("Don't use intro commands outside the intro");
                        }
                        AssertCommand("introShowCutscene", args);
                        Pause();
                        Intro.gameObject.SetActive(true);
                        return result | StartLineResult.MidBattleScreen;
                    case "introShowUpgradeMenu":
                        // Params: none
                        if (Knowledge == null)
                        {
                            throw Bugger.Error("Don't use intro commands outside the intro");
                        }
                        AssertCommand("introShowUpgradeMenu", args);
                        Pause();
                        Knowledge.SetActive(true);
                        return result | StartLineResult.MidBattleScreen;
                    case "introShowTutorial":
                        // Params: none
                        if (Tutorial == null)
                        {
                            throw Bugger.Error("Don't use intro commands outside the intro");
                        }
                        AssertCommand("introShowTutorial", args);
                        if (SavedData.Load("ConversationData", "FlagTutorialFinish", 0) == 0)
                        {
                            Pause();
                            Tutorial.Begin();
                            return result | StartLineResult.MidBattleScreen;
                        }
                        break;
                }
                return result | StartLineTrue(num + 1);
            }
            else if (line[0] == '#' || line[0] == '}') // Comment, like this one :) Or end of if block
            {
                return result | StartLineTrue(num + 1);
            }
            if (line.IndexOf(':') != -1)
            {
                string portraitText = line.Split(':')[0];
                SetSpeakerFromText(portraitText);
            }
            if (line.Contains("[")) // Variable name (like button name)
            {
                //Bugger.Info(line);
                line = line.Replace("[AButton]", Control.DisplayShortButtonName("A"));
                line = line.Replace("[BButton]", Control.DisplayShortButtonName("B"));
                line = line.Replace("[StartButton]", Control.DisplayShortButtonName("Start"));
                line = line.Replace("[Name]", Name.text);
                //Bugger.Info(line);
            }
            line = line.Replace(@"\a", "\a");
            // Find the line break
            string trueLine = FindLineBreaks(TrueLine(line));
            // Check if it's short (aka no line break) and had previous
            if (line.IndexOf(':') < 0 && LineAddition(trueLine) && previousLineParts != null)
            {
                targetLine = previousLineParts[previousLineParts.Length - 1] + '\n' + trueLine;
                currentChar = previousLineParts[previousLineParts.Length - 1].Length;
                Text.text = previousLineParts[previousLineParts.Length - 1] + targetLine[currentChar].ToString();
            }
            else
            {
                targetLine = trueLine;
                currentChar = 0;
                Text.text = targetLine[currentChar].ToString();
            }
            previousLineParts = trueLine.Split('\n');
            PlayLetter(targetLine[currentChar]);
            count = 0;
            state = CurrentState.Writing;
            Arrow.SetActive(false);
            return result;
        }

        StartLineResult result = StartLineTrue(num);
        if (beforeBattleStart)
        {
            if (GameController.Current != null)
            {
                if ((result & StartLineResult.LoadMap) == 0)
                {
                    GameController.Current.LoadMap();
                }
                if ((result & StartLineResult.LoadUnits) == 0)
                {
                    GameController.Current.LoadLevelUnits();
                }
            }
            if (shouldFadeIn && (result & StartLineResult.Fade) == 0)
            {
                Pause();
                PaletteController.Current.FadeIn(() => Resume(0));
            }
        }
        delayedAction?.Invoke(result);
        return result;
    }
    private StartLineResult FinishConversation(bool fadedOut = false)
    {
        // Check if this is the last part
        if (functionStack.Count > 0)
        {
            FunctionStackObject function = functionStack.Pop();
            lines = function.Lines;
            if (fadedOut)
            {
                FadeThisIn(() => { SoftResume(); StartLine(function.LineNumber + 1); }, playMode != PlayMode.PostBattle);
                return StartLineResult.Fade;
            }
            else
            {
                return StartLine(function.LineNumber + 1);
            }
        }
        // If there's a CG, hide it first
        if (CGController.Active)
        {
            Pause();
            CGController.FadeOutCG(() => FinishConversation(true));
            return StartLineResult.Fade;
        }
        if (fadedOut && playMode != PlayMode.PostBattle) // Fade in if this isn't the post-battle part
        {
            FadeThisIn(() => { Resume(); });
            return StartLineResult.Fade;
        }
        else if (!fadedOut && playMode == PlayMode.PostBattle) // Fade out if this is the post-battle part
        {
            gameObject.SetActive(false);
            FadeThisOut(() => { if (MidBattleScreen.HasCurrent) { MidBattleScreen.Set(this, false); } FinishConversation(true); }, null, false);
            return StartLineResult.Fade;
        }
        else if (playMode != PlayMode.PostBattle) // If this isn't the post-battle part, unset this mid-battle screen
        {
            if (MidBattleScreen.HasCurrent)
            { 
                MidBattleScreen.Set(this, false);
            }
        }
        // Finish conversation
        lines.Clear();
        gameObject.SetActive(false);
        state = CurrentState.Sleep;
        SetSinglePortrait(true);
        currentSpeakerIsLeft = false;
        previousLineParts = null;
        skipping = false;
        if (GameController.Current == null)
        {
            // Intro conversations
            origin.Choose(true);
            SavedData.SaveAll(SaveMode.Slot);
            SceneController.LoadScene("Map");
            return StartLineResult.FinishConversation;
        }
        else
        {
            // Battle conversations
            switch (playMode)
            {
                case PlayMode.PreBattle:
                    CrossfadeMusicPlayer.Current.Play(GameController.Current.LevelMetadata.MusicName, false);
                    GameController.Current.BeginBattle();
                    if (origin.PostBattleLines.Count <= 0)
                    {
                        origin.Choose(true);
                    }
                    else
                    {
                        origin.Choose(false);
                    }
                    break;
                case PlayMode.MidBattle:
                    CrossfadeMusicPlayer.Current.Play(GameController.Current.LevelMetadata.MusicName, false);
                    break;
                case PlayMode.PostBattle:
                    origin.Choose(true);
                    origin = null;
                    // Clear temp flags
                    GameController.Current.TempFlags.Clear();
                    GameController.Current.Win();
                    break;
                default:
                    break;
            }
        }
        // Clear TempPortraits & cleanup
        PortraitController.Current.GeneratedGenericPortraits.Clear();
        System.Action action = OnFinishConversation;
        action?.Invoke();
        OnFinishConversation = null;
        return StartLineResult.FinishConversation;
    }
    private string TrueLine(string line)
    {
        int index = line.IndexOf(':');
        line = line.Substring(index >= 0 ? index + 2 : 0);
        return line;
    }
    private string FindLineBreaks(string line)
    {
        int lineWidth = LineWidth;
        string cutLine = line;
        for (int i = line.IndexOf(' '); i > -1; i = cutLine.IndexOf(' ', i + 1))
        {
            int nextLength = cutLine.Substring(i + 1).Split(' ')[0].Length;
            int length = i + 1 + nextLength - cutLine.Substring(0, i + 1 + nextLength).Count(a => a == '\a');
            if (length > lineWidth)
            {
                //ErrorController.Info("Length (" + cutLine.Substring(0, i + 1) + "): " + (i + 1) + ", next word (" + cutLine.Substring(i + 1).Split(' ')[0] + "): " + nextLength + @", \a count: " + cutLine.Substring(0, i + 1 + nextLength).Count(a => a == '\a') + ", total: " + length + " / " + lineWidth);
                line = line.Substring(0, line.LastIndexOf('\n') + 1) + cutLine.Substring(0, i) + '\n' + cutLine.Substring(i + 1);
                i = 0;
                cutLine = line.Substring(line.LastIndexOf('\n') + 1);
            }
        }
        //ErrorController.Info(line);
        return line;
    }
    private int SkipBlock(int currentLine)
    {
        int numBrackets = 1;
        while (numBrackets > 0)
        {
            numBrackets -= lines[++currentLine] == "}" ? 1 : 0;
            numBrackets += lines[currentLine].Contains("{") ? 1 : 0;
        }
        return currentLine;
    }
    private bool LineAddition(string trueLine)
    {
        return /*trueLine.IndexOf('\n') < 0 &&*/ currentLine >= 1;
    }
    private void SetSpeakerFromText(string speakerText)
    {
        // Format: name|displayName|L/R
        string[] parts = speakerText.Split('|');
        Portrait portrait = PortraitController.Current.FindPortrait(parts[0]);
        if (parts.Length > 1 && parts[1] != "")
        {
            Name.text = parts[1];
        }
        else
        {
            Name.text = portrait.TheDisplayName;
        }
        bool left = (parts.Length > 2 && parts[2] != "") ? parts[2] == "L" : !currentSpeakerIsLeft;
        bool updatePortrait = (parts.Length > 3 && parts[3] != "") ? parts[3] == "T" : true;
        if (left)
        {
            if (updatePortrait)
            {
                PortraitL.Portrait = portrait;
            }
            speakerL = Name.text;
            SetSpeaker(true);
        }
        else
        {
            if (updatePortrait)
            {
                PortraitR.Portrait = portrait;
            }
            speakerR = Name.text;
            SetSpeaker(false);
        }
        voice = portrait.Voice;
    }
    private void SetSpeaker(bool left)
    {
        PortraitHolder target = left ? PortraitL : PortraitR;
        NameHolder.anchoredPosition = new Vector2(left ? 64 : 112, NameHolder.anchoredPosition.y);
        Name.text = left ? speakerL : speakerR;
        voice = target.Portrait?.Voice ?? null;
        TextHolderPalette.Palette = target.Portrait?.AccentColor ?? 0;
        NameHolderPalette.Palette = target.Portrait?.AccentColor ?? 0;
        target.gameObject.SetActive(true);
        if (left)
        {
            PortraitLHolderPalette.Palette = target.Portrait?.AccentColor ?? 0;
            PortraitRHolderPalette.Palette = 3;
        }
        else
        {
            PortraitRHolderPalette.Palette = target.Portrait?.AccentColor ?? 0;
            PortraitLHolderPalette.Palette = 3;
        }
        currentSpeakerIsLeft = left;
    }
    private void SetNoPortrait()
    {
        PortraitL.gameObject.SetActive(false);
        PortraitR.gameObject.SetActive(false);
        currentSpeakerIsLeft = false;
    }
    private void SetSinglePortrait(bool left)
    {
        PortraitL.gameObject.SetActive(left);
        PortraitR.gameObject.SetActive(!left);
        SetSpeaker(left);
    }
    private void PlayLetter(char letter)
    {
        letter = letter.ToString().ToLower()[0];
        if (letter < 'a' || letter > 'z' || playingVoice)
        {
            playingVoice = false;
            return;
        }
        if (letter >= 'y')
        {
            letter = 'x';
        }
        if (letter <= 'b')
        {
            letter = 'c';
        }
        //playingVoice = true;
        float voiceMod = ((letter - 'm') / 13) * VoiceMod;
        SoundController.PlaySound(VoiceTypes[(int)voice.VoiceType], voice.Pitch + voiceMod);
    }
    private void AssertCommand(string commandName, string[] args, params CAT[] commandArguments)
    {
        if (GameCalculations.Debug)
        {
            string errorMessage = "Incorrect arguemnts: " + commandName + " requires " + string.Join(":", commandArguments).Replace("Op", "(optional)") + " arguments - " + string.Join(":", args) + " is incompatible";
            // Since args always contains "" at the end, the last argument doesn't really exist
            if ((args.Length - 1) > commandArguments.Length || ((args.Length - 1) < commandArguments.Length && (int)commandArguments[args.Length - 1] < 10))
            {
                throw Bugger.Error(errorMessage);
            }
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (!MatchesCommandType(args[i], (CAT)((int)commandArguments[i] % 10)))
                {
                    throw Bugger.Error(errorMessage);
                }
            }
        }
    }
    private bool MatchesCommandType(string part, CAT command)
    {
        switch (command)
        {
            case CAT.String:
                return true;
            case CAT.Int:
                return int.TryParse(part, out _);
            case CAT.Float:
                return float.TryParse(part, out _);
            case CAT.Bool:
                return part.ToUpper() == "T" || part.ToUpper() == "F" || part.ToUpper() == "L" || part.ToUpper() == "R";
            case CAT.Team:
                return true; // Non-existant team is all teams
            case CAT.AIType:
                return part.ToAIType() != null;
            default:
                throw Bugger.Error("There's no command of type " + command + "!");
        }
    }

    public SuspendDataConversationPlayer SaveToSuspendData()
    {
        return new SuspendDataConversationPlayer(origin);
    }

    public void LoadFromSuspendData(SuspendDataConversationPlayer data)
    {
        origin = new ConversationData(data.Origin);
    }

    private class FunctionStackObject
    {
        public int LineNumber;
        public List<string> Lines;

        public FunctionStackObject(int lineNumber, List<string> lines)
        {
            LineNumber = lineNumber;
            Lines = lines;
        }
    }
}

[System.Serializable]
public class SuspendDataConversationPlayer // People cannot suspend in the middle of a conversation
{
    public ConversationData Origin;

    public SuspendDataConversationPlayer(ConversationData origin)
    {
        Origin = origin;
    }
}