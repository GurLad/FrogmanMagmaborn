using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CAT = EventCommandProcessor.CommandArgumentType;
using StartLineResult = ConversationPlayer.StartLineResult;
using PlayMode = ConversationPlayer.PlayMode;

public static class EventCommandProcessor
{
    public enum CommandArgumentType { String, Int, Float, Bool, Team, AIType, OpString = 10, OpInt, OpFloat, OpBool, OpTeam, OpAIType } // Assume there aren't more than 10 types
    public enum CommandType { Level, Conversation, MidBattleScreen, Global, Syntax, Tutorial, Menu }

    private static List<CommandStruct> AllCommands { get; } = new List<CommandStruct>(new CommandStruct[]
    {
        // Level commands

        new CommandStruct("addUnit", CommandType.Level, CAT.String),
        new CommandStruct("loadUnits", CommandType.Level, CAT.OpString, CAT.OpTeam, CAT.OpBool),
        new CommandStruct("loadMap", CommandType.Level, CAT.OpString),
        new CommandStruct("setTeam", CommandType.Level, CAT.String, CAT.Team),
        new CommandStruct("setBattleQuote", CommandType.Level, CAT.String, CAT.String),
        new CommandStruct("setDeathQuote", CommandType.Level, CAT.String, CAT.String),
        new CommandStruct("addSkill", CommandType.Level, CAT.String, CAT.String),
        new CommandStruct("killUnit", CommandType.Level, CAT.String, CAT.OpBool),
        new CommandStruct("hideUnit", CommandType.Level, CAT.String),
        new CommandStruct("replaceUnit", CommandType.Level, CAT.String, CAT.String, CAT.OpBool),
        new CommandStruct("killTeam", CommandType.Level, CAT.Team),
        new CommandStruct("setTeamAI", CommandType.Level, CAT.Team, CAT.AIType),
        new CommandStruct("lose", CommandType.Level),
        new CommandStruct("win", CommandType.Level),

        // Conversation commands
        
        new CommandStruct("play", CommandType.Conversation, CAT.String, CAT.OpBool),
        new CommandStruct("playIntro", CommandType.Conversation, CAT.String),
        new CommandStruct("setMapTheme", CommandType.Conversation, CAT.String),
        new CommandStruct("addGenericCharacter", CommandType.Conversation, CAT.String, CAT.OpString),
        new CommandStruct("getGenericCharacter", CommandType.Conversation, CAT.String, CAT.OpTeam),
        new CommandStruct("setSingleSpeaker", CommandType.Conversation, CAT.OpBool),
        new CommandStruct("setSpeaker", CommandType.Conversation, CAT.String),
        new CommandStruct("showCG", CommandType.Conversation, CAT.String),
        new CommandStruct("hideCG", CommandType.Conversation),
        new CommandStruct("screenShake", CommandType.Conversation, CAT.OpFloat, CAT.OpFloat),
        new CommandStruct("darkenScreen", CommandType.Conversation, CAT.OpBool),

        // Show other screens (MidBattleScreens)

        new CommandStruct("showInfoDialogue", CommandType.MidBattleScreen, CAT.String),
        new CommandStruct("showPartTitle", CommandType.MidBattleScreen, CAT.String, CAT.String),
        new CommandStruct("showChoice", CommandType.MidBattleScreen, CAT.String, CAT.String, CAT.String),
        new CommandStruct("showBase", CommandType.MidBattleScreen),

        // Global commands

        new CommandStruct("unlockKnowledge", CommandType.Global, CAT.String),
        new CommandStruct("setFlag", CommandType.Global, CAT.String),
        new CommandStruct("setTempFlag", CommandType.Global, CAT.String),
        new CommandStruct("markDone", CommandType.Global),
        new CommandStruct("setCounter", CommandType.Global, CAT.String, CAT.Int),
        new CommandStruct("addCounter", CommandType.Global, CAT.String, CAT.Int),
        new CommandStruct("unlockAchievement", CommandType.Global, CAT.String),

        // Syntax commands

        new CommandStruct("if", CommandType.Syntax, false),
        new CommandStruct("else", CommandType.Syntax, false),
        new CommandStruct("call", CommandType.Syntax, CAT.String),
        new CommandStruct("callOther", CommandType.Syntax, CAT.String),
        new CommandStruct("wait", CommandType.Syntax, false),
        new CommandStruct("return", CommandType.Syntax),
        new CommandStruct("finishConversation", CommandType.Syntax),

        // Tutorial

        new CommandStruct("tutorialForceButton", CommandType.Tutorial, false),
        new CommandStruct("tutorialShowMarker", CommandType.Tutorial, false),
        new CommandStruct("tutorialFinish", CommandType.Tutorial, false),

        // Menu

        new CommandStruct("introShowCutscene", CommandType.Menu),
        new CommandStruct("introShowUpgradeMenu", CommandType.Menu),
        new CommandStruct("introShowTutorial", CommandType.Menu)
    });

