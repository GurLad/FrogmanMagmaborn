using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ConversationPlayer : MidBattleScreen
{
    public enum CurrentState { Writing, Waiting, Sleep }
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
    private List<string> tempFlags = new List<string>();
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
                        count += Time.deltaTime * LettersPerSecond;
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
        StartLine(0);
    }
    public void Pause()
    {
        gameObject.SetActive(false);
        MidBattleScreen.Set(this, false);
        SetSinglePortrait(true);
        currentSpeakerIsLeft = false;
    }
    public void Resume(int mod = 1)
    {
        MidBattleScreen.Set(this, true);
        gameObject.SetActive(true);
        enabled = true;
        if (currentLine + mod >= lines.Count)
        {
            FinishConversation();
            return;
        }
        StartLine(currentLine + mod);
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
            switch (parts[1])
            {
                // Level (gameplay) commands

                case "addUnit":
                    // Params: string name
                    GameController.Current.PlayerUnits.Add(GameController.Current.CreatePlayerUnit(parts[2]));
                    break;
                case "loadUnits":
                    // Params: string mapName = chosenMap, Team team = allTeams
                    if (parts.Length < 4)
                    {
                        GameController.Current.LoadLevelUnits(parts[2]);
                    }
                    else
                    {
                        GameController.Current.LoadLevelUnits(parts[2], parts[3].ToTeam());
                    }
                    break;
                case "loadMap":
                    // Params: string mapName = chosenMap
                    GameController.Current.LoadMap(parts[2]);
                    break;
                case "setTeam":
                    // Params: string unitName, Team changeTo
                    // Changes a unit's team
                    Unit target = GameController.Current.GetNamedUnit(parts[2]);
                    if (target != null)
                    {
                        target.TheTeam = parts[3].ToTeam() ?? target.TheTeam;
                        target.Moved = target.Moved;
                    }
                    else
                    {
                        throw new System.Exception("No matching unit! (" + parts[2] + ")");
                    }
                    break;
                case "setBattleQuote":
                    // Params: string unitName, string functionName
                    // Set a unit's battle quote (aka add boss battle quote). Must use a function.
                    Unit target1 = GameController.Current.GetNamedUnit(parts[2]);
                    if (target1 != null)
                    {
                        if (origin.Functions.ContainsKey(parts[3]))
                        {
                            target1.BattleQuote = string.Join("\n", origin.Functions[parts[3]]);
                        }
                        else
                        {
                            throw new System.Exception("No matching function! (" + parts[3] + ")");
                        }
                    }
                    else
                    {
                        throw new System.Exception("No matching unit! (" + parts[2] + ")");
                    }
                    break;
                case "setTeamAI":
                    // Params: Team team, AIType ai
                    Team team = parts[2].ToTeam() ?? throw new System.Exception("No team!");
                    GameController.Current.AssignAIToTeam(team, parts[3].ToAIType());
                    break;
                case "lose":
                    // Params: none
                    GameController.Current.Lose();
                    return;
                case "win":
                    // Params: none
                    GameController.Current.Win();
                    return;

                // Conversation (graphics, music etc.) commands

                case "play":
                    // Params: string name, bool keepTimestamp = false
                    CrossfadeMusicPlayer.Current.Play(parts[2], parts.Length > 3 ? (parts[3] == "T") : false);
                    break;
                case "playIntro":
                    // Params: string name
                    CrossfadeMusicPlayer.Current.PlayIntro(parts[2], false);
                    break;
                case "addGenericCharacter":
                    // Params: string internalName, string forceTags = none
                    // Add to TempPortraits with parts[2] internal name and parts[3] tags
                    PortraitController.Current.GeneratedGenericPortraits.Add(parts[2], PortraitController.Current.FindGenericPortrait(parts[3]));
                    break;
                case "getGenericCharacter":
                    // Params: string internalName, Team fromTeam = null
                    PortraitController.Current.GeneratedGenericPortraits.Add(parts[2], GameController.Current.GetGenericPortrait(parts.Length > 3 ? parts[3].ToTeam() : null));
                    break;
                case "setSingleSpeaker":
                    // Params: bool left = true
                    // Removes all speakers and makes sure the next is on the left/right
                    bool left = parts.Length > 2 ? parts[2] == "L" : true;
                    SetSinglePortrait(left);
                    currentSpeakerIsLeft = !left;
                    break;
                case "showCG":
                    // Params: string name
                    // Removes the previous CG (if any), then shows the requested CG until manually removed
                    CGController.HideCG(); // Reset saved palettes, just in case
                    CGController.ShowCG(parts[2]);
                    break;
                case "hideCG":
                    // Params: string name
                    // Removes the previous CG (if any)
                    CGController.HideCG();
                    break;

                // Show other screens (MidBattleScreens)

                case "showInfoDialogue":
                    // Args: title
                    Pause();
                    InfoDialogue.Text.text = parts[2];
                    InfoDialogue.Begin();
                    return;
                case "showPartTitle":
                    // Args: subtitle, title
                    Pause();
                    PartTitleAnimation partTitle = Instantiate(GameController.Current.PartTitle).GetComponentInChildren<PartTitleAnimation>();
                    partTitle.Begin(new List<string>(new string[] { parts[2], parts[3] }));
                    GameController.Current.TransitionToMidBattleScreen(partTitle);
                    return;
                case "showChoice":
                    // Args: choosingCharacterName, option1, option2
                    Pause();
                    gameObject.SetActive(true);
                    enabled = false;
                    Text.text = "";
                    Arrow.SetActive(false);
                    PortraitL.Portrait = PortraitController.Current.FindPortrait(Name.text = parts[2]);
                    SetSinglePortrait(true);
                    SetSpeaker(true);
                    if (parts.Length != 5)
                    {
                        throw new System.Exception("Currently, choices of more than 2 options aren't supported.");
                    }
                    ChoiceMenu.MenuItems[0].Text = parts[3];
                    ChoiceMenu.MenuItems[1].Text = parts[4];
                    // To prevent people from thinking a choice is "correct"
                    ChoiceMenu.SelectItem(Random.Range(0, 2));
                    ChoiceMenu.Begin();
                    return;

                // Global commands

                case "unlockKnowledge":
                    GameCalculations.UnlockKnowledge(parts[2]);
                    break;
                case "setFlag":
                    SavedData.Save("ConversationData", "Flag" + parts[2], 1);
                    break;
                case "setTempFlag":
                    // Params: name
                    SavedData.Save("ConversationData", "TempFlag" + parts[2], 1);
                    tempFlags.Add(parts[2]);
                    break;
                case "markDone":
                    // Params: none
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
                    num = SkipBlock(num);
                    break;
                case "call":
                    // Store current lines & position
                    functionStack.Push(new FunctionStackObject(num, lines));
                    // Load new lines
                    if (origin.Functions.ContainsKey(parts[2]))
                    {
                        lines = origin.Functions[parts[2]];
                        StartLine(0);
                        return;
                    }
                    throw new System.Exception("No matching function! (" + parts[2] + ")");
                case "callOther":
                    // Store current lines & position
                    functionStack.Push(new FunctionStackObject(num, lines));
                    // Load new conversation
                    ConversationData conversation = ConversationController.Current.SelectConversationByID(parts[2]);
                    if (conversation != null)
                    {
                        lines = postBattle ? conversation.PostBattleLines : conversation.Lines;
                        StartLine(0);
                        return;
                    }
                    throw new System.Exception("No matching conversation! (" + parts[2] + ")");
                case "wait":
                    // Params: string[] requirement
                    waitRequirement = line.Substring(line.IndexOf(':', 1) + 1);
                    Pause();
                    if (!postBattle)
                    {
                        CrossfadeMusicPlayer.Current.Play(GameController.Current.LevelMetadata.MusicName, false);
                    }
                    return;
                case "return":
                    if (functionStack.Count == 0)
                    {
                        throw new System.Exception("Nothing to return from!");
                    }
                    FunctionStackObject function = functionStack.Pop();
                    lines = function.Lines;
                    StartLine(function.LineNumber + 1);
                    return;
                case "finishConversation":
                    // Params: none
                    FinishConversation();
                    return;

                // Tutorial commands

                case "tutorialForceButton":
                    TutorialGameController.ForceButton forceButton = new TutorialGameController.ForceButton();
                    forceButton.Move = System.Enum.TryParse(parts[2], out forceButton.Button);
                    if (parts.Length > 3)
                    {
                        string[] pos = parts[3].Split(',');
                        forceButton.Pos = new Vector2Int(int.Parse(pos[0]), int.Parse(pos[1]));
                        if (parts.Length > 4)
                        {
                            forceButton.WrongLine = int.Parse(parts[4]);
                        }
                    }
                    TutorialGameController.Current.CurrentForceButton = forceButton;
                    TutorialGameController.Current.WaitingForForceButton = true;
                    Pause();
                    return;
                case "tutorialShowMarker":
                    string[] markerPos = parts[2].Split(',');
                    TutorialGameController.Current.ShowMarkerCursor(new Vector2Int(int.Parse(markerPos[0]), int.Parse(markerPos[1])));
                    break;
                case "tutorialFinish":
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
            string[] parts = line.Split(':')[0].Split('|');
            Portrait portrait = PortraitController.Current.FindPortrait(parts[0]);
            if (parts.Length > 1 && parts[1] != "")
            {
                Name.text = parts[1];
            }
            else
            {
                Name.text = portrait.Name;
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
        if (line.Contains("[")) // Variable name (like button name)
        {
            Debug.Log(line);
            line = line.Replace("[AButton]", Control.DisplayShortButtonName("A"));
            line = line.Replace("[BButton]", Control.DisplayShortButtonName("B"));
            line = line.Replace("[StartButton]", Control.DisplayShortButtonName("Start"));
            line = line.Replace("[Name]", Name.text);
            Debug.Log(line);
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
                tempFlags.ForEach(a => SavedData.Save("ConversationData", "TempFlag" + a, 0));
                tempFlags.Clear();
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
                //Debug.Log("Length (" + cutLine.Substring(0, i + 1) + "): " + (i + 1) + ", next word (" + cutLine.Substring(i + 1).Split(' ')[0] + "): " + nextLength + @", \a count: " + cutLine.Substring(0, i + 1 + nextLength).Count(a => a == '\a') + ", total: " + length + " / " + lineWidth);
                line = line.Substring(0, line.LastIndexOf('\n') + 1) + cutLine.Substring(0, i) + '\n' + cutLine.Substring(i + 1);
                i = 0;
                cutLine = line.Substring(line.LastIndexOf('\n') + 1);
            }
        }
        //Debug.Log(line);
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
