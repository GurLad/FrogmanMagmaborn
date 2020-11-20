using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGameController : GameController
{
    public new static TutorialGameController Current;
    public ForceButton CurrentForceButton;
    protected override void Awake()
    {
        base.Awake();
        Current = this;
    }
    protected override void Update()
    {
        base.Update();
        if (MidBattleScreen.Current == null && CurrentForceButton == null)
        {
            ConversationPlayer.Current.Resume();
        }
    }
    protected override void HandleAButton()
    {
        if (CurrentForceButton.Button == Control.CB.A && cursorPos == CurrentForceButton.Pos)
        {
            base.HandleAButton();
            CurrentForceButton = null;
        }
        else
        {
            WrongInput();
        }
    }
    protected override void HandleBButton()
    {
        if (CurrentForceButton.Button == Control.CB.B && cursorPos == CurrentForceButton.Pos)
        {
            base.HandleBButton();
            CurrentForceButton = null;
        }
        else
        {
            WrongInput();
        }
    }
    protected override void HandleSelectButton()
    {
        if (CurrentForceButton.Button == Control.CB.Select && cursorPos == CurrentForceButton.Pos)
        {
            base.HandleSelectButton();
            CurrentForceButton = null;
        }
        else
        {
            WrongInput();
        }
    }
    protected override void HandleStartButton()
    {
        if (CurrentForceButton.Button == Control.CB.Start && cursorPos == CurrentForceButton.Pos)
        {
            base.HandleStartButton();
            CurrentForceButton = null;
        }
        else
        {
            WrongInput();
        }
    }
    protected override void CheckDifficulty()
    {
        difficulty = Difficulty.Hard;
    }
    private void WrongInput()
    {
        ConversationPlayer.Current.Resume(-1); // Repeat last line
    }

    public class ForceButton
    {
        public Control.CB Button;
        public Vector2Int Pos;
        public bool Move;
    }
}
