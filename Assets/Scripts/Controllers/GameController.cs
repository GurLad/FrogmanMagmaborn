using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public enum Difficulty { NotSet, Easy, Medium, Hard }
public class GameController : MonoBehaviour
{
    public enum Objective { Rout, Boss }
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
    public InclinationIndicator UIAttackerInclination;
    public PalettedSprite UIDefenderPanel;
    public Text UIDefenderInfo;
    public InclinationIndicator UIDefenderInclination;
    public TurnAnimation TurnAnimation;
    [Header("Mid-battle screens")]
    public GameObject Battle;
    public GameObject StatusScreen;
    public GameObject LevelUpScreen;
    public GameObject PauseMenu;
    public GameObject DifficultyMenu;
    [Header("Misc")]
    public float EnemyAIMoveDelay = 2;
    [Header("Torment palette")]
    public Palette TormentPalette;
    [Header("Debug")]
    public bool StartAtEndgame;
    public int SpeedMultiplier;
    [Header("Objects")]
    public GameObject CameraBlackScreen; // Fixes an annoying UI bug
    public GameObject Cursor;
    public GameObject Canvas;
    public Unit BaseUnit;
    public UnitClassData UnitClassData;
    public Marker EnemyMarker;
    public Marker EnemyAttackMarker;
    public PointerMarker PointerMarker;
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
    [HideInInspector]
    public int Turn;
    private Palette basePalette;
    private List<Room> rooms;
    private Team currentPhase = Team.Player;
    private float cursorMoveDelay;
    private float enemyMoveDelayCount;
    private Vector2Int previousPos = new Vector2Int(-1, -1);
    private Camera main;
    private Transform currentMapObject;
    private Transform currentUnitsObject;
    private bool checkPlayerDead;
    private Room selectedRoom;
    private int currentKnowledge;
    private Difficulty difficulty;
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
                    Unit unit = CreateUnit();
                    unit.Load(playerUnits[i]);
                    unit.name = "Unit" + unit.Name;
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
    public static string TeamToString(Team team)
    {
        if (Current.LevelNumber % 4 == 0 && team == Team.Monster)
        {
            return "Torment";
        }
        return team.ToString();
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
            string[] selectedRoom = Rooms[i].text.Replace("\r", "").Split('\n');
            // Level numer
            room.RoomNumber = int.Parse(selectedRoom[3]);
            // Tile set
            room.TileSet = TileSets1.Find(a => a.Name == selectedRoom[2]);
            // Objective
            string[] objectiveParts = selectedRoom[4].Split(':');
            room.Objective = (Objective)System.Enum.Parse(typeof(Objective), objectiveParts[0]);
            room.ObjectiveData = selectedRoom[4].Substring(selectedRoom[4].IndexOf(':') + 1);
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
        // Set base palette
        basePalette = new Palette(PaletteController.Current.SpritePalettes[1]);
    }
    private void Start()
    {
        Time.timeScale = SpeedMultiplier; // For debugging
        if (StartAtEndgame)
        {
            LevelNumber = 4;
            playerUnitsCache = new List<Unit>();
            PlayerUnits.Add(CreatePlayerUnit("Frogman"));
            PlayerUnits.Add(CreatePlayerUnit("Firbell"));
            PlayerUnits.Add(CreatePlayerUnit("Xirveros"));
        }
        else
        {
            LevelNumber = 1;
            playerUnitsCache = new List<Unit>();
        }
        difficulty = (Difficulty)SavedData.Load("Difficulty", 0);
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
        if (difficulty == Difficulty.NotSet) // Set during the first level, so the player will have played/skipped the tutorial before selecting difficulty
        {
            if (DifficultyMenu != null) // Get difficulty
            {
                DifficultyMenu.SetActive(true);
                MidBattleScreen.Current = DifficultyMenu.GetComponentInChildren<MenuController>();
                return;
            }
            else // Update difficulty
            {
                difficulty = (Difficulty)SavedData.Load("Difficulty", 0);
                if (difficulty != Difficulty.Hard) // Level up player units
                {
                    foreach (Unit unit in units)
                    {
                        if (unit.TheTeam == Team.Player)
                        {
                            unit.Stats = unit.Stats.GetLevel0Stat() + unit.Stats.GetLevelUp(++unit.Level);
                            unit.Health = unit.Stats.MaxHP;
                        }
                    }
                }
            }
        }
        if (checkPlayerDead)
        {
            if (units.Find(a => a.Name == "Frogman") == null)
            {
                // Lose
                SavedData.Appeand("Knowledge", currentKnowledge);
                SceneController.LoadScene("Menu");
            }
            else if (CheckPlayerWin())
            {
                // Win
                ConversationPlayer.Current.PlayPostBattle();
            }
            if (difficulty == Difficulty.Easy) // "Kill" player units on easy
            {
                List<Unit> playerDeadUnits = units.FindAll(a => a.TheTeam == Team.Player && a.Statue);
                playerDeadUnits.ForEach(a => a.Pos = Vector2Int.one * -1);
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
                                ShowDangerArea();
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
            else if (Control.GetButtonDown(Control.CB.Start))
            {
                // Only works in None
                switch (InteractState)
                {
                    case InteractState.None:
                        MenuController pauseMenu = Instantiate(PauseMenu, Canvas.transform).GetComponentInChildren<MenuController>();
                        MidBattleScreen.Current = pauseMenu;
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
                UITileInfo.text = Map[cursorPos.x, cursorPos.y].ToString();
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
                    UIUnitInfo.text = unit.ToString() + "\nHP:" + unit.Health + "/" + unit.Stats.MaxHP;
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
                        UIAttackerInclination.Display(Selected, unit);
                        UIDefenderInclination.Display(unit, Selected);
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
            if (Control.GetButton(Control.CB.B))
            {
                // Speed up
                enemyMoveDelayCount += Time.deltaTime;
            }
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
        if (team == Team.Player)
        {
            Turn++;
        }
    }
    public void TransitionToMidBattleScreen(MidBattleScreen screen)
    {
        transform.parent.gameObject.SetActive(false);
        MidBattleScreen.Current = screen;
        CameraBlackScreen.SetActive(true);
    }
    public void KillUnit(Unit unit)
    {
        if (difficulty == Difficulty.Easy && unit.TheTeam == Team.Player && unit.Name != "Frogman") // No perma-death
        {
            Debug.Log("Pseudo-killed " + unit.Name);
            unit.Statue = true;
            unit.ReinforcementTurn = int.MaxValue;
        }
        else // Perma-death
        {
            MapObjects.Remove(unit);
            Destroy(unit.gameObject);
        }
        checkPlayerDead = true; // Since I need to wait for the battle animation to finish first
    }
    public void Win()
    {
        LevelNumber++;
        currentKnowledge++;
        PlayersLevelUp();
    }
    private void PlayersLevelUp()
    {
        // Save player characters
        List<Unit> playerCharacters = units.Where(a => a.TheTeam == Team.Player).ToList();
        playerCharacters.ForEach(a => a.Statue = false); // Revive "dead" units on Easy
        playerCharacters.Sort((a, b) => a.Name == "Frogman" ? -1 : (b.Name == "Frogman" ? 1 : 0));
        playerCharacters = playerCharacters.ToList();
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
        if (currentMapObject != null)
        {
            Destroy(currentMapObject.gameObject);
        }
        if (currentUnitsObject != null)
        {
            Destroy(currentUnitsObject.gameObject);
        }
        Turn = 1;
        // Set palette for Torment levels
        if (LevelNumber % 4 == 0)
        {
            PaletteController.Current.SpritePalettes[1] = TormentPalette;
        }
        else
        {
            PaletteController.Current.SpritePalettes[1] = basePalette;
        }
        // Select conversation
        ConversationData conversation = ConversationController.Current.SelectConversation();
        // Select room
        List<Room> options = rooms.FindAll(a => a.MatchesDemands(conversation)); // TBA - add room demands for conversations
        selectedRoom = options[Random.Range(0, options.Count)];
        Debug.Log("Selected room: " + selectedRoom.Name);
        // Play conversation
        ConversationPlayer.Current.Play(conversation);
    }
    private Unit CreateUnit()
    {
        Unit unit = Instantiate(BaseUnit.gameObject, currentUnitsObject).GetComponent<Unit>();
        unit.Name = "TEMP";
        unit.name = "UnitTEMP";
        unit.gameObject.SetActive(true);
        return unit;
    }
    public Unit CreatePlayerUnit(string name, int level = -1)
    {
        level = level >= 0 ? level : LevelNumber;
        if (difficulty != Difficulty.Hard && difficulty != Difficulty.NotSet)
        {
            level++;
        }
        Unit unit = CreateUnit();
        unit.Name = name;
        unit.name = "Unit" + name;
        unit.Level = level;
        unit.TheTeam = Team.Player;
        unit.Class = UnitClassData.UnitClasses.Find(a => a.Unit == unit.Name).Class;
        GrowthsStruct unitGrowths;
        unit.Stats = new Stats();
        unit.Stats.Growths = (unitGrowths = UnitClassData.UnitGrowths.Find(a => a.Name == unit.Name)).Growths;
        unit.Flies = unitGrowths.Flies;
        unit.Weapon = UnitClassData.ClassBaseWeapons.Find(a => a.ClassName == unit.Class);
        unit.Inclination = unitGrowths.Inclination;
        int inclination = KnowledgeController.GetInclination(unit.Name);
        if (inclination > 0)
        {
            Debug.Log("Changing inclination!");
            unit.ChangeInclination((Inclination)(inclination - 1));
        }
        AssignUnitMapAnimation(unit);
        unit.Stats += unit.Stats.GetLevelUp(level);
        unit.Init();
        return unit;
    }
    private void AssignUnitMapAnimation(Unit unit)
    {
        Instantiate(UnitClassData.ClassAnimations.Find(a => a.Name == unit.Class).Animation, unit.transform).Renderer = unit.GetComponent<SpriteRenderer>();
    }
    public void LoadMap(string roomName = "")
    {
        Room room;
        if (roomName == "")
        {
            room = selectedRoom;
        }
        else
        {
            room = rooms.Find(a => a.Name == roomName);
            if (room == null)
            {
                throw new System.Exception("No matching room! (" + roomName + ")");
            }
        }
        // Clear previous level
        if (currentMapObject != null)
        {
            Destroy(currentMapObject.gameObject);
        }
        // Load room
        Set = room.TileSet;
        Debug.Log(Set);
        PaletteController.Current.BackgroundPalettes[0] = Set.Palette1;
        PaletteController.Current.BackgroundPalettes[1] = Set.Palette2;
        // Map
        currentMapObject = new GameObject("MapObject").transform;
        currentMapObject.parent = transform;
        Map = new Tile[MapSize.x, MapSize.y];
        for (int i = 0; i < MapSize.x; i++)
        {
            for (int j = 0; j < MapSize.y; j++)
            {
                int tileID = room.Map[i, j];
                Tile newTile = Instantiate(Set.Tiles[tileID].gameObject, currentMapObject).GetComponent<Tile>();
                newTile.transform.position = new Vector2(TileSize * i, -TileSize * j);
                newTile.gameObject.SetActive(true);
                Map[i, j] = newTile;
            }
        }
    }
    public void LoadLevelUnits(string roomName = "")
    {
        Room room;
        if (roomName == "")
        {
            room = selectedRoom;
        }
        else
        {
            room = rooms.Find(a => a.Name == roomName);
            if (room == null)
            {
                throw new System.Exception("No matching room! (" + roomName + ")");
            }
        }
        // Clear previous level
        MapObjects.Clear();
        if (currentUnitsObject != null)
        {
            Destroy(currentUnitsObject.gameObject);
        }
        currentUnitsObject = new GameObject("UnitsObject").transform;
        currentUnitsObject.parent = transform;
        List<Unit> playerCharacters = PlayerUnits;
        // Units
        List<string> unitDatas = room.Units;
        int numPlayers = 0;
        for (int i = 0; i < unitDatas.Count; i++)
        {
            string[] parts = unitDatas[i].Split(',');
            Team team = (Team)int.Parse(parts[0]);
            string name = parts[1];
            if (team == Team.Player)
            {
                Unit unit;
                if (name == "P")
                {
                    if (playerCharacters.Count > numPlayers + 1) // I can't make multiple rooms for evey combination of living characters...
                    {
                        unit = playerCharacters[++numPlayers];
                        unit.transform.parent = currentUnitsObject;
                        unit.Health = unit.Stats.MaxHP;
                        unit.Pos = new Vector2Int(int.Parse(parts[4]), int.Parse(parts[5]));
                        if (!MapObjects.Contains(unit)) // This shouldn't exist - it's because of the weird "no Start" bug on units created with CreatePlayerUnit.
                        {
                            Debug.Log("Why isn't " + unit.Name + " part of MapObjects?");
                            MapObjects.Add(unit);
                        }
                    }
                    continue;
                }
                else if (name == "Frogman" && playerCharacters.Count > 0)
                {
                    unit = playerCharacters[0];
                    unit.transform.parent = currentUnitsObject;
                    unit.Health = unit.Stats.MaxHP;
                    unit.Pos = new Vector2Int(int.Parse(parts[4]), int.Parse(parts[5]));
                    if (!MapObjects.Contains(unit)) // This shouldn't exist - it's because of the weird "no Start" bug on units created with CreatePlayerUnit.
                    {
                        Debug.Log("Why isn't " + unit.Name + " part of MapObjects?");
                        MapObjects.Add(unit);
                    }
                    cursorPos = unit.Pos; // Auto-cursor
                    continue;
                }
                else
                {
                    unit = CreatePlayerUnit(name, int.Parse(parts[2]));
                    unit.Health = unit.Stats.MaxHP;
                    unit.Pos = new Vector2Int(int.Parse(parts[4]), int.Parse(parts[5]));
                    if (name != "Frogman")
                    {
                        Debug.LogWarning("Please refrain from hard placing units in maps. Use P and addUnit event instead.");
                    }
                    else
                    {
                        cursorPos = unit.Pos; // Auto-cursor
                    }
                }
            }
            else // Enemy units
            {
                Unit unit = CreateUnit();
                unit.TheTeam = team;
                unit.name = "Unit" + name;
                GrowthsStruct unitGrowths;
                unit.Name = TeamToString(team);
                unit.Class = parts[1];
                unit.Stats.Growths = (unitGrowths = UnitClassData.ClassGrowths.Find(a => a.Name == unit.Class)).Growths;
                unit.MovementMarker = EnemyMarker;
                unit.AttackMarker = EnemyAttackMarker;
                if (parts.Length > 6)
                {
                    unit.ReinforcementTurn = int.Parse(parts[6]);
                    unit.Statue = parts[7] == "T";
                }
                unit.Flies = unitGrowths.Flies;
                unit.Inclination = unitGrowths.Inclination;
                unit.Stats += unit.Stats.GetLevelUp(int.Parse(parts[2]));
                unit.Level = int.Parse(parts[2]);
                unit.Weapon = UnitClassData.ClassBaseWeapons.Find(a => a.ClassName == unit.Class);
                unit.AIType = (AIType)int.Parse(parts[3]);
                unit.Pos = new Vector2Int(int.Parse(parts[4]), int.Parse(parts[5]));
                if (unit.ReinforcementTurn > 0 && !unit.Statue)
                {
                    unit.PreviousPos = unit.Pos;
                    unit.Pos = Vector2Int.one * -1;
                }
                AssignUnitMapAnimation(unit);
                unit.gameObject.SetActive(true);
            }
        }
        Debug.Log("Units: " + string.Join(", ", units));
        currentPhase = Team.Player;
        interactable = true;
    }
    public void ShowDangerArea()
    {
        units.FindAll(a => a.TheTeam != Team.Player && !a.Moved).ForEach(a => a.MarkDangerArea());
    }
    public string ObjectiveData()
    {
        switch (selectedRoom.Objective)
        {
            case Objective.Rout:
                return "Rout the enemy";
            case Objective.Boss:
                return "Defeat " + selectedRoom.ObjectiveData;
            default:
                break;
        }
        return "";
    }
    public void ShowPointerMarker(Unit origin, int paletteID)
    {
        PointerMarker pointerMarker = Instantiate(PointerMarker.gameObject).GetComponent<PointerMarker>();
        pointerMarker.Pos = origin.Pos;
        pointerMarker.Origin = origin;
        pointerMarker.PalettedSprite.Awake();
        pointerMarker.PalettedSprite.Palette = paletteID;
        pointerMarker.gameObject.SetActive(true);
    }
    private bool CheckPlayerWin()
    {
        switch (selectedRoom.Objective)
        {
            case Objective.Rout:
                return units.FindAll(a => a.TheTeam != Team.Player && a.ReinforcementTurn <= 0).Count == 0;
            case Objective.Boss:
                return units.FindAll(a => a.Class == selectedRoom.ObjectiveData).Count == 0;
            default:
                throw new System.Exception("No objective!");
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
        public Objective Objective;
        public string ObjectiveData;
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
                    return string.Join("|", Units).Contains(parts[1]);
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