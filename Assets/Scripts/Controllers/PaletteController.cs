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
    public Palette[] BackgroundPalettes = new Palette[4];
    public Palette[] SpritePalettes = new Palette[4];
    private static PaletteController current;

    private void Reset()
    {
        for (int i = 0; i < 4; i++)
        {
            BackgroundPalettes[i].Colors[0] = Color.black;
            BackgroundPalettes[i].Colors[1] = Color.white;
            BackgroundPalettes[i].Colors[2] = Color.gray;
            BackgroundPalettes[i].Colors[3] = Color.black;
            SpritePalettes[i].Colors[0] = Color.black;
            SpritePalettes[i].Colors[1] = Color.white;
            SpritePalettes[i].Colors[2] = Color.gray;
            SpritePalettes[i].Colors[3] = new Color(1,0,1,0);
        }
    }

    private void Awake()
    {
        if (current != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        else
        {
            current = this;
        }
        DontDestroyOnLoad(gameObject);
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
}

[System.Serializable]
public class Palette
{
    public Color[] Colors = new Color[4];
    public Color this[int i]
    {
        get
        {
            return Colors[i];
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
            Colors[i] = Color.black;
        }
    }
    public Palette Clone()
    {
        return new Palette(this);
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
            Source.Colors[3].a = 0;
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
                    Source.Colors[i + 1] = Source[i];
                }
                Source.Colors[1] = Target[Current--];
                if (!Background)
                {
                    Source.Colors[3].a = 0;
                }
            }
            else
            {
                for (int i = 2; i < 4; i++)
                {
                    Source.Colors[i - 1] = Source[i];
                }
                Source.Colors[3] = Target[4 - (Current--)];
                if (!Background)
                {
                    Source.Colors[3].a = 0;
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