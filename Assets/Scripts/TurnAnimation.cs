using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnAnimation : MonoBehaviour
{
    public float TargetPos = 200;
    public float Speed;
    private Text text;
    private PalettedSprite palette;
    private RectTransform rectTransform;
    private float pos;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        text = GetComponentInChildren<Text>();
        palette = GetComponent<PalettedSprite>();
        pos = TargetPos;
    }
    public void ShowTurn(Team team)
    {
        text.text = team + " turn";
        palette.Palette = (int)team;
        pos = -TargetPos;
    }
    private void Update()
    {
        if (pos < TargetPos)
        {
            pos += Time.deltaTime * 60 * Speed;
            if (pos >= TargetPos)
            {
                pos = TargetPos;
            }
            rectTransform.anchoredPosition = new Vector2(pos, rectTransform.anchoredPosition.y);
        }
    }
}
