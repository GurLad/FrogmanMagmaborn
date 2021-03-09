using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MidBattleScreen
{
    public List<MenuItem> MenuItems;
    public int Selected;
    public bool Cancelable; // For things like the main menu vs. pause menu
    public bool FinishOnSelect = true;
    public Trigger CancelTrigger;
    public Trigger StartTrigger;
    public Trigger AfterMenuDone;
    private Text text;
    [HideInInspector]
    public Text Text
    {
        get
        {
            if (text == null)
            {
                text = GetComponentInChildren<Text>(true);
            }
            return text;
        }
    }
    private int count;
    private int previousSign;
    public void Start()
    {
        MenuItems[Selected].Select();
        count = MenuItems.Count;
    }
    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.A))
        {
            MenuDone();
            MenuItems[Selected].Activate();
            AfterMenuDone?.Activate();
            return;
        }
        if (Control.GetButtonDown(Control.CB.B) && Cancelable)
        {
            MenuDone();
            if (CancelTrigger != null)
            {
                CancelTrigger.Activate();
            }
            return;
        }
        if (Control.GetButtonDown(Control.CB.Start))
        {
            if (StartTrigger != null)
            {
                MenuDone();
                StartTrigger.Activate();
            }
            return;
        }
        if (Control.GetAxisInt(Control.Axis.Y) != 0 && Control.GetAxisInt(Control.Axis.Y) != previousSign)
        {
            SelectItem((Selected - Control.GetAxisInt(Control.Axis.Y) + count) % count);
        }
        previousSign = Control.GetAxisInt(Control.Axis.Y);
    }
    public void SelectItem(int index)
    {
        MenuItems[Selected].Unselect();
        Selected = index;
        MenuItems[Selected].Select();
    }
    private void MenuDone()
    {
        if (FinishOnSelect)
        {
            // Generic menu done behaviour. Maybe add ContinuousTrigger/isDone?
            if (GameController.Current != null)
            {
                MidBattleScreen.Set(this, false);
            }
            gameObject.SetActive(false);
        }
    }
}
