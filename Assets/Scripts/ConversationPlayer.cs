using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConversationPlayer : MidBattleScreen
{
    public enum CurrentState { Writing, Waiting }
    public new static ConversationPlayer Current;
    public float LettersPerSecond;
    public Text Name;
    public Text Text;
    public PortraitHolder Portrait;
    public GameObject Arrow;
    private CurrentState state;
    private ConversationData origin;
    private int currentLine;
    private int currentChar;
    private float count;
    private void Awake()
    {
        Current = this;
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
                        int index = origin.Lines[currentLine].IndexOf(':');
                        Text.text = origin.Lines[currentLine].Substring(index >= 0 ? index + 2 : 0);
                        Arrow.SetActive(true);
                        state = CurrentState.Waiting;
                    }
                    else
                    {
                        count += Time.deltaTime * LettersPerSecond;
                        if (count >= 1)
                        {
                            if (++currentChar < origin.Lines[currentLine].Length)
                            {
                                count -= 1;
                                Text.text += origin.Lines[currentLine][currentChar];
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
                        if (++currentLine >= origin.Lines.Count)
                        {
                            // Finish conversation
                            origin = null;
                            MidBattleScreen.Current = null;
                            CrossfadeMusicPlayer.Current.Play(GameController.Current.RoomThemes[GameController.Current.LevelNumber - 1], false);
                            gameObject.SetActive(false);
                            return;
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
        gameObject.SetActive(true);
        MidBattleScreen.Current = this;
        origin = conversation;
        StartLine(0);
    }
    private void StartLine(int num)
    {
        currentLine = num;
        if (origin.Lines[currentLine][0] == ':')
        {
            // Command
            string[] parts = origin.Lines[currentLine].Split(':');
            switch (parts[1])
            {
                case "play":
                    CrossfadeMusicPlayer.Current.Play(parts[2], false);
                    break;
                default:
                    break;
            }
            StartLine(num + 1);
            return;
        }
        if (origin.Lines[currentLine].IndexOf(':') != -1)
        {
            string[] parts = origin.Lines[currentLine].Split(':');
            Name.text = parts[0]; // Also change image
            Portrait.Portrait = PortraitController.Current.FindPortrait(parts[0]);
            currentChar = origin.Lines[currentLine].IndexOf(':') + 2;
        }
        else
        {
            currentChar = 0;
        }
        count = 0;
        state = CurrentState.Writing;
        Text.text = origin.Lines[currentLine][currentChar].ToString();
        Arrow.SetActive(false);
    }
}
