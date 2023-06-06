using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapAnimation : MidBattleScreen
{
    [HideInInspector]
    public System.Action OnFinishAnimation;
    public bool Done = false; // Since after destorying, there's one critical frame where this isn't null
    protected float count;
    protected bool init = false;
    private bool active = false;

    private void Update()
    {
        if (active)
        {
            Time.timeScale = GameCalculations.GameSpeed(); // Double speed
            count += Time.deltaTime;
            Animate();
        }
    }

    public virtual void StartAnimation()
    {
        if (!init)
        {
            throw Bugger.FMError("Map Animation: Missing init!");
        }
        MidBattleScreen.Set(this, true);
        active = true;
    }

    protected abstract void Animate();

    protected void EndAnimation()
    {
        Done = true;
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
        MapAnimationsController.Current.TryPlayNextAnimation();
    }

    protected void FlipX(Vector2Int direction, SpriteRenderer unitRenderer)
    {
        unitRenderer.flipX = direction.x != 0 ? (direction.x > 0 ? true : false) : unitRenderer.flipX;
    }

    protected AdvancedSpriteSheetAnimation CreateAnimationOnUnit(Unit unit, AdvancedSpriteSheetAnimation baseAnimation)
    {
        AdvancedSpriteSheetAnimation animation = Instantiate(baseAnimation.gameObject, baseAnimation.transform.parent).GetComponent<AdvancedSpriteSheetAnimation>();
        animation.transform.position = unit.transform.position;
        animation.transform.position += new Vector3(0, 0, -0.5f);
        animation.Start();
        animation.gameObject.SetActive(true);
        return animation;
    }
}
