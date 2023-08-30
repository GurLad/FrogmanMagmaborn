using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameTormentModelAnimator : AGameControllerListener, IUnitListener
{
    private enum State { Idling, UnitDeath, Damaged }
    [Header("Idle")]
    public List<AdvancedAnimation> IdleAnimations;
    public Vector2 IdleRate;
    [Header("Unit Death")]
    public List<AdvancedAnimation> UnitDeathAnimations;
    public List<AudioClip> UnitDeathSFX;
    [Header("Damaged")]
    public List<AdvancedAnimation> DamagedAnimations;
    public List<AudioClip> DamagedSFX;
    [Header("Palettes")]
    public EndgamePaletteCheater EndgamePaletteCheater;
    public EndgameSummoner EndgameSummoner;
    [Header("BasePalette")]
    public Palette BasePalette;
    [Header("DamagedPalette")]
    public Palette DamagedPalette;
    // Misc
    [HideInInspector]
    public Unit TormentUnit;
    private State state;
    // Idle
    private float idleCount = 0;
    private AdvancedAnimation currentIdle;
    // Unit Death
    private AdvancedAnimation currentUnitDeath;
    // Damaged
    private AdvancedAnimation currentDamaged;

    protected override void Start()
    {
        base.Start();
        transform.parent = GameController.Current.transform;
    }

    private void Update()
    {
        if (Time.timeScale > 0 && TormentUnit == null) // At Start GameController still contains the previous Torment apparently
        {
            TormentUnit = GameController.Current.GetNamedUnits(StaticGlobals.FinalBossName)[0];
            TormentUnit.AddListener(this);
            TormentUnit.GetComponent<SpriteRenderer>().enabled = false;
        }
        switch (state)
        {
            case State.Idling:
                if (!(currentIdle?.Active ?? false))
                {
                    currentIdle = null;
                    idleCount -= Time.deltaTime;
                    if (idleCount <= 0)
                    {
                        (currentIdle = IdleAnimations.RandomItemInList()).Activate();
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
                if (!currentDamaged.Active)
                {
                    EndgamePaletteCheater.TrueColours[0] = BasePalette;
                    state = State.Idling;
                }
                break;
            default:
                break;
        }
    }

    public override void OnUnitDeath(string unitName)
    {
        if (unitName == StaticGlobals.FinalBossName)
        {
            Destroy(this);
            return;
        }
        ClearIdle();
        (currentUnitDeath = UnitDeathAnimations.RandomItemInList()).Activate(true);
        state = State.UnitDeath;
        SoundController.PlaySound(UnitDeathSFX.RandomItemInList(), 1);
    }

    public void OnDamaged()
    {
        ClearIdle();
        (currentDamaged = DamagedAnimations.RandomItemInList()).Activate(true);
        state = State.Damaged;
        EndgamePaletteCheater.TrueColours[0] = DamagedPalette;
        EndgameSummoner.SummonWisp();
        SoundController.PlaySound(DamagedSFX.RandomItemInList(), 1);
    }

    private void ClearIdle()
    {
        currentIdle?.Deactivate();
        idleCount = IdleRate.RandomValueInRange();
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

    public void OnSpawn()
    {
        // Spawn animation maybe?
    }

    public void OnHit()
    {
        // Impossible
    }

    public void OnMiss()
    {
        // Impossible
    }

    public void OnBlocked()
    {
        // Impossible
    }

    public void OnDodged()
    {
        // Impossible(?)
    }
}
