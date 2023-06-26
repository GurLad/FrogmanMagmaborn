using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameTormentModelAnimator : AGameControllerListener
{
    private enum State { Idling, UnitDeath, Damaged }
    [Header("Idle")]
    public List<AdvancedAnimation> IdleAnimations;
    public Vector2 IdleRate;
    [Header("Unit Death")] // TBA
    public List<AdvancedAnimation> UnitDeathAnimations;
    [Header("Damaged")] // TBA
    public List<AdvancedAnimation> DamagedAnimations;
    private State state;
    // Idle
    private float idleCount = 0;
    private AdvancedAnimation currentIdle;
    // Unit Death
    private AdvancedAnimation currentUnitDeath;

    protected override void Start()
    {
        base.Start();
        transform.parent = GameController.Current.transform;
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idling:
                if (!(currentIdle?.Active ?? false))
                {
                    currentIdle = null;
                    idleCount -= Time.deltaTime;
                    if (idleCount <= 0)
                    {
                        (currentIdle = IdleAnimations[Random.Range(0, IdleAnimations.Count)]).Activate();
                        idleCount = IdleRate.RandomValueInRange();
                    }
                }
                break;
            case State.UnitDeath:
                if (!currentUnitDeath.Active)
                {
                    state = State.Idling;
                }
                break;
            case State.Damaged:
                break;
            default:
                break;
        }
    }

    public override void OnBeginPlayerTurn(List<Unit> units)
    {
        // Do nothing
    }

    public override void OnEndLevel(List<Unit> units, bool playerWon)
    {
        // Do nothing
    }

    public override void OnPlayerWin(List<Unit> units)
    {
        // Do nothing
    }

    public override void OnUnitDeath(string unitName)
    {
        ClearIdle();
        (currentUnitDeath = UnitDeathAnimations[Random.Range(0, UnitDeathAnimations.Count)]).Activate(true);
        state = State.UnitDeath;
    }

    private void ClearIdle()
    {
        currentIdle?.Deactivate();
        idleCount = IdleRate.RandomValueInRange();
    }
}
