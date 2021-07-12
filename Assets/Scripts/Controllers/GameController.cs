using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public enum Objective { Rout, Boss, Escape, Survive }
    public static GameController Current;
    [Header("Rooms data")]
    public Vector2Int MapSize;
    public float TileSize;
    public List<TileSet> TileSets;
    public List<UnitReplacement> UnitReplacements;
    public List<TextAsset> Rooms;
    [Header("UI")]
    public RectTransform UITileInfoPanel;
    public Text UITileInfo;
    public RectTransform UIUnitInfoPanel;
    public Text UIUnitInfo;
    public RectTransform UIFightPanel;
    public MiniBattleStatsPanel UIAttackerPanel;
    public MiniBattleStatsPanel UIDefenderPanel;
    public TurnAnimation TurnAnimation;
    [Header("Mid-battle screens")]
    public GameObject Battle;
    public GameObject StatusScreen;
    public GameObject LevelUpScreen;
    public GameObject PauseMenu;
    public GameObject DifficultyMenu;
    public GameObject PartTitle;
    [Header("Other data controllers")]
    public UnitClassData UnitClassData;
    public LevelMetadataController LevelMetadataController;
    [Header("Debug")] // TODO: Move all this (and related code) to a seperate class
    public bool DebugStartAtEndgame;
    public int DebugEndgameLevel;
    public List<string> DebugUnits;
    public bool DebugUnlimitedMove;
    public bool DebugOPPlayers;
    [Header("Objects")]
    public GameObject CameraBlackScreen; // Fixes an annoying UI bug
    public GameObject Cursor;
    public GameObject Canvas;
    public Unit BaseUnit;
    public Marker EnemyMarker;
    public Marker EnemyAttackMarker;
    public PointerMarker PointerMarker;
    public GameObject EscapeMarker;
    [HideInInspector]
    public int LevelNumber;
    [HideInInspector]
    public LevelMetadata LevelMetadata;
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
    public Unit Target; // Very bad workaround
    [HideInInspector]
    public int Turn;
    [HideInInspector]
    public int NumDeadPlayerUnits; // Count for stats - maybe move to a different class? Listeners? GameController should probably have listeners anyway.
    protected Difficulty difficulty;
    private List<Room> rooms;
    private Team currentPhase = Team.Player;
    private float cursorMoveDelay;
    private float enemyMoveDelayCount;
    private Vector2Int previousPos = new Vector2Int(-1, -1);
    private Camera main;
    private Transform currentMapObject;
    private Transform currentUnitsObject;
    private bool checkPlayerDead;
    private bool checkEndTurn;
    private Room selectedRoom;
    private int currentKnowledge;
    private int enemyCount; // To increase performance
    private List<Unit> playerUnitsCache;
    public List<Unit> PlayerUnits
    {
        get
        {
            if (playerUnitsCache == null)
            {
                Debug.Log("Loading player units...");
                playerUnitsCache = new List<Unit>();
                string[] playerUnits = SavedData.Load<string>("PlayerDatas").Split('\n');
                for (int i = 0; i < playerUnits.Length - 1; i++)
                {
                    Unit unit = CreateUnit();
                    unit.Load(playerUnits[i]);
                    unit.name = "Unit" + unit.Name;
                    Debug.Log("Loading " + unit.Name);
                    AssignUnitMapAnimation(unit, UnitClassData.ClassDatas.Find(a => a.Name == unit.Class));
                    unit.gameObject.SetActive(true);
                    playerUnitsCache.Add(unit);
                }
            }
            return playerUnitsCache;
        }
    }
    public int NumRuns
    {
        get
        {
            return SavedData.Load<int>("NumRuns");
        }
        set
        {
            SavedData.Save<int>("NumRuns", value);
        }
    }
    protected Vector2Int cursorPos
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
    private List<Unit> units
    {
        get
        {
            return MapObjects.Where(a => a is Unit).Cast<Unit>().ToList();
        }
    }
    private Unit frogman
    {
        get
        {
            return units.Find(a => a.Name == StaticGlobals.MAIN_CHARACTER_NAME);
        }
    }
    private Vector2Int _escapePos;
    private Vector2Int escapePos
    {
        get
        {
            return _escapePos;
        }
        set
        {
            _escapePos = value;
            EscapeMarker.transform.position = new Vector3(value.x * TileSize, -value.y * TileSize, EscapeMarker.transform.position.z);
        }
    }

    protected virtual void Awake()
    {
        /*
         * I had a few different ideas:
         * -Current
         * -Dungeon: floors and walls, and generate straight lines
         * -Roads: continue random number of steps in a direction, change. Randomaly split/stop.
         */
        Current = this;
        main = Camera.main;
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
            room.TileSet = TileSets.Find(a => a.Name == selectedRoom[2]);
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
        // Init unit replacements
        UnitReplacements.ForEach(a => a.Init());
        // Awake enemy marker
        EnemyMarker.GetComponent<PalettedSprite>().Awake();
    }
    private void Start()
    {
        playerUnitsCache = new List<Unit>();
        if (DebugStartAtEndgame)
        {
            LevelNumber = DebugEndgameLevel;
            playerUnitsCache = new List<Unit>();
            foreach (string unit in DebugUnits)
            {
                PlayerUnits.Add(CreatePlayerUnit(unit));
            }
            if (DebugUnlimitedMove)
            {
                foreach (Unit unit in PlayerUnits)
                {
                    unit.Movement = 50;
                    unit.Flies = true;
                }
            }
            if (DebugOPPlayers)
            {
                foreach (Unit unit in PlayerUnits)
                {
                    unit.Stats += unit.AutoLevel(50);
                }
            }
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
    protected virtual void Update()
    {
        if (MidBattleScreen.HasCurrent) // For ConversationPlayer
        {
            UIUnitInfoPanel.gameObject.SetActive(false);
            UIFightPanel.gameObject.SetActive(false);
            Cursor.gameObject.SetActive(false);
            return;
        }
        if (CheckGameState())
        {
            return;
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
                        Control.GetAxisInt(Control.Axis.X),
                        Control.GetAxisInt(Control.Axis.Y)) * TileSize;
                    Cursor.transform.position = new Vector3(
                        Mathf.Clamp(cursorPos.x, 0, MapSize.x - 1) * TileSize,
                        -Mathf.Clamp(cursorPos.y, 0, MapSize.y - 1) * TileSize,
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
                HandleAButton();
            }
            else if (Control.GetButtonDown(Control.CB.B))
            {
                HandleBButton();
            }
            else if (Control.GetButtonDown(Control.CB.Select))
            {
                HandleSelectButton();
            }
            else if (Control.GetButtonDown(Control.CB.Start))
            {
                HandleStartButton();
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
                if (enemyCount == -1)
                {
                    enemyCount = units.FindAll(a => a.TheTeam != Team.Player).Count;
                }
                UIUnitInfoPanel.gameObject.SetActive(true);
                if (unit != null)
                {
                    UIUnitInfo.text = unit.ToString() + "\nHP:" + unit.Health + "/" + unit.Stats.MaxHP;
                    UIUnitInfoPanel.GetComponent<PalettedSprite>().Palette = (int)unit.TheTeam;
                }
                else
                {
                    UIUnitInfo.text = GetInfoObjectiveText();
                    UIUnitInfoPanel.GetComponent<PalettedSprite>().Palette = 3;
                }
                UIUnitInfoPanel.anchorMin = anchor;
                UIUnitInfoPanel.anchorMax = anchor;
                UIUnitInfoPanel.pivot = anchor;
                if (InteractState != InteractState.None)
                {
                    UIFightPanel.gameObject.SetActive(true);
                    anchor.y = 0.5f;
                    UIFightPanel.anchorMin = anchor;
                    UIFightPanel.anchorMax = anchor;
                    UIFightPanel.pivot = anchor;
                    DisplayBattleForecast(Selected, unit);
                    DisplayBattleForecast(unit, Selected, true);
                }
                else
                {
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
        // End Interact/UI code
        EnemyAI();
    }
    /// <summary>
    /// Does all every-frame checks (mostly win/lose and handling unit death). Returns true if a side won.
    /// </summary>
    /// <returns>True if the level ended (aka don't activate OnFinishAnimation), false otherwise.</returns>
    public bool CheckGameState()
    {
        CheckDifficulty();
        if (checkPlayerDead)
        {
            if (CheckConveresationWait()) // Most characterNumber/alive/whatever commands
            {
                return true;
            }
            if (frogman == null)
            {
                // Lose
                Lose();
                return true;
            }
            else if (CheckPlayerWin())
            {
                // Win
                RemoveMarkers();
                InteractState = InteractState.None; // To prevent weird corner cases.
                ConversationPlayer.Current.PlayPostBattle();
                return true;
            }
            if (!GameCalculations.PermaDeath) // "Kill" player units when perma-death is off
            {
                List<Unit> playerDeadUnits = units.FindAll(a => a.TheTeam == Team.Player && a.Statue);
                playerDeadUnits.ForEach(a => a.Pos = Vector2Int.one * -1);
            }
            enemyCount = units.FindAll(a => a.TheTeam != Team.Player).Count;
            checkPlayerDead = false;
        }
        if (checkEndTurn)
        {
            if (units.Find(a => a.TheTeam == currentPhase && !a.Moved) == null)
            {
                RemoveMarkers();
                Team current = currentPhase;
                do
                {
                    current = (Team)(((int)current + 1) % 3);
                    if (current == currentPhase)
                    {
                        throw new System.Exception("Infinite loop in EndTurn - no living units, probably");
                    }
                } while (units.Find(a => a.TheTeam == current) == null);
                Debug.Log("Begin " + current + " phase, units: " + string.Join(", ", units.FindAll(a => a.TheTeam == current)));
                StartPhase(current);
                if (CheckPlayerWin(Objective.Survive))
                {
                    // Win
                    ConversationPlayer.Current.PlayPostBattle();
                    return true;
                }
                TurnAnimation.ShowTurn(currentPhase);
            }
            else if (CheckPlayerWin(Objective.Escape))
            {
                // Win
                ConversationPlayer.Current.PlayPostBattle();
                return true;
            }
            checkEndTurn = false;
        }
        return false;
    }
    public void ManuallyEndTurn()
    {
        units.FindAll(a => a.TheTeam == currentPhase && !a.Moved).ForEach(a => a.Moved = true);
        checkEndTurn = true;
    }
    protected virtual void HandleAButton()
    {
        InteractWithTile(cursorPos.x, cursorPos.y);
    }
    protected virtual void HandleBButton()
    {
        switch (InteractState)
        {
            case InteractState.None: // View chosen character's stats
                if (!RemoveMarkers()) // If not viewing enemy range
                {
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
            case InteractState.Move:// Undo turn in move/attack
                Selected = null;
                RemoveMarkers();
                InteractState = InteractState.None;
                break;
            case InteractState.Attack:
                Selected.MoveTo(Selected.PreviousPos, true);
                Selected.Interact(InteractState = InteractState.None);
                break;
            default:
                break;
        }
    }
    protected virtual void HandleSelectButton()
    {
        switch (InteractState)
        {
            case InteractState.None:
                // Select next unit
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
            case InteractState.Attack:
                // Select selected unit
                cursorPos = Selected.Pos;
                break;
            default:
                break;
        }
    }
    protected virtual void HandleStartButton()
    {
        // Only works in None
        switch (InteractState)
        {
            case InteractState.None:
                MenuController pauseMenu = Instantiate(PauseMenu, Canvas.transform).GetComponentInChildren<MenuController>();
                MidBattleScreen.Set(pauseMenu, true);
                break;
            case InteractState.Move:
                break;
            case InteractState.Attack:
                break;
            default:
                break;
        }
    }
    protected virtual void CheckDifficulty()
    {
        if (difficulty == Difficulty.NotSet) // Set during the first level, so the player will have played/skipped the tutorial before selecting difficulty
        {
            if (DifficultyMenu != null) // Get difficulty
            {
                DifficultyMenu.SetActive(true);
                MidBattleScreen.Set(DifficultyMenu.GetComponentInChildren<MenuController>(), true);
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
                            unit.Stats = unit.Stats.GetLevel0Stat() + unit.AutoLevel(++unit.Level);
                            unit.Health = unit.Stats.MaxHP;
                        }
                    }
                }
            }
        }
    }
    private void EnemyAI()
    {
        // Enemy AI. TBA: Change to check which team is player controlled and which is computer controlled.
        if (currentPhase != Team.Player)
        {
            if (enemyMoveDelayCount > MapAnimationsController.Current.DelayTime) // Delay once before the phase begins
            {
                Unit currentEnemy = units.Find(a => a.TheTeam == currentPhase && !a.Moved);
                // AI
                currentEnemy.AI(units);
            }
            else
            {
                enemyMoveDelayCount += Time.deltaTime;
            }
        }
    }
    private void DisplayBattleForecast(Unit origin, Unit target, bool reverse = false)
    {
        MiniBattleStatsPanel panel = reverse ? UIDefenderPanel : UIAttackerPanel;
        panel.DisplayBattleForecast(origin, target, reverse);
    }
    public void InteractWithTile(int x, int y)
    {
        MapObjectsAtPos(x, y).ForEach(a => a.Interact(InteractState));
    }
    public bool MarkerAtPos<T>(int x, int y) where T : Marker
    {
        return MapObjects.Find(a => a.Pos.x == x && a.Pos.y == y && a is T) == null;
    }
    public bool MarkerAtPos<T>(Vector2Int pos) where T : Marker
    {
        return MarkerAtPos<T>(pos.x, pos.y);
    }
    private List<MapObject> MapObjectsAtPos(int x, int y)
    {
        return MapObjects.FindAll(a => a.Pos.x == x && a.Pos.y == y);
    }
    private List<MapObject> MapObjectsAtPos(Vector2Int pos)
    {
        return MapObjectsAtPos(pos.x, pos.y);
    }
    /// <summary>
    /// Removes all markers.
    /// </summary>
    /// <returns>True if there were any markers, false otherwise.</returns>
    public bool RemoveMarkers()
    {
        previousPos = new Vector2Int(-1, -1);
        List<MapObject> markers = MapObjects.FindAll(a => a is Marker);
        if (markers.Count > 0)
        {
            markers.ForEach(a => Destroy(a.gameObject));
            return true;
        }
        return false;
    }
    public void FinishMove(Unit unit)
    {
        unit.Moved = true;
        FinishMoveDead();
    }
    public void FinishMoveDead()
    {
        RemoveMarkers();
        InteractState = InteractState.None;
        checkEndTurn = true;
        if (!MidBattleScreen.HasCurrent) // Prevent the extra frame of waiting
        {
            CheckGameState();
        }
        CrossfadeMusicPlayer.Current.SwitchBattleMode(false);
    }
    public Unit FindUnitAtPos(int x, int y)
    {
        return (Unit)MapObjects.Find(a => a is Unit && a.Pos.x == x && a.Pos.y == y);
    }
    public void StartPhase(Team team)
    {
        currentPhase = team;
        enemyMoveDelayCount = 0;
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
            GameCalculations.EndTurnEvents(units); // Pretty bad code - replace with listeners? But wouldn't work with game calculations...
        }
        CheckConveresationWait(); // Wait for turn events
    }
    public void TransitionToMidBattleScreen(MidBattleScreen screen)
    {
        transform.parent.gameObject.SetActive(false);
        MidBattleScreen.Set(screen, true);
        CameraBlackScreen.SetActive(true);
    }
    public void KillUnit(Unit unit)
    {
        if (!GameCalculations.PermaDeath && unit.TheTeam == Team.Player && unit.Name != StaticGlobals.MAIN_CHARACTER_NAME) // No perma-death
        {
            Debug.Log("Pseudo-killed " + unit.Name);
            unit.Statue = true;
            unit.ReinforcementTurn = int.MaxValue;
            unit.PreviousPos = unit.Pos;
            unit.Pos = -Vector2Int.one;
        }
        else // Perma-death
        {
            if (PlayerUnits.Contains(unit))
            {
                PlayerUnits.Remove(unit);
                NumDeadPlayerUnits++;
            }
            Destroy(unit.gameObject);
        }
        checkPlayerDead = true; // Since I need to wait for the battle animation to finish first
    }
    public void Win()
    {
        LevelNumber++;
        currentKnowledge++;
        SavedData.SaveAll(SaveMode.Slot);
        PlayersLevelUp();
    }
    private void PlayersLevelUp()
    {
        List<Unit> playerCharacters = units.Where(a => a.TheTeam == Team.Player).ToList();
        // Custom level-up system
        foreach (Unit character in playerCharacters)
        {
            character.Level++;
        }
        LevelUpController levelUpController = Instantiate(LevelUpScreen).GetComponentInChildren<LevelUpController>();
        levelUpController.Players = playerCharacters;
        TransitionToMidBattleScreen(levelUpController);
    }
    public void PlayersSave()
    {
        // Save player characters
        List<Unit> playerCharacters = PlayerUnits;
        playerCharacters.ForEach(a => a.Statue = false); // Revive "dead" units on Easy
        playerCharacters.Sort((a, b) => a.Name == StaticGlobals.MAIN_CHARACTER_NAME ? -1 : (b.Name == StaticGlobals.MAIN_CHARACTER_NAME ? 1 : 0));
        playerCharacters = playerCharacters.ToList();
        string saveData = "";
        playerCharacters.ForEach(a => saveData += a.Save() + "\n");
        SavedData.Save("PlayerDatas", saveData);
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            Debug.Log(playerCharacters[i].Save());
        }
    }
    public void CreateLevel()
    {
        List<Unit> playerCharacters = PlayerUnits;
        // Select conversation
        Debug.Log(string.Join(", ", playerCharacters));
        ConversationData conversation = ConversationController.Current.SelectConversation();
        // Select room
        List<Room> options = rooms.FindAll(a => a.MatchesDemands(conversation)); // TBA - add room demands for conversations
        selectedRoom = options[UnityEngine.Random.Range(0, options.Count)];
        Debug.Log("Selected room: " + selectedRoom.Name);
        // Clear previous level
        if (currentMapObject != null)
        {
            Destroy(currentMapObject.gameObject);
        }
        if (currentUnitsObject != null)
        {
            Destroy(currentUnitsObject.gameObject);
        }
        Turn = 1;
        // Set palettes
        SetPalettesFromMetadata(LevelMetadata = LevelMetadataController[LevelNumber]);
        // Room-specific behaviours
        if (selectedRoom.Objective == Objective.Escape)
        {
            string[] parts = selectedRoom.ObjectiveData.Split(':');
            escapePos = new Vector2Int(int.Parse(parts[0]), int.Parse(parts[1]));
            EscapeMarker.gameObject.SetActive(true);
        }
        else
        {
            EscapeMarker.gameObject.SetActive(false);
        }
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
        Unit unit = CreateUnit();
        unit.Name = name;
        unit.name = "Unit" + name;
        unit.Level = level;
        unit.TheTeam = Team.Player;
        UnitData unitData = UnitClassData.UnitDatas.Find(a => a.Name == unit.Name);
        unit.Class = unitData.Class;
        ClassData classData = UnitClassData.ClassDatas.Find(a => a.Name == unit.Class);
        unit.Stats = new Stats();
        unit.Stats.Growths = unitData.Growths.Values;
        unit.Flies = classData.Flies;
        unit.Weapon = classData.Weapon;
        unit.Inclination = unitData.Inclination;
        unit.LoadInclination();
        AssignUnitMapAnimation(unit, classData);
        unit.Stats += unit.AutoLevel(level);
        unit.Init();
        return unit;
    }
    public Unit CreateEnemyUnit(string name, int level, Team team, bool canReplace)
    {
        Unit unit = CreateUnit();
        unit.TheTeam = team;
        unit.Name = name;
        unit.name = "Unit" + name;
        if (canReplace)
        {
            UnitReplacement replacement = UnitReplacements.Find(a => a.Class == name);
            if (replacement != null)
            {
                name = replacement.Get();
            }
        }
        unit.Class = name;
        ClassData classData = UnitClassData.ClassDatas.Find(a => a.Name == unit.Class);
        unit.Stats.Growths = classData.Growths.Values;
        unit.MovementMarker = EnemyMarker;
        unit.AttackMarker = EnemyAttackMarker;
        unit.Flies = classData.Flies;
        unit.Inclination = classData.Inclination;
        unit.Stats += unit.AutoLevel(level);
        unit.Level = level;
        unit.Weapon = classData.Weapon;
        AssignUnitMapAnimation(unit, classData);
        unit.Priorities.Set(LevelMetadata.TeamDatas[(int)team].AI);
        return unit;
    }
    private void AssignUnitMapAnimation(Unit unit, ClassData classData)
    {
        AdvancedSpriteSheetAnimation animation = Instantiate(UnitClassData.BaseAnimation, unit.transform);
        animation.Renderer = unit.GetComponent<SpriteRenderer>();
        animation.Animations[0].SpriteSheet = classData.MapSprite;
        animation.Start();
        animation.Activate(0);
    }
    private Room LoadRoom(string roomName = "")
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
        return room;
    }
    public void LoadMap(string roomName = "")
    {
        Room room = LoadRoom(roomName);
        // Clear previous level
        if (currentMapObject != null)
        {
            Destroy(currentMapObject.gameObject);
        }
        // Load room
        Set = room.TileSet;
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
    public void LoadLevelUnits(string roomName = "", Team? ofTeam = null)
    {
        Room room = LoadRoom(roomName);
        // Clear previous level
        if (currentUnitsObject != null)
        {
            PlayersSave();
            DestroyImmediate(currentUnitsObject.gameObject);
            playerUnitsCache = null;
        }
        currentUnitsObject = new GameObject("UnitsObject").transform;
        currentUnitsObject.parent = transform;
        List<Unit> playerCharacters = PlayerUnits.Where(a => a != null).ToList();
        if ((ofTeam ?? Team.Player) != Team.Player)
        {
            // "Remove" player units
            foreach (Unit player in playerCharacters)
            {
                player.Pos = Vector2Int.one * -1;
                player.transform.parent = currentUnitsObject;
            }
        }
        // Units
        List<string> unitDatas = room.Units;
        int numPlayers = 0;
        for (int i = 0; i < unitDatas.Count; i++)
        {
            string[] parts = unitDatas[i].Split(',');
            Team team = (Team)int.Parse(parts[0]);
            if (team != (ofTeam ?? team))
            {
                continue;
            }
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
                    }
                    continue;
                }
                else if (name == StaticGlobals.MAIN_CHARACTER_NAME && playerCharacters.Count > 0)
                {
                    unit = playerCharacters[0];
                    unit.transform.parent = currentUnitsObject;
                    unit.Health = unit.Stats.MaxHP;
                    unit.Pos = new Vector2Int(int.Parse(parts[4]), int.Parse(parts[5]));
                    cursorPos = unit.Pos; // Auto-cursor
                    continue;
                }
                else
                {
                    unit = CreatePlayerUnit(name, int.Parse(parts[2]));
                    unit.Health = unit.Stats.MaxHP;
                    unit.Pos = new Vector2Int(int.Parse(parts[4]), int.Parse(parts[5]));
                    PlayerUnits.Add(unit);
                    if (name != StaticGlobals.MAIN_CHARACTER_NAME)
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
                Unit unit = CreateEnemyUnit(name, int.Parse(parts[2]), team, !(parts.Length > 6 && parts[7] == "T"));
                if (parts.Length > 6)
                {
                    unit.ReinforcementTurn = int.Parse(parts[6]);
                    unit.Statue = parts[7] == "T";
                }
                unit.AIType = (AIType)int.Parse(parts[3]);
                unit.Pos = new Vector2Int(int.Parse(parts[4]), int.Parse(parts[5]));
                if (unit.ReinforcementTurn > 0 && !unit.Statue)
                {
                    unit.PreviousPos = unit.Pos;
                    unit.Pos = Vector2Int.one * -1;
                }
                unit.gameObject.SetActive(true);
            }
        }
        enemyCount = -1;
        currentPhase = Team.Player;
        interactable = true;
    }
    public void ShowDangerArea()
    {
        units.FindAll(a => a.TheTeam != Team.Player && !a.Moved).ForEach(a => a.MarkDangerArea());
    }
    public string GetPauseObjectiveText()
    {
        switch (selectedRoom.Objective)
        {
            case Objective.Rout:
                return "Rout the enemy";
            case Objective.Boss:
                return "Defeat " + selectedRoom.ObjectiveData;
            case Objective.Escape:
                return "Escape!";
            case Objective.Survive:
                return "Survive " + selectedRoom.ObjectiveData + " turn";
            default:
                break;
        }
        return "";
    }
    public string GetInfoObjectiveText()
    {
        switch (selectedRoom.Objective)
        {
            case Objective.Rout:
                return "Kill " + enemyCount + "\nenemies";
            case Objective.Boss:
                return "Defeat\n" + selectedRoom.ObjectiveData;
            case Objective.Escape:
                return "Frogman\nto mark";
            case Objective.Survive:
                return "Survive\n" + (int.Parse(selectedRoom.ObjectiveData) - Turn + 1) + " turns";
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
    public void Lose()
    {
        SetPalettesFromMetadata(LevelMetadataController[0]); // Fix Torment palette
        NumRuns++; // To prevent abuse, like the knowledge
        SavedData.Append("Knowledge", "Amount", currentKnowledge);
        SavedData.SaveAll(SaveMode.Slot);
        SceneController.LoadScene("GameOver");
    }
    public int LeftToMove()
    {
        return units.FindAll(a => a.TheTeam == Team.Player && !a.Moved).Count;
    }
    public int GameSpeed()
    {
        return (SavedData.Load("GameSpeed", 0, SaveMode.Global) == 1 ^ Control.GetButton(Control.CB.B)) ? 2 : 1;
    }
    public bool IsValidPos(int x, int y)
    {
        return x >= 0 && y >= 0 && x < MapSize.x && y < MapSize.y;
    }
    public bool IsValidPos(Vector2Int pos)
    {
        return IsValidPos(pos.x, pos.y);
    }
    // TODO: Replace all instances of "frogman/boss dead" with this function.
    public bool CheckUnitAlive(string name)
    {
        return GetNamedUnit(name) != null;
    }
    public Unit GetNamedUnit(string name)
    {
        return units.Find(a => a.ToString() == name);
    }
    public void AssignGenericPortraitsToUnits(Team? team = null)
    {
        List<Unit> targetUnits = units.FindAll(a => a.TheTeam == (team ?? a.TheTeam) && LevelMetadata.TeamDatas[(int)a.TheTeam].PortraitLoadingMode == PortraitLoadingMode.Generic);
        List<Portrait> portraits = PortraitController.Current.GeneratedGenericPortraits.Values.ToList();
        for (int i = 0; i < portraits.Count && i < targetUnits.Count; i++)
        {
            targetUnits[i].SetIcon(portraits[i]);
        }
    }
    private bool CheckPlayerWin()
    {
        switch (selectedRoom.Objective)
        {
            case Objective.Rout:
                return units.FindAll(a => a.TheTeam != Team.Player && a.ReinforcementTurn <= 0).Count == 0;
            case Objective.Boss:
                return !CheckUnitAlive(selectedRoom.ObjectiveData);
            case Objective.Escape:
                return frogman.Pos == escapePos;
            case Objective.Survive:
                return Turn > int.Parse(selectedRoom.ObjectiveData) || units.FindAll(a => a.TheTeam != Team.Player && a.ReinforcementTurn <= 0).Count == 0;
            default:
                throw new System.Exception("No objective!");
        }
    }
    private bool CheckPlayerWin(Objective toCheck)
    {
        if (toCheck != selectedRoom.Objective)
        {
            return false;
        }
        return CheckPlayerWin();
    }
    /// <summary>
    /// Checks whether the current conversation's wait requiremenet was met.
    /// </summary>
    /// <returns>True if it was (aka resumes conversation), false if it wasn't.</returns>
    private bool CheckConveresationWait()
    {
        // For now, always check it (change to until it's done, as 99% of the time conversations won't have wait commands)
        if (ConversationPlayer.Current != null)
        {
            return ConversationPlayer.Current.CheckWait();
        }
        return false;
    }
    private void SetPalettesFromMetadata(LevelMetadata metadata)
    {
        for (int i = 0; i < 3; i++)
        {
            PaletteController.Current.SpritePalettes[i] = metadata.TeamDatas[i].Palette;
        }
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
                case "mapID":
                    return Name == parts[1];
                default:
                    break;
            }
            return true;
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

    [System.Serializable]
    public class UnitReplacement
    {
        public string Class;
        public List<string> ReplacedBy;

        public void Init()
        {
            ReplacedBy.Add(Class);
        }

        public string Get()
        {
            return ReplacedBy[Random.Range(0, ReplacedBy.Count)];
        }
    }
}