using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleAnimationController : MidBattleScreen, IAdvancedSpriteSheetAnimationListener
{
    private enum State { AttackerWalking, AttackerAttacking, AttackerFinishingAttack, DefenderAttacking, DefenderFinishingAttack, WaitTime }
    [Header("Class Animations")]
    public List<ClassAnimation> ClassAnimations;
    [Header("Animation Data")]
    public float AttackerTargetPos = 3;
    public float DefenderTargetPos = 4;
    public float AttackerSpeed;
    public float WaitTime = 0.5f;
    public SpriteRenderer AttackerObject;
    public SpriteRenderer DefenderObject;
    [Header("Attacker UI")]
    public Text AttackerInfo;
    public PortraitHolder AttackerIcon;
    public RectTransform AttackerHealthbarFull;
    public RectTransform AttackerHealthbarEmpty;
    public List<PalettedSprite> AttackerSprites;
    [Header("Defender UI")]
    public Text DefenderInfo;
    public PortraitHolder DefenderIcon;
    public RectTransform DefenderHealthbarFull;
    public RectTransform DefenderHealthbarEmpty;
    public List<PalettedSprite> DefenderSprites;
    //[HideInInspector]
    public Unit Attacker;
    //[HideInInspector]
    public Unit Defender;
    private AdvancedSpriteSheetAnimation attackerAnimation;
    private AdvancedSpriteSheetAnimation defenderAnimation;
    private State state;
    private Vector3 currentAttackerPos;
    private float count = 0;

    public void StartBattle()
    {
        attackerAnimation = Instantiate(ClassAnimations.Find(a => a.Name == Attacker.Class).Animation, AttackerObject.transform);
        attackerAnimation.Renderer = AttackerObject;
        defenderAnimation = Instantiate(ClassAnimations.Find(a => a.Name == Defender.Class).Animation, DefenderObject.transform);
        defenderAnimation.Renderer = DefenderObject;
        attackerAnimation.EditorPreview();
        defenderAnimation.EditorPreview();
        attackerAnimation.Activate("Idle");
        attackerAnimation.Listeners.Add(this);
        defenderAnimation.Listeners.Add(this);
        currentAttackerPos = AttackerObject.transform.position;
        attackerAnimation.Activate("Walk");
        defenderAnimation.Activate("Idle");
        state = State.AttackerWalking;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        AttackerInfo.text = Attacker.Name.PadRight(7) + '\n' + Attacker.AttackPreview(Defender, 3);
        AttackerIcon.Portrait = Attacker.Icon;
        AttackerHealthbarFull.sizeDelta = new Vector2(Attacker.Health * 4, 8);
        AttackerHealthbarEmpty.sizeDelta = new Vector2(Attacker.Stats.MaxHP * 4, 8);
        foreach (var item in AttackerSprites)
        {
            item.Palette = (int)Attacker.TheTeam;
        }
        DefenderInfo.text = Defender.Name.PadRight(7) + '\n' + Defender.AttackPreview(Attacker, 3);
        DefenderIcon.Portrait = Defender.Icon;
        DefenderHealthbarFull.sizeDelta = new Vector2(Defender.Health * 4, 8);
        DefenderHealthbarEmpty.sizeDelta = new Vector2(Defender.Stats.MaxHP * 4, 8);
        foreach (var item in DefenderSprites)
        {
            item.Palette = (int)Defender.TheTeam;
        }
    }

    private void Update()
    {
        switch (state)
        {
            case State.AttackerWalking:
                currentAttackerPos.x -= Time.deltaTime * AttackerSpeed;
                if (currentAttackerPos.x <= AttackerTargetPos)
                {
                    currentAttackerPos.x = AttackerTargetPos;
                    state = State.AttackerAttacking;
                    attackerAnimation.Activate("AttackStart");
                }
                AttackerObject.transform.position = currentAttackerPos;
                break;
            case State.AttackerAttacking:
                break;
            case State.AttackerFinishingAttack:
                break;
            case State.DefenderAttacking:
                break;
            case State.DefenderFinishingAttack:
                break;
            case State.WaitTime:
                count += Time.deltaTime;
                if (count >= WaitTime)
                {
                    CrossfadeMusicPlayer.Instance.SwitchBattleMode(false);
                    Quit();
                }
                break;
            default:
                break;
        }
    }

    public void FinishedAnimation(int id, string name)
    {
        if (name == "AttackStart")
        {
            if (state == State.AttackerAttacking)
            {
                attackerAnimation.Activate("AttackEnd");
                state = State.AttackerFinishingAttack;
                HandleDamage(Attacker, Defender, true);
            }
            else if (state == State.DefenderAttacking)
            {
                defenderAnimation.Activate("AttackEnd");
                state = State.DefenderFinishingAttack;
                HandleDamage(Defender, Attacker, false);
            }
        }
        else if (name == "AttackEnd")
        {
            if (state == State.AttackerFinishingAttack)
            {
                if (Defender == null)
                {
                    state = State.WaitTime;
                    return;
                }
                DefenderObject.transform.position = new Vector3(DefenderTargetPos, DefenderObject.transform.position.y, DefenderObject.transform.position.z);
                defenderAnimation.Activate("AttackStart");
                attackerAnimation.Activate("Idle");
                float temp = AttackerObject.transform.position.z;
                AttackerObject.transform.position += new Vector3(0, 0, DefenderObject.transform.position.z - temp);
                DefenderObject.transform.position -= new Vector3(0, 0, DefenderObject.transform.position.z - temp);
                state = State.DefenderAttacking;
            }
            else if (state == State.DefenderFinishingAttack)
            {
                if (Attacker == null)
                {
                    state = State.WaitTime;
                    return;
                }
                AttackerObject.transform.position = new Vector3(AttackerTargetPos, AttackerObject.transform.position.y, AttackerObject.transform.position.z);
                attackerAnimation.Activate("Idle");
                defenderAnimation.Activate("Idle");
                state = State.WaitTime;
            }
        }
    }

    public void ChangedFrame(int id, string name, int newFrame)
    {
        // Do nothing
    }

    private void HandleDamage(Unit attacker, Unit defender, bool attackerAttack)
    {
        bool? result = attacker.Attack(defender);
        switch (result)
        {
            case true:
                // Normal
                break;
            case false:
                // Move for miss
                if (attackerAttack)
                {
                    DefenderObject.transform.position -= new Vector3(1, 0, 0);
                }
                else
                {
                    AttackerObject.transform.position += new Vector3(1, 0, 0);
                }
                break;
            case null:
                if (attackerAttack)
                {
                    Destroy(DefenderObject.gameObject);
                }
                else
                {
                    Destroy(AttackerObject.gameObject);
                }
                break;
        }
        UpdateDisplay();
    }
}

[System.Serializable]
public class ClassAnimation
{
    public string Name;
    public AdvancedSpriteSheetAnimation Animation;
}