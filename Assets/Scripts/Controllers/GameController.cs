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
    public List<TextAsset> Rooms;
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
    public GameObject LevelUpScreen;
    [Header("Misc")]
    public float EnemyAIMoveDelay = 2;
    [Header("Objects")]
    public GameObject CameraBlackScreen; // Fixes an annoying UI bug
    public GameObject Cursor;
    public Unit BaseUnit;
    public UnitClassData UnitClassData;
    public Marker EnemyMarker;
    [HideInInspector]
    public int LevelNumber;
    [HideInInspector]
    public TileSet Set;
    [HideInInspector]
    public Tile[,] Map;
    [HideInInspector]
    public List<MapObject> MapObjects;
    [HideInInspector]
    public InteractState InteractState = InteractState.None;
    [HideInInspector]
    public Unit Selected;
    private List<Room> rooms;
    private Team currentPhase = Team.Player;
    private float cursorMoveDelay;
    private float enemyMoveDelayCount;
    private Vector2Int previousPos = new Vector2Int(-1, -1);
    private Camera main;
    private Transform currentLevel;
    private bool checkPlayerDead;
    private Room selectedRoom;
    private List<Unit> playerUnitsCache;
    public List<Unit> PlayerUnits
    {
        get
        {
            if (playerUnitsCache == null)
            {
                playerUnitsCache = new List<Unit>();
                string[] playerUnits = SavedData.Load<string>("PlayerDatas").Split('\n');
                for (int i = 0; i < playerUnits.Length - 1; i++)
                {
                    Unit unit = Instantiate(BaseUnit.gameObject, currentLevel).GetComponent<Unit>();
                    unit.Load(playerUnits[i]);
                    AssignUnitMapAnimation(unit);
                    unit.gameObject.SetActive(true);
                    playerUnitsCache.Add(unit);
                }
            }
            return playerUnitsCache;
        }
    }
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
        Application.targetFrameRate = 60; // To prevent my laptop from burning itself trying to run the game at 700 FPS
        // Load rooms
        rooms = new List<Room>();
        for (int i = 0; i < Rooms.Count; i++)
        {
            Room room = new Room();
            room.Name = Rooms[i].name;
            string[] selectedRoom = Rooms[i].text.Split('\n');
            // Level numer
            room.RoomNumber = int.Parse(selectedRoom[3]);
            // Tile set
            room.TileSet = TileSets1.Find(a => a.Name == selectedRoom[2]);
            // Map
            string[] lines = selectedRoom[0].Split(';');
            room.Map = new int[MapSize.x, MapSize.y];
            for (int k = 0; k < MapSize.x; k++)
            {
                string[] line = lines[k].Split('|');
                for (int j = 0; j < MapSize.y; j++)
                {
                    room.Map[k, j] = int.Parse(line[j]);
                }
            }
            // Units
            room.Units = selectedRoom[1].Split(';').ToList();
            rooms.Add(room);
        }
        // Awake enemy marker
        EnemyMarker.GetComponent<PalettedSprite>().Awake();
    }
    private void Start()
    {
        //Time.timeScale = 3; // For debugging
        LevelNumber = 1;
        playerUnitsCache = new List<Unit>();
        CreateLevel();
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
            if (units.Find(a => a.Name == "Frogman") == null)
            {
                // Lose
                SceneManager.LoadScene("Menu");
            }
            else if (units.FindAll(a => a.TheTeam != Team.Player).Count == 0)
            {
                // Win
                ConversationPlayer.Current.PlayPostBattle();
            }
            checkPlayerDead = false;
        }
        // Interact/UI code
        if (interactable)
        {
            Cursor.gameObject.SetActive(true);
            if (cursorMoveDelay <= 0)
            {
                if (Mathf.Abs(Control.GetAxis(Control.Axis.X)) >= 0.5f || Mathf.Abs(Control.GetAxis(Control.Axis.Y)) >= 0.5f)
                {
                    Cursor.transform.position += new Vector3(
                        Sign(Control.GetAxis(Control.Axis.X)),
                        Sign(Control.GetAxis(Control.Axis.Y))) * TileSize;
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
            if (Mathf.Abs(Control.GetAxis(Control.Axis.X)) < 0.5f && Mathf.Abs(Control.GetAxis(Control.Axis.Y)) < 0.5f)
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
                            else
                            {
                                // Show danger area
                                units.FindAll(a => a.TheTeam != Team.Player && !a.Moved).ForEach(a => a.MarkDangerArea());
                            }
                        }
                        break;
                    case InteractState.Move:
                        Selected = null;
                        RemoveMarkers();
                        InteractState = InteractState.None;
                        break;
                    case InteractState.Attack:
                        Selected.MoveTo(Selected.PreviousPos);
                        Selected.Interact(InteractState = InteractState.None);
                        break;
                    default:
                        break;
                }
            }
            else if (Control.GetButtonDown(Control.CB.Select))
            {
                // Only works in None
                switch (InteractState)
                {
                    case InteractState.None:
                        Unit selected = FindUnitAtPos(cursorPos.x, cursorPos.y);
                        if (selected != null)
                        {
                            bool foundSelected = false;
                            List<Unit> trueUnits = units;
                            for (int i = 0; i < trueUnits.Count; i++)
                            {
                                if (foundSelected && trueUnits[i].TheTeam == Team.Player && trueUnits[i].Moved == false)
                                {
                                    cursorPos = trueUnits[i].Pos;
                                    foundSelected = false;
                                    break;
                                }
                                if (trueUnits[i] == selected)
                                {
                                    foundSelected = true;
                                }
                            }
                            if (foundSelected)
                            {
                                for (int i = 0; i < trueUnits.Count; i++)
                                {
                                    if (trueUnits[i].TheTeam == Team.Player && trueUnits[i].Moved == false)
                                    {
                                        cursorPos = trueUnits[i].Pos;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            List<Unit> trueUnits = units;
                            for (int i = 0; i < trueUnits.Count; i++)
                            {
                                if (trueUnits[i].TheTeam == Team.Player && trueUnits[i].Moved == false)
                                {
                                    cursorPos = trueUnits[i].Pos;
                                    break;
                                }
                            }
                        }
                        break;
                    case InteractState.Move:
                        break;
                    case InteractState.Attack:
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
        if (currentPhase == Team.Monster)
        {
            enemyMoveDelayCount += Time.deltaTime;
            if (enemyMoveDelayCount > EnemyAIMoveDelay)
            {
                enemyMoveDelayCount -= EnemyAIMoveDelay;
                Unit currentEnemy = units.Find(a => a.TheTeam == Team.Monster && !a.Moved);
                // AI
                currentEnemy.AI(units);
            }
        }
        // End Interact/UI code
    }
    public void InteractWithTile(int x, int y)
    {
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
        CrossfadeMusicPlayer.Current.Play(CrossfadeMusicPlayer.Current.Playing.Replace("Battle", ""));
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
            if (item.TheTeam != Team.Player && item.ReinforcementTurn > 0)
            {
                if (team == Team.Player)
                {
                    item.ReinforcementTurn--;
                }
                if (item.ReinforcementTurn > 0)
                {
                    item.Moved = true;
                    if (!item.Statue)
                    {
                        if (item.Pos != Vector2Int.one * -1)
                        {
                            item.PreviousPos = item.Pos;
                        }
                        item.Pos = Vector2Int.one * -1;
                    }
                }
                else if (!item.Statue && FindUnitAtPos(item.PreviousPos.x, item.PreviousPos.y) == null)
                {
                    item.Pos = item.PreviousPos;
                }
                else if (item.Statue)
                {
                    item.Statue = false;
                }
            }
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
    public void Win()
    {
        LevelNumber++;
        PlayersLevelUp();
    }
    private void PlayersLevelUp()
    {
        // Save player characters
        List<Unit> playerCharacters = units.Where(a => a.TheTeam == Team.Player).ToList();
        playerCharacters.Sort((a, b) => a.Name == "Frogman" ? -1 : (b.Name == "Frogman" ? 1 : 0));
        playerCharacters = playerCharacters.Distinct().ToList(); // A very bad workaround - I need to find the true cause of the problem
        string saveData = "";
        playerCharacters.ForEach(a => saveData += a.Save() + "\n");
        SavedData.Save("PlayerDatas", saveData);
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            Debug.Log(playerCharacters[i].Save());
            Destroy(playerCharacters[i].gameObject);
        }
        playerUnitsCache = null;
        playerCharacters = PlayerUnits;
        // Custom level-up system
        foreach (Unit character in playerCharacters)
        {
            character.Level++;
            character.transform.parent = transform;
        }
        LevelUpController levelUpController = Instantiate(LevelUpScreen).GetComponentInChildren<LevelUpController>();
        levelUpController.Players = playerCharacters;
        TransitionToMidBattleScreen(levelUpController);

    }
    public void CreateLevel()
    {
        List<Unit> playerCharacters = PlayerUnits;
        // Clear previous level
        MapObjects.Clear();
        if (currentLevel != null)
        {
            Destroy(currentLevel.gameObject);
        }
        // Select conversation
        ConversationData conversation = ConversationController.Current.SelectConversation();
        // Select room
        List<Room> options = rooms.FindAll(a => a.MatchesDemands(conversation)); // TBA - add room demands for conversations
        selectedRoom = options[Random.Range(0, options.Count)];
        Debug.Log("Selected room: " + selectedRoom.Name);
        // Load room
        Set = selectedRoom.TileSet;
        PaletteController.Current.BackgroundPalettes[0] = Set.Palette1;
        PaletteController.Current.BackgroundPalettes[1] = Set.Palette2;
        // Map
        currentLevel = Instantiate(new GameObject(), transform).transform;
        Map = new Tile[MapSize.x, MapSize.y];
        for (int i = 0; i < MapSize.x; i++)
        {
            for (int j = 0; j < MapSize.y; j++)
            {
                int tileID = selectedRoom.Map[i, j];
                Tile newTile = Instantiate(Set.Tiles[tileID].gameObject, currentLevel).GetComponent<Tile>();
                newTile.transform.position = new Vector2(TileSize * i, -TileSize * j);
                newTile.gameObject.SetActive(true);
                Map[i, j] = newTile;
            }
        }
        // Play conversation
        ConversationPlayer.Current.Play(conversation);
    }
    public Unit CreatePlayerUnit(string name)
    {
        Unit unit = Instantiate(BaseUnit.gameObject, currentLevel).GetComponent<Unit>();
        unit.Name = name;
        unit.Level = LevelNumber;
        unit.TheTeam = Team.Player;
        unit.Class = UnitClassData.UnitClasses.Find(a => a.Unit == unit.Name).Class;
        GrowthsStruct unitGrowths;
        unit.Stats = new Stats();
        unit.Stats.Growths = (unitGrowths = UnitClassData.UnitGrowths.Find(a => a.Name == unit.Name)).Growths;
        unit.Flies = unitGrowths.Flies;
        unit.Stats += unit.Stats.GetLevelUp(LevelNumber);
        unit.Weapon = UnitClassData.ClassBaseWeapons.Find(a => a.ClassName == unit.Class);
        AssignUnitMapAnimation(unit);
        unit.gameObject.SetActive(true);
        return unit;
    }
    private void AssignUnitMapAnimation(Unit unit)
    {
        Instantiate(UnitClassData.ClassAnimations.Find(a => a.Name == unit.Class).Animation, unit.transform).Renderer = unit.GetComponent<SpriteRenderer>();
    }
    public void LoadLevelUnits()
    {
        List<Unit> playerCharacters = PlayerUnits;
        // Units
        List<string> unitDatas = selectedRoom.Units;
        int numPlayers = 0;
        for (int i = 0; i < unitDatas.Count; i++)
        {
            Unit unit = Instantiate(BaseUnit.gameObject, currentLevel).GetComponent<Unit>();
            string[] parts = unitDatas[i].Split(',');
            unit.TheTeam = (Team)int.Parse(parts[0]);
            GrowthsStruct unitGrowths;
            if (unit.TheTeam == Team.Player)
            {
                unit.Name = parts[1];
                if (unit.Name == "P" && playerCharacters.Count > numPlayers + 1)
                {
                    Destroy(unit);
                    unit = playerCharacters[++numPlayers];
                    unit.Health = unit.Stats.MaxHP;
                    unit.Pos = new Vector2Int(int.Parse(parts[4]), int.Parse(parts[5]));
                    MapObjects.Add(unit);
                    continue;
                }
                else if (unit.Name == "Frogman" && playerCharacters.Count > 0)
                {
                    Destroy(unit);
                    unit = playerCharacters[0];
                    unit.Health = unit.Stats.MaxHP;
                    unit.Pos = new Vector2Int(int.Parse(parts[4]), int.Parse(parts[5]));
                    cursorPos = unit.Pos; // Auto-cursor
                    MapObjects.Add(unit);
                    continue;
                }
                unit.Class = UnitClassData.UnitClasses.Find(a => a.Unit == unit.Name).Class;
                unit.Stats.Growths = (unitGrowths = UnitClassData.UnitGrowths.Find(a => a.Name == unit.Name)).Growths;
            }
            else
            {
                unit.Name = unit.TheTeam.ToString();
                unit.Class = parts[1];
                unit.Stats.Growths = (unitGrowths = UnitClassData.ClassGrowths.Find(a => a.Name == unit.Class)).Growths;
                unit.MovementMarker = EnemyMarker;
                if (parts.Length > 6)
                {
                    unit.ReinforcementTurn = int.Parse(parts[6]);
                    unit.Statue = parts[7] == "T";
                }
            }
            unit.Flies = unitGrowths.Flies;
            unit.Stats += unit.Stats.GetLevelUp(int.Parse(parts[2]));
            unit.Level = int.Parse(parts[2]);
            unit.Weapon = UnitClassData.ClassBaseWeapons.Find(a => a.ClassName == unit.Class);
            unit.AIType = (AIType)int.Parse(parts[3]);
            unit.Pos = new Vector2Int(int.Parse(parts[4]), int.Parse(parts[5]));
            if (unit.Name == "Frogman")
            {
                cursorPos = unit.Pos; // Auto-cursor
            }
            AssignUnitMapAnimation(unit);
            unit.gameObject.SetActive(true);
        }
    }
    private int Sign(float number)
    {
        return number < 0 ? -1 : (number > 0 ? 1 : 0);
    }

    private class Room
    {
        public string Name;
        public int RoomNumber;
        public int[,] Map;
        public List<string> Units;
        public TileSet TileSet;
        public bool MatchesDemands(ConversationData conversation)
        {
            if (RoomNumber != Current.LevelNumber)
            {
                return false;
            }
            foreach (var demand in conversation.Demands)
            {
                if (demand[0] == '!')
                {
                    if (MeetsDemand(demand.Substring(1)))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!MeetsDemand(demand))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private bool MeetsDemand(string demand)
        {
            string[] parts = demand.Split(':');
            switch (parts[0])
            {
                case "hasCharacter":
                    return Units.Contains(parts[1]);
                case "charactersAlive":
                    // Find number of returning playable characters in map (excluding Frogman and recruitments)
                    int count = 0;
                    foreach (string unit in Units)
                    {
                        if (unit.Split(',')[1] == "P")
                        {
                            // Is player
                            count++;
                        }
                    }
                    int targetNumber = int.Parse(parts[1].Substring(1));
                    // Format: charactersAlive:?X, ex. charactersAlive:>2
                    switch (parts[1][0])
                    {
                        case '>':
                            return count > targetNumber;
                        case '<':
                            return count < targetNumber;
                        case '=':
                            return count == targetNumber;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return true;
        }
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