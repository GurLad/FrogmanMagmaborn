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
    private Palette previousBG1Palette = null;
    private Palette previousBG2Palette = null;

    private void Reset()
    {
        CG cg = new CG();
        cg.BGPalette1 = new Palette();
        cg.BGPalette2 = new Palette();
        CGs.Add(cg);
    }

    private void Awake()
    {
        Container.SetActive(false);
    }

    public void ShowCG(string name)
    {
        previousBG1Palette = PaletteController.Current.BackgroundPalettes[0];
        previousBG2Palette = PaletteController.Current.BackgroundPalettes[1];
        Debug.Log("Searching for " + name);
        CG toShow = CGs.Find(a => a.Name == name);
        Debug.Log("Found " + (toShow?.FGPalette1.ToString() ?? "Null"));
        LoadImage(BG1, toShow.BGImage1);
        LoadImage(BG2, toShow.BGImage2);
        LoadImage(FG1, toShow.FGImage1);
        LoadImage(FG2, toShow.FGImage2);
        PaletteController.Current.BackgroundPalettes[0] = toShow.BGPalette1;
        PaletteController.Current.BackgroundPalettes[1] = toShow.BGPalette2;
        BG1.Palette = 0;
        BG2.Palette = 1;
        FG1.Palette = toShow.FGPalette1;
        FG2.Palette = toShow.FGPalette2;
        Container.SetActive(true);
    }

    public void HideCG()
    {
        PaletteController.Current.BackgroundPalettes[0] = previousBG1Palette ?? PaletteController.Current.BackgroundPalettes[0];
        PaletteController.Current.BackgroundPalettes[1] = previousBG2Palette ?? PaletteController.Current.BackgroundPalettes[1];
        Container.SetActive(false);
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

#if UNITY_EDITOR
    public void AutoLoad()
    {
        // Load json
        string json = FrogForgeImporter.LoadFile<TextAsset>("CGs.json").text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("CGs"), this);
        // Load sprites
        for (int i = 0; i < CGs.Count; i++)
        {
            CGs[i].BGImage1 = FrogForgeImporter.LoadFile<Sprite>("Images/CGs/" + CGs[i].Name + "/BG1.png");
            CGs[i].BGImage2 = FrogForgeImporter.LoadFile<Sprite>("Images/CGs/" + CGs[i].Name + "/BG2.png");
            CGs[i].FGImage1 = FrogForgeImporter.LoadFile<Sprite>("Images/CGs/" + CGs[i].Name + "/FG1.png");
            CGs[i].FGImage2 = FrogForgeImporter.LoadFile<Sprite>("Images/CGs/" + CGs[i].Name + "/FG2.png");
        }
        UnityEditor.EditorUtility.SetDirty(gameObject);
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