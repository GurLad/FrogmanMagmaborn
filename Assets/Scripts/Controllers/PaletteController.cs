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
        for (int i = 0; i < 4; i++)
        {
            backgroundMaterials[i] = Instantiate(backgroundMaterials[i]);
            BackgroundPalettes[i] = new PaletteWithMaterial(GetMaterial(true, i));
            spriteMaterials[i] = Instantiate(spriteMaterials[i]);
            SpritePalettes[i] = new PaletteWithMaterial(GetMaterial(false, i));
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
            Colors[i] = new Color(copyFrom[i].r, copyFrom[i].g, copyFrom[i].b);
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
        count += Time.deltaTime * Speed;
        if (count >= 1)
        {
            if (Current <= 0)
            {
                Destroy(this);
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