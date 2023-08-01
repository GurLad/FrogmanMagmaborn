using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TSaveSlotStartCopy : Trigger
{
    public TSaveSlotConfirmCopy SaveSlotConfirmCopy;
    public MenuController DialogMenu;
    public Text DialogText;
    public MenuItem DialogYes;
    public MenuItem DialogNo;

    private void Start()
    {
        DialogMenu.MenuItems.ForEach(a => a.Awake());
    }

    public override void Activate()
    {
        int i = SavedData.SaveSlot, init = SavedData.SaveSlot;
        do
        {
            i = (i + 1) % SavedData.MaxNumSlots;
            SavedData.SaveSlot = i;
            if (SavedData.Load("NewGame", 1) != 0) break;
        } while (i != init);
        SavedData.SaveSlot = init;
        if (i == SavedData.SaveSlot)
        {
            // No empty slots
            DialogText.text = "--  Copy  --\nNo empty slots!";
            DialogYes.gameObject.SetActive(false);
            if (DialogMenu.MenuItems.Contains(DialogYes))
            {
                DialogMenu.MenuItems.Remove(DialogYes);
            }
            DialogMenu.Selected = 0;
            DialogMenu.SelectItem(0);
            DialogNo.Text = "Back";
        }
        else
        {
            // Found an empty slot
            DialogText.text = "--  Copy  --\nSlot " + (SavedData.SaveSlot + 1) + " > " + (i + 1);
            DialogYes.gameObject.SetActive(true);
            if (!DialogMenu.MenuItems.Contains(DialogYes))
            {
                DialogMenu.MenuItems.Insert(0, DialogYes);
            }
            DialogMenu.SelectItem(1);
            DialogNo.Text = "No";
            SaveSlotConfirmCopy.Target = i;
        }
    }
}
