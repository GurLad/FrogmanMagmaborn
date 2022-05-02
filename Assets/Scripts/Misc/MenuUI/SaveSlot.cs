using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    public Text Text;
    public PalettedSprite MVP;
    public int MaxSlots;
    public Trigger StartTrigger;
    public UnitClassData UnitClassData;
    public List<GameObject> ContinueOnlyObjects;
    private int currentSlot;
    private int previousSign;
    private AdvancedSpriteSheetAnimation animation = null;

    private void Start()
    {
        MVP.Palette = (int)StaticGlobals.MainPlayerTeam;
        Select(SavedData.Load("DefaultSaveSlot", 0, SaveMode.Global));
    }

    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.Start) || Control.GetButtonDown(Control.CB.A))
        {
            SavedData.Save("NewGame", 0);
            StartTrigger.Activate();
            gameObject.SetActive(false);
            return;
        }
        if (Control.GetAxisInt(Control.Axis.Y) != 0 && Control.GetAxisInt(Control.Axis.Y) != previousSign)
        {
            Select((currentSlot - Control.GetAxisInt(Control.Axis.Y) + MaxSlots) % MaxSlots);
        }
        previousSign = Control.GetAxisInt(Control.Axis.Y);
    }

    private void Select(int newSlot)
    {
        // Change the internal slot
        SavedData.SaveSlot = currentSlot = newSlot;
        SavedData.CreateSaveSlotFiles();
        SavedData.Save("DefaultSaveSlot", currentSlot, SaveMode.Global);
        // Display the new slot's data
        if (SavedData.Load("NewGame", 1) != 1)
        {
            Text.text = "-- Slot " + (currentSlot + 1) + " --\n" +
                        "Runs: " + SavedData.Load<int>("NumRuns") + "\n" +
                        "Best: " + SavedData.Load<int>("FurthestLevel") + "\n" +
                        "Time: " + SecondsToTime(SavedData.Load<float>("PlayTime"));
            if (SavedData.Load("HasSuspendData", 0) != 0)
            {
                Text.text = Text.text.ToColoredString(2);
            }
            // MVP display
            AssignUnitMapAnimation(GetMVP());
            // TBA - give the MVP a background depending on the current part (ex. magma floor for part 1, tiles for part 2, Fortress tile for 3)
            ContinueOnlyObjects.ForEach(a => a.SetActive(true));
        }
        else
        {
            Text.text = "-- Slot " + (currentSlot + 1) + " --\n\n" + "  New Game ";
            ContinueOnlyObjects.ForEach(a => a.SetActive(false));
        }
    }

    private string SecondsToTime(float seconds)
    {
        return Mathf.FloorToInt(seconds / 3600).ToString().PadLeft(3, '0') + ":" + Mathf.FloorToInt((seconds / 60) % 60).ToString().PadLeft(2, '0');
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
