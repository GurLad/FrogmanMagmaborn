using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingStatsController : MidBattleScreen
{
    public List<string> TormentPowers;
    public int TormentPowersPerRow;
    [Header("Fin palette")]
    public Palette FinPalette;
    [Header("Objects")]
    public RectTransform Holder;
    public RectTransform PowersHolder;
    public Text WonText;
    public Text RunStats;
    public TormentPowerDisplay BaseDisplay;
    public GameObject FinObject;

    public void Display(string wonName, int wonPalette)
    {
        // Won text
        WonText.text = wonName.ToColoredString(wonPalette) + " won!";
        // Run stats - requires the runStats rework
        RunStatsController.Current?.RecordGameEvent(RunStatsController.GameEvent.RecordPlayTime);
        Difficulty difficulty = (Difficulty)SavedData.Load("Knowledge", "UpgradeDifficulty", 0);
        RunStats.text = difficulty.ToString().ToColoredString((int)difficulty - 1) + "\n" +
            RunStatsController.Current.GameStats.TotalTurns + "\n" +
            RunStatsController.Current.GameStats.PlayTime.SecondsToTime();
        AchievementController.UnlockAchievement("HardcodedWin");
        if (difficulty == Difficulty.Insane)
        {
            AchievementController.UnlockAchievement("HardcodedWinInsane");
        }
        // TormentPowers
        int balance = 0;
        for (int i = 0; i < TormentPowers.Count; i++)
        {
            int perRow = Mathf.Min(TormentPowersPerRow, TormentPowers.Count - (i / TormentPowersPerRow) * TormentPowersPerRow);
            TormentPowerDisplay newDisplay = Instantiate(BaseDisplay, BaseDisplay.transform.parent);
            newDisplay.Icon.Awake();
            balance += newDisplay.Display(TormentPowers[i]);
            newDisplay.RectTransform.anchoredPosition += new Vector2(
                (PowersHolder.sizeDelta.x - BaseDisplay.RectTransform.sizeDelta.x) * (perRow > 1 ? (i % perRow) / (float)(perRow - 1) : 0.5f),
                -(BaseDisplay.RectTransform.sizeDelta.y + 8) * (i / TormentPowersPerRow));
            newDisplay.gameObject.SetActive(true);
        }
        if (balance >= TormentPowers.Count)
        {
            AchievementController.UnlockAchievement("HardcodedWinAllTormentI");
        }
        else if (balance <= -TormentPowers.Count)
        {
            AchievementController.UnlockAchievement("HardcodedWinAllTormentII");
        }
    }

    private void Update()
    {
        if (Control.GetButtonUp(Control.CB.A) && Time.timeScale > 0)
        {
            if (Holder.gameObject.activeSelf)
            {
                PaletteController.Current.FadeOut(() =>
                {
                    Holder.gameObject.SetActive(false);
                    FinObject.gameObject.SetActive(true);
                    GameController.Current.LevelMetadata.SetPalettesFromMetadata();
                    PaletteController.Current.BackgroundPalettes[0].CopyFrom(FinPalette);
                    PaletteController.Current.FadeIn(null, 30 / 4);
                }, 30 / 4);
            }
            else
            {
                PaletteController.Current.FadeOut(() =>
                {
                    RunStatsController.Current?.AddToTotal(true, false);
                    SavedData.Save("FinishedGame", 1);
                    SavedData.SaveAll();
                    SceneController.LoadScene("Menu");
                }, 30 / 4);
            }
        }
    }
}
