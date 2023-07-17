using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameHueShifter : MonoBehaviour
{
    public EndgameTormentModelAnimator EndgameTormentModelAnimator;
    public List<Material> EndgameMaterials;
    public Vector2 HueRange;
    public float LerpStrength;
    private float TargetHue
    {
        get
        {
            float percent = EndgameTormentModelAnimator.TormentUnit.Health / EndgameTormentModelAnimator.TormentUnit.Stats.Base.MaxHP;
            return HueRange.x * percent + HueRange.y * (1 - percent);
        }
    }
    private List<Vector3> baseColors;
    private float currentHue;

    private void Start()
    {
        currentHue = HueRange.x;
        Vector3 temp;
        baseColors = EndgameMaterials.ConvertAll(a => { Color.RGBToHSV(a.color, out temp.x, out temp.y, out temp.z); return temp; });
        UpdateMaterials();
    }

    private void Update()
    {
        if (EndgameTormentModelAnimator.TormentUnit != null && Mathf.Abs(currentHue - TargetHue) > 0.0001f)
        {
            currentHue = Mathf.Lerp(currentHue, TargetHue, LerpStrength);
            UpdateMaterials();
        }
    }

    private void UpdateMaterials()
    {
        for (int i = 0; i < EndgameMaterials.Count; i++)
        {
            EndgameMaterials[i].color = Color.HSVToRGB(baseColors[i].x + currentHue, baseColors[i].y, baseColors[i].z);
        }
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        for (int i = 0; i < EndgameMaterials.Count; i++)
        {
            EndgameMaterials[i].color = Color.HSVToRGB(baseColors[i].x, baseColors[i].y, baseColors[i].z);
        }
    }
#endif
}
