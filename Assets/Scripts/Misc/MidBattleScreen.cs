using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidBattleScreen : MonoBehaviour
{
    private static MidBattleScreen Current;
    public static bool HasCurrent
    {
        get
        {
            return Current != null;
        }
    }
    /// <summary>
    /// Sets the current mid-battle screen to null or called (depending on on). Usage:
    /// MidBattleScreen.Set(this, true if shows the mid-battle screen, false if closes it);
    /// </summary>
    /// <param name="caller">The mid-battle screen to set if on, and a failsafe if off (to make sure there are no contradictions).</param>
    /// <param name="on">True when the mid-battle screen is displayed, and false when it closes.</param>
    public static void Set(MidBattleScreen caller, bool on)
    {
        if (on ? Current != null : Current != caller)
        {
            throw new System.Exception("Another mid-battle screen is already running! Current: " + Current + ", caller: " + caller + ", mode: " + on);
        }
        Current = on ? caller : null;
    }
    public static void CurrentQuit()
    {
        Current.Quit();
    }
    public void Quit()
    {
        Set(this, false);
        GameController.Current.transform.parent.gameObject.SetActive(true);
        Destroy(transform.parent.gameObject);
    }
}
