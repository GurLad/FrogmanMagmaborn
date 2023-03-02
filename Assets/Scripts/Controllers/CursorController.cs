using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public GameUIController GameUIController;
    [SerializeField]
    private PalettedSprite palettedSprite;
    public Vector2Int Pos
    {
        get
        {
            return new Vector2Int((int)(transform.position.x / tileSize), -(int)(transform.position.y / tileSize));
        }
        set
        {
            transform.position = new Vector3(value.x * tileSize, -value.y * tileSize, transform.position.z);
            if (Visible)
            {
                SystemSFXController.Play(SystemSFXController.Type.CursorMove);
            }
        }
    }
    public bool Visible
    {
        get
        {
            return palettedSprite.gameObject.activeSelf;
        }
        set
        {
            //Bugger.Info("Visible: " + Visible + " -> " + value);
            palettedSprite.gameObject.SetActive(value);
        }
    }
    public int Palette { set => palettedSprite.Palette = value; }
    private float tileSize => GameController.Current.TileSize;
    private Vector2Int mapSize => GameController.Current.MapSize;
    private float cursorMoveDelay;
    private Vector2Int previousPos = new Vector2Int(-1, -1);

    private void Update()
    {
        if (MidBattleScreen.HasCurrent) // For ConversationPlayer
        {
            GameUIController.HideUI();
            return;
        }
        if (Time.timeScale == 0 || GameController.Current.CheckGameState() != GameState.Normal)
        {
            return;
        }
        // Interact/UI code
        if (GameController.Current.Interactable)
        {
            if (!Visible)
            {
                GameUIController.ShowUI(Pos);
            }
            if (cursorMoveDelay <= 0)
            {
                if (Mathf.Abs(Control.GetAxis(Control.Axis.X)) >= 0.5f || Mathf.Abs(Control.GetAxis(Control.Axis.Y)) >= 0.5f)
                {
                    Pos = new Vector2Int(
                        Mathf.Clamp(Pos.x + Control.GetAxisInt(Control.Axis.X), 0, mapSize.x - 1),
                        Mathf.Clamp(Pos.y - Control.GetAxisInt(Control.Axis.Y), 0, mapSize.y - 1));
                    cursorMoveDelay = 0.15f;
                    if (Pos != previousPos)
                    {
                        cursorMoveDelay = 0.15f;
                    }
                    else
                    {
                        cursorMoveDelay -= Time.deltaTime;
                    }
                }
            }
            else
            {
                cursorMoveDelay -= Time.deltaTime;
            }
            if (Mathf.Abs(Control.GetAxis(Control.Axis.X)) < 0.5f && Mathf.Abs(Control.GetAxis(Control.Axis.Y)) < 0.5f)
            {
                cursorMoveDelay = 0;
            }
            if (previousPos != Pos)
            {
                GameUIController.ShowUI(Pos);
            }
            previousPos = Pos;
            if (Control.GetButtonDown(Control.CB.A))
            {
                GameController.Current.HandleAButton(Pos);
            }
            else if (Control.GetButtonDown(Control.CB.B))
            {
                GameController.Current.HandleBButton(Pos);
            }
            else if (Control.GetButtonDown(Control.CB.Select))
            {
                GameController.Current.HandleSelectButton(Pos);
            }
            else if (Control.GetButtonDown(Control.CB.Start))
            {
                GameController.Current.HandleStartButton(Pos);
            }
        }
    }

    public void RefreshNextFrame()
    {
        previousPos = -Vector2Int.one;
    }
}
