using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameTormentModelAnimator : MonoBehaviour
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

    private void Start()
    {
        transform.parent = GameController.Current.transform;
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idling:
                if (!(currentIdle?.Active ?? false))
                {
                    idleCount -= Time.deltaTime;
                    if (idleCount <= 0)
                    {
                        (currentIdle = IdleAnimations[Random.Range(0, IdleAnimations.Count)]).Activate();
                        idleCount = IdleRate.RandomValueInRange();
                    }
                }
                break;
            case State.UnitDeath:
                break;
            case State.Damaged:
                break;
            default:
                break;
        }
    }
}
