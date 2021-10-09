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
    public Transform BattleBackgroundsAttackerContainer;
    public Transform BattleBackgroundsDefenderContainer;
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
    public float BattleFlashTime;
    public SpriteRenderer AttackerObject;
    public SpriteRenderer DefenderObject;
    [Header("Attacker UI")]
    public Text AttackerInfoObject;
    public InclinationIndicator AttackerInclinationObject;
    public PortraitHolder AttackerIconObject;
    public HealthbarPanel AttackerHealthbarObject;
    public List<PalettedSprite> AttackerSpritesObject;
    [Header("Defender UI")]
    public Text DefenderInfoObject;
    public InclinationIndicator DefenderInclinationObject;
    public PortraitHolder DefenderIconObject;
    public HealthbarPanel DefenderHealthbarObject;
    public List<PalettedSprite> DefenderSpritesObject;
    [HideInInspector]
    public CombatantData Attacker;
    [HideInInspector]
    public CombatantData Defender;
    private float battleTrueFlashTime;
    private GameObject currentProjectile;
    private State _state;
    private State state
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            count = 0;
        }
    }
    private Vector3 currentAttackerPos;
    private float count = 0;

    public void StartBattle(Unit attacker, Unit defender)
    {
        Attacker = new CombatantData(AttackerInfoObject, AttackerInclinationObject, AttackerIconObject, AttackerHealthbarObject, AttackerSpritesObject, AttackerObject, attacker);
        Defender = new CombatantData(DefenderInfoObject, DefenderInclinationObject, DefenderIconObject, DefenderHealthbarObject, DefenderSpritesObject, DefenderObject, defender);
        Attacker.Init(this);
        Defender.Init(this);
        Attacker.LookingLeft = true;
        Defender.LookingLeft = false;
        Tile attackerTile = GameController.Current.Map[Attacker.Unit.Pos.x, Attacker.Unit.Pos.y];
        AttackerBattleBackgrounds.Find(a => a.TileSet == GameController.Current.Set.Name && attackerTile.Name == a.Tile).Background.SetActive(true);
        Tile defenderTile = GameController.Current.Map[Defender.Unit.Pos.x, Defender.Unit.Pos.y];
        DefenderBattleBackgrounds.Find(a => a.TileSet == GameController.Current.Set.Name && defenderTile.Name == a.Tile).Background.SetActive(true);
        if (Vector2.Distance(Attacker.Unit.Pos, Defender.Unit.Pos) <= 1)
        {
            // Melee attack
            currentAttackerPos = Attacker.Object.transform.position;
            Attacker.Animation.Activate("Walk");
            state = State.AttackerWalking;
        }
        else
        {
            // Ranged attack
            Attacker.Animation.Activate("AttackRangeStart");
            state = State.AttackerRangeAttacking;
        }
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        Attacker.UpdateUI(Defender.Unit);
        Defender.UpdateUI(Attacker.Unit);
    }

    private void Update()
    {
        Time.timeScale = GameController.Current.GameSpeed(); // Speed up
        count += Time.deltaTime;
        switch (state)
        {
            case State.AttackerWalking:
                currentAttackerPos.x -= Time.deltaTime * AttackerSpeed;
                if (currentAttackerPos.x <= AttackerTargetPos)
                {
                    currentAttackerPos.x = AttackerTargetPos;
                    state = State.AttackerAttacking;
                    Attacker.Animation.Activate("AttackStart");
                }
                Attacker.Object.transform.position = currentAttackerPos;
                break;
            case State.AttackerAttacking:
                break;
            case State.AttackerFinishingAttack:
                if (!Defender.Unit.Statue && count >= battleTrueFlashTime && Defender.Object != null)
                {
                    battleTrueFlashTime = Mathf.Infinity;
                    Defender.Palette.Palette = (int)Defender.Unit.TheTeam;
                }
                break;
            case State.DefenderAttacking:
                if (!Defender.Unit.Statue && Defender.Palette.Palette != (int)Defender.Unit.TheTeam) // In case the attacker post-attack animation is extremely short (aka non-existent)
                {
                    Defender.Palette.Palette = (int)Defender.Unit.TheTeam;
                }
                break;
            case State.DefenderFinishingAttack:
                if (!Attacker.Unit.Statue && count >= battleTrueFlashTime && Attacker.Object != null)
                {
                    battleTrueFlashTime = Mathf.Infinity;
                    Attacker.Palette.Palette = (int)Attacker.Unit.TheTeam;
                }
                break;
            case State.AttackerRangeAttacking:
                break;
            case State.AttackerRangeFinishingAttack:
                currentAttackerPos.x -= Time.deltaTime * ProjectileSpeed;
                if (currentAttackerPos.x <= AttackerProjectileTargetPos)
                {
                    Destroy(currentProjectile);
                    if (HandleDamage(Attacker, Defender) != null && Defender.Unit.CanAttack(Attacker.Unit))
                    {
                        state = State.DefenderRangeAttacking;
                        Defender.Animation.Activate("AttackRangeStart");
                        float temp = Attacker.Object.transform.position.z;
                        Attacker.Object.transform.position += new Vector3(0, 0, Defender.Object.transform.position.z - temp);
                        Defender.Object.transform.position -= new Vector3(0, 0, Defender.Object.transform.position.z - temp);
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
                    HandleDamage(Defender, Attacker);
                    state = State.WaitTime;
                    break;
                }
                currentProjectile.transform.position = currentAttackerPos;
                break;
            case State.WaitTime:
                if (!Attacker.Unit.Statue && Attacker.Palette.Palette != (int)Attacker.Unit.TheTeam) // In case the attacker post-attack animation is extremely short (aka non-existent)
                {
                    Attacker.Palette.Palette = (int)Attacker.Unit.TheTeam;
                }
                if (!Defender.Unit.Statue && count >= battleTrueFlashTime && Defender.Object != null)
                {
                    battleTrueFlashTime = Mathf.Infinity;
                    Defender.Palette.Palette = (int)Defender.Unit.TheTeam;
                }
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
                    Attacker.Animation.Activate("AttackEnd");
                    state = State.AttackerFinishingAttack;
                    HandleDamage(Attacker, Defender);
                }
                else if (state == State.DefenderAttacking)
                {
                    Defender.Animation.Activate("AttackEnd");
                    state = State.DefenderFinishingAttack;
                    HandleDamage(Defender, Attacker);
                }
                break;
            case "AttackEnd":
                if (state == State.AttackerFinishingAttack)
                {
                    if (Attacker.Animation.HasAnimation("IdlePost"))
                    {
                        Attacker.Animation.Activate("IdlePost");
                    }
                    else
                    {
                        Attacker.Animation.Activate("Idle");
                    }
                    if (Defender.Unit == null || Defender.Unit.Statue)
                    {
                        state = State.WaitTime;
                        return;
                    }
                    Defender.Object.transform.position = new Vector3(DefenderTargetPos, Defender.Object.transform.position.y, Defender.Object.transform.position.z);
                    if (Defender.Animation.HasAnimation("CounterStart"))
                    {
                        Defender.Animation.Activate("CounterStart");
                    }
                    else
                    {
                        Defender.Animation.Activate("AttackStart");
                    }
                    float temp = Attacker.Object.transform.position.z;
                    Attacker.Object.transform.position += new Vector3(0, 0, Defender.Object.transform.position.z - temp);
                    Defender.Object.transform.position -= new Vector3(0, 0, Defender.Object.transform.position.z - temp);
                    state = State.DefenderAttacking;
                }
                else if (state == State.DefenderFinishingAttack)
                {
                    if (Attacker == null || Attacker.Object == null)
                    {
                        if (Defender.Animation.HasAnimation("IdlePost"))
                        {
                            Defender.Animation.Activate("IdlePost");
                        }
                        else
                        {
                            Defender.Animation.Activate("Idle");
                        }
                        state = State.WaitTime;
                        return;
                    }
                    Attacker.Object.transform.position = new Vector3(AttackerTargetPos, Attacker.Object.transform.position.y, Attacker.Object.transform.position.z);
                    if (Defender.Animation.HasAnimation("IdlePost"))
                    {
                        Defender.Animation.Activate("IdlePost");
                    }
                    else
                    {
                        Defender.Animation.Activate("Idle");
                    }
                    state = State.WaitTime;
                }
                break;
            case "AttackRangeStart":
                if (state == State.AttackerRangeAttacking)
                {
                    GameObject projectileSource = ClassAnimations.Find(a => a.Name == Attacker.Unit.Class).Projectile;
                    currentProjectile = Instantiate(projectileSource, Attacker.Object.transform);
                    currentProjectile.SetActive(true);
                    currentProjectile.transform.localPosition = projectileSource.transform.localPosition;
                    currentProjectile.GetComponent<PalettedSprite>().Palette = (int)Attacker.Unit.TheTeam;
                    currentAttackerPos = currentProjectile.transform.position;
                    Attacker.Animation.Activate("AttackRangeEnd");
                    state = State.AttackerRangeFinishingAttack;
                }
                else
                {
                    GameObject projectileSource = ClassAnimations.Find(a => a.Name == Defender.Unit.Class).Projectile;
                    currentProjectile = Instantiate(projectileSource, Defender.Object.transform);
                    currentProjectile.SetActive(true);
                    Vector3 pos = projectileSource.transform.localPosition;
                    pos.x *= -1;
                    currentProjectile.transform.localPosition = pos;
                    currentProjectile.GetComponent<PalettedSprite>().Palette = (int)Defender.Unit.TheTeam;
                    currentAttackerPos = currentProjectile.transform.position;
                    currentProjectile.GetComponent<SpriteRenderer>().flipX = true;
                    Defender.Animation.Activate("AttackRangeEnd");
                    state = State.DefenderRangeFinishingAttack;
                }
                break;
            case "AttackRangeEnd":
                if (state == State.AttackerRangeFinishingAttack)
                {
                    Attacker.Animation.Activate("Idle");
                }
                else
                {
                    Defender.Animation.Activate("Idle");
                }
                break;
            case "CounterStart":
                Defender.Animation.Activate("AttackStart");
                break;
            default:
                break;
        }
    }

    public void ChangedFrame(int id, string name, int newFrame)
    {
        // Do nothing
    }

    public bool? HandleDamage(CombatantData attacker, CombatantData defender)
    {
        bool? result = attacker.Unit.Attack(defender.Unit);
        switch (result)
        {
            case true:
                // Play sound for hit
                int damage = attacker.Unit.GetDamage(defender.Unit);
                if (damage == 0)
                {
                    SoundController.PlaySound(NoDamageSFX, 1);
                }
                else
                {
                    SoundController.PlaySound(HitSFX, 1.5f - (float)damage / defender.Unit.Stats.MaxHP);
                    battleTrueFlashTime = BattleFlashTime / (1.5f - (float)damage / defender.Unit.Stats.MaxHP);
                    if (!defender.Unit.Statue)
                    {
                        defender.Palette.Palette = 3;
                    }
                }
                break;
            case false:
                // Move for miss
                SoundController.PlaySound(MissSFX, 1);
                defender.Object.transform.position += new Vector3(defender.LookingLeft ? 1 : -1, 0, 0);
                break;
            case null:
                // Destroy sprite for dead
                SoundController.PlaySound(HitSFX, 0.5f);
                Destroy(defender.Object.gameObject);
                break;
        }
        UpdateDisplay();
        return result;
    }

