using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MidBattleScreen : MonoBehaviour
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
            throw Bugger.Error("Another mid-battle screen is already running! Current: " + (Current != null ? Current.ToString() : "Null") + ", caller: " + caller + ", mode: " + on, false);
        }
        Current = on ? caller : null;
    }
    public static void CurrentQuit(bool fadeTransition = true)
    {
        Current.Quit(fadeTransition);
    }
    public bool IsCurrent()
    {
        return MidBattleScreen.Current == this;
    }
    public void Quit(bool fadeTransition = true, System.Action postQuitAction = null, PaletteController.PaletteControllerState postFadeOutState = null)
    {
        if (!fadeTransition)
        {
            Set(this, false);
            GameController.Current.transform.parent.gameObject.SetActive(true);
            Destroy(transform.parent.gameObject);
            postQuitAction?.Invoke();
        }
        else
        {
            // Pause the game so nothing accidently breaks
            enabled = false;
            GameController.Current.enabled = false;
            // Prepare the actions
            PaletteController.PaletteControllerState state = PaletteController.Current.SaveState();
            System.Action postFadeIn = () =>
            {
                GameController.Current.enabled = true;
                postQuitAction?.Invoke();
            };
            System.Action postFadeOut = () =>
            {
                Destroy(transform.parent.gameObject);
                GameController.Current.transform.parent.gameObject.SetActive(true);
                Set(this, false);
                PaletteController.Current.LoadState(postFadeOutState ?? state);
                PaletteController.Current.Fade(true, postFadeIn, 10 * GameController.Current.GameSpeed(false));
            };
            // Begin the fade
            PaletteController.Current.Fade(false, postFadeOut, 10 * GameController.Current.GameSpeed(false));
        }
    }
    public void TransitionToThis(bool fadeTransition = true, System.Action postTransitionAction = null, PaletteController.PaletteControllerState postFadeOutState = null)
    {
        if (!fadeTransition)
        {
            Set(this, true);
            transform.parent.gameObject.SetActive(true);
            GameController.Current.transform.parent.gameObject.SetActive(false);
            postTransitionAction?.Invoke();
        }
        else
        {
            // Pause the game so nothing accidently breaks
            GameController.Current.enabled = false;
            enabled = false;
            transform.parent.gameObject.SetActive(false);
            // Prepare the actions
            PaletteController.PaletteControllerState state = PaletteController.Current.SaveState();
            System.Action postFadeIn = () =>
            {
                enabled = true;
            };
            System.Action postFadeOut = () =>
            {
                GameController.Current.transform.parent.gameObject.SetActive(false);
                GameController.Current.enabled = true;
                transform.parent.gameObject.SetActive(true);
                Set(this, true);
                PaletteController.Current.LoadState(postFadeOutState ?? state);
                PaletteController.Current.Fade(true, postFadeIn, 10 * GameController.Current.GameSpeed(false));
            };
            // Begin the fade
            PaletteController.Current.Fade(false, postFadeOut, 10 * GameController.Current.GameSpeed(false));
        }
    }
}
