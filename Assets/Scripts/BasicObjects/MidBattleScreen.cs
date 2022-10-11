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
    /// <summary>
    /// All it does is enable the GameObject & call MidBattleScreen.Set. For convenience.
    /// </summary>
    public void Begin()
    {
        gameObject.SetActive(true);
        MidBattleScreen.Set(this, true);
    }

    public static void CurrentQuit(bool fadeTransition = true)
    {
        Current.Quit(fadeTransition);
    }

    public bool IsCurrent()
    {
        return MidBattleScreen.Current == this;
    }
    /// <summary>
    /// Does the bare minimum to fade this out - disables GameController + this, and destroys this after fading out
    /// </summary>
    /// <param name="postFadeOutAction">An action to invoke after fading out</param>
    /// <param name="postFadeOutState">The palette state after fading out (for the next fade in)</param>
    public void FadeThisOut(System.Action postFadeOutAction = null, PaletteController.PaletteControllerState postFadeOutState = null, bool disableThis = true)
    {
        // Pause the game so nothing accidently breaks
        enabled = false;
        if (GameController.Current != null)
        {
            GameController.Current.enabled = false;
        }
        // Prepare the actions
        PaletteController.PaletteControllerState state = PaletteController.Current.SaveState();
        System.Action postFadeOut = () =>
        {
            if (disableThis)
            {
                transform.parent.gameObject.SetActive(false);
                Set(this, false);
            }
            if (GameController.Current != null)
            {
                GameController.Current.enabled = true; // Just in case
            }
            PaletteController.Current.LoadState(postFadeOutState ?? state);
            postFadeOutAction?.Invoke();
        };
        // Begin the fade
        PaletteController.Current.FadeOut(postFadeOut);
    }
    /// <summary>
    /// Does the bare minimum to fade this in - disables GameController + this, displays this, and enables this after the fade
    /// </summary>
    /// <param name="postFadeInAction">An action to invoke after fading in</param>
    public void FadeThisIn(System.Action postFadeInAction = null, bool enableThis = true)
    {
        // Pause the game so nothing accidently breaks
        enabled = false;
        if (GameController.Current != null)
        {
            GameController.Current.enabled = false;
        }
        if (enableThis)
        {
            transform.parent.gameObject.SetActive(true);
            Set(this, true);
        }
        // Prepare the actions
        System.Action postFadeIn = () =>
        {
            enabled = true;
            if (GameController.Current != null)
            {
                GameController.Current.enabled = true; // Just in case - there's a mid-battle screen to stop it, anyway
            }
            postFadeInAction?.Invoke();
        };
        PaletteController.Current.FadeIn(postFadeIn);
    }

    protected void Quit(bool fadeTransition = true, System.Action postQuitAction = null, PaletteController.PaletteControllerState postFadeOutState = null)
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
                PaletteController.Current.FadeIn(postFadeIn);
            };
            // Begin the fade
            FadeThisOut(postFadeOut, postFadeOutState);
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
                PaletteController.Current.FadeIn(postFadeIn);
            };
            // Begin the fade
            PaletteController.Current.FadeOut(postFadeOut);
        }
    }
}
