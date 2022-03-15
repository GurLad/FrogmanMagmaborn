using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompletePalette : MonoBehaviour
{
    [SerializeField]
    private Texture2D paletteTexture;
    [SerializeField]
    private int blackColor;
    [SerializeField]
    private int transparentColor;
    [SerializeField]
    private int brightnessJump;
    [SerializeField]
    private Color32CopyArray colors;
    private static CompletePalette current;
    public static int BrightnessJump { get => current?.brightnessJump ?? 14; }
    public static int TransparentColor { get => current?.transparentColor ?? 56; }
    public static int BlackColor { get => current?.blackColor ?? 55; }
    public static Color32CopyArray Colors { get => current?.colors; }

    private void Awake()
    {
        current = this;
    }

#if UNITY_EDITOR
    [ContextMenu("Load palette texture")]
    public void LoadTexture()
    {
        brightnessJump = paletteTexture.width;
        Color32[] tempColors = paletteTexture.GetPixels32();
        Color32[] tempColors2 = new Color32[tempColors.Length + 1];
        for (int i = 0; i < tempColors.Length; i++)
        {
            tempColors2[i] = tempColors[i];
        }
        tempColors2[tempColors.Length] = Color.clear;
        colors = new Color32CopyArray(tempColors2);
        blackColor = tempColors.Length - 1;
        transparentColor = tempColors.Length;
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
#endif

    [System.Serializable]
    public class Color32CopyArray
    {
        [SerializeField]
        private Color32[] colors;
        public Color32 this[int i]
        {
            get
            {
                if (i >= colors.Length)
                {
                    throw Bugger.Error("Invalid color! (" + i + ")");
                }
                return new Color32(colors[i].r, colors[i].g, colors[i].b, colors[i].a);
            }
        }

        public Color32CopyArray(Color32[] colors) // Should clone the colors, but I'm lazy and people should be unable to access this anyway
        {
            this.colors = colors;
        }
    }
}
