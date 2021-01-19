using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpObject : MonoBehaviour
{
    public RectTransform RectTransform;
    public PalettedSprite PalettedSprite;
    public Text Text;
    [HideInInspector]
    public Stats Stats;
    private void Reset()
    {
        RectTransform = GetComponent<RectTransform>();
        PalettedSprite = GetComponent<PalettedSprite>();
        Text = GetComponentInChildren<Text>();
    }
}
