using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameSummoner : AGameControllerListener
{
    private enum SummonOnUnitModes { Damage, Teleport, EndMarker }
    private enum SummonNoUnitModes { CreateMagmaborn, CreateChaosEnemy, EndMarker }

    public string ChaosCircleName;
    public float ChaosModifierBaseIncrease;
    public float ChaosModifierTormentHealthMultiplierIncrease;
    public Sprite CircleSpriteOn;
    public Sprite CircleSpriteOff;
    private Unit torment;
    private float chaosModifier = 0;
    private float chaosModifierIncrease => ChaosModifierBaseIncrease + ChaosModifierTormentHealthMultiplierIncrease * (torment.Stats.MaxHP - torment.Health) / torment.Stats.MaxHP;
    private List<SummonCircle> circles;

    public void Process(Tile[,] tiles, Vector2Int size)
    {
        List<Unit> torments = GameController.Current.GetNamedUnits("Torment");
        if (torments.Count != 1)
        {
            throw Bugger.Error("There must be exactly 1 unit named Torment for the endgame summoner to work");
        }
        torment = GameController.Current.GetNamedUnits("Torment")[0];
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (tiles[i, j].Name == ChaosCircleName)
                {
                    circles.Add(new SummonCircle(new Vector2Int(i, j), false, tiles[i, j].GetComponent<AdvancedSpriteSheetAnimation>()));
                }
            }
        }
    }

    public override void OnBeginPlayerTurn(List<Unit> units)
    {
        // TEMP
        if (torment == null)
        {
            Process(GameController.Current.Map, GameController.Current.MapSize);
        }
        // Activate old summons
        foreach (SummonCircle circle in circles.FindAll(a => a.Summoning))
        {
            if (!circle.Summoning)
            {
                continue;
            }
            Unit onCircle = GameController.Current.FindUnitAtPos(circle.Pos);
            if (onCircle != null)
            {
                SummonActionOnUnit(circle, onCircle);
            }
            else
            {
                SummonActionNoUnit(circle);
            }
            circle.Summoning = false;
        }
        // Begin new summons
        chaosModifier += chaosModifierIncrease;
        for (int i = 0; i < chaosModifier; i++)
        {
            List<SummonCircle> availableCircles = circles.FindAll(a => !a.Summoning);
            if (availableCircles.Count > 0)
            {
                SummonCircle selected = availableCircles[Random.Range(0, availableCircles.Count)];
                selected.Tile.Animations[0].SpriteSheet = CircleSpriteOn;
                selected.Tile.Animations[0].Split();
                selected.Summoning = true;
            }
            else
            {
                return;
            }
            chaosModifier--;
        }
    }

    public override void OnEndLevel(List<Unit> units, bool playerWon)
    {
        // Do nothing
    }

    public override void OnPlayerWin(List<Unit> units)
    {
        // Do nothing
    }

    private void SummonActionOnUnit(SummonCircle circle, Unit target)
    {
        SummonOnUnitModes mode = (SummonOnUnitModes)Random.Range(0, (int)SummonOnUnitModes.EndMarker);
        switch (mode)
        {
            case SummonOnUnitModes.Damage: // TBA
            case SummonOnUnitModes.Teleport:
                SummonCircle other = circles[Random.Range(0, circles.Count)];
                Unit onOtherCircle = GameController.Current.FindUnitAtPos(other.Pos);
                if (onOtherCircle != null)
                {
                    onOtherCircle.Pos = target.Pos;
                }
                target.Pos = other.Pos;
                other.Summoning = false;
                break;
            case SummonOnUnitModes.EndMarker:
                break;
            default:
                break;
        }
    }

    private void SummonActionNoUnit(SummonCircle circle)
    {
        SummonNoUnitModes mode = (SummonNoUnitModes)Random.Range(0, (int)SummonNoUnitModes.EndMarker);
        switch (mode)
        {
            case SummonNoUnitModes.CreateMagmaborn: // TBA
            case SummonNoUnitModes.CreateChaosEnemy:
                List<ClassData> classes = GameController.Current.UnitClassData.ClassDatas.FindAll(a => a.Name != "Torment");
                Unit summoned = GameController.Current.CreateUnit(classes[Random.Range(0, classes.Count)].Name, GameController.Current.LevelNumber, Team.Guard, false);
                summoned.TheTeam = Team.Player;
                summoned.Pos = circle.Pos;
                summoned.Moved = true;
                break;
            case SummonNoUnitModes.EndMarker:
                break;
            default:
                break;
        }
    }

    private class SummonCircle
    {
        public Vector2Int Pos;
        public bool Summoning;
        public AdvancedSpriteSheetAnimation Tile;

        public SummonCircle(Vector2Int pos, bool summoning, AdvancedSpriteSheetAnimation tile)
        {
            Pos = pos;
            Summoning = summoning;
            Tile = tile;
        }
    }
}
