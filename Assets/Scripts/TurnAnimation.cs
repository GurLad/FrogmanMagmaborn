using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnAnimation : MidBattleScreen
{
    private enum Step { Open, Display, Close, Sleep }
    public float OpenCloseTime;
    public float DisplayTime;
    private Text text;
    private PalettedSprite palette;
    private RectTransform rectTransform;
    private float initSize;
    private Step currentStep = Step.Sleep;
    private float count;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        text = GetComponentInChildren<Text>();
        palette = GetComponent<PalettedSprite>();
        initSize = rectTransform.sizeDelta.y;
        gameObject.SetActive(false);
    }
    public void ShowTurn(Team team)
    {
        ShowHideText(false);
        text.text = GameController.TeamToString(team) + " turn";
        palette.Palette = (int)team;
        currentStep = Step.Open;
        count = 0;
        gameObject.SetActive(true);
        MidBattleScreen.Set(this, true);
    }
    private void Update()
    {
        if (currentStep != Step.Sleep)
        {
            count += Time.deltaTime;
            if (count >= (currentStep == Step.Display ? DisplayTime : OpenCloseTime))
            {
                count -= currentStep == Step.Display ? DisplayTime : OpenCloseTime;
                currentStep++;
                switch (currentStep)
                {
                    case Step.Display:
                        ShowHideText(true);
                        break;
                    case Step.Close:
                        ShowHideText(false);
                        break;
                    case Step.Sleep:
                        gameObject.SetActive(false);
                        MidBattleScreen.Set(this, false);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    private void ShowHideText(bool show)
    {
        if (show)
        {
            text.gameObject.SetActive(true);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, initSize);
        }
        else
        {
            text.gameObject.SetActive(false);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, initSize / 2);
        }
    }
}
