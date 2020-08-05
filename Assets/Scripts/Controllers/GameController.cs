using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Current;
    public List<TileSet> TileSets1;
    public Vector2Int MapSize;
    public float TileSize;
    [TextArea(3,10)]
    public List<string> Rooms;
    public List<string> RoomThemes;
    [Header("UI")]
    public RectTransform UITileInfoPanel;
    public Text UITileInfo;
    public RectTransform UIUnitInfoPanel;
    public Text UIUnitInfo;
    public RectTransform UIFightPanel;
    public PalettedSprite UIAttackerPanel;
    public Text UIAttackerInfo;
    public PalettedSprite UIDefenderPanel;
    public Text UIDefenderInfo;
    public TurnAnimation TurnAnimation;
    [Header("Mid-battle screens")]
    public GameObject Battle;
    public GameObject StatusScreen;
    [Header("Misc")]
    public float EnemyAIMoveDelay = 2;
    [Header("Objects")]
    public GameObject CameraBlackScreen; // Fixes an annoying UI bug
    public GameObject Cursor;
    public Unit BaseUnit;
    public UnitClassData UnitClassData;
    public Marker EnemyMarker;
    public int LevelNumber; // Should be hidden
    [HideInInspector]
    public Tile[,] Map;
    [HideInInspector]
    public List<MapObject> MapObjects;
    [HideInInspector]
    public InteractState InteractState = InteractState.None;
    [HideInInspector]
    public Unit Selected;
    private Team currentPhase = Team.Player;
    private float cursorMoveDelay;
    private float enemyMoveDelayCount;
    private Vector2Int previousPos = new Vector2Int(-1, -1);
    private Camera main;
    private bool checkPlayerDead;
    private bool _interactable = true;
    private bool interactable
    {
        get => _interactable;
        set
        {
            _interactable = value;
            UITileInfoPanel.gameObject.SetActive(_interactable);
            Cursor.gameObject.SetActive(_interactable);
        }
    }
    private Vector2Int cursorPos
    {
        get
        {
            return new Vector2Int((int)(Cursor.transform.position.x / TileSize), -(int)(Cursor.transform.position.y / TileSize));
        }
        set
        {
            Cursor.transform.position = new Vector3(value.x * TileSize, -value.y * TileSize, Cursor.transform.position.z);
        }
    }
    private List<Unit> units
    {
        get
        {
            return MapObjects.Where(a => a is Unit).Cast<Unit>().ToList();
        }
    }
    private void Awake()
    {
        /*
         * I had a few different ideas:
         * -Current
         * -Dungeon: floors and walls, and generate straight lines
         * -Roads: continue random number of steps in a direction, change. Randomaly split/stop.
         */
        Current = this;
        main = Camera.main;
        string[] selectedRoom = Rooms[LevelNumber - 1].Split('\n'); // In the future, each level should have a selection of rooms instead of just 1
        TileSet tileSet = TileSets1.Find(a => a.Name == selectedRoom[2]);
        PaletteController.Current.BackgroundPalettes[0] = tileSet.Palette1;
        PaletteController.Current.BackgroundPalettes[1] = tileSet.Palette2;
        // Map
        string[] lines = selectedRoom[0].Split(';');
        Map = new Tile[MapSize.x, MapSize.y];
        for (int i = 0; i < MapSize.x; i++)
        {
            string[] line = lines[i].Split('|');
            for (int j = 0; j < MapSize.y; j++)
            {
                int tileID = int.Parse(line[j]);
                Tile newTile = Instantiate(tileSet.Tiles[tileID].gameObject, transform).GetComponent<Tile>();
                newTile.transform.position = new Vector2(TileSize * i, -TileSize * j);
                newTile.gameObject.SetActive(true);
                Map[i, j] = newTile;
            }
        }
        // Units
        lines = selectedRoom[1].Split(';');
        for (int i = 0; i < lines.Length; i++)
        {
            Unit unit = Instantiate(BaseUnit.gameObject, transform.parent).GetComponent<Unit>();
            string[] parts = lines[i].Split(',');
            unit.TheTeam = (Team)int.Parse(parts[0]);
            if (unit.TheTeam == Team.Player)
            {
                unit.Name = parts[1];
                unit.Class = UnitClassData.UnitClasses.Find(a => a.Unit == unit.Name).Class;
                unit.Stats.Growths = UnitClassData.UnitGrowths.Find(a => a.Name == unit.Name).Growths;
            }
            else
            {
                unit.Name = unit.TheTeam.ToString();
                unit.Class = parts[1];
                unit.Stats.Growths = UnitClassData.ClassGrowths.Find(a => a.Name == unit.Class).Growths;
                unit.MovementMarker = EnemyMarker;
                unit.AttackMarker = EnemyMarker;
            }
            unit.Stats += unit.Stats.GetLevelUp(int.Parse(parts[2]));
            unit.Pos = new Vector2Int(int.Parse(parts[3]), int.Parse(parts[4]));
            if (unit.Name == "Frogman")
            {
                cursorPos = unit.Pos; // Auto-cursor
            }
            Instantiate(UnitClassData.ClassAnimations.Find(a => a.Name == unit.Class).Animation, unit.transform).Renderer = unit.GetComponent<SpriteRenderer>();
            unit.gameObject.SetActive(true);
        }
        Application.targetFrameRate = 60; // To prevent my laptop from burning itself trying to run the game at 700 FPS
    }
    private void Start()
    {
        ConversationController.Current.PlayRandomConversation();
        CrossfadeMusicPlayer.Instance.Play(RoomThemes[LevelNumber - 1], false);
    }
    /// <summary>
    /// Used for player control.
    /// </summary>
    private void Update()
    {
        if (MidBattleScreen.Current != null) // For ConversationPlayer
        {
            UIUnitInfoPanel.gameObject.SetActive(false);
            UIFightPanel.gameObject.SetActive(false);
            Cursor.gameObject.SetActive(false);
            return;
        }
        if (checkPlayerDead)
        {
            if (units.FindAll(a => a.TheTeam == Team.Player).Count == 0)
            {
                // Lose
                SceneManager.LoadScene("Menu");
            }
            else if (units.FindAll(a => a.TheTeam != Team.Player).Count == 0)
            {
                // Win
                SceneManager.LoadScene("Menu");
            }
            checkPlayerDead = false;
        }
        // Interact/UI code
        if (interactable)
        {
            Cursor.gameObject.SetActive(true);
            if (cursorMoveDelay <= 0)
            {
                if (Mathf.Abs(Control.GetAxis(SnapAxis.X)) >= 0.5f || Mathf.Abs(Control.GetAxis(SnapAxis.Y)) >= 0.5f)
                {
                    Cursor.transform.position += new Vector3(
                        Sign(Control.GetAxis(SnapAxis.X)),
                        Sign(Control.GetAxis(SnapAxis.Y))) * TileSize;
                    Cursor.transform.position = new Vector3(
                        Mathf.Max(0, Mathf.Min(MapSize.x - 1, cursorPos.x)) * TileSize,
                        -Mathf.Max(0, Mathf.Min(MapSize.y - 1, cursorPos.y)) * TileSize,
                        Cursor.transform.position.z);
                    cursorMoveDelay = 0.15f;
                    if (cursorPos != previousPos)
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
            if (Mathf.Abs(Control.GetAxis(SnapAxis.X)) < 0.5f && Mathf.Abs(Control.GetAxis(SnapAxis.Y)) < 0.5f)
            {
                cursorMoveDelay = 0;
            }
            if (Control.GetButtonDown(Control.CB.A))
            {
                InteractWithTile(cursorPos.x, cursorPos.y);
            }
            else if (Control.GetButtonDown(Control.CB.B))
            {
                // Undo turn in move/attack
                switch (InteractState)
                {
                    case InteractState.None:
                        if (!RemoveMarkers()) // If not viewing enemy range
                        {
                            // Should move to Select button for ease of use
                            Unit selected = FindUnitAtPos(cursorPos.x, cursorPos.y);
                            if (selected != null)
                            {
                                StatusScreenController statusScreenController = Instantiate(StatusScreen).GetComponentInChildren<StatusScreenController>();
                                TransitionToMidBattleScreen(statusScreenController);
                                statusScreenController.Show(selected);
                            }
                        }
                        break;
                    case InteractState.Move:
                        Selected = null;
                        RemoveMarkers();
                        InteractState = InteractState.None;
                        break;
                    case InteractState.Attack:
                        // TBA
                        break;
                    default:
                        break;
                }
            }
            if (previousPos != cursorPos)
            {
                UITileInfo.text = Map[cursorPos.x, cursorPos.y].Name + '\n' + (Map[cursorPos.x, cursorPos.y].MovementCost <= 9 ? (Map[cursorPos.x, cursorPos.y].MovementCost + "MOV\n" + Map[cursorPos.x, cursorPos.y].ArmorModifier.ToString()[0] + "ARM") : Map[cursorPos.x, cursorPos.y].High ? "\nHigh" : "\nLow");
                Unit unit = FindUnitAtPos(cursorPos.x, cursorPos.y);
                Vector2 anchor;
                if (cursorPos.x >= MapSize.x / 2)
                {
                    anchor = new Vector2Int(0, 1);
                }
                else
                {
                    anchor = new Vector2Int(1, 1);
                }
                if (unit != null)
                {
                    UIUnitInfoPanel.gameObject.SetActive(true);
                    UIUnitInfo.text = unit.Name + "\nHP:" + unit.Health + "/" + unit.Stats.MaxHP;
                    UIUnitInfoPanel.anchorMin = anchor;
                    UIUnitInfoPanel.anchorMax = anchor;
                    UIUnitInfoPanel.pivot = anchor;
                    UIUnitInfoPanel.GetComponent<PalettedSprite>().Palette = (int)unit.TheTeam;
                    if (InteractState != InteractState.None && unit.TheTeam != Selected.TheTeam)
                    {
                        UIFightPanel.gameObject.SetActive(true);
                        anchor.y = 0.5f;
                        UIFightPanel.anchorMin = anchor;
                        UIFightPanel.anchorMax = anchor;
                        UIFightPanel.pivot = anchor;
                        UIAttackerPanel.Palette = (int)Selected.TheTeam;
                        UIDefenderPanel.Palette = (int)unit.TheTeam;
                        UIAttackerInfo.text = Selected.AttackPreview(unit);
                        UIDefenderInfo.text = unit.AttackPreview(Selected);
                    }
                    else
                    {
                        UIFightPanel.gameObject.SetActive(false);
                    }
                }
                else
                {
                    UIUnitInfoPanel.gameObject.SetActive(false);
                    UIFightPanel.gameObject.SetActive(false);
                }
                anchor.y = 0;
                UITileInfoPanel.anchorMin = anchor;
                UITileInfoPanel.anchorMax = anchor;
                UITileInfoPanel.pivot = anchor;
            }
            previousPos = cursorPos;
        }
        else
        {
            UIUnitInfoPanel.gameObject.SetActive(false);
            UIFightPanel.gameObject.SetActive(false);
            Cursor.gameObject.SetActive(false);
        }
        if (currentPhase == Team.Enemy)
        {
            enemyMoveDelayCount += Time.deltaTime;
            if (enemyMoveDelayCount > EnemyAIMoveDelay)
            {
                enemyMoveDelayCount -= EnemyAIMoveDelay;
                Unit currentEnemy = units.Find(a => a.TheTeam == Team.Enemy && !a.Moved);
                // AI
                currentEnemy.AI(units);
            }
        }
        // End Interact/UI code
    }
    public void InteractWithTile(int x, int y)
    {
        Debug.Log(InteractState);
        MapObjects.FindAll(a => a.Pos.x == x && a.Pos.y == y).ForEach(a => a.Interact(InteractState));
    }
    /// <summary>
    /// Removes all markers.
    /// </summary>
    /// <returns>True if there were any markers, flase otherwise.</returns>
    public bool RemoveMarkers()
    {
        int previousCount = MapObjects.Count;
        MapObjects.FindAll(a => a is Marker).ForEach(a => Destroy(a.gameObject));
        MapObjects.RemoveAll(a => a is Marker);
        previousPos = new Vector2Int(-1, -1);
        return previousCount != MapObjects.Count;
    }
    public void FinishMove(Unit unit)
    {
        RemoveMarkers();
        InteractState = InteractState.None;
        unit.Moved = true;
        if (units.Find(a => a.TheTeam == unit.TheTeam && !a.Moved) == null)
        {
            StartPhase((Team)(((int)unit.TheTeam + 1) % 2));
        }
        unit = null;
        // TEMP!!
        CrossfadeMusicPlayer.Instance.Play(CrossfadeMusicPlayer.Instance.Playing.Replace("Battle", ""));
    }
    public Unit FindUnitAtPos(int x, int y)
    {
        return (Unit)MapObjects.Find(a => a is Unit && a.Pos.x == x && a.Pos.y == y);
    }
    public void StartPhase(Team team)
    {
        currentPhase = team;
        TurnAnimation.ShowTurn(team);
        foreach (var item in units)
        {
            item.Moved = false;
        }
        interactable = team == Team.Player;
    }
    public void TransitionToMidBattleScreen(MidBattleScreen screen)
    {
        transform.parent.gameObject.SetActive(false);
        MidBattleScreen.Current = screen;
        CameraBlackScreen.SetActive(true);
    }
    public void KillUnit(Unit unit)
    {
        MapObjects.Remove(unit);
        Destroy(unit.gameObject);
        checkPlayerDead = true; // Since I need to wait for the battle animation to finish first
    }
    private int Sign(float number)
    {
        return number < 0 ? -1 : (number > 0 ? 1 : 0);
    }
}

[System.Serializable]
public class TileSet
{
    public string Name;
    public Palette Palette1 = new Palette();
    public Palette Palette2 = new Palette();
    public List<Tile> Tiles;

    public TileSet()
    {
        for (int i = 0; i < 4; i++)
        {
            Palette1.Colors[i] = Color.black;
            Palette2.Colors[i] = Color.black;
        }
    }
}