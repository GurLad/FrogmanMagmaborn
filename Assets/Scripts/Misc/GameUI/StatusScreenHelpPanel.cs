using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusScreenHelpPanel : MonoBehaviour
{
    public Text SmallDisplay;
    public Text ThisDisplay;
    public List<HelpPanelInfo> PanelInfos;
    public StatusScreenController Controller;
    [Header("Help")]
    public Transform Canvas;
    public DisplayStatsHelp StatsHelp;
    private int current;
    private int count;
    private int previousDir;
    private bool _showingInfo;
    private bool showingInfo
    {
        get
        {
            return _showingInfo;
        }
        set
        {
            _showingInfo = value;
            SmallDisplay.transform.parent.gameObject.SetActive(!value);
            ThisDisplay.transform.parent.gameObject.SetActive(value);
            if (!value)
            {
                PanelInfos[current].Object.Palette = PanelInfos[current].BasePalette;
            }
            else
            {
                Show(current);
            }
        }
    }

    private void Start()
    {
        for (int i = 0; i < PanelInfos.Count; i++)
        {
            if (PanelInfos[i].Object == null || PanelInfos[i].Object.enabled == false)
            {
                PanelInfos.RemoveAt(i--);
            }
            else
            {
                PanelInfos[i].BasePalette = PanelInfos[i].Object.Palette;
            }
        }
        count = PanelInfos.Count;
        SmallDisplay.text = SmallDisplay.text.Replace("Select", Control.DisplayShortButtonName("Select"));
        Show(0);
        showingInfo = false;
    }

    private void Update()
    {
        if (!Controller.IsCurrent() || !Controller.enabled)
        {
            return;
        }
        if (Control.GetButtonDown(Control.CB.Select))
        {
            showingInfo = !showingInfo;
            if (showingInfo)
            {
                Controller.ShowingHelpInfo = true; // Just in case someone presses Select and B or something at the same time
            }
        }
        if (!showingInfo)
        {
            return;
        }
        if (Control.GetButtonDown(Control.CB.B))
        {
            showingInfo = false;
        }
        if (PanelInfos[current].AllowStatsHelp && Control.GetButtonDown(Control.CB.A))
        {
            StatsHelp.Activate(Controller, Canvas);
            return;
        }
        int direction = -Control.GetAxisInt(Control.Axis.Y);
        if (direction != 0 && direction != previousDir)
        {
            Show((current + count + direction) % count);
        }
        previousDir = direction;
    }

    private void LateUpdate()
    {
        Controller.ShowingHelpInfo = showingInfo;
    }

    private void Show(int toShow)
    {
        PanelInfos[current].Object.Palette = PanelInfos[current].BasePalette;
        current = toShow;
        PanelInfos[current].Object.Palette = 3;
        ThisDisplay.text = PanelInfos[current].Info.Replace("[AButton]", Control.DisplayShortButtonName(Control.CB.A));
    }
}

[System.Serializable]
public class HelpPanelInfo
{
    public PalettedSprite Object;
    [TextArea]
    public string Info;
    public bool AllowStatsHelp;
    [HideInInspector]
    public int BasePalette;
}