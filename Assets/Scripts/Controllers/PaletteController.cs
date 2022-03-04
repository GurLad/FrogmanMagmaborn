using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteController : MonoBehaviour
{
    public static PaletteController Current
    {
        get
        {
            if (current == null)
            {
                current = FindObjectOfType<PaletteController>();
            }
            return current;
        }
        private set => current = value;
    }
    public Palette[] BackgroundPalettes { get; private set; } = new Palette[4];
    public Palette[] SpritePalettes { get; private set; } = new Palette[4];
    [SerializeField]
    private Material[] backgroundMaterials = new Material[4];
    [SerializeField]
    private Material[] spriteMaterials = new Material[4];
    [SerializeField]
    private Material textMaterial;
    private static PaletteController current;

    private void Awake()
    {
        if (current != null && current != this)
        {
            DestroyImmediate(gameObject);
            return;
        }
        else
        {
            current = this;
        }
        DontDestroyOnLoad(gameObject);
        textMaterial = Instantiate(textMaterial);
        for (int i = 0; i < 4; i++)
        {
            backgroundMaterials[i] = Instantiate(backgroundMaterials[i]);
            BackgroundPalettes[i] = new PaletteWithMaterial(GetMaterial(true, i));
            spriteMaterials[i] = Instantiate(spriteMaterials[i]);
            SpritePalettes[i] = new SpritePaletteWithMaterial(GetMaterial(false, i), textMaterial, i);
        }
    }

    public PaletteTransition TransitionTo(bool background, int id, Palette target, float speed, bool fromCurrent = false, bool reverse = false)
    {
        PaletteTransition transition = gameObject.AddComponent<PaletteTransition>();
        transition.Source = background ? BackgroundPalettes[id] : SpritePalettes[id];
        if (!fromCurrent)
        {
            transition.Source.Reset();
        }
        transition.Target = target.Clone();
        transition.Speed = speed;
        transition.Background = background;
        transition.Reverse = reverse;
        return transition;
    }

    public Material GetMaterial(bool background, int id)
    {
        return background ? backgroundMaterials[id] : spriteMaterials[id];
    }

    public Material GetTextMaterial()
    {
        return textMaterial;
    }

    public void Fade(bool fadeIn, System.Action postFadeAction, int speed = 10)
    {
        Time.timeScale = 0;
        for (int i = 0; i < 4; i++)
        {
            if (fadeIn)
            {
                if (i == 3)
                {
                    TransitionTo(true, i, BackgroundPalettes[i].Clone(), speed, false).OnEnd = () => Time.timeScale = 1;
                    TransitionTo(false, i, SpritePalettes[i].Clone(), speed, false).OnEnd = postFadeAction;
                }
                else
                {
                    TransitionTo(true, i, BackgroundPalettes[i].Clone(), speed, false);
                    TransitionTo(false, i, SpritePalettes[i].Clone(), speed, false);
                }
            }
            else
            {
                if (i == 3)
                {
                    TransitionTo(true, i, new Palette(), speed, true, true).OnEnd = () => Time.timeScale = 1;
                    TransitionTo(false, i, new Palette(), speed, true, true).OnEnd = postFadeAction;
                }
                else
                {
                    TransitionTo(true, i, new Palette(), speed, true, true);
                    TransitionTo(false, i, new Palette(), speed, true, true);
                }
            }
        }
    }

    public PaletteControllerState SaveState()
    {
        return new PaletteControllerState(this);
    }

    public void LoadState(PaletteControllerState state)
    {
        state.LoadTo(this);
    }

    public class PaletteControllerState
    {
        private Palette[] BackgroundPalettes { get; set; } = new Palette[4];
        private Palette[] SpritePalettes { get; set; } = new Palette[4];

        public PaletteControllerState(PaletteController paletteController)
        {
            for (int i = 0; i < 4; i++)
            {
                BackgroundPalettes[i] = paletteController.BackgroundPalettes[i].Clone();
                SpritePalettes[i] = paletteController.SpritePalettes[i].Clone();
            }
        }

        public void LoadTo(PaletteController paletteController)
        {
            for (int i = 0; i < 4; i++)
            {
                paletteController.BackgroundPalettes[i].CopyFrom(BackgroundPalettes[i]);
                paletteController.SpritePalettes[i].CopyFrom(SpritePalettes[i]);
            }
        }
    }

    private class PaletteWithMaterial : Palette
    {
        private Material linkedMaterial;

        public override Color this[int i]
        {
            get => base[i];
            set
            {
                base[i] = value;
                linkedMaterial?.SetColor("_Color" + (i + 1) + "out", this[i]);
            }
        }

        public PaletteWithMaterial(Material material) : base()
        {
            linkedMaterial = material;
            Reset();
        }
    }

    private class SpritePaletteWithMaterial : PaletteWithMaterial // Also affects the TextMaterial
    {
        private Material linkedTextMaterial;
        private int id;

        public override Color this[int i]
        {
            get => base[i];
            set
            {
                base[i] = value;
                if (i == 1) // All text is considered sprite letters, which use palette 1
                {
                    linkedTextMaterial?.SetColor("_Color" + (id + 1) + "out", this[1]);
                }
            }
        }

        public SpritePaletteWithMaterial(Material material, Material textMaterial, int id) : base(material)
        {
            linkedTextMaterial = textMaterial;
            this.id = id;
            Reset();
        }
    }
}

