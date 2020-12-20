using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAnimationsController : MidBattleScreen
{
    public enum AnimationType { None, Movement, Battle, Delay }
    public new static MapAnimationsController Current;
    [Header("Movement animation")]
    public float WalkSpeed;
    public AudioClip WalkSound;
    [Header("Delay animation")]
    public float DelayTime;
    [HideInInspector]
    public AnimationType CurrentAnimation;
    [HideInInspector]
    public System.Action OnFinishAnimation;
    // Movement animation vars
    private float count;
    private Unit currentUnit;
    private List<Vector2Int> path = new List<Vector2Int>();
    private void Awake()
    {
        Current = this;
    }
    private void Update()
    {
        switch (CurrentAnimation)
        {
            case AnimationType.None:
                break;
            case AnimationType.Movement:
                count += Time.deltaTime;
                if (count >= 1 / WalkSpeed)
                {
                    count -= 1 / WalkSpeed;
                    SoundController.PlaySound(WalkSound);
                    currentUnit.Pos = path[0];
                    path.RemoveAt(0);
                    if (path.Count <= 0)
                    {
                        EndAnimation();
                    }
                }
                break;
            case AnimationType.Battle:
                break;
            case AnimationType.Delay:
                count += Time.deltaTime;
                if (count >= DelayTime)
                {
                    EndAnimation();
                }
                break;
            default:
                break;
        }
    }
    private void StartAnimation(AnimationType type)
    {
        CurrentAnimation = type;
        MidBattleScreen.Current = this;
    }
    private void EndAnimation()
    {
        CurrentAnimation = AnimationType.None;
        if (MidBattleScreen.Current == this)
        {
            // Support for chaining animations & actions.
            count = 0;
            MidBattleScreen.Current = null;
            System.Action tempAction = OnFinishAnimation;
            OnFinishAnimation = null;
            tempAction();
        }
        else
        {
            throw new System.Exception("Mid-battle screen in the middle of an animation?!");
        }
    }
    public void AnimateMovement(Unit unit, Vector2Int targetPos)
    {
        // Check if an animation is even needed
        if (OnFinishAnimation == null)
        {
            throw new System.Exception("No OnFinishAnimation command - probably animated before assigning it.");
        }
        if (unit.Pos == targetPos)
        {
            OnFinishAnimation();
            return;
        }
        currentUnit = unit;
        int[,] checkedTiles = unit.GetMovement(true); // Cannot rely on given one, as will probably not ignore allies.
        // Recover path (slightly different from the AI one, find a way to merge them?)
        if (path.Count > 0)
        {
            Debug.LogWarning("Path isn't empty!");
            path.Clear();
        }
        int counter = 0;
        do
        {
            if (counter++ > 50)
            {
                throw new System.Exception("Infinite loop in AnimatedMovement! Path: " + string.Join(", ", path));
            }
            path.Add(targetPos);
            Vector2Int currentBest = Vector2Int.zero;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 || j == 0)
                    {
                        if (GameController.Current.IsValidPos(targetPos.x + i, targetPos.y + j) &&
                            checkedTiles[targetPos.x + i, targetPos.y + j] >= checkedTiles[targetPos.x + currentBest.x, targetPos.y + currentBest.y])
                        {
                            currentBest = new Vector2Int(i, j);
                        }
                    }
                }
            }
            targetPos += currentBest;
        } while (targetPos != unit.Pos);
        path.Reverse();
        // Start animation
        StartAnimation(AnimationType.Movement);
    }
    public void AnimateDelay()
    {
        if (count != 0)
        {
            Debug.LogWarning("Count isn't zero - it's " + count);
        }
        StartAnimation(AnimationType.Delay);
    }
}