    private static int SkipBlock(int currentLine, List<string> lines)
    {
        int numBrackets = 1;
        while (numBrackets > 0)
        {
            numBrackets -= lines[++currentLine] == "}" ? 1 : 0;
            numBrackets += lines[currentLine].Contains("{") ? 1 : 0;
        }
        return currentLine;
    }

    public static string[] GetArgsFromParts(string[] parts)
    {
        string[] args = parts.Length > 2 ? (parts[parts.Length - 1] == "" ? new string[parts.Length - 2] : new string[parts.Length - 1]) : new string[1];
        if (args.Length > 0)
        {
            // Array copy doesn't work for some reason?
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = (i + 2 < parts.Length) ? parts[i + 2] : "";
            }
        }
        return args;
    }

    public static CommandStruct GetCommandStruct(string commandName)
    {
        return AllCommands.Find(a => a.Name == commandName);
    }

    public static StartLineResult ExecuteLevelCommand(
        string commandName,
        string[] parts,
        string[] args,
        int num,
        ConversationData origin,
        System.Func<int, StartLineResult> StartLineTrue)
    {
        StartLineResult result = StartLineResult.None;
        switch (commandName)
        {
            case "addUnit":
                // Params: string name
                GameController.Current.PlayerUnits.Add(GameController.Current.CreatePlayerUnit(args[0]));
                break;
            case "loadUnits":
                // Params: string mapName = chosenMap, Team team = allTeams, bool keepPrevious = false
                if (parts.Length < 4)
                {
                    GameController.Current.LoadLevelUnits(args[0]);
                }
                else if (parts.Length < 5)
                {
                    GameController.Current.LoadLevelUnits(args[0], args[1].ToTeam());
                }
                else
                {
                    GameController.Current.LoadLevelUnits(args[0], args[1].ToTeam(), args[2] == "T");
                }
                result |= StartLineResult.LoadUnits;
                break;
            case "loadMap":
                // Params: string mapName = chosenMap
                GameController.Current.LoadMap(args[0]);
                result |= StartLineResult.LoadMap;
                break;
            case "setTeam":
                // Params: string unitName, Team changeTo
                // Changes a unit's team
                List<Unit> targets = GameController.Current.GetNamedUnits(args[0]);
                if (targets.Count > 0)
                {
                    foreach (Unit target in targets)
                    {
                        target.TheTeam = args[1].ToTeam() ?? target.TheTeam;
                        target.Moved = target.Moved;
                    }
                }
                else
                {
                    throw Bugger.Error("No matching unit! (" + args[0] + ")");
                }
                break;
            case "setBattleQuote":
                // Params: string unitName, string functionName
                // Set a unit's battle quote (aka add boss battle quote). Must use a function.
                List<Unit> targets1 = GameController.Current.GetNamedUnits(args[0]);
                if (targets1.Count > 0)
                {
                    foreach (Unit target in targets1)
                    {
                        if (origin.Functions.ContainsKey(args[1]))
                        {
                            target.BattleQuote = string.Join("\n", origin.Functions[args[1]]);
                            Bugger.Info(target.BattleQuote);
                        }
                        else
                        {
                            throw Bugger.Error("No matching function! (" + args[1] + ")");
                        }
                    }
                }
                else
                {
                    throw Bugger.Error("No matching unit! (" + args[0] + ")");
                }
                break;
            case "setDeathQuote":
                // Params: string unitName, string functionName
                // Set a unit's death quote (retains bewtween chapters). Must use a function.
                List<Unit> targets2 = GameController.Current.GetNamedUnits(args[0]);
                if (targets2.Count > 0)
                {
                    foreach (Unit target in targets2)
                    {
                        if (origin.Functions.ContainsKey(args[1]))
                        {
                            target.DeathQuote = string.Join("\n", origin.Functions[args[1]]);
                        }
                        else
                        {
                            throw Bugger.Error("No matching function! (" + args[1] + ")");
                        }
                    }
                }
                else
                {
                    throw Bugger.Error("No matching unit! (" + args[0] + ")");
                }
                break;
            case "addSkill":
                // Params: string unitName, string skillName
                // Adds a skill to a unit.
                List<Unit> targets3 = GameController.Current.GetNamedUnits(args[0]);
                if (targets3.Count > 0)
                {
                    foreach (Unit target in targets3)
                    {
                        target.AddSkill(args[1].ToSkill() ?? throw Bugger.Error("No matching skill! (" + args[1] + ")"));
                    }
                }
                else
                {
                    throw Bugger.Error("No matching unit! (" + args[0] + ")");
                }
                break;
            case "killUnit":
                // Params: string unitName, bool showDeathQuote = true
                // Kills a unit.
                List<Unit> targets4 = GameController.Current.GetNamedUnits(args[0]);
                if (targets4.Count > 0)
                {
                    foreach (Unit target in targets4)
                    {
                        if (args.Length > 1 && args[1] == "F")
                        {
                            // Remove death quote before killing
                            target.DeathQuote = "";
                        }
                        GameController.Current.KillUnit(target);
                    }
                }
                else
                {
                    throw Bugger.Error("No matching unit! (" + args[0] + ")");
                }
                break;
            case "hideUnit":
                // Params: string unitName
                // Hides a unit - equivalent to pseudo-kill (aka when units die with permadeath off).
                List<Unit> targets5 = GameController.Current.GetNamedUnits(args[0]);
                if (targets5.Count > 0)
                {
                    foreach (Unit target in targets5)
                    {
                        GameController.Current.PseudoKillUnit(target);
                    }
                }
                else
                {
                    throw Bugger.Error("No matching unit! (" + args[0] + ")");
                }
                break;
            case "replaceUnit":
                // Params: string oldUnit, string newUnit, bool keepHealth = false
                // Kills oldUnit and spawns newUnit in its place
                List<Unit> targets6 = GameController.Current.GetNamedUnits(args[0]);
                if (targets6.Count > 0)
                {
                    foreach (Unit target in targets6)
                    {
                        Unit newUnit = GameController.Current.CreateUnit(args[1], target.Level, target.TheTeam, false);
                        newUnit.Pos = target.Pos;
                        if (args.Length > 2 && args[2] == "T")
                        {
                            newUnit.Health = target.Health;
                        }
                        target.DeathQuote = ""; // Doesn't actually die, after all
                        GameController.Current.KillUnit(target);
                    }
                }
                else
                {
                    throw Bugger.Error("No matching unit! (" + args[0] + ")");
                }
                break;
            case "killTeam":
                // Params: string teamName
                // Kills all units in a team.
                GameController.Current.KillTeam(args[0].ToTeam() ?? throw Bugger.Error("No team!"));
                break;
            case "setTeamAI":
                // Params: Team team, AIType ai
                Team team = args[0].ToTeam() ?? throw Bugger.Error("No team!");
                GameController.Current.AssignAIToTeam(team, args[1].ToAIType() ?? throw Bugger.Error("Impossible - I just validated..."));
                break;
            case "lose":
                // Params: none
                if (GameController.Current != null)
                {
                    GameController.Current.Lose();
                }
                else
                {
                    SavedData.SaveAll(SaveMode.Slot);
                    SceneController.LoadScene("GameOver");
                }
                return result | StartLineResult.FinishLevel;
            case "win":
                // Params: none
                GameController.Current.Win();
                return result | StartLineResult.FinishLevel;
            default:
                throw Bugger.Error("No matching command! (" + commandName + ")");
        }
        return result | StartLineTrue(num + 1);
    }

    public static StartLineResult ExecuteConversationCommand(
        string commandName,
        string[] parts,
        string[] args,
        ConversationPlayer player,
        int num,
        CGController cgController,
        bool beforeBattleStart,
        bool shouldFadeIn,
        System.Func<int, StartLineResult> StartLineTrue,
        out System.Action<StartLineResult> delayedAction)
    {
        StartLineResult result = StartLineResult.None;
        delayedAction = null;
        switch (commandName)
        {
            case "play":
                // Params: string name, bool keepTimestamp = false
                CrossfadeMusicPlayer.Current.Play(args[0], parts.Length > 3 ? (args[1] == "T") : false);
                break;
            case "playIntro":
                // Params: string name
                CrossfadeMusicPlayer.Current.PlayIntro(args[0], false);
                break;
            case "setMapTheme":
                // Params: string name
                GameController.Current.LevelMetadata.MusicName = args[0]; // I really hope this doesn't break anything...
                break;
            case "addGenericCharacter":
                // Params: string internalName, string forceTags = none
                // Add to TempPortraits with args[0] internal name and args[1] tags
                PortraitController.Current.AddGenericPortrait(args[0], args[1]);
                break;
            case "getGenericCharacter":
                // Params: string internalName, Team fromTeam = null
                PortraitController.Current.AddPortraitAlias(args[0], GameController.Current.GetGenericPortrait(parts.Length > 3 ? args[1].ToTeam() : null));
                break;
            case "setSingleSpeaker":
                // Params: bool left = true
                // Removes all speakers and makes sure the next is on the left/right
                bool left = parts.Length > 2 ? args[0] == "L" : true;
                player.SetSinglePortrait(left, !left);
                break;
            case "setSpeaker":
                // Params: string speaker
                // Displays the speaker without pausing (equivelant to "name|display|L/R: bla", without the text/pause)                
                player.SetSpeakerFromText(args[0]);
                break;
            case "showCG":
                // Params: string name
                // Removes the previous CG (if any), then shows the requested CG until manually removed                
                player.Pause();
                delayedAction = (result) =>
                {
                    if (cgController.Active)
                    {
                        cgController.FadeOutCG(() => cgController.FadeInCG(args[0]));
                    }
                    else if (beforeBattleStart && shouldFadeIn)
                    {
                        PaletteController.PaletteControllerState currentPaletteState = PaletteController.Current.SaveState();
                        cgController.FadeInCG(args[0], currentPaletteState);
                    }
                    else
                    {
                        PaletteController.PaletteControllerState currentPaletteState = PaletteController.Current.SaveState();
                        PaletteController.Current.FadeOut(() => cgController.FadeInCG(args[0], currentPaletteState));
                    }
                };
                return result | StartLineResult.Fade;
            case "hideCG":
                // Params: none
                // Removes the previous CG (if any)
                if (cgController.Active)
                {
                    player.Pause();
                    cgController.FadeOutCG(() => player.Resume(1, true));
                }
                return result | StartLineResult.Fade;
            case "screenShake":
                // Params: float strength = 1, float duration = 1
                // Shakes the screen for duartion time with strength amount
                float strength = parts.Length > 2 ? float.Parse(args[0] != "" ? args[0] : "0.5") : 0.5f;
                float duration = parts.Length > 3 ? float.Parse(args[1] != "" ? args[1] : "0.5") : 0.5f;
                CameraController.Current.ScreenShake(strength, duration);
                break;
            case "darkenScreen":
                // Params: bool fixDoubleWhite = false
                // Darkens all palettes by one stage. If fixDoubleWhite is on, darkens true white (0) twice.
                PaletteController.Current.DarkenScreen(args.Length > 0 ? args[0] == "T" : false);
                break;
            default:
                throw Bugger.Error("No matching command! (" + commandName + ")");
        }
        return result | StartLineTrue(num + 1);
    }

    public static StartLineResult ExecuteMidBattleScreenCommand(
        string commandName,
        string[] parts,
        string[] args,
        ConversationPlayer player)
    {
        StartLineResult result = StartLineResult.None;
        switch (commandName)
        {
            case "showInfoDialogue":
                // Args: title
                player.Pause();
                player.InfoDialogue.Text.text = args[0];
                player.InfoDialogue.Begin();
                return result | StartLineResult.MidBattleScreen;
            case "showPartTitle":
                // Args: subtitle, title
                player.Pause();
                PartTitleAnimation partTitle = GameObject.Instantiate(GameController.Current.PartTitle).GetComponentInChildren<PartTitleAnimation>();
                partTitle.Begin(new List<string>(new string[] { args[0], args[1] }));
                partTitle.transform.parent.gameObject.SetActive(false);
                player.FadeThisOut(() => { partTitle.InitPalette(); partTitle.FadeThisIn(); }, null, false);
                return result | StartLineResult.MidBattleScreen;
            case "showChoice":
                // Args: choosingCharacterName, option1, option2
                player.Pause();
                player.gameObject.SetActive(true);
                player.enabled = false;
                player.Text.text = "";
                player.Arrow.SetActive(false);
                player.PortraitL.Portrait = PortraitController.Current.FindPortrait(player.Name.text = args[0]);
                player.SetSinglePortrait(true);
                player.SetSpeaker(true);
                if (parts.Length != 5)
                {
                    throw Bugger.Error("Currently, choices of more than 2 options aren't supported.");
                }
                player.ChoiceMenu.MenuItems[0].Text = args[1];
                player.ChoiceMenu.MenuItems[1].Text = args[2];
                // To prevent people from thinking a choice is "correct"
                player.ChoiceMenu.Begin();
                player.ChoiceMenu.SelectItem(Random.Range(0, 2));
                return result | StartLineResult.MidBattleScreen;
            case "showBase":
                // Args: none
                player.Pause();
                BaseController baseController = GameObject.Instantiate(GameController.Current.BaseMenu, GameController.Current.Canvas.transform).GetComponentInChildren<BaseController>();
                baseController.Show(GameController.Current.PlayerUnits);
                return result | StartLineResult.MidBattleScreen;
            default:
                throw Bugger.Error("No matching command! (" + commandName + ")");
        }
    }

    public static StartLineResult ExecuteGlobalCommand(
        string commandName,
        string[] parts,
        string[] args,
        ConversationData origin,
        int num,
        System.Func<int, StartLineResult> StartLineTrue)
    {
        StartLineResult result = StartLineResult.None;
        switch (commandName)
        {
            case "unlockKnowledge":
                GameCalculations.UnlockKnowledge(args[0]);
                break;
            case "setFlag":
                SavedData.Save("ConversationData", "Flag" + args[0], 1);
                break;
            case "setTempFlag":
                // Params: name
                GameController.Current.TempFlags.Add(args[0]);
                break;
            case "markDone":
                // Params: none
                origin.Choose(true);
                break;
            case "setCounter":
                // Params: string name, int amount
                SavedData.Save("ConversationData", "Counter" + args[0], int.Parse(args[1]));
                break;
            case "addCounter":
                // Params: string name, int amount
                SavedData.Append("ConversationData", "Counter" + args[0], int.Parse(args[1]));
                break;
            case "unlockAchievement":
                // Params: string name
                AchievementController.UnlockAchievement(args[0]);
                break;
            default:
                throw Bugger.Error("No matching command! (" + commandName + ")");
        }
        return result | StartLineTrue(num + 1);
    }

    public static StartLineResult ExecuteSyntaxCommand(
        string commandName,
        string[] parts,
        string[] args,
        string line,
        ConversationPlayer player,
        List<string> lines,
        int num,
        ConversationData origin,
        Stack<ConversationPlayer.FunctionStackObject> functionStack,
        PlayMode playMode,
        System.Func<int, StartLineResult> StartLineTrue)
    {
        StartLineResult result = StartLineResult.None;
        switch (commandName)
        {
            case "if":
                /* Syntax:
                 * :if:hasFlag:bla{
                 * Firbell: Will happen if hasFlag (requirement)
                 * }
                 * :else:{
                 * Firbell: Will happen if !hasFlag
                 * }
                 * Firbell: Will anyway happen
                 */
                // A bit too complex to assert for now
                string requirement = line.Substring(line.IndexOf(':', 1) + 1);
                requirement = requirement.Substring(0, requirement.IndexOf('{'));
                if (!origin.MeetsRequirement(requirement))
                {
                    num = SkipBlock(num, lines);
                    // If found an else, do that content.
                    if (lines[num + 1].Contains(":else:"))
                    {
                        return result | StartLineTrue(num + 2);
                    }
                }
                break;
            case "else":
                // Reaching an else outside an if means that it wasn't taken, so just skip the block.
                // A bit too complex to assert for now
                num = SkipBlock(num, lines);
                break;
            case "call":
                if (origin.Functions.ContainsKey(args[0]))
                {
                    // Store current lines & position
                    functionStack.Push(new ConversationPlayer.FunctionStackObject(num, lines.FindAll(a => true)));
                    // Load new lines
                    lines.Clear();
                    lines.AddRange(origin.Functions[args[0]]);
                    return result | StartLineTrue(0);
                }
                throw Bugger.Error("No matching function! (" + args[0] + ")");
            case "callOther":
                // Store current lines & position
                functionStack.Push(new ConversationPlayer.FunctionStackObject(num, lines.FindAll(a => true)));
                // Load new conversation
                ConversationData conversation = ConversationController.Current.SelectConversationByID(args[0]);
                if (conversation != null)
                {
                    lines.Clear();
                    lines.AddRange(playMode == PlayMode.PostBattle ? conversation.PostBattleLines : conversation.Lines);
                    return result | StartLineTrue(0);
                }
                throw Bugger.Error("No matching conversation! (" + args[0] + ")");
            case "wait":
                // Params: string[] requirement
                // A bit too complex to assert for now
                player.waitRequirement = line.Substring(line.IndexOf(':', 1) + 1);
                player.Pause();
                if (playMode != PlayMode.PostBattle)
                {
                    CrossfadeMusicPlayer.Current.Play(GameController.Current.LevelMetadata.MusicName, false);
                }
                return result | StartLineResult.Wait;
            case "return":
                if (functionStack.Count == 0)
                {
                    throw Bugger.Error("Nothing to return from!");
                }
                ConversationPlayer.FunctionStackObject function = functionStack.Pop();
                lines.Clear();
                lines.AddRange(function.Lines);
                return result | StartLineTrue(function.LineNumber + 1);
            case "finishConversation":
                // Params: none
                player.FinishConversation();
                return result | StartLineResult.FinishConversation;
            default:
                throw Bugger.Error("No matching command! (" + commandName + ")");
        }
        return result | StartLineTrue(num + 1);
    }

    public static StartLineResult ExecuteTutorialCommand(
        string commandName,
        string[] parts,
        string[] args,
        ConversationPlayer player,
        int num,
        System.Func<int, StartLineResult> StartLineTrue)
    {
        StartLineResult result = StartLineResult.None;
        switch (commandName)
        {
            case "tutorialForceButton":
                // Not asserting tutorials for now
                if (TutorialGameController.Current == null)
                {
                    throw Bugger.Error("Don't use tutorial commands outside the tutorial");
                }
                TutorialGameController.ForceButton forceButton = new TutorialGameController.ForceButton();
                forceButton.Move = System.Enum.TryParse(args[0], out forceButton.Button);
                if (parts.Length > 3)
                {
                    string[] pos = args[1].Split(',');
                    forceButton.Pos = new Vector2Int(int.Parse(pos[0]), int.Parse(pos[1]));
                    if (parts.Length > 4)
                    {
                        forceButton.WrongLine = int.Parse(args[2]);
                    }
                }
                TutorialGameController.Current.CurrentForceButton = forceButton;
                TutorialGameController.Current.WaitingForForceButton = true;
                player.Pause();
                return result | StartLineResult.Wait;
            case "tutorialShowMarker":
                // Not asserting tutorials for now
                if (TutorialGameController.Current == null)
                {
                    throw Bugger.Error("Don't use tutorial commands outside the tutorial");
                }
                string[] markerPos = args[0].Split(',');
                TutorialGameController.Current.ShowMarkerCursor(new Vector2Int(int.Parse(markerPos[0]), int.Parse(markerPos[1])));
                break;
            case "tutorialFinish":
                // Not asserting tutorials for now
                if (TutorialGameController.Current == null)
                {
                    throw Bugger.Error("Don't use tutorial commands outside the tutorial");
                }
                SavedData.SaveAll(SaveMode.Slot);
                SceneController.LoadScene("Map");
                return result | StartLineResult.FinishLevel;
            default:
                throw Bugger.Error("No matching command! (" + commandName + ")");
        }
        return result | StartLineTrue(num + 1);
    }

    public static StartLineResult ExecuteMenuCommand(
        string commandName,
        string[] parts,
        string[] args,
        ConversationPlayer player,
        int num,
        System.Func<int, StartLineResult> StartLineTrue)
    {
        StartLineResult result = StartLineResult.None;
        switch (commandName)
        {
            case "introShowCutscene":
                // Params: none
                if (player.Intro == null)
                {
                    throw Bugger.Error("Don't use intro commands outside the intro");
                }
                player.Pause();
                player.Intro.gameObject.SetActive(true);
                return result | StartLineResult.MidBattleScreen;
            case "introShowUpgradeMenu":
                // Params: none
                if (player.Knowledge == null)
                {
                    throw Bugger.Error("Don't use intro commands outside the intro");
                }
                player.Pause();
                player.Knowledge.SetActive(true);
                return result | StartLineResult.MidBattleScreen;
            case "introShowTutorial":
                // Params: none
                if (player.Tutorial == null)
                {
                    throw Bugger.Error("Don't use intro commands outside the intro");
                }
                if (SavedData.Load("ConversationData", "FlagTutorialFinish", 0) == 0)
                {
                    player.Pause();
                    player.Tutorial.Begin();
                    return result | StartLineResult.MidBattleScreen;
                }
                break;
            default:
                throw Bugger.Error("No matching command! (" + commandName + ")");
        }
        return result | StartLineTrue(num + 1);
    }

    public static StartLineResult ExecuteCommand(
        this ConversationPlayer player,
        List<string> lines,
        int num,
        ConversationData origin,
        CGController cgController,
        Stack<ConversationPlayer.FunctionStackObject> functionStack,
        PlayMode playMode,
        bool beforeBattleStart,
        bool shouldFadeIn,
        System.Func<int, StartLineResult> StartLineTrue,
        out System.Action<StartLineResult> delayedAction)
    {
        delayedAction = null;
        string line = lines[num];
        string[] parts = line.Split(':');
        // I need to add an empty "" arg at the end, for both ":loadMap" and ":loadMap:" to work
        string[] args = GetArgsFromParts(parts);
        CommandStruct command = GetCommandStruct(parts[1]);
        if (command == null)
        {
            throw Bugger.Error("No matching command! (" + parts[1] + ")");
        }
        command.Assert(args);
        switch (command.Type)
        {
            case CommandType.Level:
                return ExecuteLevelCommand(command.Name, parts, args, num, origin, StartLineTrue);
            case CommandType.Conversation:
                return ExecuteConversationCommand(command.Name, parts, args, player, num, cgController, beforeBattleStart, shouldFadeIn, StartLineTrue, out delayedAction);
            case CommandType.MidBattleScreen:
                return ExecuteMidBattleScreenCommand(command.Name, parts, args, player);
            case CommandType.Global:
                return ExecuteGlobalCommand(command.Name, parts, args, origin, num, StartLineTrue);
            case CommandType.Syntax:
                return ExecuteSyntaxCommand(command.Name, parts, args, line, player, lines, num, origin, functionStack, playMode, StartLineTrue);
            case CommandType.Tutorial:
                return ExecuteTutorialCommand(command.Name, parts, args, player, num, StartLineTrue);
            case CommandType.Menu:
                return ExecuteLevelCommand(command.Name, parts, args, num, origin, StartLineTrue);
            default:
                throw Bugger.Error("Impossible");
        }
    }

    public class CommandStruct
    {
        public string Name;
        public CommandType Type;
        public CAT[] Arguments;
        private bool NoAssert;

        public CommandStruct(string name, CommandType type, params CAT[] arguments)
        {
            Name = name;
            Type = type;
            Arguments = arguments;
        }

        public CommandStruct(string name, CommandType type, bool assert)
        {
            Name = name;
            Type = type;
            Arguments = new CAT[0];
            NoAssert = !assert;
        }

        public void Assert(string[] args)
        {
            if (NoAssert)
            {
                return;
            }
            AssertCommand(Name, args, Arguments);
        }

        private void AssertCommand(string commandName, string[] args, params CAT[] commandArguments)
        {
            if (GameCalculations.Debug)
            {
                string errorMessage = "Incorrect arguemnts: " + commandName + " requires " + string.Join(":", commandArguments).Replace("Op", "(optional)") + " arguments - " + string.Join(":", args) + " is incompatible";
                // Since args always contains "" at the end, the last argument doesn't really exist
                if ((args.Length - 1) > commandArguments.Length || ((args.Length - 1) < commandArguments.Length && (int)commandArguments[args.Length - 1] < 10))
                {
                    throw Bugger.Error(errorMessage);
                }
                for (int i = 0; i < args.Length - 1; i++)
                {
                    if (!MatchesCommandType(args[i], (CAT)((int)commandArguments[i] % 10)))
                    {
                        throw Bugger.Error(errorMessage);
                    }
                }
            }
        }

        private bool MatchesCommandType(string part, CAT command)
        {
            switch (command)
            {
                case CAT.String:
                    return true;
                case CAT.Int:
                    return int.TryParse(part, out _);
                case CAT.Float:
                    return float.TryParse(part, out _);
                case CAT.Bool:
                    return part.ToUpper() == "T" || part.ToUpper() == "F" || part.ToUpper() == "L" || part.ToUpper() == "R";
                case CAT.Team:
                    return true; // Non-existant team is all teams
                case CAT.AIType:
                    return part.ToAIType() != null;
                default:
                    throw Bugger.Error("There's no command of type " + command + "!");
            }
        }
    }
}