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
        }
    }
    public int Palette { set => palettedSprite.Palette = value; }
    private float tileSize => GameController.Current.TileSize;
    private Vector2Int mapSize => GameController.Current.MapSize;
    private float cursorMoveDelay;
    private Vector2Int previousPos = new Vector2Int(-1, -1);

    private void Update()
    {
        // Interact/UI code
        if (GameController.Current.Interactable)
        {
            if (!gameObject.activeSelf)
            {
                GameUIController.ShowUI(Pos);
            }
            if (cursorMoveDelay <= 0)
            {
                if (Mathf.Abs(Control.GetAxis(Control.Axis.X)) >= 0.5f || Mathf.Abs(Control.GetAxis(Control.Axis.Y)) >= 0.5f)
                {
                    transform.position += new Vector3(
                        Control.GetAxisInt(Control.Axis.X),
                        Control.GetAxisInt(Control.Axis.Y)) * tileSize;
                    transform.position = new Vector3(
                        Mathf.Clamp(Pos.x, 0, mapSize.x - 1) * tileSize,
                        -Mathf.Clamp(Pos.y, 0, mapSize.y - 1) * tileSize,
                        transform.position.z);
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
            if (previousPos != Pos)
            {
                GameUIController.ShowUI(Pos);
                SystemSFXController.Play(SystemSFXController.Type.CursorMove);
                // Movement arrows
                if (GameController.Current.InteractState == InteractState.Move)
                {
                    GameController.Current.RemoveArrowMarkers();
                    GameController.Current.MapObjectsAtPos(Pos).ForEach(a => a.Hover(GameController.Current.InteractState));
                }
            }
            previousPos = Pos;
        }
    }
}
