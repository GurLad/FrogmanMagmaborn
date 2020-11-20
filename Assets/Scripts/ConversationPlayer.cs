using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConversationPlayer : MidBattleScreen
{
    public enum CurrentState { Writing, Waiting, Sleep }
    public new static ConversationPlayer Current;
    public float LettersPerSecond;
    public int LineWidth = 22;
    public List<AudioClip> VoiceTypes;
    public float VoiceMod;
    public Text Name;
    public Text Text;
    public PortraitHolder Portrait;
    public GameObject Arrow;
    [SerializeField]
    private bool startActive = true;
    [Header("Menu only")]
    public GameObject Knowledge;
    private CurrentState state;
    private ConversationData origin;
    private CharacterVoice voice;
    private bool playingVoice;
    private int currentLine;
    private int currentChar;
    private float count;
    private bool postBattle = false;
    private string targetLine;
    private List<string> lines
    {
        get
        {
            return postBattle ? origin.PostBattleLines : origin.Lines;
        }
    }
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
        MidBattleScreen.Current = this;
        origin = conversation;
        if (GameController.Current != null)
        {
            if (!origin.Lines.Contains(":loadUnits:") && !origin.Lines.Contains(":loadUnits"))
            {
                GameController.Current.LoadLevelUnits();
            }
            if (!origin.Lines.Contains(":loadMap:") && !origin.Lines.Contains(":loadMap"))
            {
                GameController.Current.LoadMap();
            }
        }
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
        MidBattleScreen.Current = this;
        StartLine(0);
    }
    public void Pause()
    {
        gameObject.SetActive(false);
        MidBattleScreen.Current = null;
    }
    public void Resume(int mod = 1)
    {
        gameObject.SetActive(true);
        MidBattleScreen.Current = this;
        StartLine(currentLine + mod);
    }
    private void StartLine(int num)
    {
        currentLine = num;
        string line = lines[currentLine];
        if (line[0] == ':') // Command
        {
            string[] parts = line.Split(':');
            switch (parts[1])
            {
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
                    GameController.Current.LoadLevelUnits(parts[2]);
                    break;
                case "loadMap":
                    GameController.Current.LoadMap(parts[2]);
                    break;
                case "unlockKnowledge":
                    KnowledgeController.UnlockKnowledge(parts[2]);
                    break;
                case "setFlag":
                    SavedData.Save("Flag" + parts[2], 1);
                    break;
                case "if":
                    /* Syntax:
                     * if:hasFlag:bla{
                     * Firbell: Will happen if hasFlag (requirement)
                     * }
                     * Firbell: Will anyway happen
                     */
                    string requirement = line.Substring(line.IndexOf(':', 1) + 1);
                    requirement = requirement.Substring(0, requirement.IndexOf('{'));
                    if (!origin.MeetsRequirement(requirement))
                    {
                        while (lines[++num] != "}") { }
                    }
                    break;
                case "tutorialForceButton":
                    // Tutorial only (obviously)
                    TutorialGameController.ForceButton forceButton = new TutorialGameController.ForceButton();
                    forceButton.Move = System.Enum.TryParse(parts[2], out forceButton.Button);
                    string[] pos = parts[3].Split(',');
                    forceButton.Pos = new Vector2Int(int.Parse(pos[0]), int.Parse(pos[1]));
                    TutorialGameController.Current.CurrentForceButton = forceButton;
                    Pause();
                    return;
                default:
                    break;
            }
            if (num + 1 < lines.Count)
            {
                StartLine(num + 1);
            }
            else
            {
                FinishConversation();
            }
            return;
        }
        else if (line[0] == '#' || line[0] == '}') // Comment, like this one :) Or end of if block
        {
            if (num + 1 < lines.Count)
            {
                StartLine(num + 1);
            }
            else
            {
                FinishConversation();
            }
            return;
        }
        if (line.IndexOf(':') != -1)
        {
            string[] parts = line.Split(':')[0].Split('|');
            Portrait.Portrait = PortraitController.Current.FindPortrait(parts[0]);
            voice = PortraitController.Current.FindVoice(parts[0]);
            Name.text = parts[parts.Length - 1];
        }
        // Find the line break
        string trueLine = FindLineBreack(TrueLine(lines[currentLine]));
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
        // Finish conversation
        MidBattleScreen.Current = null;
        gameObject.SetActive(false);
        state = CurrentState.Sleep;
        if (GameController.Current == null)
        {
            if (postBattle || origin.PostBattleLines.Count <= 0)
            {
                origin.Choose();
                SceneController.LoadScene("Map");
            }
            else
            {
                Knowledge.SetActive(true);
            }
            return;
        }
        if (postBattle)
        {
            origin.Choose();
            origin = null;
            GameController.Current.Win();
        }
        else
        {
            CrossfadeMusicPlayer.Current.Play(GameController.Current.RoomThemes[GameController.Current.LevelNumber - 1], false);
            if (origin.PostBattleLines.Count <= 0)
            {
                origin.Choose();
            }
        }
        return;
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
}
