using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusScreenController : MidBattleScreen
{
    private void Update()
    {
        if (Control.GetButtonUp(Control.CB.B))
        {
            Quit();
        }
    }
}
