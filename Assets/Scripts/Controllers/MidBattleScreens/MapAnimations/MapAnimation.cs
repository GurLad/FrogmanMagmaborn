using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapAnimation : MidBattleScreen
{
    [HideInInspector]
    public System.Action OnFinishAnimation;
    protected float count;
    protected bool init = false;

    private void Update()
    {
        Time.timeScale = GameCalculations.GameSpeed(); // Double speed
        count += Time.deltaTime;
        Animate();
    }

    public void StartAnimation()
    {
        if (!init)
        {
            throw Bugger.FMError("Map Animation: Missing init!");
        }
        MidBattleScreen.Set(this, true);
    }

    protected void EndAnimation()
    {
        Time.timeScale = 1; // Remove double speed
        // Support for chaining animations & actions.
        count = 0;
        MidBattleScreen.Set(this, false);
        // Do a game-state check once before moving on to the next animation.
        if (GameController.Current.CheckGameState() != GameState.SideWon)
        {
            System.Action tempAction = OnFinishAnimation;
            OnFinishAnimation = null;
            tempAction?.Invoke();
        }
        Destroy(this);
    }

    protected void FlipX(Vector2Int direction, SpriteRenderer unitRenderer)
    {
        unitRenderer.flipX = direction.x != 0 ? (direction.x > 0 ? true : false) : unitRenderer.flipX;
    }

    protected abstract void Animate();
}
