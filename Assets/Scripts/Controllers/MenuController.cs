using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MidBattleScreen
{
    public List<MenuItem> MenuItems;
    public int Selected;
    public bool Cancelable; // For things like the main menu vs. pause menu
    public Trigger CancelTrigger;
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
        if (Control.GetAxisInt(Control.Axis.Y) != 0 && Control.GetAxisInt(Control.Axis.Y) != previousSign)
        {
            MenuItems[Selected].Unselect();
            Selected += -Control.GetAxisInt(Control.Axis.Y) + count;
            Selected %= count;
            MenuItems[Selected].Select();
        }
        previousSign = Control.GetAxisInt(Control.Axis.Y);
    }
    private void MenuDone()
    {
        // Generic menu done behaviour. Maybe add ContinuousTrigger/isDone?
        gameObject.SetActive(false);
    }
}
