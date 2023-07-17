using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameHueShifter : MonoBehaviour
{
    public EndgameTormentModelAnimator EndgameTormentModelAnimator;
    public List<Material> ColorMaterials;
    public List<Material> EmissionMaterials;
    public Vector2 HueRange;
    public float LerpStrength;
    private float TargetHue
    {
        get
        {
            float percent = 2 * (float)EndgameTormentModelAnimator.TormentUnit.Health / EndgameTormentModelAnimator.TormentUnit.Stats.Base.MaxHP;
            percent = Mathf.Min(1, percent * percent);
            return HueRange.x * percent + HueRange.y * (1 - percent);
        }
    }
    private List<Vector3> baseColors;
    private List<Vector3> baseEmissions;
    private float currentHue;

    public void Init()
    {
        currentHue = HueRange.x;
        Vector3 temp;
        baseColors = ColorMaterials.ConvertAll(a => { Color.RGBToHSV(a.color, out temp.x, out temp.y, out temp.z); return temp; });
        baseEmissions = EmissionMaterials.ConvertAll(a => { Color.RGBToHSV(a.GetColor("_EmissionColor"), out temp.x, out temp.y, out temp.z); return temp; });
        UpdateMaterials();
    }

    private void Update()
    {
        if (EndgameTormentModelAnimator.TormentUnit != null && Mathf.Abs(currentHue - TargetHue) > 0.0001f)
        {
            currentHue = Mathf.Lerp(currentHue, TargetHue, LerpStrength * Time.deltaTime);
            UpdateMaterials();
        }
    }

    private void UpdateMaterials()
    {
        for (int i = 0; i < ColorMaterials.Count; i++)
        {
            ColorMaterials[i].color = HSVToRGB(baseColors[i], currentHue, ColorMaterials[i].color.a);
            if (i < EmissionMaterials.Count)
            {
                EmissionMaterials[i].SetColor("_EmissionColor", HSVToRGB(baseEmissions[i], currentHue, 1));
            }
        }
    }

    private Color HSVToRGB(Vector3 hsv, float hMod, float a)
    {
        Color temp = Color.HSVToRGB((hsv.x + (hMod / 360)) % 1, hsv.y, hsv.z);
        temp.a = a;
        return temp;
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        for (int i = 0; i < ColorMaterials.Count; i++)
        {
            ColorMaterials[i].color = HSVToRGB(baseColors[i], 0, 1);
            if (i < EmissionMaterials.Count)
            {
                EmissionMaterials[i].SetColor("_EmissionColor", HSVToRGB(baseEmissions[i], 0, 1));
            }
        }
    }
#endif
}
