using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleAnimationController : MidBattleScreen
{
    [Header("Class Animations")]
    public AdvancedSpriteSheetAnimation BaseClassAnimation;
    public GameObject BaseProjectile;
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
    private Queue<System.Func<BattleAnimation>> animationParts = new Queue<System.Func<BattleAnimation>>();
    private BattleAnimation currentAnimation;
    private float count = 0;

    public void StartBattle(Unit attacker, Unit defender, float attackerRandomResult, float defenderRandomResult)
    {
        Attacker = new CombatantData(AttackerInfoObject, AttackerInclinationObject, AttackerIconObject, AttackerHealthbarObject, AttackerSpritesObject, AttackerObject, attacker);
        Defender = new CombatantData(DefenderInfoObject, DefenderInclinationObject, DefenderIconObject, DefenderHealthbarObject, DefenderSpritesObject, DefenderObject, defender);
        Attacker.Init(this);
        Defender.Init(this);
        Attacker.LookingLeft = true;
        Defender.LookingLeft = false;
        Attacker.RandomResult = attackerRandomResult;
        Defender.RandomResult = defenderRandomResult;
        Tile attackerTile = GameController.Current.Map[Attacker.Unit.Pos.x, Attacker.Unit.Pos.y];
        LoadBattleBackground(attackerTile, true);
        Tile defenderTile = GameController.Current.Map[Defender.Unit.Pos.x, Defender.Unit.Pos.y];
        LoadBattleBackground(defenderTile, false);
        bool meleeAttack = Attacker.Unit.Pos.TileDist(Defender.Unit.Pos) <= 1; // TBA: fix for multi-tile units
        if (!meleeAttack)
        {
            // Move combatants slightly
            Attacker.Object.transform.position += new Vector3(Attacker.LookingLeftSign, 0, 0);
            Defender.Object.transform.position += new Vector3(Defender.LookingLeftSign, 0, 0);
        }
        if (attacker.HasSkill(Skill.SiegeWeapon) &&
            Attacker.ClassAnimationData.BattleAnimationModeRanged == BattleAnimationMode.Teleport &&
            Attacker.Unit.Pos.TileDist(Defender.Unit.Pos) > 2) // Very specific siege weapon animation
        {
            Attacker.Object.transform.position += new Vector3(Attacker.LookingLeftSign * 20, 0, 0);
        }
        // Set init pos
        Attacker.InitPos = Attacker.Object.transform.position.x;
        Defender.InitPos = Defender.Object.transform.position.x;
        // Attacker move
        bool adjacent = FarAttack(Attacker, Defender, meleeAttack);
        // Defender move
        if (adjacent)
        {
            // Has no choice but to use adjacent attack
            animationParts.Enqueue(() => BeginAnimation<BACounterAttack>(Defender, Attacker));
            animationParts.Enqueue(() => BeginAnimation<BAMeleeAttack>(Defender, Attacker));
        }
        else
        {
            // Use a far attack
            FarAttack(Defender, Attacker, meleeAttack);
        }
        UpdateDisplay();
    }

    private void LoadBattleBackground(Tile tile, bool attacker)
    {
        BattleBackground battleBackground = (attacker ? AttackerBattleBackgrounds : DefenderBattleBackgrounds).Find(a => a.TileSet == GameController.Current.Set.Name && tile.Name == a.Tile);
        if (battleBackground != null)
        {
            battleBackground.Background.SetActive(true);
        }
        else
        {
            throw Bugger.Error("No battle background for tile " + tile.Name + "!");
        }
    }

    /// <summary>
    /// Decides the non-adjacent attack
    /// </summary>
    /// <param name="attacker">Initiator</param>
    /// <param name="defender">Target</param>
    /// <param name="melee">Use the melee/ranged mode</param>
    /// <returns>True if they ended up adjacent, false otherwise</returns>
    private bool FarAttack(CombatantData attacker, CombatantData defender, bool melee)
    {
        switch (melee ? attacker.ClassAnimationData.BattleAnimationModeMelee : attacker.ClassAnimationData.BattleAnimationModeRanged)
        {
            case BattleAnimationMode.Walk:
                animationParts.Enqueue(() => BeginAnimation<BAWalk>(attacker, defender));
                animationParts.Enqueue(() => BeginAnimation<BAMeleeAttack>(attacker, defender));
                return true;
            case BattleAnimationMode.Projectile:
                animationParts.Enqueue(() => BeginAnimation<BARangedAttack>(attacker, defender));
                return false;
            case BattleAnimationMode.Teleport:
                animationParts.Enqueue(() => BeginAnimation<BATeleport>(attacker, defender));
                animationParts.Enqueue(() => BeginAnimation<BAMeleeAttack>(attacker, defender));
                animationParts.Enqueue(() => BeginAnimation<BATeleportBack>(attacker, defender));
                return false;
            default:
                throw Bugger.Error("Invalid BattleAnimationMode!", false);
        }
    }

    private void UpdateDisplay()
    {
        Attacker.UpdateUI(Defender.Unit);
        Defender.UpdateUI(Attacker.Unit);
    }

    private void Update()
    {
        Time.timeScale = GameCalculations.GameSpeed(); // Speed up
        if (currentAnimation == null)
        {
            if (animationParts.Count > 0)
            {
                // Play the next animation
                currentAnimation = animationParts.Dequeue().Invoke();
            }
            else
            {
                // Wait, then end the battle animation
                count += Time.deltaTime;
                if (count >= WaitTime)
                {
                    CrossfadeMusicPlayer.Current.SwitchBattleMode(false);
                    Time.timeScale = 1;
                    Quit();
                }
            }
        }
    }

    private T BeginAnimation<T>(CombatantData thisCombatant, CombatantData otherCombatant) where T : BattleAnimation
    {
        if (thisCombatant.Object == null)
        {
            return null;
        }
        T animation = gameObject.AddComponent<T>();
        animation.Init(thisCombatant, otherCombatant, this);
        return animation;
    }

    public bool? HandleDamage(CombatantData attacker, CombatantData defender)
    {
        bool? result = attacker.Unit.Attack(defender.Unit, attacker.RandomResult);
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
                    SoundController.PlaySound(HitSFX, 1.5f - (float)damage / defender.Unit.Stats.Base.MaxHP);
                    BeginAnimation<BADamageFlash>(defender, attacker).BattleFlashTime = BattleFlashTime / (1.5f - (float)damage / defender.Unit.Stats.Base.MaxHP);
                }
                break;
            case false:
                // Move for miss
                SoundController.PlaySound(MissSFX, 1);
                defender.Object.transform.position += new Vector3(defender.LookingLeftSign, 0, 0);
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

#if UNITY_EDITOR || MODDABLE_BUILD
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
        foreach (Transform child in BaseProjectile.transform.parent)
        {
            if (child != BaseProjectile.transform)
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
        string json = FrogForgeImporter.LoadTextFile("Classes.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("ClassAnimations"), this);
        for (int i = 0; i < ClassAnimations.Count; i++)
        {
            // Load animations
            AdvancedSpriteSheetAnimation animation = Instantiate(BaseClassAnimation, transform);
            animation.gameObject.name = ClassAnimations[i].Name;
            animation.AffectedByGameSpeed = true;
            foreach (BattleBackgroundData animationName in ClassAnimations[i].BattleAnimations)
            {
                Sprite file = FrogForgeImporter.LoadSpriteFile("Images/ClassBattleAnimations/" + ClassAnimations[i].Name + "/" + animationName.Name + ".png");
                if (file != null)
                {
                    Bugger.Info(animationName.Name + ", " + file.name);
                    SpriteSheetData newData = new SpriteSheetData();
                    newData.SpriteSheet = file;
                    newData.NumberOfFrames = (int)file.rect.width / (int)file.rect.height;
                    newData.Speed = 0;
                    newData.Name = animationName.Name;
                    newData.Loop = newData.Name == "Walk" || newData.Name == "Idle"; // Quick & dirty, think of a better fix
                    animation.Animations.Add(newData);
                }
                else
                {
                    Bugger.Warning("No animation file for " + ClassAnimations[i].Name + "'s " + animationName.Name + " animation");
                }
            }
            ClassAnimations[i].Animation = animation;
            // Load projectile
            string projectileLocation = "Images/ClassBattleAnimations/_Projectiles/" + ClassAnimations[i].Name + ".png";
            if (FrogForgeImporter.CheckFileExists<Sprite>(projectileLocation))
            {
                GameObject projectileObject = Instantiate(BaseProjectile, BaseProjectile.transform.parent);
                Sprite projectileSprite = FrogForgeImporter.LoadSpriteFile(projectileLocation);
                FrogForgeImporter.LoadSpriteOrAnimationToObject(projectileObject, projectileSprite, 8, BaseClassAnimation.BaseSpeed);
                projectileObject.transform.position += new Vector3(ClassAnimations[i].ProjectileExtraData.Pos.x, -ClassAnimations[i].ProjectileExtraData.Pos.y, 0) / 16;
                projectileObject.name = ClassAnimations[i].Name;
                ClassAnimations[i].Projectile = projectileObject;
            }

        }
        // Set dirty
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
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
        string json = FrogForgeImporter.LoadTextFile("Tilesets.json").Text;
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
                    Sprite sprite = FrogForgeImporter.LoadSpriteFile("Images/BattleBackgrounds/" + tileset.Name + "/" + battleBackground.Name + "1.png");
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
                    Sprite sprite = FrogForgeImporter.LoadSpriteFile("Images/BattleBackgrounds/" + tileset.Name + "/" + battleBackground.Name + "2.png");
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
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
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
    public class BattleBackgroundData
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
        public ClassAnimation ClassAnimationData;
        public float InitPos;
        public float RandomResult;
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
        public int LookingLeftSign
        {
            get
            {
                return LookingLeft ? 1 : -1;
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
            Animation = Instantiate((ClassAnimationData = battleAnimationController.ClassAnimations.Find(a => a.Name == Unit.Class)).Animation, Object.transform);
            Animation.Renderer = Object;
            Animation.Animations.ForEach(a => a.Split());
            Animation.EditorPreview();
            Animation.Activate("Idle");
            Palette = Animation.transform.parent.gameObject.GetComponent<PalettedSprite>();
            foreach (var item in Sprites)
            {
                item.Palette = (int)Unit.TheTeam;
            }
            Palette.Palette = Unit.Statue ? 3 : (int)Unit.TheTeam;
            UIHealthbar.SetMax(Unit.Stats.Base.MaxHP);
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

        public void MoveInFront(CombatantData other)
        {
            if (Object.transform.position.z > other.Object.transform.position.z)
            {
                float temp = other.Object.transform.position.z;
                other.Object.transform.position += new Vector3(0, 0, Object.transform.position.z - temp);
                Object.transform.position -= new Vector3(0, 0, Object.transform.position.z - temp);
            }
        }
    }
}

[System.Serializable]
public class ClassAnimation
{
    public string Name;
    public AdvancedSpriteSheetAnimation Animation;
    public GameObject Projectile;
    public List<BattleAnimationController.BattleBackgroundData> BattleAnimations;
    public BattleAnimationMode BattleAnimationModeMelee;
    public BattleAnimationMode BattleAnimationModeRanged;
    public BADWalk WalkExtraData;
    public BADProjectile ProjectileExtraData;
    public BADTeleport TeleportExtraData;

    [System.Serializable]
    public class BADWalk
    {
        public const float DEFAULT_SPEED = 2;
        public float Speed = -1;
        public bool CustomSpeed
        {
            get
            {
                return Speed >= 1;
            }
            set
            {
                if (!value)
                {
                    Speed = -1;
                }
                else if (value && Speed < 1)
                {
                    Speed = DEFAULT_SPEED;
                }
            }
        }
    }

    [System.Serializable]
    public class BADProjectile
    {
        public Vector2Int Pos;
    }

    [System.Serializable]
    public class BADTeleport
    {
        public bool Backstab;
    }
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