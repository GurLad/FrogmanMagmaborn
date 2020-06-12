using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleAnimationController : MidBattleScreen, IAdvancedSpriteSheetAnimationListener
{
    private enum State { AttackerWalking, AttackerAttacking, AttackerFinishingAttack, DefenderAttacking, DefenderFinishingAttack, WaitTime }
    [Header("AnimationData")]
    public float AttackerTargetPos = 3;
    public float AttackerSpeed;
    public float WaitTime = 0.5f;
    public AdvancedSpriteSheetAnimation AttackerAnimation;
    public AdvancedSpriteSheetAnimation DefenderAnimation;
    [Header("Attacker UI")]
    public Text AttackerInfo;
    public Image AttackerIcon;
    public RectTransform AttackerHealthbarFull;
    public RectTransform AttackerHealthbarEmpty;
    public List<PalettedSprite> AttackerSprites;
    [Header("Defender UI")]
    public Text DefenderInfo;
    public Image DefenderIcon;
    public RectTransform DefenderHealthbarFull;
    public RectTransform DefenderHealthbarEmpty;
    public List<PalettedSprite> DefenderSprites;
    //[HideInInspector]
    public Unit Attacker;
    //[HideInInspector]
    public Unit Defender;
    private State state;
    private Vector3 currentAttackerPos;
    private float count = 0;

    private void Awake()
    {
        Current = this;
        AttackerAnimation.Activate("Idle");
    }

    public void StartBattle()
    {
        AttackerAnimation.Listeners.Add(this);
        DefenderAnimation.Listeners.Add(this);
        currentAttackerPos = AttackerAnimation.transform.position;
        AttackerAnimation.Activate("Walk");
        DefenderAnimation.Activate("Idle");
        state = State.AttackerWalking;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        AttackerInfo.text = Attacker.Name.PadRight(7) + '\n' + Attacker.AttackPreview(Defender.Stats, 3);
        AttackerIcon.sprite = Attacker.Icon;
        AttackerHealthbarFull.sizeDelta = new Vector2(Attacker.Health * 4, 8);
        AttackerHealthbarEmpty.sizeDelta = new Vector2(Attacker.Stats.MaxHP * 4, 8);
        foreach (var item in AttackerSprites)
        {
            item.Palette = (int)Attacker.TheTeam;
        }
        DefenderInfo.text = Defender.Name.PadRight(7) + '\n' + Defender.AttackPreview(Attacker.Stats, 3);
        DefenderIcon.sprite = Defender.Icon;
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
                    AttackerAnimation.Activate("AttackStart");
                }
                AttackerAnimation.transform.position = currentAttackerPos;
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
                Attacker.Attack(Defender);
                UpdateDisplay();
                AttackerAnimation.Activate("AttackEnd");
                state = State.AttackerFinishingAttack;
            }
            else if (state == State.DefenderAttacking)
            {
                Defender.Attack(Attacker);
                UpdateDisplay();
                DefenderAnimation.Activate("AttackEnd");
                state = State.DefenderFinishingAttack;
            }
        }
        else if (name == "AttackEnd")
        {
            if (state == State.AttackerFinishingAttack)
            {
                DefenderAnimation.Activate("AttackStart");
                AttackerAnimation.Activate("Idle");
                float temp = AttackerAnimation.transform.position.z;
                AttackerAnimation.transform.position += new Vector3(0, 0, DefenderAnimation.transform.position.z - temp);
                DefenderAnimation.transform.position -= new Vector3(0, 0, DefenderAnimation.transform.position.z - temp);
                state = State.DefenderAttacking;
            }
            else if (state == State.DefenderFinishingAttack)
            {
                AttackerAnimation.Activate("Idle");
                DefenderAnimation.Activate("Idle");
                state = State.WaitTime;
            }
        }
    }

    public void ChangedFrame(int id, string name, int newFrame)
    {
        // Do nothing
    }
}
