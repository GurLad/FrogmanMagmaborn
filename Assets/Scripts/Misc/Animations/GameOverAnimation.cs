using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverAnimation : MonoBehaviour
{
    [Header("Data")]
    public float Speed;
    public float Delay;
    public Palette GameOverPalette;
    [Header("Objects")]
    public PalettedSprite GameOverImage;
    private bool finishedOne;
    private int lastCheckedCurrent;
    private PaletteTransition transition;
    private void Start()
    {
        transition = PaletteController.Current.PaletteTransitionTo(true, 0, GameOverPalette, Speed);
        CrossfadeMusicPlayer.Current.Play("GameOver", false);
    }
    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.Start))
        {
            End();
            return;
        }
        if (transition == null)
        {
            Delay -= Time.deltaTime;
            if (Delay <= 0)
            {
                if (finishedOne)
                {
                    End();
                }
                else
                {
                    transition = PaletteController.Current.PaletteTransitionTo(true, 0, new Palette(), Speed, true, true);
                    finishedOne = true;
                }
            }
        }
        else if (lastCheckedCurrent != transition.Current)
        {
            GameOverImage.UpdatePalette();
            lastCheckedCurrent = transition.Current;
        }
    }
    private void End()
    {
        // TBA: Add restart/return to menu/exit menu
        SceneController.LoadScene("Menu");
    }
}
