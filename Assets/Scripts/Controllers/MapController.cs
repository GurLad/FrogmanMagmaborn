using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour // TBA: Move all map-related stuff here. Currently only a tileset controller
{
    [Header("Data")]
    public List<Tileset> Tilesets;
    [Header("Objects")]
    public Tile BaseTile;
    public Transform TilesetsContainer;

    #if UNITY_EDITOR
    public void AutoLoad()
    {
        // Clear previous data
        Tilesets.Clear();
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in TilesetsContainer)
        {
            toDestroy.Add(child.gameObject);
        }
        while (toDestroy.Count > 0)
        {
            DestroyImmediate(toDestroy[0]);
            toDestroy.RemoveAt(0);
        }
        // Load jsons
        string json = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/Tilesets.json").text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("Tilesets"), this);
        // Generate objects & load sprites
        for (int i = 0; i < Tilesets.Count; i++)
        {
            GameObject container = new GameObject(Tilesets[i].Name);
            container.transform.parent = TilesetsContainer;
            Tilesets[i].GenerateObjects(BaseTile, container);
            for (int j = 0; j < Tilesets[i].TileObjects.Count; j++)
            {
                Sprite file = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Data/Images/Tilesets/" + Tilesets[i].Name + "/" + j + ".png");
                if (file.rect.width > 16)
                {
                    AdvancedSpriteSheetAnimation anim = Tilesets[i].TileObjects[j].gameObject.AddComponent<AdvancedSpriteSheetAnimation>();
                    SpriteSheetData newData = new SpriteSheetData();
                    newData.SpriteSheet = file;
                    newData.NumberOfFrames = (int)file.rect.width / (int)file.rect.height;
                    newData.Speed = 0;
                    newData.Name = file.name;
                    newData.Loop = true;
                    anim.Animations = new List<SpriteSheetData>();
                    anim.Animations.Add(newData);
                    anim.FixedSpeed = anim.ActivateOnStart = true;
                    anim.Renderer = Tilesets[i].TileObjects[j].gameObject.GetComponent<SpriteRenderer>();
                }
                else
                {
                    Tilesets[i].TileObjects[j].gameObject.GetComponent<SpriteRenderer>().sprite = file;
                }
            }
        }
    }
    #endif
}

[System.Serializable]
public class Tileset
{
    public string Name;
    public Palette Palette1 = new Palette();
    public Palette Palette2 = new Palette();
    public List<Tile> TileObjects;
    [HideInInspector]
    [SerializeField]
    private List<TileData> Tiles;

    public Tileset()
    {
        for (int i = 0; i < 4; i++)
        {
            Palette1.Colors[i] = Color.black;
            Palette2.Colors[i] = Color.black;
        }
    }

    public void GenerateObjects(Tile baseTile, GameObject container)
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            Tile tile = GameObject.Instantiate(baseTile.gameObject, container.transform).GetComponent<Tile>();
            tile.Name = Tiles[i].Name;
            tile.ArmorModifier = Tiles[i].ArmorMod;
            tile.MovementCost = Tiles[i].MoveCost;
            tile.High = Tiles[i].High;
            tile.GetComponent<PalettedSprite>().ForceSilentSetPalette(Tiles[i].Palette - 1);
            tile.name = i + " - " + tile.Name;
            TileObjects.Add(tile);
        }
    }

    [System.Serializable]
    public class TileData
    {
        public string Name;
        public int MoveCost;
        public int ArmorMod;
        public int Palette;
        public bool High;
    }
}