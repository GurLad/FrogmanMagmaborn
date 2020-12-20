using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGameController : GameController
{
    public new static TutorialGameController Current;
    public ForceButton CurrentForceButton;
    [Header("Tutorial Only")]
    [SerializeField]
    private AdvancedSpriteSheetAnimation MarkerCursor;
    [HideInInspector]
    public bool WaitingForForceButton;
    public void ShowMarkerCursor(Vector2Int pos)
    {
        MarkerCursor.transform.position = new Vector3(pos.x * TileSize, -pos.y * TileSize, MarkerCursor.transform.position.z);
        MarkerCursor.gameObject.SetActive(true);
        if (MarkerCursor.Active)
        {
            MarkerCursor.Restart();
        }
    }
    protected override void Awake()
    {
        base.Awake();
        Current = this;
    }
    protected override void Update()
    {
        base.Update();
        if (WaitingForForceButton && MidBattleScreen.Current == null && CurrentForceButton == null)
        {
            WaitingForForceButton = false;
            ConversationPlayer.Current.Resume();
        }
    }
    protected override void HandleAButton()
    {
        if (CurrentForceButton.Button == Control.CB.A && (CurrentForceButton.Pos == cursorPos || CurrentForceButton.Pos == Vector2Int.one * -1))
        {
            base.HandleAButton();
            CurrentForceButton = null;
            MarkerCursor.gameObject.SetActive(false);
        }
        else
        {
            WrongInput();
        }
    }
    protected override void HandleBButton()
    {
        if (CurrentForceButton.Button == Control.CB.B && (CurrentForceButton.Pos == cursorPos || CurrentForceButton.Pos == Vector2Int.one * -1))
        {
            base.HandleBButton();
            CurrentForceButton = null;
            MarkerCursor.gameObject.SetActive(false);
        }
        else
        {
            WrongInput();
        }
    }
    protected override void HandleSelectButton()
    {
        base.HandleSelectButton();
    }
    protected override void HandleStartButton()
    {
        // No menu in tutorial (?)
    }
    protected override void CheckDifficulty()
    {
        difficulty = Difficulty.Hard;
    }
    protected override void EnemyAI()
    {
        if (!WaitingForForceButton)
        {
            base.EnemyAI();
        }
    }
    private void WrongInput()
    {
        ConversationPlayer.Current.Resume(CurrentForceButton.WrongLine); // Repeat last line
    }

    public class ForceButton
    {
        public Control.CB Button;
        public Vector2Int Pos = Vector2Int.one * -1;
        public bool Move;
        public int WrongLine = -1;
    }
}
