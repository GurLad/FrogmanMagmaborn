using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TSaveSlotStartDelete : Trigger
{
    public MenuController DialogMenu;
    public Text DialogText;
    public MenuItem DialogYes;
    public MenuItem DialogNo;

    private void Start()
    {
        //DialogMenu.MenuItems.ForEach(a => a.Awake()); // Copy does it already
    }

    public override void Activate()
    {
        DialogText.text = "-- Delete --\nSlot " + (SavedData.SaveSlot + 1);
        DialogYes.gameObject.SetActive(true);
        if (!DialogMenu.MenuItems.Contains(DialogYes))
        {
            DialogMenu.MenuItems.Insert(0, DialogYes);
        }
        DialogMenu.SelectItem(1);
        DialogNo.Text = "No";
    }
}
