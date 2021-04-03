using System.Collections;
using System.Collections.Generic;
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
    [Header("Objects")]
    public Text Name;
    public Text Text;
    public PortraitHolder Portrait;
    public GameObject Arrow;
    public MenuController ChoiceMenu;
    public MenuController InfoDialogue;
    [Header("Main menu only")]
    public GameObject Knowledge;
    public GameObject Tutorial;
    [SerializeField]
    private bool startActive = true;
    private CurrentState state;
    private ConversationData origin;
    private CharacterVoice voice;
    private bool playingVoice;
    private int currentLine;
    private int currentChar;
    private float count;
    private bool postBattle = false;
    private string targetLine;
    private string waitRequirement = "";
    private Stack<FunctionStackObject> functionStack = new Stack<FunctionStackObject>();
    private List<string> lines;
    private void Awake()
    {
        Current = this;
        gameObject.SetActive(startActive);
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
                        Text.text = targetLine;
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
                                Text.text += targetLine[currentChar];
                                PlayLetter(targetLine[currentChar]);
                            }
                            else
                            {
                                Arrow.SetActive(true);
                                state = CurrentState.Waiting;
                            }
                        }
                    }
                    break;
                case CurrentState.Waiting:
                    if (Control.GetButtonDown(Control.CB.A))
                    {
                        if (++currentLine >= lines.Count)
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
    }
    public void Resume(int mod = 1)
    {
        MidBattleScreen.Set(this, true);
        if (currentLine + mod >= lines.Count)
        {
            FinishConversation();
            return;
        }
        gameObject.SetActive(true);
        enabled = true;
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
                // Level/conversation-specific commands

                case "play":
                    CrossfadeMusicPlayer.Current.Play(parts[2], parts.Length > 3 ? (parts[3] == "T") : false);
                    break;
                case "playIntro":
                    CrossfadeMusicPlayer.Current.PlayIntro(parts[2], false);
                    break;
                case "addUnit":
                    GameController.Current.PlayerUnits.Add(GameController.Current.CreatePlayerUnit(parts[2]));
                    break;
                case "loadUnits":
                    if (parts.Length < 4)
                    {
                        GameController.Current.LoadLevelUnits(parts[2]);
                    }
                    else
                    {
                        GameController.Current.LoadLevelUnits(parts[2], parts[3].ToLower() switch
                        {
                            "player" => Team.Player,
                            "monster" => Team.Monster,
                            "guard" => Team.Guard,
                            _ => null
                        });
                    }
                    break;
                case "loadMap":
                    GameController.Current.LoadMap(parts[2]);
                    break;
                case "addGenericCharacter":
                    // Add to TempPortraits with parts[2] internal name and parts[3] tags
                    PortraitController.Current.TempPortraits.Add(parts[2], PortraitController.Current.FindGenericPortrait(parts[3]));
                    break;
                case "wait":
                    waitRequirement = line.Substring(line.IndexOf(':', 1) + 1);
                    Pause();
                    return;
                case "lose":
                    GameController.Current.Lose();
                    return;
                case "win":
                    GameController.Current.Win();
                    return;
                case "finishConversation":
                    FinishConversation();
                    return;

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
                    Portrait.Portrait = PortraitController.Current.FindPortrait(Name.text = parts[2]);
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
                    KnowledgeController.UnlockKnowledge(parts[2]);
                    break;
                case "setFlag":
                    SavedData.Save("ConversationData", "Flag" + parts[2], 1);
                    break;

                // Syntax commands (ifs, functions...)

                case "if":
                    /* Syntax:
                     * :if:hasFlag:bla{
                     * Firbell: Will happen if hasFlag (requirement)
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
                case "return":
                    if (functionStack.Count == 0)
                    {
                        throw new System.Exception("Nothing to return from!");
                    }
                    FunctionStackObject function = functionStack.Pop();
                    lines = function.Lines;
                    StartLine(function.LineNumber + 1);
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
            Portrait.Portrait = portrait;
            voice = portrait.Voice;
            if (parts.Length > 1)
            {
                Name.text = parts[parts.Length - 1];
            }
            else
            {
                Name.text = portrait.Name;
            }
        }
        if (line.Contains("[")) // Variable name (like button name)
        {
            Debug.Log(line);
            line = line.Replace("[AButton]", Control.DisplayShortButtonName("A"));
            line = line.Replace("[BButton]", Control.DisplayShortButtonName("B"));
            line = line.Replace("[Name]", Name.text);
            Debug.Log(line);
        }
        // Find the line break
        string trueLine = FindLineBreack(TrueLine(line));
        // Check if it's short (aka no line break) and had previous
        if (line.IndexOf(':') < 0 && LineAddition(trueLine))
        {
            string[] previousLineParts = targetLine.Split('\n');
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
        MidBattleScreen.Set(this, false);
        gameObject.SetActive(false);
        state = CurrentState.Sleep;
        // Clear TempPortraits
        PortraitController.Current.TempPortraits.Clear();
        if (GameController.Current == null)
        {
            // Intro conversations
            if (postBattle || origin.PostBattleLines.Count <= 0)
            {
                origin.Choose(true);
                SavedData.SaveAll(SaveMode.Slot);
                if (SavedData.Load("ConversationData", "FlagTutorialFinish", 0) == 0)
                {
                    Tutorial.SetActive(true);
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
                GameController.Current.Win();
            }
            else
            {
                CrossfadeMusicPlayer.Current.Play(GameController.Current.RoomThemes[GameController.Current.LevelNumber - 1], false);
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
    }
    private string TrueLine(string line)
    {
        int index = line.IndexOf(':');
        line = line.Substring(index >= 0 ? index + 2 : 0);
        return line;
    }
    private string FindLineBreack(string line)
    {
        for (int i = line.IndexOf(' '); i > -1; i = line.IndexOf(' ', i + 1))
        {
            int length = line.Substring(0, i + 1).Length + line.Substring(i + 1).Split(' ')[0].Length;
            if (length > LineWidth)
            {
                line = line.Substring(0, i) + '\n' + line.Substring(i + 1);
                break;
            }
        }
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
        return trueLine.IndexOf('\n') < 0 && currentLine >= 1;
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
