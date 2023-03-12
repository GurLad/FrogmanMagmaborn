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

    protected override void Start()
    {
        LevelNumber = 1;
        ConversationPlayer.Current.Play(CreateLevel());
    }

    protected override void Update()
    {
        base.Update();
        if (WaitingForForceButton && !MidBattleScreen.HasCurrent && CurrentForceButton == null && Time.timeScale != 0)
        {
            WaitingForForceButton = false;
            ConversationPlayer.Current.Resume();
        }
    }

    public override void HandleAButton(Vector2Int pos)
    {
        if (CurrentForceButton.Button == Control.CB.A && (CurrentForceButton.Pos == pos || CurrentForceButton.Pos == Vector2Int.one * -1))
        {
            base.HandleAButton(pos);
            CurrentForceButton = null;
            MarkerCursor.gameObject.SetActive(false);
        }
        else
        {
            WrongInput();
        }
    }

    public override void HandleBButton(Vector2Int pos)
    {
        if (CurrentForceButton.Button == Control.CB.B && (CurrentForceButton.Pos == pos || CurrentForceButton.Pos == Vector2Int.one * -1))
        {
            base.HandleBButton(pos);
            CurrentForceButton = null;
            MarkerCursor.gameObject.SetActive(false);
        }
        else
        {
            WrongInput();
        }
    }

    public override void HandleSelectButton(Vector2Int pos)
    {
        base.HandleSelectButton(pos);
    }

    public override void HandleStartButton(Vector2Int pos)
    {
        // No menu in tutorial (?)
    }

    protected override void CheckDifficulty()
    {
        difficulty = Difficulty.Insane;
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
