using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CGController : MonoBehaviour
{
    public List<CG> CGs;
    public GameObject Container;
    public PalettedSprite BG1;
    public PalettedSprite BG2;
    public PalettedSprite FG1;
    public PalettedSprite FG2;
    [HideInInspector]
    public string CurrentCG;
    public bool Active
    {
        get
        {
            return Container.activeSelf;
        }
    }
    public PaletteController.PaletteControllerState PreviousState { get; private set; } = null;

    private void Reset()
    {
        CG cg = new CG();
        cg.BGPalette1 = new Palette();
        cg.BGPalette2 = new Palette();
        CGs.Add(cg);
    }

    public void Init()
    {
        BG1.Awake();
        BG2.Awake();
        FG1.Awake();
        FG2.Awake();
        Container.SetActive(false);
    }

    public void FadeInCG(string name, PaletteController.PaletteControllerState currentState = null)
    {
        PreviousState = currentState ?? PreviousState;
        CG toShow = CGs.Find(a => a.Name == name);
        if (toShow == null)
        {
            throw Bugger.Error("No matching CG! (" + name + ")");
        }
        LoadImage(BG1, toShow.BGImage1);
        LoadImage(BG2, toShow.BGImage2);
        LoadImage(FG1, toShow.FGImage1);
        LoadImage(FG2, toShow.FGImage2);
        PaletteController.Current.LoadState(PreviousState);
        PaletteController.Current.BackgroundPalettes[0].CopyFrom(toShow.BGPalette1.Clone());
        PaletteController.Current.BackgroundPalettes[1].CopyFrom(toShow.BGPalette2.Clone());
        BG1.Palette = 0;
        BG2.Palette = 1;
        FG1.Palette = toShow.FGPalette1;
        FG2.Palette = toShow.FGPalette2;
        CurrentCG = name;
        Container.SetActive(true);
        PaletteController.Current.FadeIn(() => ConversationPlayer.Current.Wait(1));
    }

    public void FadeOutCG(System.Action postFadeOutAction)
    {
        if (Active)
        {
            PaletteController.Current.FadeOut(() =>
            {
                PaletteController.Current.LoadState(PreviousState ?? PaletteController.Current.SaveState());
                Container.SetActive(false);
                postFadeOutAction?.Invoke();
            });
        }
        else
        {
            throw Bugger.Error("Frogman Magmaborn error - fading out a CG when there is none. Please report to the dev.");
        }
    }

    private void LoadImage(PalettedSprite display, Sprite image)
    {
        if (image != null)
        {
            display.gameObject.GetComponent<Image>().sprite = image;
            display.gameObject.SetActive(true);
        }
        else
        {
            display.gameObject.SetActive(false);
        }
    }

#if UNITY_EDITOR || MODDABLE_BUILD
    public void AutoLoad()
    {
        // Load json
        string json = FrogForgeImporter.LoadTextFile("CGs.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("CGs"), this);
        // Load sprites
        for (int i = 0; i < CGs.Count; i++)
        {
            CGs[i].BGImage1 = FrogForgeImporter.LoadSpriteFile("Images/CGs/" + CGs[i].Name + "/BG1.png");
            CGs[i].BGImage2 = FrogForgeImporter.LoadSpriteFile("Images/CGs/" + CGs[i].Name + "/BG2.png");
            CGs[i].FGImage1 = FrogForgeImporter.LoadSpriteFile("Images/CGs/" + CGs[i].Name + "/FG1.png");
            CGs[i].FGImage2 = FrogForgeImporter.LoadSpriteFile("Images/CGs/" + CGs[i].Name + "/FG2.png");
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }
#endif
}

[System.Serializable]
public class CG
{
    public string Name;
    public Palette BGPalette1 = new Palette();
    public Palette BGPalette2 = new Palette();
    public int FGPalette1;
    public int FGPalette2;
    public Sprite BGImage1;
    public Sprite BGImage2;
    public Sprite FGImage1;
    public Sprite FGImage2;
}