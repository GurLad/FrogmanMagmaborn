using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogForgeImporter : MonoBehaviour
{
    public ConversationController ConversationController;
    public UnitClassData UnitClassData;
    public BattleAnimationController BattleAnimationController;
    public PortraitController PortraitController;
    public LevelMetadataController LevelMetadataController;
    public MapController MapController;

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

#if UNITY_EDITOR
    public static T LoadFile<T>(string path) where T : Object // For the future moddable version
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>("Assets/Data/" + path);
    }
#endif

#if UNITY_EDITOR
    public static bool CheckFileExists<T>(string path) where T : Object // For the future moddable version
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>("Assets/Data/" + path) != null;
    }
#endif
}
