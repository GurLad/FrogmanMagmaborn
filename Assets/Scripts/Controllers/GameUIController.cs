using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [Header("UI")]
    public RectTransform UITileInfoPanel;
    public Text UITileInfo;
    public RectTransform UIUnitInfoPanel;
    public Text UIUnitInfo;
    public RectTransform UIFightPanel;
    public MiniBattleStatsPanel UIAttackerPanel;
    public MiniBattleStatsPanel UIDefenderPanel;
    [Header("Objects")]
    public CursorController Cursor;

    public void ShowUI(Vector2Int pos)
    {
        UITileInfoPanel.gameObject.SetActive(true);
        UIUnitInfoPanel.gameObject.SetActive(true);
        Cursor.gameObject.SetActive(true);
        UITileInfo.text = GameController.Current.Map[pos.x, pos.y].ToString();
        Unit unit = GameController.Current.FindUnitAtPos(pos.x, pos.y);
        Vector2 anchor;
        if (pos.x >= pos.x / 2)
        {
            anchor = new Vector2Int(0, 1);
        }
        else
        {
            anchor = new Vector2Int(1, 1);
        }
        if (unit != null)
        {
            UIUnitInfo.text = unit.ToString() + "\nHP:" + unit.Health + "/" + unit.Stats.MaxHP;
            UIUnitInfoPanel.GetComponent<PalettedSprite>().Palette = (int)unit.TheTeam;
        }
        else
        {
            UIUnitInfo.text = GameController.Current.GetInfoObjectiveText();
            UIUnitInfoPanel.GetComponent<PalettedSprite>().Palette = 3;
        }
        UIUnitInfoPanel.anchorMin = anchor;
        UIUnitInfoPanel.anchorMax = anchor;
        UIUnitInfoPanel.pivot = anchor;
        if (GameController.Current.InteractState != InteractState.None)
        {
            UIFightPanel.gameObject.SetActive(true);
            anchor.y = 0.5f;
            UIFightPanel.anchorMin = anchor;
            UIFightPanel.anchorMax = anchor;
            UIFightPanel.pivot = anchor;
            DisplayBattleForecast(GameController.Current.Selected, unit);
            DisplayBattleForecast(unit, GameController.Current.Selected, true);
        }
        else
        {
            UIFightPanel.gameObject.SetActive(false);
        }
        anchor.y = 0;
        UITileInfoPanel.anchorMin = anchor;
        UITileInfoPanel.anchorMax = anchor;
        UITileInfoPanel.pivot = anchor;
    }

    public void HideUI()
    {
        UITileInfoPanel.gameObject.SetActive(false);
        UIUnitInfoPanel.gameObject.SetActive(false);
        UIFightPanel.gameObject.SetActive(false);
        Cursor.gameObject.SetActive(false);
    }

    private void DisplayBattleForecast(Unit origin, Unit target, bool reverse = false)
    {
        MiniBattleStatsPanel panel = reverse ? UIDefenderPanel : UIAttackerPanel;
        panel.DisplayBattleForecast(origin, target, reverse);
    }
}