#if UNITY_EDITOR
    public void AutoLoadAnimations()
    {
        // Clear previous
        ClassAnimations.Clear();
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in transform)
        {
            if (child != BaseClassAnimation.transform)
            {
                toDestroy.Add(child.gameObject);
            }
        }
        while (toDestroy.Count > 0)
        {
            DestroyImmediate(toDestroy[0]);
            toDestroy.RemoveAt(0);
        }
        // Now load new
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
        // Set dirty
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }

    public void AutoLoadBackgrounds()
    {
        // Clear previous
        AttackerBattleBackgrounds.Clear();
        DefenderBattleBackgrounds.Clear();
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in BattleBackgroundsAttackerContainer)
        {
            toDestroy.Add(child.gameObject);
        }
        foreach (Transform child in BattleBackgroundsDefenderContainer)
        {
            toDestroy.Add(child.gameObject);
        }
        while (toDestroy.Count > 0)
        {
            DestroyImmediate(toDestroy[0]);
            toDestroy.RemoveAt(0);
        }
        // Now load new
        TilesetsData tilesets = new TilesetsData();
        string json = FrogForgeImporter.LoadFile<TextAsset>("Tilesets.json").text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("Tilesets"), tilesets);
        foreach (TilesetData tileset in tilesets.Tilesets)
        {
            Transform attObject = new GameObject().transform;
            Transform defObject = new GameObject().transform;
            defObject.name = attObject.name = tileset.Name;
            attObject.parent = BattleBackgroundsAttackerContainer;
            attObject.localPosition = Vector3.zero;
            defObject.parent = BattleBackgroundsDefenderContainer;
            defObject.localPosition = Vector3.zero;
            foreach (BattleBackgroundData battleBackground in tileset.BattleBackgrounds)
            {
                Transform container = new GameObject().transform;
                container.name = battleBackground.Name;
                if (FrogForgeImporter.CheckFileExists<Sprite>("Images/BattleBackgrounds/" + tileset.Name + "/" + battleBackground.Name + "1.png"))
                {
                    Sprite sprite = FrogForgeImporter.LoadFile<Sprite>("Images/BattleBackgrounds/" + tileset.Name + "/" + battleBackground.Name + "1.png");
                    GameObject layer1 = new GameObject();
                    FrogForgeImporter.LoadSpriteOrAnimationToObject(layer1, sprite, 128);
                    layer1.transform.parent = container;
                    layer1.name = "1";
                    layer1.transform.localPosition = Vector3.zero;
                    PalettedSprite palettedSprite = layer1.AddComponent<PalettedSprite>();
                    palettedSprite.Background = true;
                    palettedSprite.ForceSilentSetPalette(0);
                }
                if (FrogForgeImporter.CheckFileExists<Sprite>("Images/BattleBackgrounds/" + tileset.Name + "/" + battleBackground.Name + "2.png"))
                {
                    Sprite sprite = FrogForgeImporter.LoadFile<Sprite>("Images/BattleBackgrounds/" + tileset.Name + "/" + battleBackground.Name + "2.png");
                    GameObject layer2 = new GameObject();
                    FrogForgeImporter.LoadSpriteOrAnimationToObject(layer2, sprite, 128);
                    layer2.transform.parent = container;
                    layer2.name = "2";
                    layer2.transform.localPosition = Vector3.zero;
                    PalettedSprite palettedSprite = layer2.AddComponent<PalettedSprite>();
                    palettedSprite.Background = true;
                    palettedSprite.ForceSilentSetPalette(1);
                }
                container.gameObject.SetActive(false);
                container.parent = attObject;
                container.localPosition = Vector3.zero;
                AttackerBattleBackgrounds.Add(new BattleBackground(tileset.Name, battleBackground.Name, container.gameObject));
                container = Instantiate(container.gameObject, defObject).transform;
                container.localPosition = Vector3.zero;
                DefenderBattleBackgrounds.Add(new BattleBackground(tileset.Name, battleBackground.Name, container.gameObject));
            }
        }
        // Set dirty
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
#endif

    [System.Serializable]
    private class TilesetsData
    {
        public List<TilesetData> Tilesets; 
    }

    [System.Serializable]
    private class TilesetData
    {
        public string Name;
        public List<BattleBackgroundData> BattleBackgrounds;
    }

    [System.Serializable]
    private class BattleBackgroundData
    {
        public string Name;
    }

    public class CombatantData
    {
        public Text UIInfo;
        public InclinationIndicator UIInclination;
        public PortraitHolder UIPortrait;
        public HealthbarPanel UIHealthbar;
        public List<PalettedSprite> Sprites;
        public Unit Unit;
        public AdvancedSpriteSheetAnimation Animation;
        public PalettedSprite Palette;
        public SpriteRenderer Object;
        private bool _lookingLeft;
        public bool LookingLeft
        {
            get
            {
                return _lookingLeft;
            }
            set
            {
                _lookingLeft = value;
                Object.flipX = !LookingLeft;
            }
        }

        public CombatantData(Text uiInfo, InclinationIndicator uiInclination, PortraitHolder uiPortrait, HealthbarPanel uiHealthbar, List<PalettedSprite> sprites, SpriteRenderer @object, Unit unit)
        {
            UIInfo = uiInfo;
            UIInclination = uiInclination;
            UIPortrait = uiPortrait;
            UIHealthbar = uiHealthbar;
            Sprites = sprites;
            Object = @object;
            Unit = unit;
        }

        public void Init(BattleAnimationController battleAnimationController)
        {
            Animation = Instantiate(battleAnimationController.ClassAnimations.Find(a => a.Name == Unit.Class).Animation, Object.transform);
            Animation.Renderer = Object;
            Animation.Animations.ForEach(a => a.Split());
            Animation.EditorPreview();
            Animation.Listeners.Add(battleAnimationController);
            Animation.Activate("Idle");
            Palette = Animation.transform.parent.gameObject.GetComponent<PalettedSprite>();
            foreach (var item in Sprites)
            {
                item.Palette = (int)Unit.TheTeam;
            }
            Palette.Palette = Unit.Statue ? 3 : (int)Unit.TheTeam;
            UIHealthbar.SetMax(Unit.Stats.MaxHP);
            UIPortrait.Portrait = Unit.Icon;
        }

        public void UpdateUI(Unit target)
        {
            UIInfo.text = Unit.ToString().PadRight(8) + '\n' + Unit.AttackPreview(target, 4, Unit.CanAttack(target));
            if (UIInclination != null)
            {
                UIInclination.Display(Unit, target);
            }
            UIHealthbar.SetValue(Unit.Health);
        }
    }
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

    public BattleBackground(string tileSet, string tile, GameObject background)
    {
        TileSet = tileSet;
        Tile = tile;
        Background = background;
    }

    public BattleBackground() {}
}