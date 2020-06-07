using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Current;
    public List<Tile> Tiles;
    public Vector2Int MapSize;
    public float TileSize;
    [TextArea(3,10)]
    public List<string> Rooms;
    public GameObject Cursor;
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
    [Header("Misc")]
    public float EnemyAIMoveDelay = 2;
    public GameObject Battle;
    [HideInInspector]
    public Tile[,] Map;
    [HideInInspector]
    public List<MapObject> MapObjects;
    [HideInInspector]
    public InteractState InteractState = InteractState.None;
    [HideInInspector]
    public Unit Selected;
    private Team currentPhase = Team.Player;
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
    private bool _interactable = true;
    private float cursorMoveDelay;
    private float enemyMoveDelayCount;
    private Vector2Int previousPos = new Vector2Int(-1, -1);
    private Vector2Int cursorPos
    {
        get
        {
            return new Vector2Int((int)(Cursor.transform.position.x / TileSize), -(int)(Cursor.transform.position.y / TileSize));
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
        string selectedRoom = Rooms[Random.Range(0, Rooms.Count)];
        string[] lines = selectedRoom.Split(';');
        Map = new Tile[MapSize.x, MapSize.y];
        for (int i = 0; i < MapSize.x; i++)
        {
            string[] line = lines[i].Split('|');
            for (int j = 0; j < MapSize.y; j++)
            {
                int tileID = int.Parse(line[j]);
                Tile newTile = Instantiate(Tiles[tileID].gameObject, transform).GetComponent<Tile>();
                newTile.transform.position = new Vector2(TileSize * i, -TileSize * j);
                newTile.gameObject.SetActive(true);
                Map[i, j] = newTile;
            }
        }
    }
    /// <summary>
    /// Used for player control.
    /// </summary>
    private void Update()
    {
        if (BattleAnimationController.Current != null)
        {
            return;
        }
        // Interact/UI code
        if (interactable)
        {
            if (cursorMoveDelay <= 0)
            {
                if (Mathf.Abs(Input.GetAxis("Horizontal")) >= 0.5f || Mathf.Abs(Input.GetAxis("Vertical")) >= 0.5f)
                {
                    Cursor.transform.position += new Vector3(
                        Sign(Input.GetAxis("Horizontal")),
                        Sign(Input.GetAxis("Vertical"))) * TileSize;
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
            if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.5f && Mathf.Abs(Input.GetAxis("Vertical")) < 0.5f)
            {
                cursorMoveDelay = 0;
            }
            if (Input.GetButtonUp("Fire1"))
            {
                InteractWithTile(cursorPos.x, cursorPos.y);
            }
            if (previousPos != cursorPos)
            {
                UITileInfo.text = Map[cursorPos.x, cursorPos.y].Name + '\n' + (Map[cursorPos.x, cursorPos.y].MovementCost <= 9 ? (Map[cursorPos.x, cursorPos.y].MovementCost + "MOV") : "");
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
                        UIAttackerInfo.text = Selected.AttackPreview(unit.Stats);
                        UIDefenderInfo.text = unit.AttackPreview(Selected.Stats);
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
    public void RemoveMarkers()
    {
        MapObjects.FindAll(a => a is Marker).ForEach(a => Destroy(a.gameObject));
        MapObjects.RemoveAll(a => a is Marker);
        previousPos = new Vector2Int(-1, -1);
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
    int Sign(float number)
    {
        return number < 0 ? -1 : (number > 0 ? 1 : 0);
    }
}
