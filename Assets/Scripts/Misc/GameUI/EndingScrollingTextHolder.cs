using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingScrollingTextHolder : MonoBehaviour
{
    private enum State { Idle, Scrolling, Waiting }

    [Header("Vars")]
    public float ScrollTime;
    public float WaitTime;
    public int LineWidth = 30;
    [Header("Objects")]
    public EndingCardsController EndingCardsController;
    public Text Text;
    private State state;
    private float count;

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                break;
            case State.Scrolling:
                count += Time.deltaTime;
                if (Control.GetButton(Control.CB.Start))
                {
                    count += 2 * Time.deltaTime;
                }
                if (count >= ScrollTime)
                {
                    count = 0;
                    Text.rectTransform.anchoredPosition = Vector2.zero;
                    state = State.Waiting;
                    break;
                }
                float percent = count / ScrollTime;
                Text.rectTransform.anchoredPosition = new Vector2(0, Mathf.RoundToInt(Mathf.Lerp(-Text.rectTransform.sizeDelta.y, 0, percent)));
                break;
            case State.Waiting:
                count += Time.deltaTime;
                if (count >= WaitTime)
                {
                    count = 0;
                    state = State.Idle;
                    PaletteController.Current.FadeOut(() => EndingCardsController.DisplayNext(), 30 / 4);
                }
                break;
            default:
                break;
        }
    }

    public void Display(EndingCardsController.GlobalEndingData endingData)
    {
        Text.text = endingData.Text;
        Text.rectTransform.anchoredPosition = new Vector2(0, -Text.rectTransform.sizeDelta.y);
        PaletteController.Current.LoadState(EndingCardsController.SavedState);
        PaletteController.Current.FadeIn(() => { state = State.Scrolling; });
    }
}
