using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CAT = ConversationPlayer.CommandArgumentType;

public class ConversationPlayer : MidBattleScreen
{
    public enum CommandArgumentType { String, Int, Float, Bool, Team, AIType, OpString = 10, OpInt, OpFloat, OpBool, OpTeam, OpAIType } // Assume there aren't mroe than 10 types
    private enum CurrentState { Writing, Waiting, Sleep }

    public new static ConversationPlayer Current;
    [Header("Stats")]
    public float LettersPerSecond;
    public int LineWidth = 22;
    public List<AudioClip> VoiceTypes;
    public float VoiceMod;
    public float PunctuationDelay;
    [Header("Objects")]
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
    private bool postBattle = false;
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
    }
    private void Start()
    {
        PortraitR.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (origin != null)
        {
            if (skipping)
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
                    }
                    break;
                default:
                    break;
            }
        }
    }
    public void Play(ConversationData conversation)
    {
        postBattle = false;
        gameObject.SetActive(true);
        MidBattleScreen.Set(this, true);
        origin = conversation;
        if (GameController.Current != null)
        {
            if (origin.Lines.Find(a => a.Contains(":loadUnits")) == null)
            {
                GameController.Current.LoadLevelUnits();
            }
            if (origin.Lines.Find(a => a.Contains(":loadMap")) == null)
            {
                GameController.Current.LoadMap();
            }
        }
        lines = origin.Lines;
        speed = LettersPerSecond * (SavedData.Load("TextSpeed", 0, SaveMode.Global) + 1);
        StartLine(0);
    }
    public void PlayOneShot(string text)
    {
        // Store current lines & position
        if (currentLine < lines.Count)
        {
            functionStack.Push(new FunctionStackObject(currentLine, lines));
        }
        // Load new lines
        lines = new List<string>(text.Split('\n'));
        speed = LettersPerSecond * (SavedData.Load("TextSpeed", 0, SaveMode.Global) + 1);
        StartLine(0);
        gameObject.SetActive(true);
        MidBattleScreen.Set(this, true);
    }
    public void PlayPostBattle()
    {
        if (origin.PostBattleLines.Count <= 0)
        {
            GameController.Current.Win();
            return;
        }
        postBattle = true;
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
            SetSinglePortrait(true);
            currentSpeakerIsLeft = false;
        }
    }
    /// <summary>
    /// Resumes the conversations & plays the next line
    /// </summary>
    /// <param name="mod">Skip mod lines (0 for repeat last line, 1 for play next, 2 or more for skip lines)</param>
    public void Resume(int mod = 1)
    {
        SoftResume();
        if (currentLine + mod >= lines.Count)
        {
            FinishConversation();
            return;
        }
        StartLine(currentLine + mod);
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
    private void StartLine(int num)
    {
        if (num >= lines.Count)
        {
            FinishConversation();
            return;
        }
        currentLine = num;
        string line = lines[currentLine];
        if (line.Length <= 0) // Empty line
        {
            StartLine(num + 1);
            return;
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
            Bugger.Info("Args: " + '"' + string.Join(":", args) + '"');
            switch (parts[1])
            {
                // Level (gameplay) commands

                case "addUnit":
                    // Params: string name
                    AssertCommand("addUnit", args, CAT.String);
                    GameController.Current.PlayerUnits.Add(GameController.Current.CreatePlayerUnit(args[0]));
                    break;
                case "loadUnits":
                    // Params: string mapName = chosenMap, Team team = allTeams
                    AssertCommand("loadUnits", args, CAT.OpString, CAT.OpTeam);
                    if (parts.Length < 4)
                    {
                        GameController.Current.LoadLevelUnits(args[0]);
                    }
                    else
                    {
                        GameController.Current.LoadLevelUnits(args[0], args[1].ToTeam());
                    }
                    break;
                case "loadMap":
                    // Params: string mapName = chosenMap
                    AssertCommand("loadMap", args, CAT.OpString);
                    GameController.Current.LoadMap(args[0]);
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
                case "setTeamAI":
                    // Params: Team team, AIType ai
                    AssertCommand("setTeamAI", args, CAT.Team, CAT.AIType);
                    Team team = args[0].ToTeam() ?? throw Bugger.Error("No team!");
                    GameController.Current.AssignAIToTeam(team, args[1].ToAIType() ?? throw Bugger.Error("Impossible - I just validated..."));
                    break;
                case "lose":
                    // Params: none
                    AssertCommand("lose", args);
                    GameController.Current.Lose();
                    return;
                case "win":
                    // Params: none
                    AssertCommand("win", args);
                    GameController.Current.Win();
                    return;

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
                    CGController.HideCG(); // Reset saved palettes, just in case
                    CGController.ShowCG(args[0]);
                    break;
                case "hideCG":
                    // Params: none
                    // Removes the previous CG (if any)
                    AssertCommand("hideCG", args);
                    CGController.HideCG();
                    break;
                case "screenShake":
                    // Params: float strength = 1, float duration = 1
                    // Shakes the screen for duartion time with strength amount
                    AssertCommand("screenShake", args, CAT.Float, CAT.Float);
                    float strength = parts.Length > 2 ? float.Parse(args[0] != "" ? args[0] : "0.5") : 0.5f;
                    float duration = parts.Length > 3 ? float.Parse(args[1] != "" ? args[1] : "0.5") : 0.5f;
                    CameraController.Current.ScreenShake(strength, duration);
                    break;

                // Show other screens (MidBattleScreens)

                case "showInfoDialogue":
                    // Args: title
                    AssertCommand("showInfoDialogue", args, CAT.String);
                    Pause();
                    InfoDialogue.Text.text = args[0];
                    InfoDialogue.Begin();
                    return;
                case "showPartTitle":
                    // Args: subtitle, title
                    AssertCommand("showPartTitle", args, CAT.String, CAT.String);
                    Pause();
                    PartTitleAnimation partTitle = Instantiate(GameController.Current.PartTitle).GetComponentInChildren<PartTitleAnimation>();
                    partTitle.Begin(new List<string>(new string[] { args[0], args[1] }));
                    GameController.Current.TransitionToMidBattleScreen(partTitle);
                    return;
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
                    ChoiceMenu.SelectItem(Random.Range(0, 2));
                    ChoiceMenu.Begin();
                    return;

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
                            StartLine(num + 2);
                            return;
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
                        StartLine(0);
                        return;
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
                        lines = postBattle ? conversation.PostBattleLines : conversation.Lines;
                        StartLine(0);
                        return;
                    }
                    throw Bugger.Error("No matching conversation! (" + args[0] + ")");
                case "wait":
                    // Params: string[] requirement
                    // A bit too complex to assert for now
                    waitRequirement = line.Substring(line.IndexOf(':', 1) + 1);
                    Pause();
                    if (!postBattle)
                    {
                        CrossfadeMusicPlayer.Current.Play(GameController.Current.LevelMetadata.MusicName, false);
                    }
                    return;
                case "return":
                    AssertCommand("return", args);
                    if (functionStack.Count == 0)
                    {
                        throw Bugger.Error("Nothing to return from!");
                    }
                    FunctionStackObject function = functionStack.Pop();
                    lines = function.Lines;
                    StartLine(function.LineNumber + 1);
                    return;
                case "finishConversation":
                    // Params: none
                    AssertCommand("finishConversation", args);
                    FinishConversation();
                    return;

                // Tutorial commands

                case "tutorialForceButton":
                    // Not asserting tutorials for now
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
                    return;
                case "tutorialShowMarker":
                    // Not asserting tutorials for now
                    string[] markerPos = args[0].Split(',');
                    TutorialGameController.Current.ShowMarkerCursor(new Vector2Int(int.Parse(markerPos[0]), int.Parse(markerPos[1])));
                    break;
                case "tutorialFinish":
                    // Not asserting tutorials for now
                    SavedData.SaveAll(SaveMode.Slot);
                    SceneController.LoadScene("Map");
                    return;
                default:
                    break;
            }
            StartLine(num + 1);
            return;
        }
        else if (line[0] == '#' || line[0] == '}') // Comment, like this one :) Or end of if block
        {
            StartLine(num + 1);
            return;
        }
        if (line.IndexOf(':') != -1)
        {
            string portraitText = line.Split(':')[0];
            SetSpeakerFromText(portraitText);
        }
        if (line.Contains("[")) // Variable name (like button name)
        {
            Bugger.Info(line);
            line = line.Replace("[AButton]", Control.DisplayShortButtonName("A"));
            line = line.Replace("[BButton]", Control.DisplayShortButtonName("B"));
            line = line.Replace("[StartButton]", Control.DisplayShortButtonName("Start"));
            line = line.Replace("[Name]", Name.text);
            Bugger.Info(line);
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
    }
    private void FinishConversation()
    {
        // Check if this is the last part
        if (functionStack.Count > 0)
        {
            FunctionStackObject function = functionStack.Pop();
            lines = function.Lines;
            StartLine(function.LineNumber + 1);
            return;
        }
        // Finish conversation
        lines.Clear();
        MidBattleScreen.Set(this, false);
        gameObject.SetActive(false);
        state = CurrentState.Sleep;
        SetSinglePortrait(true);
        currentSpeakerIsLeft = false;
        previousLineParts = null;
        skipping = false;
        if (GameController.Current == null)
        {
            // Intro conversations
            if (postBattle || origin.PostBattleLines.Count <= 0)
            {
                origin.Choose(true);
                SavedData.SaveAll(SaveMode.Slot);
                if (SavedData.Load("ConversationData", "FlagTutorialFinish", 0) == 0)
                {
                    Tutorial.Begin();
                }
                else
                {
                    SceneController.LoadScene("Map");
                }
            }
            else
            {
                Knowledge.SetActive(true);
            }
            return;
        }
        else
        {
            // Battle conversations
            if (postBattle)
            {
                origin.Choose(true);
                origin = null;
                // Clear temp flags
                GameController.Current.TempFlags.Clear();
                GameController.Current.Win();
            }
            else
            {
                CrossfadeMusicPlayer.Current.Play(GameController.Current.LevelMetadata.MusicName, false);
                GameController.Current.AssignGenericPortraitsToUnits();
                if (origin.PostBattleLines.Count <= 0)
                {
                    origin.Choose(true);
                }
                else
                {
                    origin.Choose(false);
                }
            }
        }
        // Clear TempPortraits & cleanup
        PortraitController.Current.GeneratedGenericPortraits.Clear();
        System.Action action = OnFinishConversation;
        action?.Invoke();
        OnFinishConversation = null;
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
        bool left = parts.Length > 2 ? parts[2] == "L" : !currentSpeakerIsLeft;
        if (left)
        {
            PortraitL.Portrait = portrait;
            speakerL = Name.text;
            SetSpeaker(true);
        }
        else
        {
            PortraitR.Portrait = portrait;
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