[System.Serializable]
public class Palette
{
    [SerializeField]
    private Color[] Colors = new Color[4];
    public virtual Color this[int i]
    {
        get
        {
            return Colors[i];
        }
        set
        {
            Colors[i] = value;
        }
    }

    public Palette()
    {
        Reset();
    }

    public Palette(Palette copyFrom)
    {
        for (int i = 0; i < 4; i++)
        {
            Colors[i] = new Color(copyFrom[i].r, copyFrom[i].g, copyFrom[i].b, copyFrom[i].a);
        }
    }

    public void Reset()
    {
        for (int i = 0; i < 4; i++)
        {
            this[i] = Color.black;
        }
    }

    public Palette Clone()
    {
        return new Palette(this);
    }

    public void CopyFrom(Palette palette)
    {
        for (int i = 0; i < 4; i++)
        {
            this[i] = palette[i];
        }
    }
}

public class PaletteTransition : MonoBehaviour
{
    public float Speed;
    public Palette Source;
    public Palette Target;
    public int Current = 3;
    public bool Background;
    public bool Reverse;
    public System.Action OnEnd;
    private float count = 0;
    private List<PalettedSprite> toUpdate = new List<PalettedSprite>();
    public void Start()
    {
        if (!Background)
        {
            Source[3] = Color.clear;
        }
    }
    private void Update()
    {
        count += Time.unscaledDeltaTime * Speed;
        if (count >= 1)
        {
            if (Current <= 0)
            {
                Destroy(this);
                OnEnd?.Invoke();
                return;
            }
            count -= 1;
            if (!Reverse)
            {
                for (int i = 2; i > 0; i--)
                {
                    Source[i + 1] = Source[i];
                }
                Source[1] = Target[Current--];
                if (!Background && Source[1].a < 1)
                {
                    Source[1] = Color.black;
                }
                if (!Background)
                {
                    Source[3] = Color.clear;
                }
            }
            else
            {
                for (int i = 2; i < 4; i++)
                {
                    Source[i - 1] = Source[i];
                }
                Source[3] = Target[4 - (Current--)];
                if (!Background && Source[2].a < 1)
                {
                    Source[2] = Color.black;
                }
                if (!Background)
                {
                    Source[3] = Color.clear;
                }
            }
            toUpdate.ForEach(a => a.UpdatePalette());
        }
    }
    public void AddPalettedSprite(PalettedSprite sprite)
    {
        toUpdate.Add(sprite);
    }
}