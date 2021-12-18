using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
//#define MODDABLE_BUILD // Because Visual Studio doesn't work properly otherwise...

public class FrogForgeImporter : MonoBehaviour
{
#if MODDABLE_BUILD
    private static string DataPath;
#endif

    public ConversationController ConversationController;
    public UnitClassData UnitClassData;
    public BattleAnimationController BattleAnimationController;
    public PortraitController PortraitController;
    public CGController CGController;
    public LevelMetadataController LevelMetadataController;
    public MapController MapController;

    private void Awake()
    {
#if MODDABLE_BUILD && !UNITY_EDITOR
        DataPath = Application.dataPath.Replace("Frogman Magmaborn_Data", "") + @"\Data\";
        Bugger.Info("Working! " + DataPath);
        ConversationController?.AutoLoad();
        UnitClassData?.AutoLoad();
        BattleAnimationController?.AutoLoadAnimations();
        BattleAnimationController?.AutoLoadBackgrounds();
        PortraitController?.AutoLoad();
        CGController?.AutoLoad();
        LevelMetadataController?.AutoLoad();
        MapController?.AutoLoadMaps();
        MapController?.AutoLoadTilesets();
#endif
    }

    public static void LoadSpriteOrAnimationToObject(GameObject gameObject, Sprite sprite, int width, int speed = -1, bool loop = true, bool activateOnStart = true)
    {
        SpriteRenderer renderer = gameObject.GetOrAddComponenet<SpriteRenderer>();
        if (sprite.rect.width > width)
        {
            AdvancedSpriteSheetAnimation anim = gameObject.AddComponent<AdvancedSpriteSheetAnimation>();
            SpriteSheetData newData = new SpriteSheetData();
            newData.SpriteSheet = sprite;
            newData.NumberOfFrames = (int)sprite.rect.width / width;
            newData.Speed = speed > 0 ? speed : 0;
            newData.Name = sprite.name;
            newData.Loop = loop;
            anim.Animations = new List<SpriteSheetData>();
            anim.Animations.Add(newData);
            anim.FixedSpeed = speed <= 0;
            anim.ActivateOnStart = activateOnStart;
            anim.Renderer = renderer;
        }
        else
        {
            renderer.sprite = sprite;
        }
    }

    public static Sprite LoadSpriteFile(string path)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Data/" + path);
#elif MODDABLE_BUILD
        Sprite LoadNewSprite(string filePath) // From https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            Texture2D Tex2D;
            byte[] FileData;

            if (File.Exists(filePath))
            {
                FileData = File.ReadAllBytes(filePath);
                Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
                if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                {
                    // If data = readable -> return texture

                    // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

                    Texture2D SpriteTexture = Tex2D;
                    SpriteTexture.filterMode = FilterMode.Point;
                    Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, 0.5f), 16, 0, SpriteMeshType.Tight);

                    return NewSprite;
                }
            }
            return null;
        }

        return LoadNewSprite(DataPath + path);
#endif
    }

    public static TextFile LoadTextFile(string path, bool includesPath = false)
    {
#if UNITY_EDITOR
        return new TextFile(UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/" + path));
#elif MODDABLE_BUILD
        return new TextFile(File.ReadAllText((includesPath ? "" : DataPath) + path), path.Substring(path.LastIndexOf(@"\") + 1));
#endif
    }

    public static bool CheckFileExists<T>(string path) where T : Object // For the future moddable version
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>("Assets/Data/" + path) != null;
#elif MODDABLE_BUILD
        return File.Exists(DataPath + path);
#endif
    }

    public static string[] GetAllFilesAtPath(string path)
    {
#if UNITY_EDITOR
        string[] GUIDs = UnityEditor.AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/Data/" + path });
        for (int i = 0; i < GUIDs.Length; i++)
        {
            GUIDs[i] = UnityEditor.AssetDatabase.GUIDToAssetPath(GUIDs[i]).Replace("Assets/Data/", "");
        }
        return GUIDs;
#elif MODDABLE_BUILD
        return Directory.GetFiles(DataPath + path, "*.*", SearchOption.AllDirectories).Where(a => !a.EndsWith(".meta")).ToArray();
#endif
    }
}
