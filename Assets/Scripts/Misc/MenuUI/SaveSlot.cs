using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    public Text Text;
    public Image MVP;
    public int MaxSlots;
    public Trigger StartTrigger;
    private int currentSlot;
    private int previousSign;

    private void Start()
    {
        Select(SavedData.Load("DefaultSaveSlot", 0, SaveMode.Global));
    }

    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.Start) || Control.GetButtonDown(Control.CB.A))
        {
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
        Text.text = "-- Slot " + (currentSlot + 1) + " --\n" + 
                    "Runs: " + SavedData.Load<int>("NumRuns") + "\n" +
                    "Best: " + SavedData.Load<int>("FurthestLevel") + "\n" + 
                    "Time: " + SecondsToTime(SavedData.Load<float>("PlayTime"));
        // MVP display TBA - requires LevelMetadata, ClassData & UnitData in the main menu...
        MVP.gameObject.SetActive(false);
    }

    private string SecondsToTime(float seconds)
    {
        return Mathf.FloorToInt(seconds / 3600).ToString().PadLeft(3, '0') + ":" + Mathf.FloorToInt((seconds / 60) % 60).ToString().PadLeft(2, '0');
    }
}
