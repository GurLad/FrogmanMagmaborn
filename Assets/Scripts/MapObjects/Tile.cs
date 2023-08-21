using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public string Name;
    public int MovementCost;
    public int ArmorModifier;
    public bool High;
    public bool HasBattleBackground;
    public bool Passable
    {
        get
        {
            return !(MovementCost > 5 && High);
        }
    }

    public override string ToString()
    {
        return Name.Substring(0, Mathf.Min(Name.Length, 4)) + '\n' + (MovementCost <= 9 ? (MovementCost + (High ? "All\n" : "Mov\n") + ArmorModifier.ToString()[0] + "Arm") : High ? "\nHigh" : "\nLow");
    }
}
