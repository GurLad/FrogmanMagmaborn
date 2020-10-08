using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public string Name;
    public int MovementCost;
    public int ArmorModifier;
    public bool High;
    public bool Passable
    {
        get
        {
            return !(MovementCost > 5 && High);
        }
    }
    public override string ToString()
    {
        return Name + '\n' + (MovementCost <= 9 ? (MovementCost + (High ? "All\n" : "Mov\n") + ArmorModifier.ToString()[0] + "Arm") : High ? "\nHigh" : "\nLow");
    }
    public int GetArmorModifier(Unit unit)
    {
        return (unit.Flies && !High) ? 0 : ArmorModifier;
    }
}
