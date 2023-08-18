using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TormentPowerDisplay : MonoBehaviour
{
    public RectTransform RectTransform;
    public PalettedSprite Icon;
    public Text Description;
    public List<Sprite> Modes;

    private void Reset()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public int Display(string powerName)
    {
        int split = powerName.IndexOf(powerName.Substring(1).Where(a => a.ToString().ToUpper()[0] == a).ToList()[0]); // Clumsy as heck, but whatever
        TormentPowerState state = (TormentPowerState)Mathf.Max(0, SavedData.Load<int>("Knowledge", "UpgradeTorment" + powerName));
        Icon.Sprite = Modes[(int)state];
        switch (state)
        {
            case TormentPowerState.None:
                Description.text = "------";
                Icon.Palette = 3;
                return 0;
            case TormentPowerState.I:
                Description.text = powerName.Substring(0, split);
                Icon.Palette = 1;
                return 1;
            case TormentPowerState.II:
                Description.text = powerName.Substring(split);
                Icon.Palette = 2;
                return -1;
            default:
                throw Bugger.FMError("Impossible Torment Power state");
        }
    }
}
