using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour, ISuspendable<SuspendDataGameController>
{
    public static readonly Vector2Int MAP_SIZE = new Vector2Int(16, 15);
    public static GameController Current;
    [Header("Map data")]
    public Vector2Int MapSize = new Vector2Int(16, 15);
    public float TileSize;
    [Header("UI")]
    public TurnAnimation TurnAnimation;
    [Header("Mid-battle screens")]
    public GameObject Battle;
    public GameObject StatusScreen;
    public GameObject LevelUpScreen;
    public GameObject PauseMenu;
    public GameObject DifficultyMenu;
    public GameObject PartTitle;
    public GameObject BaseMenu;
    [Header("Other data controllers")]
    public UnitClassData UnitClassData;
    public LevelMetadataController LevelMetadataController;
    public MapController MapController;
    [Header("Debug")] // TODO: Move all this (and related code) to a seperate class
    public DebugOptions DebugOptions;
    [Header("Objects")]
    public GameUIController GameUIController;
    public CursorController Cursor;
    public GameObject Canvas;
    public Unit BaseUnit;
    public PointerMarker PointerMarker;
    public Marker MoveMarker;
    public Marker AttackMarker;
    public GameObject EscapeMarker;
    public GameObject ListenersObject;
    [HideInInspector]
    public int LevelNumber;
    [HideInInspector]
    public LevelMetadata LevelMetadata;
    [HideInInspector]
    public Tileset Set;
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
    [HideInInspector]
    public List<string> DeadPlayerUnits; // Count for stats - maybe move to a different class? Listeners? GameController should probably have listeners anyway.
    [HideInInspector]
    public List<string> TempFlags = new List<string>();
    [HideInInspector]
    public Team CurrentPhase = GameCalculations.FirstTurnTeam;
    protected Difficulty difficulty;
    private List<IGameControllerListener> listeners = new List<IGameControllerListener>();
    private float enemyMoveDelayCount;
    private Transform currentMapObject;
    private Transform currentUnitsObject;
    private DeadUnitData deadUnit;
    private bool checkEndTurn;
    private Map selectedMap;
    private int currentKnowledge;
    private int _enemyCount; // To increase performance
    private int enemyCount
    {
        get
        {
            if (_enemyCount < 0)
            {
                _enemyCount = units.FindAll(a => !a.TheTeam.IsMainPlayerTeam()).Count;
            }
            return _enemyCount;
        }
        set
        {
            _enemyCount = value;
        }
    }
    private SuspendDataGameController suspendData = new SuspendDataGameController();
    private List<Unit> playerUnitsCache;
    public List<Unit> PlayerUnits
    {
        get
        {
            if (playerUnitsCache == null)
            {
                //Bugger.Info("Loading player units...");
                playerUnitsCache = new List<Unit>();
                string[] playerUnits = SavedData.Load<string>("PlayerDatas").Split('\n');
                for (int i = 0; i < playerUnits.Length - 1; i++)
                {
                    Unit unit = CreateEmptyUnit();
                    unit.Load(playerUnits[i], true);
                    unit.name = "Unit" + unit.Name;
                    unit.Health = unit.Stats.MaxHP;
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
    private bool _interactable = true;
    public bool Interactable
    {
        get => _interactable;
        private set
        {
            _interactable = value;
            if (Interactable)
            {
                GameUIController.ShowUI(Cursor.Pos);
            }
            else
            {
                GameUIController.HideUI();
            }
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
            return units.Find(a => a.Name == StaticGlobals.MainCharacterName);
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
        Current = this;
        // Init maps
        MapController.Maps.ForEach(a => a.Init());
        // Init markers
        MoveMarker.Init();
        AttackMarker.Init();
    }

    protected virtual void Start()
    {
        // Randomize
        if (GameCalculations.HasKnowledge("Randomize"))
        {
            Randomizer randomizer = new Randomizer();
            randomizer.Randomize(PortraitController.Current, UnitClassData, MapController, LevelMetadataController);
        }
        // Init
        playerUnitsCache = new List<Unit>();
        if (SavedData.Load("HasSuspendData", 0) != 0 && (!DebugOptions.Enabled || !DebugOptions.KillAutoSaves)) // Has suspended data
        {
            SuspendController.Current.LoadFromSuspendData();
        }
        else
        {
            difficulty = (Difficulty)SavedData.Load("Knowledge", "UpgradeDifficulty", 0);
            NumRuns++; // While I'd like to prevent abuse, like the knowledge, it looks weird in the save selection screen when there are 0 runs
            SavedData.Append("Log", "Data", "Began run " + NumRuns + ", difficulty: " + difficulty + "\n");
            if (DebugOptions.Enabled)
            {
                DebugOptions.Apply(this, playerUnitsCache);
            }
            else
            {
                LevelNumber = 1;
                ConversationPlayer.Current.Play(CreateLevel());
            }
        }
    }
    /// <summary>
    /// Used for player control.
    /// </summary>
    protected virtual void Update()
    {
        if (MidBattleScreen.HasCurrent) // For ConversationPlayer
        {
            GameUIController.HideUI();
            return;
        }
        if (Time.timeScale == 0 || CheckGameState() != GameState.Normal)
        {
            return;
        }
        // End Interact/UI code
        EnemyAI();
    }
    /// <summary>
    /// Does all every-frame checks (mostly win/lose and handling unit death). Returns the game state afterwards.
    /// </summary>
    /// <returns>SideWon if the level ended (aka don't activate OnFinishAnimation), ShowingEvent if death quote (aka activate OnFinishAnimation afterwards), Normal otherwise.</returns>
    public GameState CheckGameState()
    {
        CheckDifficulty();
        if (deadUnit != null)
        {
            if (CheckConveresationWait()) // Most characterNumber/alive/whatever commands
            {
                return GameState.ShowingEvent;
            }
            else if (deadUnit.DeathQuote != "")
            {
                Unit target = deadUnit.Origin; // In case another unit dies during the death quote...
                PortraitController.Current.AddPortraitAlias("Dead", deadUnit.Origin.Icon);
                ConversationPlayer.Current.OnFinishConversation = () => { if (target != null) KillUnit(target); };
                ConversationPlayer.Current.PlayOneShot(deadUnit.DeathQuote);
                deadUnit.DeathQuote = "";
                return GameState.ShowingEvent;
            }
            if (CheckPlayerWin())
            {
                // Win
                RemoveMarkers();
                InteractState = InteractState.None; // To prevent weird corner cases.
                PlayPostBattle();
                return GameState.SideWon;
            }
            if (!GameCalculations.PermaDeath) // "Kill" player units when perma-death is off
            {
                List<Unit> playerDeadUnits = units.FindAll(a => a.TheTeam.IsMainPlayerTeam() && a.Statue);
                playerDeadUnits.ForEach(a => a.Pos = Vector2Int.one * -1);
            }
            enemyCount = units.FindAll(a => !a.TheTeam.IsMainPlayerTeam()).Count;
            deadUnit = null;
        }
        if (checkEndTurn)
        {
            if (units.Find(a => a.TheTeam == CurrentPhase && !a.Moved) == null)
            {
                RemoveMarkers();
                Team current = CurrentPhase;
                if (units.Count == 0)
                {
                    throw Bugger.Crash("No units?");
                }
                do
                {
                    current = (Team)(((int)current + 1) % 3);
                    if (current == CurrentPhase)
                    {
                        //Bugger.Info("Only one team is alive - " + current);
                    }
                } while (units.Find(a => a.TheTeam == current) == null);
                //Bugger.Info("Begin " + current + " phase, units: " + string.Join(", ", units.FindAll(a => a.TheTeam == current)));
                bool showTurnAnimation = StartPhase(current);
                if (CheckPlayerWin(Objective.Survive) || CheckPlayerWin(Objective.Escape))
                {
                    // Win
                    PlayPostBattle();
                    return GameState.SideWon;
                }
                if (showTurnAnimation)
                {
                    TurnAnimation.ShowTurn(CurrentPhase);
                }
                else
                {
                    ConversationPlayer.Current.OnFinishConversation = () => TurnAnimation.ShowTurn(CurrentPhase);
                }
            }
            else if (CheckPlayerWin(Objective.Escape))
            {
                // Win
                PlayPostBattle();
                return GameState.SideWon;
            }
            checkEndTurn = false;
        }
        AutoSaveClearAction(); // Once the GameState is normal again, we can clear the current action
        return GameState.Normal;
    }

    public void ManuallyEndTurn()
    {
        units.FindAll(a => a.TheTeam == CurrentPhase && !a.Moved).ForEach(a => a.Moved = true);
        checkEndTurn = true;
    }

    public virtual void HandleAButton(Vector2Int pos)
    {
        switch (InteractState)
        {
            case InteractState.None:
                InteractWithTile(pos.x, pos.y);
                break;
            case InteractState.Move:
            case InteractState.Attack:
                if (MarkerAtPos<Marker>(pos))
                {
                    InteractWithTile(pos.x, pos.y);
                }
                else
                {
                    SystemSFXController.Play(SystemSFXController.Type.UnitForbidden);
                    if (Selected.gameObject.GetComponent<UnitFlash>() == null)
                    {
                        Selected.gameObject.AddComponent<UnitFlash>().Init(Selected);
                    }
                }
                break;
            default:
                break;
        }
    }

    public virtual void HandleBButton(Vector2Int pos)
    {
        switch (InteractState)
        {
            case InteractState.None: // View chosen character's stats
                if (!RemoveMarkers()) // If not viewing enemy range
                {
                    Unit selected = FindUnitAtPos(pos.x, pos.y);
                    if (selected != null)
                    {
                        StatusScreenController statusScreenController = Instantiate(StatusScreen).GetComponentInChildren<StatusScreenController>();
                        List<List<Unit>> unitLists = new List<List<Unit>>();
                        for (int i = 0; i < 3; i++)
                        {
                            unitLists.Add(units.FindAll(a => (int)a.TheTeam == i && (a.TheTeam == CurrentPhase || !a.Moved)));
                        }
                        statusScreenController.Show(selected, unitLists);
                        statusScreenController.TransitionToThis();
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
                Cursor.RefreshNextFrame();
                InteractState = InteractState.None;
                SystemSFXController.Play(SystemSFXController.Type.UnitCancel);
                break;
            case InteractState.Attack:
                RemoveMarkers();
                Selected.MovementMarker.PalettedSprite.Palette = (int)Selected.TheTeam; // Probably not necessary, but just in case
                Selected.MoveTo(Selected.PreviousPos, true);
                Selected.SelectOrder();
                Cursor.RefreshNextFrame();
                SystemSFXController.Play(SystemSFXController.Type.UnitCancel);
                break;
            default:
                break;
        }
    }

    public virtual void HandleSelectButton(Vector2Int pos)
    {
        switch (InteractState)
        {
            case InteractState.None:
                // Select next unit
                Unit selected = FindUnitAtPos(pos.x, pos.y);
                if (selected != null)
                {
                    bool foundSelected = false;
                    List<Unit> trueUnits = units;
                    for (int i = 0; i < trueUnits.Count; i++)
                    {
                        if (foundSelected && trueUnits[i].TheTeam == CurrentPhase && trueUnits[i].Moved == false)
                        {
                            Cursor.Pos = trueUnits[i].Pos;
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
                            if (trueUnits[i].TheTeam == CurrentPhase && trueUnits[i].Moved == false)
                            {
                                Cursor.Pos = trueUnits[i].Pos;
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
                        if (trueUnits[i].TheTeam == CurrentPhase && trueUnits[i].Moved == false)
                        {
                            Cursor.Pos = trueUnits[i].Pos;
                            break;
                        }
                    }
                }
                break;
            case InteractState.Move:
            case InteractState.Attack:
                // Select selected unit
                Cursor.Pos = Selected.Pos;
                break;
            default:
                break;
        }
    }

    public virtual void HandleStartButton(Vector2Int pos)
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
                difficulty = (Difficulty)SavedData.Load("Knowledge", "UpgradeDifficulty", 0);
                if (difficulty != Difficulty.Insane) // Level up player units
                {
                    foreach (Unit unit in units)
                    {
                        if (unit.TheTeam.IsMainPlayerTeam())
                        {
                            unit.Stats = unit.Stats.GetLevel0Stat() + unit.AutoLevel(unit.Level);
                            unit.Health = unit.Stats.MaxHP;
                        }
                    }
                }
            }
        }
    }

    private void EnemyAI()
    {
        // Enemy AI
        if (!CurrentPhase.PlayerControlled())
        {
            if (enemyMoveDelayCount > MapAnimationsController.Current.DelayTime) // Delay once before the phase begins
            {
                Unit currentEnemy = units.Find(a => a.TheTeam == CurrentPhase && !a.Moved);
                // AI
                if (currentEnemy == null)
                {
                    checkEndTurn = true;
                }
                else
                {
                    currentEnemy.AI(units);
                }
            }
            else
            {
                enemyMoveDelayCount += Time.deltaTime;
            }
        }
    }

    public void InteractWithTile(int x, int y)
    {
        MapObjectsAtPos(x, y).ForEach(a => a.Interact(InteractState));
    }

    public bool MarkerAtPos<T>(int x, int y) where T : Marker
    {
        return GetMarkerAtPos<T>(x, y) != null;
    }

    public bool MarkerAtPos<T>(Vector2Int pos) where T : Marker
    {
        return MarkerAtPos<T>(pos.x, pos.y);
    }

    public T GetMarkerAtPos<T>(int x, int y) where T : Marker
    {
        return (T)MapObjects.Find(a => a.Pos.x == x && a.Pos.y == y && a is T);
    }

    public T GetMarkerAtPos<T>(Vector2Int pos) where T : Marker
    {
        return GetMarkerAtPos<T>(pos.x, pos.y);
    }

    public List<MapObject> MapObjectsAtPos(int x, int y)
    {
        return MapObjectsAtPos(new Vector2Int(x, y));
    }

    public List<MapObject> MapObjectsAtPos(Vector2Int pos)
    {
        return MapObjects.FindAll(a => a.Pos == pos || (a is Unit unit && unit.AtPos(pos)));
    }
    /// <summary>
    /// Removes all markers.
    /// </summary>
    /// <returns>True if there were any markers, false otherwise.</returns>
    public bool RemoveMarkers()
    {
        Cursor.RefreshNextFrame();
        List<MapObject> markers = MapObjects.FindAll(a => a is Marker);
        if (markers.Count > 0)
        {
            markers.ForEach(a => Destroy(a.gameObject));
            return true;
        }
        return false;
    }

    public void RemoveArrowMarkers()
    {
        MapObjects.FindAll(a => a is MarkerWithArrow).ForEach(a => ((MarkerWithArrow)a).HideArrow());
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
        if (!MidBattleScreen.HasCurrent && Time.timeScale > 0) // Prevent the extra frame of waiting
        {
            CheckGameState();
        }
    }

    public Unit FindUnitAtPos(Vector2Int pos)
    {
        return (Unit)MapObjects.Find(a => a is Unit unit && unit.AtPos(pos));
    }

    public Unit FindUnitAtPos(int x, int y)
    {
        return FindUnitAtPos(new Vector2Int(x, y));
    }
    /// <summary>
    /// Starts the given team's phase. If it's the player's (aka team 0), also advance the turn.
    /// </summary>
    /// <param name="team"></param>
    /// <returns>Whether to display the begin turn animation - true if display, false otherwise</returns>
    public bool StartPhase(Team team)
    {
        CurrentPhase = team;
        enemyMoveDelayCount = 0;
        foreach (var item in units)
        {
            item.Moved = false;
            if (item.ReinforcementTurn > 0)
            {
                if (team == GameCalculations.FirstTurnTeam && item.ReinforcementTurn < int.MaxValue)
                {
                    item.ReinforcementTurn--;
                }
                if (item.ReinforcementTurn > 0)
                {
                    item.Moved = true;
                }
                else if (!item.Statue)
                {
                    if (FindUnitAtPos(item.PreviousPos.x, item.PreviousPos.y) == null) // Spawn now
                    {
                        item.Pos = item.PreviousPos;
                    }
                    else // Spawn in a turn
                    {
                        item.Moved = true;
                        item.ReinforcementTurn = 1;
                    }
                }
                else if (item.Statue)
                {
                    item.Statue = false;
                }
            }
        }
        Interactable = team.PlayerControlled();
        Cursor.Palette = (int)team;
        if (team == GameCalculations.FirstTurnTeam)
        {
            Turn++;
            NotifyListeners(a => a.OnBeginPlayerTurn(units));
        }
        return !CheckConveresationWait() && !MidBattleScreen.HasCurrent; // Wait for turn events
    }

    public void KillUnit(Unit unit)
    {
        if (!GameCalculations.PermaDeath && unit.TheTeam.IsMainPlayerTeam() && unit.Name != StaticGlobals.MainCharacterName) // No perma-death
        {
            PseudoKillUnit(unit);
        }
        else // Perma-death
        {
            deadUnit = new DeadUnitData(unit, unit.DeathQuote);
            if (unit.DeathQuote != "")
            {
                unit.DeathQuote = "";
            }
            else
            {
                if (PlayerUnits.Contains(unit))
                {
                    PlayerUnits.Remove(unit);
                    DeadPlayerUnits.Add(unit.Name);
                }
                Destroy(unit.gameObject);
            }
        }
    }

    public void PseudoKillUnit(Unit unit)
    {
        unit.Statue = false; // I'm not sure why it used to be true? Check later to be safe
        unit.ReinforcementTurn = int.MaxValue;
        unit.PreviousPos = unit.Pos;
        unit.Pos = -Vector2Int.one;
    }

    public void Fight(Unit source, Unit attacker, Unit defender, float attackerRandomResult, float defenderRandomResult)
    {
        if (attacker == null || defender == null) // One unit is dead, so skip the battle (for when the battle quote kills one of them - aka Werse)
        {
            if (source != null)
            {
                FinishMove(source);
            }
            else
            {
                FinishMoveDead();
            }
        }
        else if (SavedData.Load<int>("BattleAnimationsMode", 0, SaveMode.Global) == 0) // Real animations
        {
            CrossfadeMusicPlayer.Current.SwitchBattleMode(true);
            BattleAnimationController battleAnimationController = Instantiate(Battle).GetComponentInChildren<BattleAnimationController>();
            battleAnimationController.transform.parent.gameObject.SetActive(true);
            battleAnimationController.StartBattle(attacker, defender, attackerRandomResult, defenderRandomResult);
            battleAnimationController.TransitionToThis();
            FinishMove(source);
        }
        else // Map animations
        {
            RemoveMarkers();
            MapAnimationsController.Current.AnimateBattle(attacker, defender, attackerRandomResult, defenderRandomResult);
            MapAnimationsController.Current.OnFinishAnimation = () =>
            {
                if (source != null)
                {
                    FinishMove(source);
                }
                else
                {
                    FinishMoveDead();
                }
            };
        }
    }

    private void PlayPostBattle()
    {
        NotifyListeners(a => a.OnPlayerWin(units));
        ConversationPlayer.Current.PlayPostBattle();
    }

    public void Win()
    {
        NotifyListeners(a => a.OnEndLevel(units, true));
        LevelNumber++;
        currentKnowledge++;
        SavedData.Save("FurthestLevel", Mathf.Max(LevelNumber, SavedData.Load("FurthestLevel", 0)));
        SavedData.Append("Log", "Data", "Won :)\n");
        SavedData.SaveAll(SaveMode.Slot);
        PlayersLevelUp();
    }

    private void PlayersLevelUp()
    {
        transform.parent.gameObject.SetActive(false);
        enabled = true;
        List<Unit> playerCharacters = units.Where(a => a.TheTeam.IsMainPlayerTeam()).ToList();
        // Custom level-up system
        foreach (Unit character in playerCharacters)
        {
            character.Level++;
            character.Moved = false;
            character.ReinforcementTurn = 0;
        }
        LevelUpController levelUpController = Instantiate(LevelUpScreen).GetComponentInChildren<LevelUpController>();
        levelUpController.Init(playerCharacters);
        levelUpController.FadeThisIn();
    }

    public void PlayersSave()
    {
        // Save player characters
        List<Unit> playerCharacters = PlayerUnits;
        playerCharacters.ForEach(a => a.Statue = false); // Revive "dead" units on Easy
        playerCharacters.Sort((a, b) => a.Name == StaticGlobals.MainCharacterName ? -1 : (b.Name == StaticGlobals.MainCharacterName ? 1 : 0));
        playerCharacters = playerCharacters.ToList();
        string saveData = "";
        playerCharacters.ForEach(a => saveData += a.Save() + "\n");
        SavedData.Save("PlayerDatas", saveData);
    }
    /// <summary>
    /// Creates a new level - selects a conversation & map, setts the palettes, inits stuff...
    /// Doesn't play the selected conversation, returning it instead, to allow other mid-battle screens (aka level up) to close before playing.
    /// </summary>
    /// <returns>The selected conversation</returns>
    public ConversationData CreateLevel(string forceConversation = "", string forceMap = "")
    {
        List<Unit> playerCharacters = PlayerUnits;
        Bugger.Info("New level! Player characters: " + string.Join(", ", playerCharacters));
        // Select conversation
        ConversationData conversation;
        if (forceConversation != "")
        {
            conversation = ConversationController.Current.SelectConversationByID(forceConversation);
        }
        else
        {
            conversation = ConversationController.Current.SelectConversation();
        }
        if (conversation == null)
        {
            throw Bugger.Crash("Zero possible conversations!");
        }
        Bugger.Info("Selected conversation: " + conversation.ID);
        // Select room
        List<Map> options = MapController.Maps.FindAll(a => a.MatchesDemands(conversation)); // TBA - add room demands for conversations
        if (forceMap != "")
        {
            options = options.FindAll(a => a.ToString() == forceMap);
        }
        if (options.Count <= 0)
        {
            throw Bugger.Crash("Zero possible maps!");
        }
        selectedMap = options[Random.Range(0, options.Count)];
        Bugger.Info("Selected room: " + selectedMap.Name);
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
        (LevelMetadata = LevelMetadataController[LevelNumber]).SetPalettesFromMetadata();
        // Init unit replacements
        LevelMetadata.UnitReplacements.ForEach(a => a.Init());
        // Room-specific behaviours
        InitRoomObjective();
        // Log this level
        SavedData.Append("Log", "Data", "Created level " + LevelNumber + " - conversation is " + conversation.ID + ", map is " + selectedMap.Name + "\n");
        // Play conversation
        return conversation;
    }

    private void InitRoomObjective()
    {
        if (selectedMap.Objective == Objective.Escape)
        {
            string[] parts = selectedMap.ObjectiveData.Split(':');
            escapePos = new Vector2Int(int.Parse(parts[0]), int.Parse(parts[1]));
            EscapeMarker.gameObject.SetActive(true);
        }
        else
        {
            EscapeMarker.gameObject.SetActive(false);
        }
    }

    private Unit CreateEmptyUnit()
    {
        Unit unit = Instantiate(BaseUnit.gameObject, currentUnitsObject).GetComponent<Unit>();
        unit.Name = "TEMP";
        unit.name = "UnitTEMP";
        unit.gameObject.SetActive(true);
        return unit;
    }

    public Unit CreateUnit(string name, int level, Team team, bool canReplace)
    {
        // Find replacement, fix level
        if (canReplace)
        {
            LevelMetadata.UnitReplacement replacement = LevelMetadata.UnitReplacements.Find(a => a.Name == name);
            if (replacement != null)
            {
                name = replacement.Get();
            }
        }
        level = level >= 0 ? level : LevelNumber;
        // Generate basic unit
        Unit unit = CreateEmptyUnit();
        unit.Name = name;
        unit.name = "Unit" + name;
        unit.Level = level;
        unit.TheTeam = team;
        unit.Stats += unit.AutoLevel(level);
        // Find ClassData, and determine whether to use it or UnitData (based on PortraitLoadingMode)
        ClassData classData;
        switch (LevelMetadata.TeamDatas[(int)team].PortraitLoadingMode)
        {
            case PortraitLoadingMode.Name:
                UnitData unitData = UnitClassData.UnitDatas.Find(a => a.Name == unit.Name);
                if (unitData == null)
                {
                    throw Bugger.Error("No matching unit! (" + unit.Name + ")");
                }
                unit.Class = unitData.Class;
                unit.DisplayName = unitData.DisplayName;
                classData = UnitClassData.ClassDatas.Find(a => a.Name == unit.Class);
                if (classData == null)
                {
                    throw Bugger.Error("No matching class! (" + unit.Class + ")");
                }
                unit.Stats = new Stats();
                unit.Stats.Growths = unitData.Growths.Values;
                unit.DeathQuote = unitData.DeathQuote;
                unit.Inclination = unitData.Inclination;
                unit.LoadInclination();
                break;
            case PortraitLoadingMode.Team:
            case PortraitLoadingMode.Generic:
                unit.Class = name;
                classData = UnitClassData.ClassDatas.Find(a => a.Name == unit.Class);
                if (classData == null)
                {
                    throw Bugger.Error("No matching class! (" + unit.Class + ")");
                }
                unit.Stats = new Stats();
                unit.Stats.Growths = classData.Growths.Values;
                unit.Inclination = classData.Inclination;
                break;
            default:
                throw Bugger.Error("Impossible!", false);
        }
        // Use ClassData for class-specific stuff (flies, weapon...)
        unit.Flies = classData.Flies;
        unit.Weapon = classData.Weapon;
        unit.Stats += unit.AutoLevel(level);
        // Load sprite, priorities, skills, init
        AssignUnitMapAnimation(unit, classData);
        unit.Priorities.Set(LevelMetadata.TeamDatas[(int)team].AI);
        unit.LoadSkills();
        unit.Init(true);
        return unit;
    }

    public Unit CreatePlayerUnit(string name)
    {
        return CreateUnit(name, -1, StaticGlobals.MainPlayerTeam, false);
    }

    private void AssignUnitMapAnimation(Unit unit, ClassData classData)
    {
        AdvancedSpriteSheetAnimation animation = Instantiate(UnitClassData.BaseAnimation, unit.transform);
        animation.Renderer = unit.GetComponent<SpriteRenderer>();
        animation.Animations[0].SpriteSheet = classData.MapSprite;
        animation.Start();
        animation.Activate(0);
    }

    private Map LoadMapData(string mapName = "")
    {
        Map map;
        if (mapName == "")
        {
            map = selectedMap;
        }
        else
        {
            map = MapController.Maps.Find(a => a.Name == mapName);
            if (map == null)
            {
                throw Bugger.Error("No matching map! (" + mapName + ")");
            }
        }
        return map;
    }

    public void LoadMap(string mapName = "")
    {
        LoadMap(LoadMapData(mapName));
    }

    public void LoadMap(Map map)
    {
        // Clear previous level
        if (currentMapObject != null)
        {
            Destroy(currentMapObject.gameObject);
        }
        // Load map
        Set = MapController.Tilesets.Find(a => a.Name == map.Tileset);
        PaletteController.Current.BackgroundPalettes[0].CopyFrom(Set.Palette1);
        PaletteController.Current.BackgroundPalettes[1].CopyFrom(Set.Palette2);
        // Create map
        currentMapObject = new GameObject("MapObject").transform;
        currentMapObject.parent = transform;
        Map = new Tile[MapSize.x, MapSize.y];
        for (int i = 0; i < MapSize.x; i++)
        {
            for (int j = 0; j < MapSize.y; j++)
            {
                int tileID = map.Tilemap[i, j];
                Tile newTile = Instantiate(Set.TileObjects[tileID].gameObject, currentMapObject).GetComponent<Tile>();
                newTile.transform.position = new Vector2(TileSize * i, -TileSize * j);
                newTile.gameObject.SetActive(true);
                Map[i, j] = newTile;
            }
        }
        // Load map events
        foreach (MapEventData mapEvent in map.MapEvents)
        {
            LMapEventListener listener = ListenersObject.AddComponent<LMapEventListener>();
            listener.Init(mapEvent);
        }
    }

    public void LoadLevelUnits(string roomName = "", Team? ofTeam = null, bool keepPrevious = false)
    {
        Map room = LoadMapData(roomName);
        List<Unit> playerCharacters;
        if (!keepPrevious)
        {
            // Clear previous level
            if (currentUnitsObject != null)
            {
                PlayersSave();
                DestroyImmediate(currentUnitsObject.gameObject);
                playerUnitsCache = null;
            }
            currentUnitsObject = new GameObject("UnitsObject").transform;
            currentUnitsObject.parent = transform;
            playerCharacters = PlayerUnits.Where(a => a != null).ToList();
            if (!(ofTeam ?? StaticGlobals.MainPlayerTeam).IsMainPlayerTeam())
            {
                // "Remove" player units
                foreach (Unit player in playerCharacters)
                {
                    player.Pos = Vector2Int.one * -1;
                    player.transform.parent = currentUnitsObject;
                }
            }
        }
        else
        {
            playerCharacters = PlayerUnits.Where(a => a != null).ToList();
        }
        // Units
        List<MapController.UnitPlacementData> unitDatas = room.Units;
        int numPlayers = 0;
        for (int i = 0; i < unitDatas.Count; i++)
        {
            MapController.UnitPlacementData unitData = unitDatas[i];
            Team team = unitData.Team;
            if (team != (ofTeam ?? team))
            {
                continue;
            }
            string name = unitData.Class;
            if (team.IsMainPlayerTeam())
            {
                Unit unit;
                if (name == "P")
                {
                    if (playerCharacters.Count > numPlayers + 1) // I can't make multiple rooms for evey combination of living characters...
                    {
                        unit = playerCharacters[++numPlayers];
                        unit.transform.parent = currentUnitsObject;
                        unit.Health = unit.Stats.MaxHP;
                        unit.Pos = unitData.Pos;
                    }
                    continue;
                }
                else if (name == StaticGlobals.MainCharacterName && playerCharacters.Count > 0)
                {
                    unit = playerCharacters[0];
                    unit.transform.parent = currentUnitsObject;
                    unit.Health = unit.Stats.MaxHP;
                    unit.Pos = unitData.Pos;
                    Cursor.Pos = unit.Pos; // Auto-cursor
                    continue;
                }
                else
                {
                    unit = CreateUnit(name, unitData.Level, StaticGlobals.MainPlayerTeam, false);
                    unit.ReinforcementTurn = unitData.ReinforcementTurn;
                    unit.Statue = unitData.Statue;
                    unit.AIType = unitData.AIType;
                    unit.Pos = unitData.Pos;
                    if (unit.ReinforcementTurn > 0 && !unit.Statue)
                    {
                        unit.PreviousPos = unit.Pos;
                        unit.Pos = Vector2Int.one * -1;
                        unit.Moved = true;
                    }
                    else if (unit.Statue)
                    {
                        unit.Moved = true;
                    }
                    PlayerUnits.Add(unit);
                    if (name != StaticGlobals.MainCharacterName)
                    {
                        Bugger.Warning("Please refrain from hard placing units in maps. Use P and addUnit event instead.");
                    }
                    else
                    {
                        Cursor.Pos = unit.Pos; // Auto-cursor
                    }
                }
            }
            else // Enemy units
            {
                Unit unit = CreateUnit(name, unitData.Level, team, !unitData.Statue);
                unit.ReinforcementTurn = unitData.ReinforcementTurn;
                unit.Statue = unitData.Statue;
                unit.AIType = unitData.AIType;
                unit.Pos = unitData.Pos;
                if (unit.ReinforcementTurn > 0 && !unit.Statue)
                {
                    unit.PreviousPos = unit.Pos;
                    unit.Pos = Vector2Int.one * -1;
                    unit.Moved = true;
                }
                else if (unit.Statue)
                {
                    unit.Moved = true;
                }
                unit.gameObject.SetActive(true);
            }
        }
        enemyCount = -1;
        CurrentPhase = GameCalculations.FirstTurnTeam;
        //interactable = true;
    }

    public void ShowDangerArea()
    {
        units.FindAll(a => a.TheTeam.IsEnemy(CurrentPhase) && !a.Moved).ForEach(a => a.MarkDangerArea());
    }

    public string GetPauseObjectiveText()
    {
        switch (selectedMap.Objective)
        {
            case Objective.Rout:
                return "Rout the enemy";
            case Objective.Boss:
                return "Defeat " + selectedMap.ObjectiveData;
            case Objective.Escape:
                return "Escape!";
            case Objective.Survive:
                return "Reach turn " + (int.Parse(selectedMap.ObjectiveData) + 1);
            case Objective.Custom:
                return selectedMap.ObjectiveData;
            default:
                break;
        }
        return "";
    }

    public string GetInfoObjectiveText()
    {
        switch (selectedMap.Objective)
        {
            case Objective.Rout:
                return "Kill " + enemyCount + "\nenemies";
            case Objective.Boss:
                return "Defeat\n" + selectedMap.ObjectiveData;
            case Objective.Escape:
                return StaticGlobals.MainCharacterName + "\nto mark";
            case Objective.Survive:
                return "Survive\n" + (int.Parse(selectedMap.ObjectiveData) - Turn + 1) + " turns";
            case Objective.Custom:
                return selectedMap.ObjectiveData;
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
        NotifyListeners(a => a.OnEndLevel(units, false));
        LevelMetadataController[0].SetPalettesFromMetadata(); // Fix Torment palette
        SavedData.Append("Knowledge", "Amount", currentKnowledge);
        SavedData.Append("PlayTime", Time.timeSinceLevelLoad);
        SavedData.Append("Log", "Data", "Lost :(\n");
        SavedData.Save("HasSuspendData", 0);
        SavedData.SaveAll(SaveMode.Slot);
        SceneController.LoadScene("GameOver");
    }
    /// <summary>
    /// Called once the pre-battle conversation ends
    /// </summary>
    public void BeginBattle()
    {
        AssignGenericPortraitsToUnits();
        // Begin the level properly
        Interactable = GameCalculations.FirstTurnTeam.PlayerControlled();
        TurnAnimation.ShowTurn(GameCalculations.FirstTurnTeam);
        Cursor.Palette = (int)GameCalculations.FirstTurnTeam;
        // Stats - increase the maps count of player units
        units.FindAll(a => a.TheTeam.PlayerControlled()).ForEach(a => SavedData.Append("Statistics", a.ToString() + "MapsCount", 1));
    }

    private void AssignGenericPortraitsToUnits(Team? team = null)
    {
        List<Unit> targetUnits = units.FindAll(a => a.TheTeam == (team ?? a.TheTeam) && LevelMetadata.TeamDatas[(int)a.TheTeam].PortraitLoadingMode == PortraitLoadingMode.Generic);
        List<GeneratedPortrait> portraits = PortraitController.Current.SaveAllGeneratedPortraits();
        for (int i = 0; i < portraits.Count && i < targetUnits.Count; i++)
        {
            targetUnits[i].SetIcon(portraits[i].Portrait);
        }
    }

    public int LeftToMove()
    {
        return units.FindAll(a => a.TheTeam == CurrentPhase && !a.Moved).Count;
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
        return GetNamedUnits(name, true).Count > 0;
    }

    public int CountUnitsAlive(Team? team)
    {
        List<Unit> targetUnits = units.FindAll(a => a.TheTeam == (team ?? a.TheTeam));
        return targetUnits.Count;
    }

    public int FindMinMaxPosUnit(Team? team, bool x, bool max)
    {
        //Bugger.Info("Checking " + (x ? "x" : "y") + " of team " + (team ?? Team.Guard).Name() + ", for " + (max ? "max" : "min"));
        int minMax = max ? -1 : (x ? MapSize.x : MapSize.y);
        List<Unit> targets = units.FindAll(a => a.TheTeam == (team ?? a.TheTeam));
        foreach (Unit unit in targets)
        {
            if ((x ? unit.Pos.x : unit.Pos.y) * (max ? 1 : -1) > minMax * (max ? 1 : -1))
            {
                minMax = x ? unit.Pos.x : unit.Pos.y;
            }
        }
        //Bugger.Info("Result: " + minMax);
        return minMax;
    }

    public List<Unit> GetNamedUnits(string name, bool displayName = false)
    {
        return units.FindAll(a => (displayName ? a.ToString() : a.Name) == name);
    }

    public Portrait GetGenericPortrait(Team? team = null)
    {
        List<Unit> targetUnits = units.FindAll(a => a.TheTeam == (team ?? a.TheTeam) && LevelMetadata.TeamDatas[(int)a.TheTeam].PortraitLoadingMode == PortraitLoadingMode.Generic);
        return targetUnits[0].Icon;
    }

    public void AssignAIToTeam(Team team, AIType ai)
    {
        List<Unit> targetUnit = units.FindAll(a => a.TheTeam == team);
        targetUnit.ForEach(a => a.AIType = ai);
    }

    public void KillTeam(Team team)
    {
        units.FindAll(a => a.TheTeam == team).ForEach(a => KillUnit(a));
    }

    public void ForceSetCursorPos(Vector2Int pos)
    {
        Cursor.Pos = pos;
        GameUIController.ShowUI(Cursor.Pos); // Update UI
        if (MidBattleScreen.HasCurrent)
        {
            GameUIController.HideUI();
        }
    }

    public void AddListener(IGameControllerListener listener)
    {
        listeners.Add(listener);
    }

    public void RemoveListener(IGameControllerListener listener)
    {
        listeners.Remove(listener);
    }

    private void NotifyListeners(System.Action<IGameControllerListener> action)
    {
        listeners.ForEach(a => action(a));
    }

    private bool CheckPlayerWin()
    {
        switch (selectedMap.Objective)
        {
            case Objective.Rout:
                return units.FindAll(a => !a.TheTeam.IsMainPlayerTeam() && a.ReinforcementTurn <= 0).Count == 0;
            case Objective.Boss:
                return !CheckUnitAlive(selectedMap.ObjectiveData);
            case Objective.Escape:
                return frogman.Pos == escapePos;
            case Objective.Survive:
                return Turn > int.Parse(selectedMap.ObjectiveData) || units.FindAll(a => !a.TheTeam.IsMainPlayerTeam() && a.ReinforcementTurn <= 0).Count == 0;
            case Objective.Custom:
                return false; // Custom objective means the modder needs to add a custom event with :win:
            default:
                throw Bugger.Error("No objective!");
        }
    }

    private bool CheckPlayerWin(Objective toCheck)
    {
        if (toCheck != selectedMap.Objective)
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

    public void AutoSaveSaveAction(SuspendDataGameController.CurrentAction.ActionType type, Vector2Int origin, Vector2Int target, string additionalData)
    {
        if (suspendData.OnLoadAction != null)
        {
            throw Bugger.Error("Auto-save error: trying to save an action during another action!");
        }
        suspendData = SaveToSuspendData();
        suspendData.OnLoadAction = new SuspendDataGameController.CurrentAction(type, origin, target, additionalData);
    }

    public void AutoSaveClearAction()
    {
        if (suspendData.OnLoadAction != null)
        {
            suspendData.OnLoadAction = null;
            suspendData = SaveToSuspendData();
        }
    }

    public void AutoSaveExecuteAction()
    {
        if (suspendData.OnLoadAction != null && suspendData.OnLoadAction.Type != SuspendDataGameController.CurrentAction.ActionType.None)
        {
            // Create vars which we'll (probably) use later
            Unit origin, target;
            origin = FindUnitAtPos(suspendData.OnLoadAction.Origin);
            switch (suspendData.OnLoadAction.Type)
            {
                case SuspendDataGameController.CurrentAction.ActionType.Move:
                    origin.MoveTo(suspendData.OnLoadAction.Target);
                    break;
                case SuspendDataGameController.CurrentAction.ActionType.Combat:
                    // Load all the parts
                    string[] parts = suspendData.OnLoadAction.AdditionalData.Split(';');
                    string[] posParts = parts[0].Split(',');
                    string[] randomResultParts = parts[1].Split(',');
                    Vector2Int attackerPos = new Vector2Int(int.Parse(posParts[0]), int.Parse(posParts[1]));
                    Unit attacker = FindUnitAtPos(attackerPos);
                    target = FindUnitAtPos(suspendData.OnLoadAction.Target);
                    if (attacker == origin)
                    {
                        origin.Fight(target, int.Parse(randomResultParts[0]), int.Parse(randomResultParts[1]));
                    }
                    else
                    {
                        target.Fight(origin, int.Parse(randomResultParts[0]), int.Parse(randomResultParts[1]));
                    }
                    break;
                case SuspendDataGameController.CurrentAction.ActionType.Push:
                    target = FindUnitAtPos(suspendData.OnLoadAction.Target);
                    origin.Push(target);
                    break;
                case SuspendDataGameController.CurrentAction.ActionType.Pull:
                    target = FindUnitAtPos(suspendData.OnLoadAction.Target);
                    origin.Pull(target);
                    break;
                default:
                    break;
            }
        }
    }

    public bool AutoSaveHasAction()
    {
        return suspendData.OnLoadAction != null;
    }

    public SuspendDataGameController SaveToSuspendData()
    {
        if (suspendData.OnLoadAction != null)
        {
            // Don't update the suspend data during an action to prevent double effects (ex. a unit taking damage twice)
            return suspendData;
        }
        suspendData.LevelNumber = LevelNumber;
        suspendData.LevelMetadata = LevelMetadata; // In case the map theme is changed
        suspendData.Turn = Turn;
        suspendData.DeadPlayerUnits = DeadPlayerUnits;
        suspendData.TempFlags = TempFlags;
        suspendData.Difficulty = difficulty;
        suspendData.CurrentPhase = CurrentPhase;
        suspendData.CurrentKnowledge = currentKnowledge;
        suspendData.EnemyCount = enemyCount;
        suspendData.SelectedMap = selectedMap;
        // Only add the remaining MapEvents
        suspendData.SelectedMap.MapEvents = listeners.FindAll(a => a is LMapEventListener).ConvertAll(a => ((LMapEventListener)a).EventData);
        suspendData.Units = units.ConvertAll(a => a.Save());
        return suspendData;
    }

    public void LoadFromSuspendData(SuspendDataGameController data)
    {
        LevelNumber = data.LevelNumber;
        LevelMetadata = data.LevelMetadata;
        Turn = data.Turn;
        DeadPlayerUnits = data.DeadPlayerUnits;
        TempFlags = data.TempFlags;
        difficulty = data.Difficulty;
        CurrentPhase = data.CurrentPhase;
        currentKnowledge = data.CurrentKnowledge;
        enemyCount = data.EnemyCount;
        selectedMap = data.SelectedMap;
        selectedMap.Init();
        LoadMap(selectedMap);
        InitRoomObjective();
        LevelMetadata.SetPalettesFromMetadata();
        LevelMetadata.ReloadSymbols(LevelMetadataController[LevelNumber]);
        currentUnitsObject = new GameObject("UnitsObject").transform;
        currentUnitsObject.parent = transform;
        foreach (string unitJSON in data.Units)
        {
            Unit unit = CreateEmptyUnit();
            unit.Load(unitJSON);
            Vector2Int tempPos = unit.Pos;
            bool tempMoved = unit.Moved;
            int tempHealth = unit.Health;
            unit.Init(true);
            unit.name = "Unit" + unit.Name;
            unit.Pos = tempPos;
            unit.Moved = tempMoved;
            unit.Health = tempHealth;
            AssignUnitMapAnimation(unit, UnitClassData.ClassDatas.Find(a => a.Name == unit.Class));
            unit.gameObject.SetActive(true);
            if (unit.TheTeam.IsMainPlayerTeam())
            {
                playerUnitsCache.Add(unit);
                if (unit.Name == StaticGlobals.MainCharacterName)
                {
                    Cursor.Pos = unit.Pos; // Auto-cursor
                }
            }
        }
        if (ConversationPlayer.Current.Playing)
        {
            // Queue the saved action to after the conversation ends (aka for unit death)
            ConversationPlayer.Current.OnFinishConversation = () => AutoSaveExecuteAction();
        }
        else
        {
            // Hide the conversation player - terrible workaround
            ConversationPlayer.Current.PlayOneShot("");
            CrossfadeMusicPlayer.Current.PlayOnStart = false;
            if (AutoSaveHasAction())
            {
                AutoSaveExecuteAction();
            }
            else
            {
                TurnAnimation.ShowTurn(CurrentPhase);
            }
        }
        SavedData.Append("Log", "Data", "Resumed\n");
    }

    /// <summary>
    /// Updates all units for settings change. Currently only exists for the extra symbols accessibility option.
    /// </summary>
    public void ReflectSettingsUpdate()
    {
        units.ForEach(a => a.ReflectSettingsUpdate());
    }

    private class DeadUnitData
    {
        public Unit Origin { get; }
        public string DeathQuote { get; set; }

        public DeadUnitData(Unit origin, string deathQuote)
        {
            Origin = origin;
            DeathQuote = deathQuote;
        }
    }
}

[System.Serializable]
public class SuspendDataGameController
{
    public int LevelNumber;
    public LevelMetadata LevelMetadata;
    public int Turn;
    public List<string> DeadPlayerUnits;
    public List<string> TempFlags;
    public Difficulty Difficulty;
    public Team CurrentPhase;
    public int CurrentKnowledge;
    public int EnemyCount;
    public Map SelectedMap;
    public List<string> Units;
    public CurrentAction OnLoadAction;

    [System.Serializable]
    public class CurrentAction
    {
        public enum ActionType { None, Move, Combat, Push, Pull }

        public ActionType Type;
        public Vector2Int Origin;
        public Vector2Int Target;
        public string AdditionalData;

        public CurrentAction(ActionType type, Vector2Int origin, Vector2Int target, string additionalData)
        {
            Type = type;
            Origin = origin;
            Target = target;
            AdditionalData = additionalData;
        }
    }
}