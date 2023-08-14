using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    public Text Text;
    public PalettedSprite MVP;
    public PalettedSprite NewGamePlus;
    public int MaxSlots;
    public Trigger StartTrigger;
    public UnitClassData UnitClassData;
    public List<GameObject> ContinueOnlyObjects;
    public List<PalettedSprite> AffectedByPlayerTeamSprites;
    [Header("External menus")]
    public MenuController BackMenu;
    public MenuController SelectMenu;
    private int currentSlot;
    private int previousSign;
    private new AdvancedSpriteSheetAnimation animation = null;

    private void Start()
    {
        Select(SavedData.Load("DefaultSaveSlot", 0, SaveMode.Global));
    }

    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.Start) || Control.GetButtonDown(Control.CB.A))
        {
            SavedData.Save("HasASaveSlot", 1, SaveMode.Global);
            SavedData.Save("NewGame", 0);
            StartTrigger.Activate();
            gameObject.SetActive(false);
            SystemSFXController.Play(SystemSFXController.Type.MenuSelect);
            return;
        }
        if (Control.GetButtonDown(Control.CB.B))
        {
            gameObject.SetActive(false);
            BackMenu.Begin();
            SystemSFXController.Play(SystemSFXController.Type.MenuCancel);
            return;
        }
        if (Control.GetButtonDown(Control.CB.Select))
        {
            if (SavedData.Load("NewGame", 1) != 1)
            {
                gameObject.SetActive(false);
                SelectMenu.Begin();
                SelectMenu.SelectItem(0);
                SystemSFXController.Play(SystemSFXController.Type.MenuSelect);
            }
            else
            {
                SystemSFXController.Play(SystemSFXController.Type.UnitForbidden);
            }
            return;
        }
        if (Control.GetAxisInt(Control.Axis.Y) != 0 && Control.GetAxisInt(Control.Axis.Y) != previousSign)
        {
            Select((currentSlot + Control.GetAxisInt(Control.Axis.Y) + MaxSlots) % MaxSlots);
            SystemSFXController.Play(SystemSFXController.Type.MenuMove);
        }
        previousSign = Control.GetAxisInt(Control.Axis.Y);
    }

    public void Select(int newSlot = -1)
    {
        // Change the internal slot
        SavedData.SaveSlot = currentSlot = (newSlot >= 0 ? newSlot : currentSlot);
        SavedData.CreateSaveSlotFiles();
        SavedData.Save("DefaultSaveSlot", currentSlot, SaveMode.Global);
        // Display the new slot's data
        if (SavedData.Load("NewGame", 1) != 1)
        {
            Text.text = "-- Slot " + (currentSlot + 1) + " --\n" +
                        "Runs: " + SavedData.Load<int>("NumRuns") + "\n" +
                        "Best: " + SavedData.Load<int>("FurthestLevel") + "\n" +
                        "Time: " + SavedData.Load<float>("PlayTime").SecondsToTime();
            if (SavedData.Load("HasSuspendData", 0) != 0)
            {
                Text.text = Text.text.ToColoredString(2);
            }
            // Color
            int palette = SavedData.Load("SaveSlotPalette", -1) >= 0 ? SavedData.Load("SaveSlotPalette", -1) : (int)StaticGlobals.MainPlayerTeam;
            AffectedByPlayerTeamSprites.ForEach(a => a.Palette = palette);
            // MVP display
            AssignUnitMapAnimation(GetMVP());
            // TBA - give the MVP a background depending on the current part (ex. magma floor for part 1, tiles for part 2, Fortress tile for 3)
            ContinueOnlyObjects.ForEach(a => a.SetActive(true));
            // New game+ display (aka turn on iff finished the game once)
            NewGamePlus.gameObject.SetActive(SavedData.Load("FinishedGame", 0) != 0);
        }
        else
        {
            Text.text = "-- Slot " + (currentSlot + 1) + " --\n\n" + "  New Game ";
            ContinueOnlyObjects.ForEach(a => a.SetActive(false));
        }
    }

    private ClassData GetMVP()
    {
        UnitData target = UnitClassData.UnitDatas[0];
        int targetValue = int.MinValue;
        foreach (UnitData unit in UnitClassData.UnitDatas)
        {
            int mapCount, battleCount, killCount, deathCount, mvpValue;
            if ((mapCount = SavedData.Load<int>("Statistics", unit.Name + "MapsCount", 0)) > 0)
            {
                battleCount = SavedData.Load<int>("Statistics", unit.Name + "BattleCount", 0);
                killCount = SavedData.Load<int>("Statistics", unit.Name + "KillCount", 0);
                deathCount = SavedData.Load<int>("Statistics", unit.Name + "DeathCount", 0);
                mvpValue = GameCalculations.MVPValue(mapCount, battleCount, killCount, deathCount);
                Bugger.Info(unit.Name + " - M: " + mapCount + ", B: " + battleCount + ", K: " + killCount + ", D: " + deathCount + ", total: " + mvpValue);
                if (mvpValue > targetValue)
                {
                    target = unit;
                    targetValue = mvpValue;
                }
            }
        }
        return UnitClassData.ClassDatas.Find(a => a.Name == target.Class);
    }

    private void AssignUnitMapAnimation(ClassData classData)
    {
        if (animation != null)
        {
            Destroy(animation.gameObject);
        }
        animation = Instantiate(UnitClassData.BaseAnimation, MVP.transform);
        animation.Renderer = MVP.GetComponent<SpriteRenderer>();
        animation.Animations[0].SpriteSheet = classData.MapSprite;
        animation.Start();
        animation.Activate(0);
    }
}
