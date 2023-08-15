using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndgameScreenCover : MonoBehaviour
{
    private enum State { Idle, FadeWhiteIn, HoldWhite, FadeWhiteToBlack, FadeBlackOut, HoldNothing, FadeBlackIn }

    public static EndgameScreenCover Current;
    public float Speed;
    public Image Cover;
    private State state = State.Idle;
    private float count = 0;
    private Color color = Color.black;
    private System.Action postFadeAction;

    private void Reset()
    {
        Cover = GetComponent<Image>();
    }

    private void Awake()
    {
        Current = this;
    }

    private void Update()
    {
        count += Time.deltaTime * Speed;
        switch (state)
        {
            case State.Idle:
                break;
            case State.FadeWhiteIn:
                color.a = Mathf.Min(1, count);
                if (count >= 1)
                {
                    count = 0;
                    state = State.HoldWhite;
                }
                break;
            case State.HoldWhite:
                if (count >= 1)
                {
                    count = 0;
                    state = State.FadeWhiteToBlack;
                }
                break;
            case State.FadeWhiteToBlack:
                color = Color.Lerp(Color.white, Color.black, Mathf.Min(1, count));
                if (count >= 1)
                {
                    count = 0;
                    state = State.HoldNothing;
                }
                break;
            case State.FadeBlackOut:
                color.a = 1 - Mathf.Min(1, count);
                if (count >= 1)
                {
                    count = 0;
                    state = State.HoldNothing;
                }
                break;
            case State.HoldNothing:
                if (count >= 1)
                {
                    count = 0;
                    state = State.Idle;
                    Cover.gameObject.SetActive(false);
                    postFadeAction?.Invoke();
                }
                break;
            case State.FadeBlackIn:
                color.a = Mathf.Min(1, count);
                if (count >= 1)
                {
                    count = 0;
                    state = State.HoldNothing;
                }
                break;
            default:
                break;
        }
        Cover.color = color;
    }

    public void FadeBlackOut(System.Action postFadeAction)
    {
        this.postFadeAction = postFadeAction;
        color = Color.black;
        count = 0;
        state = State.FadeBlackOut;
        Cover.gameObject.SetActive(true);
    }

    public void FadeToWhite(System.Action postFadeAction)
    {
        this.postFadeAction = postFadeAction;
        color = Color.white;
        count = 0;
        state = State.FadeWhiteIn;
        Cover.gameObject.SetActive(true);
    }

    public void FadeToBlack(System.Action postFadeAction)
    {
        this.postFadeAction = postFadeAction;
        color = Color.black;
        count = 0;
        state = State.FadeBlackIn;
        Cover.gameObject.SetActive(true);
    }
}
