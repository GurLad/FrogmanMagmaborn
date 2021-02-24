using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleAnimationController : MidBattleScreen, IAdvancedSpriteSheetAnimationListener
{
    private enum State { AttackerWalking, AttackerAttacking, AttackerFinishingAttack, DefenderAttacking, DefenderFinishingAttack, AttackerRangeAttacking, AttackerRangeFinishingAttack, DefenderRangeAttacking, DefenderRangeFinishingAttack, WaitTime}
    [Header("Class Animations")]
    public AdvancedSpriteSheetAnimation BaseClassAnimation;
    public List<ClassAnimation> ClassAnimations;
    [Header("Battle Backgrounds")]
    public List<BattleBackground> AttackerBattleBackgrounds;
    public List<BattleBackground> DefenderBattleBackgrounds;
    [Header("SFX")]
    public AudioClip HitSFX;
    public AudioClip MissSFX;
    public AudioClip NoDamageSFX;
    [Header("Animation Data")]
    public float AttackerTargetPos = 3;
    public float DefenderTargetPos = 4;
    public float AttackerProjectileTargetPos;
    public float DefenderProjectileTargetPos;
    public float AttackerSpeed;
    public float ProjectileSpeed;
    public float WaitTime = 0.5f;
    public SpriteRenderer AttackerObject;
    public SpriteRenderer DefenderObject;
    [Header("Attacker UI")]
    public Text AttackerInfo;
    public InclinationIndicator AttackerInclination;
    public PortraitHolder AttackerIcon;
    public RectTransform AttackerHealthbarFull;
    public RectTransform AttackerHealthbarEmpty;
    public List<PalettedSprite> AttackerSprites;
    [Header("Defender UI")]
    public Text DefenderInfo;
    public InclinationIndicator DefenderInclination;
    public PortraitHolder DefenderIcon;
    public RectTransform DefenderHealthbarFull;
    public RectTransform DefenderHealthbarEmpty;
    public List<PalettedSprite> DefenderSprites;
    [HideInInspector]
    public Unit Attacker;
    [HideInInspector]
    public Unit Defender;
    private AdvancedSpriteSheetAnimation attackerAnimation;
    private AdvancedSpriteSheetAnimation defenderAnimation;
    private GameObject currentProjectile;
    private State state;
    private Vector3 currentAttackerPos;
    private float count = 0;

    public void StartBattle()
    {
        attackerAnimation = Instantiate(ClassAnimations.Find(a => a.Name == Attacker.Class).Animation, AttackerObject.transform);
        attackerAnimation.Renderer = AttackerObject;
        attackerAnimation.Animations.ForEach(a => a.Split());
        defenderAnimation = Instantiate(ClassAnimations.Find(a => a.Name == Defender.Class).Animation, DefenderObject.transform);
        defenderAnimation.Renderer = DefenderObject;
        defenderAnimation.Animations.ForEach(a => a.Split());
        attackerAnimation.EditorPreview();
        defenderAnimation.EditorPreview();
        attackerAnimation.Listeners.Add(this);
        attackerAnimation.Activate("Idle");
        defenderAnimation.Listeners.Add(this);
        defenderAnimation.Activate("Idle");
        Tile attackerTile = GameController.Current.Map[Attacker.Pos.x, Attacker.Pos.y];
        AttackerBattleBackgrounds.Find(a => a.TileSet == GameController.Current.Set.Name && attackerTile.Name == a.Tile).Background.SetActive(true);
        Tile defenderTile = GameController.Current.Map[Defender.Pos.x, Defender.Pos.y];
        DefenderBattleBackgrounds.Find(a => a.TileSet == GameController.Current.Set.Name && defenderTile.Name == a.Tile).Background.SetActive(true);
        if (Vector2.Distance(Attacker.Pos, Defender.Pos) <= 1)
        {
            // Melee attack
            currentAttackerPos = AttackerObject.transform.position;
            attackerAnimation.Activate("Walk");
            state = State.AttackerWalking;
        }
        else
        {
            // Ranged attack
            attackerAnimation.Activate("AttackRangeStart");
            state = State.AttackerRangeAttacking;
        }
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        AttackerInfo.text = Attacker.ToString().PadRight(7) + '\n' + Attacker.AttackPreview(Defender, 3, Attacker.CanAttack(Defender));
        if (AttackerInclination != null)
        {
            AttackerInclination.Display(Attacker, Defender);
        }
        AttackerIcon.Portrait = Attacker.Icon;
        AttackerHealthbarFull.sizeDelta = new Vector2(Attacker.Health * 4, 8);
        AttackerHealthbarEmpty.sizeDelta = new Vector2(Attacker.Stats.MaxHP * 4, 8);
        foreach (var item in AttackerSprites)
        {
            item.Palette = (int)Attacker.TheTeam;
        }
        DefenderInfo.text = Defender.ToString().PadRight(7) + '\n' + Defender.AttackPreview(Attacker, 3, Defender.CanAttack(Attacker));
        if (DefenderInclination != null)
        {
            DefenderInclination.Display(Defender, Attacker);
        }
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
        Time.timeScale = GameController.Current.GameSpeed(); // Speed up
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
            case State.AttackerRangeAttacking:
                break;
            case State.AttackerRangeFinishingAttack:
                currentAttackerPos.x -= Time.deltaTime * ProjectileSpeed;
                if (currentAttackerPos.x <= AttackerProjectileTargetPos)
                {
                    Destroy(currentProjectile);
                    if (HandleDamage(Attacker, Defender, true) != null && Defender.CanAttack(Attacker))
                    {
                        state = State.DefenderRangeAttacking;
                        defenderAnimation.Activate("AttackRangeStart");
                        float temp = AttackerObject.transform.position.z;
                        AttackerObject.transform.position += new Vector3(0, 0, DefenderObject.transform.position.z - temp);
                        DefenderObject.transform.position -= new Vector3(0, 0, DefenderObject.transform.position.z - temp);
                    }
                    else
                    {
                        state = State.WaitTime;
                    }
                    break;
                }
                currentProjectile.transform.position = currentAttackerPos;
                break;
            case State.DefenderRangeAttacking:
                break;
            case State.DefenderRangeFinishingAttack:
                currentAttackerPos.x += Time.deltaTime * ProjectileSpeed;
                if (currentAttackerPos.x >= DefenderProjectileTargetPos)
                {
                    Destroy(currentProjectile);
                    HandleDamage(Defender, Attacker, false);
                    state = State.WaitTime;
                    break;
                }
                currentProjectile.transform.position = currentAttackerPos;
                break;
            case State.WaitTime:
                count += Time.deltaTime;
                if (count >= WaitTime)
                {
                    CrossfadeMusicPlayer.Current.SwitchBattleMode(false);
                    Time.timeScale = 1;
                    Quit();
                }
                break;
            default:
                break;
        }
    }

    public void FinishedAnimation(int id, string name)
    {
        // This code has a ton of copy-past: should replace with AttackerAnimation and DefenderAnimation classes and functions
        switch (name)
        {
            case "AttackStart":
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
                break;
            case "AttackEnd":
                if (state == State.AttackerFinishingAttack)
                {
                    attackerAnimation.Activate("Idle");
                    if (Defender == null || Defender.Statue)
                    {
                        state = State.WaitTime;
                        return;
                    }
                    DefenderObject.transform.position = new Vector3(DefenderTargetPos, DefenderObject.transform.position.y, DefenderObject.transform.position.z);
                    if (defenderAnimation.HasAnimation("CounterStart"))
                    {
                        defenderAnimation.Activate("CounterStart");
                    }
                    else
                    {
                        defenderAnimation.Activate("AttackStart");
                    }
                    float temp = AttackerObject.transform.position.z;
                    AttackerObject.transform.position += new Vector3(0, 0, DefenderObject.transform.position.z - temp);
                    DefenderObject.transform.position -= new Vector3(0, 0, DefenderObject.transform.position.z - temp);
                    state = State.DefenderAttacking;
                }
                else if (state == State.DefenderFinishingAttack)
                {
                    if (Attacker == null || AttackerObject == null)
                    {
                        defenderAnimation.Activate("Idle");
                        state = State.WaitTime;
                        return;
                    }
                    AttackerObject.transform.position = new Vector3(AttackerTargetPos, AttackerObject.transform.position.y, AttackerObject.transform.position.z);
                    attackerAnimation.Activate("Idle");
                    defenderAnimation.Activate("Idle");
                    state = State.WaitTime;
                }
                break;
            case "AttackRangeStart":
                if (state == State.AttackerRangeAttacking)
                {
                    GameObject projectileSource = ClassAnimations.Find(a => a.Name == Attacker.Class).Projectile;
                    currentProjectile = Instantiate(projectileSource, AttackerObject.transform);
                    currentProjectile.SetActive(true);
                    currentProjectile.transform.localPosition = projectileSource.transform.localPosition;
                    currentProjectile.GetComponent<PalettedSprite>().Palette = (int)Attacker.TheTeam;
                    currentAttackerPos = currentProjectile.transform.position;
                    attackerAnimation.Activate("AttackRangeEnd");
                    state = State.AttackerRangeFinishingAttack;
                }
                else
                {
                    GameObject projectileSource = ClassAnimations.Find(a => a.Name == Defender.Class).Projectile;
                    currentProjectile = Instantiate(projectileSource, DefenderObject.transform);
                    currentProjectile.SetActive(true);
                    Vector3 pos = projectileSource.transform.localPosition;
                    pos.x *= -1;
                    currentProjectile.transform.localPosition = pos;
                    currentProjectile.GetComponent<PalettedSprite>().Palette = (int)Defender.TheTeam;
                    currentAttackerPos = currentProjectile.transform.position;
                    currentProjectile.GetComponent<SpriteRenderer>().flipX = true;
                    defenderAnimation.Activate("AttackRangeEnd");
                    state = State.DefenderRangeFinishingAttack;
                }
                break;
            case "AttackRangeEnd":
                if (state == State.AttackerRangeFinishingAttack)
                {
                    attackerAnimation.Activate("Idle");
                }
                else
                {
                    defenderAnimation.Activate("Idle");
                }
                break;
            case "CounterStart":
                defenderAnimation.Activate("AttackStart");
                break;
            default:
                break;
        }
    }

    public void ChangedFrame(int id, string name, int newFrame)
    {
        // Do nothing
    }

    private bool? HandleDamage(Unit attacker, Unit defender, bool attackerAttack)
    {
        bool? result = attacker.Attack(defender);
        switch (result)
        {
            case true:
                // Play sound for hit
                int damage = attacker.GetDamage(defender);
                if (damage == 0)
                {
                    SoundController.PlaySound(NoDamageSFX, 1);
                }
                else
                {
                    SoundController.PlaySound(HitSFX, 1.5f - (float)damage / defender.Stats.MaxHP);
                }
                break;
            case false:
                // Move for miss
                SoundController.PlaySound(MissSFX, 1);
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
                // Destroy sprite for dead
                SoundController.PlaySound(HitSFX, 0.5f);
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
        return result;
    }

    #if UNITY_EDITOR
    public void AutoLoad()
    {
        ClassAnimations.Clear();
        string[] folders = UnityEditor.AssetDatabase.GetSubFolders("Assets/Data/Images/ClassBattleAnimations");
        Debug.Log(string.Join(",", folders));
        foreach (string folder in folders)
        {
            AdvancedSpriteSheetAnimation animation = Instantiate(BaseClassAnimation, transform);
            string[] fileNames = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { folder });
            foreach (string fileName in fileNames)
            {
                Sprite file = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(UnityEditor.AssetDatabase.GUIDToAssetPath(fileName));
                Debug.Log(fileName + ", " + file.name);
                SpriteSheetData newData = new SpriteSheetData();
                newData.SpriteSheet = file;
                newData.NumberOfFrames = (int)file.rect.width / (int)file.rect.height;
                newData.Speed = 0;
                newData.Name = file.name;
                newData.Loop = newData.Name == "Walk" || newData.Name == "Idle"; // Quick & dirty, think of a better fix
                animation.Animations.Add(newData);
            }
            ClassAnimation classAnimation = new ClassAnimation();
            classAnimation.Animation = animation;
            string[] temp = folder.Split('/');
            classAnimation.Name = animation.name = temp[temp.Length - 1];
            ClassAnimations.Add(classAnimation);
        }
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
    #endif
}

[System.Serializable]
public class ClassAnimation
{
    public string Name;
    public AdvancedSpriteSheetAnimation Animation;
    public GameObject Projectile;
}

[System.Serializable]
public class BattleBackground
{
    public string TileSet;
    public string Tile;
    public GameObject Background;
}