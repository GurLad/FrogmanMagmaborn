using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConversationPlayer : MidBattleScreen
{
    public enum CurrentState { Writing, Waiting, Sleep }
    public new static ConversationPlayer Current;
    public float LettersPerSecond;
    public Text Name;
    public Text Text;
    public PortraitHolder Portrait;
    public GameObject Arrow;
    [SerializeField]
    private bool startActive = true;
    private CurrentState state;
    private ConversationData origin;
    private int currentLine;
    private int currentChar;
    private float count;
    private bool postBattle = false;
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
                    string line = lines[currentLine];
                    if (Control.GetButtonDown(Control.CB.A))
                    {
                        int index = line.IndexOf(':');
                        Text.text = line.Substring(index >= 0 ? index + 2 : 0);
                        Arrow.SetActive(true);
                        state = CurrentState.Waiting;
                    }
                    else
                    {
                        count += Time.deltaTime * LettersPerSecond;
                        if (count >= 1)
                        {
                            if (++currentChar < line.Length)
                            {
                                count -= 1;
                                Text.text += line[currentChar];
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
        else if (line[0] == '#') // Comment, like this one :)
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
            string[] parts = line.Split(':');
            Name.text = parts[0]; // Also change image
            Portrait.Portrait = PortraitController.Current.FindPortrait(parts[0]);
            currentChar = line.IndexOf(':') + 2;
        }
        else
        {
            currentChar = 0;
        }
        count = 0;
        state = CurrentState.Writing;
        Text.text = line[currentChar].ToString();
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
            SceneManager.LoadScene("Map");
        }
        if (postBattle)
        {
            origin = null;
            GameController.Current.Win();
        }
        else
        {
            CrossfadeMusicPlayer.Current.Play(GameController.Current.RoomThemes[GameController.Current.LevelNumber - 1], false);
        }
        return;
    }
}
