using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ConversationPlayer : MidBattleScreen, ISuspendable<SuspendDataConversationPlayer>
{
    public enum StartLineResult { None = 0, LoadMap = 1, LoadUnits = 2, Fade = 4, MidBattleScreen = 8, FinishLevel = 16, FinishConversation = 32, Wait = 64 }
    public enum PlayMode { PreBattle, MidBattle, PostBattle }
    private enum CurrentState { Writing, Waiting, Sleep, Hold }
    private enum LineType { Empty, Command, Comment, EndOfBlock, Text }

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
    public bool Playing;
    public string waitRequirement { private get; set; } = "";
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
    private Stack<FunctionStackObject> functionStack { get; } = new Stack<FunctionStackObject>();
    private List<string> lines { get; } = new List<string>();
    private string[] previousLineParts;
    private bool skipping;

    private void Awake()
    {
        Current = this;
        gameObject.SetActive(Playing = startActive);
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
                        count += Time.deltaTime * speed * voice.Speed;
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
        Playing = true;
        playMode = PlayMode.PreBattle;
        gameObject.SetActive(true);
        MidBattleScreen.Set(this, true);
        origin = conversation;
        lines.Clear();
        lines.AddRange(origin.Lines);
        UpdateSpeed();
        StartLine(0, true, shouldFadeIn);
    }

    public void PlayOneShot(string text)
    {
        Playing = true;
        // Store current lines & position
        if (currentLine < (lines?.Count ?? 0))
        {
            functionStack.Push(new FunctionStackObject(currentLine, lines.Clone()));
        }
        // Load new lines
        playMode = PlayMode.MidBattle;
        lines.Clear();
        lines.AddRange(text.Split('\n'));
        UpdateSpeed();
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
        Playing = true;
        playMode = PlayMode.PostBattle;
        gameObject.SetActive(true);
        MidBattleScreen.Set(this, true);
        lines.Clear();
        lines.AddRange(origin.PostBattleLines);
        UpdateSpeed();
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
        Playing = true;
        SkipEmptyLines(currentLine + mod);
        if (currentLine >= lines.Count)
        {
            if ((FinishConversation(fadeIn) & (StartLineResult.FinishConversation | StartLineResult.Fade)) == 0)
            {
                SoftResume();
            }
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
        Playing = true;
        MidBattleScreen.Set(this, true);
        gameObject.SetActive(true);
        enabled = true;
    }

    public void Skip()
    {
        skipping = true;
        Resume();
    }

    public void CancelSkip()
    {
        skipping = false;
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

    private LineType GetLineType(string line)
    {
        if (line.Length <= 0) // Empty line
        {
            return LineType.Empty;
        }
        if (line[0] == ':') // Command
        {
            return LineType.Command;
        }
        else if (line[0] == '#') // Comment, like this one :)
        {
            return LineType.Comment;
        }
        else if (line[0] == '}') // End of if block
        {
            return LineType.EndOfBlock;
        }
        return LineType.Text;
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
            string line = EventCommandProcessor.ProcessLine(lines[currentLine]);
            LineType type = GetLineType(line);
            switch (type)
            {
                case LineType.Empty:
                case LineType.Comment:
                case LineType.EndOfBlock:
                    return result | StartLineTrue(num + 1);
                case LineType.Command:
                    return result | this.ExecuteCommand(
                        lines,
                        num,
                        origin,
                        CGController,
                        functionStack,
                        playMode,
                        beforeBattleStart,
                        shouldFadeIn,
                        StartLineTrue,
                        out delayedAction);
                case LineType.Text:
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
                default:
                    throw Bugger.Crash("Impossible");
            }
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

    public StartLineResult FinishConversation(bool fadedOut = false)
    {
        // Check if this is the last part
        if (functionStack.Count > 0)
        {
            FunctionStackObject function = functionStack.Pop();
            lines.Clear();
            lines.AddRange(function.Lines);
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
        Playing = false;
        lines.Clear();
        gameObject.SetActive(false);
        state = CurrentState.Sleep;
        SetSinglePortrait(true);
        PortraitL.gameObject.SetActive(false);
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
        PortraitController.Current.ClearGeneratedPortraits();
        speakerL = speakerR = "";
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

    private bool LineAddition(string trueLine)
    {
        return /*trueLine.IndexOf('\n') < 0 &&*/ currentLine >= 1;
    }

    public void SetSpeakerFromText(string speakerText)
    {
        // Format: name|displayName|L/R
        string[] parts = speakerText.Split('|');
        Portrait portrait = PortraitController.Current.FindPortrait(parts[0]);
        bool left = (parts.Length > 2 && parts[2] != "") ? parts[2] == "L" : !currentSpeakerIsLeft;
        bool updatePortrait = (parts.Length > 3 && parts[3] != "") ? parts[3] == "T" : true;
        if (left)
        {
            if (updatePortrait)
            {
                PortraitL.Portrait = portrait;
            }
            speakerL = parts[0];
            SetSpeaker(true);
        }
        else
        {
            if (updatePortrait)
            {
                PortraitR.Portrait = portrait;
            }
            speakerR = parts[0];
            SetSpeaker(false);
        }
        if (parts.Length > 1 && parts[1] != "") // Imperfect fix, but whatever
        {
            Name.text = parts[1];
        }
        voice = portrait.Voice;
    }

    public void SetSpeaker(bool left)
    {
        PortraitHolder target = left ? PortraitL : PortraitR;
        NameHolder.anchoredPosition = new Vector2(left ? 64 : 112, NameHolder.anchoredPosition.y);
        Name.text = (left ? PortraitL.Portrait?.TheDisplayName : PortraitR.Portrait?.TheDisplayName) ?? "";
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

    public void SetSinglePortrait(bool left, bool? newCurrentSpeakerIsLeft = null)
    {
        PortraitL.gameObject.SetActive(left);
        PortraitR.gameObject.SetActive(!left);
        if (left)
        {
            speakerR = "";
        }
        else
        {
            speakerL = "";
        }
        SetSpeaker(left);
        currentSpeakerIsLeft = newCurrentSpeakerIsLeft ?? currentSpeakerIsLeft;
    }

    private void SetNoPortrait()
    {
        PortraitL.gameObject.SetActive(false);
        PortraitR.gameObject.SetActive(false);
        currentSpeakerIsLeft = false;
        speakerL = speakerR = "";
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

    private void UpdateSpeed()
    {
        speed = LettersPerSecond * (SavedData.Load("TextSpeed", 0, SaveMode.Global) + 1);
    }

    public SuspendDataConversationPlayer SaveToSuspendData()
    {
        if (Playing) // If there's a saved action, it's supposed to happen after this conversation (ex. battle)
        {
            return new SuspendDataConversationPlayer(origin, currentLine, functionStack, lines, speakerL, speakerR, currentSpeakerIsLeft, CGController);
        }
        else
        {
            return new SuspendDataConversationPlayer(origin);
        }
    }

    public void LoadFromSuspendData(SuspendDataConversationPlayer data)
    {
        void RestoreSpeakers()
        {
            if (data.SpeakerL != "")
            {
                SetSpeakerFromText(data.SpeakerL + "||L");
            }
            if (data.SpeakerR != "")
            {
                SetSpeakerFromText(data.SpeakerR + "||R");
            }
            //Bugger.Info(data.SpeakerL + ", " + data.SpeakerR + ", " + string.Join("; ", data.Lines) + ", " + data.CurrentLine + ", " + data.Playing);
            currentSpeakerIsLeft = data.Lines[data.CurrentLine].Contains(":") ^ data.CurrentSpeakerIsLeft;
        }

        origin = new ConversationData(data.Origin);
        if (data.Playing)
        {
            Bugger.Info("Playing! CG is " + data.CurrentCG);
            // TBA: Test
            lines.Clear();
            lines.AddRange(data.Lines);
            currentLine = data.CurrentLine; // To repeat the previous line
            data.FunctionStack.ForEach(a => functionStack.Push(a));
            PortraitController.Current.LoadGeneratedPortraits(data.GeneratedPortraits);
            CrossfadeMusicPlayer.Current.Play(data.CurrentMusic);
            UpdateSpeed();
            if (data.CurrentCG != "")
            {
                // Dumb fix to prevent the previous state from overriding the speaker's palettes
                PaletteController.Current.LoadState(data.CGPreviousState);
                RestoreSpeakers();
                data.CGPreviousState = PaletteController.Current.SaveState();
                gameObject.SetActive(false);
                CGController.FadeInCG(data.CurrentCG, data.CGPreviousState);
            }
            else
            {
                RestoreSpeakers();
                Resume(0);
                //Bugger.Info("State is " + state);
            }
        }
        else
        {
            Playing = false;
            gameObject.SetActive(false); // The GameController will properly disable this ConversationPlayer later
        }
    }

    [System.Serializable]
    public class FunctionStackObject
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
public class SuspendDataConversationPlayer
{
    public ConversationData Origin;
    public bool Playing;
    public int CurrentLine;
    public List<ConversationPlayer.FunctionStackObject> FunctionStack = new List<ConversationPlayer.FunctionStackObject>();
    public List<string> Lines;
    public List<GeneratedPortrait> GeneratedPortraits;
    public string SpeakerL;
    public string SpeakerR;
    public bool CurrentSpeakerIsLeft;
    public string CurrentCG;
    public PaletteController.PaletteControllerState CGPreviousState;
    public string CurrentMusic;

    public SuspendDataConversationPlayer(ConversationData origin)
    {
        Origin = origin;
        Playing = false;
    }

    public SuspendDataConversationPlayer(
        ConversationData origin,
        int currentLine,
        Stack<ConversationPlayer.FunctionStackObject> functionStack,
        List<string> lines,
        string speakerL,
        string speakerR,
        bool currentSpeakerIsLeft,
        CGController cgController) : this(origin)
    {
        CurrentLine = currentLine;
        Lines = lines;
        SpeakerL = speakerL;
        SpeakerR = speakerR;
        CurrentSpeakerIsLeft = currentSpeakerIsLeft;
        while (functionStack.Count > 0)
        {
            FunctionStack.Add(functionStack.Pop());
        }
        GeneratedPortraits = PortraitController.Current.SaveAllGeneratedPortraits();
        CurrentCG = cgController.Active ? cgController.CurrentCG : "";
        CGPreviousState = cgController.PreviousState;
        CurrentMusic = CrossfadeMusicPlayer.Current.Playing;
        Playing = true;
    }
}
