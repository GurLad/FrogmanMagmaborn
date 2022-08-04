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

    public PaletteTransition PaletteTransitionTo(bool background, int id, Palette target, float speed, bool fromCurrent = false, bool reverse = false)
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

    public void DarkenScreen(bool fixDoubleWhite = false)
    {
        void DarkenPalette(Palette palette)
        {
            for (int i = 0; i < 4; i++)
            {
                if (palette[i] < CompletePalette.TransparentColor)
                {
                    if (fixDoubleWhite && palette[i] == 0)
                    {
                        palette[i] += CompletePalette.BrightnessJump;
                    }
                    palette[i] = Mathf.Min(CompletePalette.BlackColor, palette[i] + CompletePalette.BrightnessJump);
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            DarkenPalette(BackgroundPalettes[i]);
            DarkenPalette(SpritePalettes[i]);
        }
    }

    public void FadeIn(System.Action postFadeAction, float speed = 30)
    {
        Fade(true, postFadeAction, speed);
    }

    public void FadeOut(System.Action postFadeAction, float speed = 30)
    {
        Fade(false, postFadeAction, speed);
    }

    private void Fade(bool fadeIn, System.Action postFadeAction, float speed)
    {
        if (GameCalculations.TransitionsOn)
        {
            Time.timeScale = 0;
            //speed *= GameCalculations.GameSpeed(false);
            for (int i = 0; i < 4; i++)
            {
                if (i == 3)
                {
                    FadePaletteTransitionTo(fadeIn, true, i, BackgroundPalettes[i].Clone(), speed).OnEnd = () => Time.timeScale = 1;
                    FadePaletteTransitionTo(fadeIn, false, i, SpritePalettes[i].Clone(), speed).OnEnd = postFadeAction;
                }
                else
                {
                    FadePaletteTransitionTo(fadeIn, true, i, BackgroundPalettes[i].Clone(), speed);
                    FadePaletteTransitionTo(fadeIn, false, i, SpritePalettes[i].Clone(), speed);
                }
            }
        }
        else
        {
            postFadeAction?.Invoke();
        }
    }

    private FadePaletteTransition FadePaletteTransitionTo(bool fadeIn, bool background, int id, Palette target, float speed)
    {
        FadePaletteTransition transition = gameObject.AddComponent<FadePaletteTransition>();
        transition.Source = background ? BackgroundPalettes[id] : SpritePalettes[id];
        transition.Target = target.Clone();
        transition.Speed = speed;
        transition.FadeIn = fadeIn;
        return transition;
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
        private Color32[] colors;

        public override int this[int i]
        {
            get => base[i];
            set
            {
                base[i] = value;
                linkedMaterial?.SetColor("_Color" + (i + 1) + "out", CompletePalette.Colors[this[i]]);
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

        public override int this[int i]
        {
            get => base[i];
            set
            {
                base[i] = value;
                if (i == 1) // All text is considered sprite letters, which use palette 1
                {
                    linkedTextMaterial?.SetColor("_Color" + (id + 1) + "out", CompletePalette.Colors[this[1]]);
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
    private PaletteColorObject[] Colors = new PaletteColorObject[4];
    public virtual int this[int i]
    {
        get
        {
            return Colors[i].id;
        }
        set
        {
            Colors[i].id = value;
        }
    }

    public Palette()
    {
        for (int i = 0; i < 4; i++)
        {
            Colors[i] = new PaletteColorObject();
        }
        Reset();
    }

    public Palette(Palette copyFrom) : this()
    {
        CopyFrom(copyFrom);
    }

    public void Reset()
    {
        for (int i = 0; i < 4; i++)
        {
            this[i] = CompletePalette.BlackColor;
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

    [System.Serializable]
    private class PaletteColorObject
    {
        public int id;
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

    public void Start()
    {
        if (!Background)
        {
            Source[3] = CompletePalette.TransparentColor;
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
                if (!Background && Source[1] == CompletePalette.TransparentColor)
                {
                    Source[1] = CompletePalette.BlackColor;
                }
                if (!Background)
                {
                    Source[3] = CompletePalette.TransparentColor;
                }
            }
            else
            {
                for (int i = 2; i < 4; i++)
                {
                    Source[i - 1] = Source[i];
                }
                Source[3] = Target[4 - (Current--)];
                if (!Background && Source[2] == CompletePalette.TransparentColor)
                {
                    Source[2] = CompletePalette.BlackColor;
                }
                if (!Background)
                {
                    Source[3] = CompletePalette.TransparentColor;
                }
            }
        }
    }
}

public class FadePaletteTransition : MonoBehaviour
{
    public float Speed;
    public Palette Source;
    public Palette Target;
    public int Indicator = CompletePalette.TransparentColor;
    public bool FadeIn;
    public System.Action OnEnd;
    private float count = 0;

    private void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Source[i] < CompletePalette.TransparentColor)
            {
                Source[i] = FadeIn ? CompletePalette.BlackColor : Target[i];
            }
        }
    }

    private void Update()
    {
        count += Time.unscaledDeltaTime * Speed;
        if (count >= 1)
        {
            if (Indicator <= 0)
            {
                Destroy(this);
                OnEnd?.Invoke();
                return;
            }
            count -= 1;
            Indicator -= CompletePalette.BrightnessJump;
            for (int i = 0; i < 4; i++)
            {
                if (Source[i] < CompletePalette.TransparentColor)
                {
                    Source[i] = FadeIn ? Mathf.Min(Target[i] + Indicator, CompletePalette.BlackColor) : Mathf.Min(Target[i] + CompletePalette.TransparentColor - Indicator, CompletePalette.BlackColor);
                }
            }
        }
    }
}