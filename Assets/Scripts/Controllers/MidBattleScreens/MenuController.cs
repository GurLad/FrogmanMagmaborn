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
    protected int count;
    private int previousSign;

    protected virtual void Start()
    {
        SelectItem(Selected);
        count = MenuItems.Count;
    }

    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.A))
        {
            FinishMenu();
            MenuItems[Selected].Activate();
            AfterMenuDone?.Activate();
            return;
        }
        if (Control.GetButtonDown(Control.CB.B) && Cancelable)
        {
            FinishMenu();
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
                FinishMenu();
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

    public virtual void SelectItem(int index)
    {
        MenuItems[Selected].Unselect();
        Selected = index;
        MenuItems[Selected].Select();
    }

    public void Finish()
    {
        // Generic menu done behaviour. Maybe add ContinuousTrigger/isDone?
        MidBattleScreen.Set(this, false);
        gameObject.SetActive(false);
        MenuItems.ForEach(a => a.OnMenuDone());
    }

    private void FinishMenu()
    {
        if (FinishOnSelect)
        {
            Finish();
        }
    }
}
