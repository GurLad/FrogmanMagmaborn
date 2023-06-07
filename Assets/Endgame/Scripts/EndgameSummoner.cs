using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameSummoner : AGameControllerListener
{
    private enum SummonOnUnitModes { Damage, Teleport, EndMarker }
    private enum SummonNoUnitModes { CreateMagmaborn, CreateChaosEnemy, EndMarker }

    private static Sprite circleSpriteOn; // Terrible workaround but it works
    private static Sprite circleSpriteOff;

    public string ChaosCircleName;
    public float ChaosModifierBaseIncrease;
    public float ChaosModifierTormentHealthMultiplierIncrease;
    public Sprite CircleSpriteOn;
    public Sprite CircleSpriteOff;
    private Unit torment;
    private float chaosModifier = 0;
    private float chaosModifierIncrease => ChaosModifierBaseIncrease + ChaosModifierTormentHealthMultiplierIncrease * (torment.Stats.MaxHP - torment.Health) / torment.Stats.MaxHP;
    private List<SummonCircle> circles = new List<SummonCircle>();

    public void Process(Tile[,] tiles, Vector2Int size)
    {
        circleSpriteOn ??= CircleSpriteOn;
        circleSpriteOff ??= CircleSpriteOff;
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
                    circles.Add(new SummonCircle(new Vector2Int(i, j), tiles[i, j].GetComponent<AdvancedSpriteSheetAnimation>()));
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
        // Store current summons
        List<SummonCircle> currentSummons = circles.FindAll(a => a.Summoning);
        // Activate old summons
        foreach (SummonCircle circle in currentSummons)
        {
            if (!circle.Summoning)
            {
                circle.Summoning = false;
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
        }
        // Begin new summons
        chaosModifier += chaosModifierIncrease;
        for (int i = 0; i < Mathf.FloorToInt(chaosModifier); i++)
        {
            List<SummonCircle> availableCircles = circles.FindAll(a => !a.Summoning && !currentSummons.Contains(a));
            if (availableCircles.Count > 0)
            {
                SummonCircle selected = availableCircles[Random.Range(0, availableCircles.Count)];
                //Bugger.Info("Available: " + string.Join(", ", availableCircles.ConvertAll(a => a.Pos)) + ", chose: " + selected.Pos);
                selected.Summoning = true;
            }
            else
            {
                return;
            }
            chaosModifier--;
        }
        Bugger.Info("Chaos is now " + chaosModifier);
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
                SummonCircle other = circles.FindAll(a => a != circle)[Random.Range(0, circles.Count - 1)];
                Unit onOtherCircle = GameController.Current.FindUnitAtPos(other.Pos);
                if (onOtherCircle != null) // TBA: Fix animation
                {
                    MapAnimationsController.Current.OnFinishAnimation = () => other.Summoning = circle.Summoning = false;
                    MapAnimationsController.Current.AnimateSwapTeleport(target, onOtherCircle);
                }
                else
                {
                    MapAnimationsController.Current.OnFinishAnimation = () => circle.Summoning = false;
                    MapAnimationsController.Current.AnimateTeleport(target, other.Pos, true);
                }
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
                Unit summoned = GameController.Current.CreateUnit(classes[Random.Range(0, classes.Count)].Name, GameController.Current.LevelNumber, Team.Player, false);
                summoned.Moved = true;
                MapAnimationsController.Current.OnFinishAnimation = () => circle.Summoning = false;
                MapAnimationsController.Current.AnimateTeleport(summoned, circle.Pos, false);
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
        public AdvancedSpriteSheetAnimation Tile;
        private bool _summoning = false;
        public bool Summoning
        {
            get => _summoning;
            set
            {
                _summoning = value;
                Tile.Animations[0].SpriteSheet = value ? circleSpriteOn : circleSpriteOff;
                Tile.Animations[0].Split();
            }
        }

        public SummonCircle(Vector2Int pos, AdvancedSpriteSheetAnimation tile)
        {
            Pos = pos;
            Tile = tile;
            Tile.gameObject.transform.position -= new Vector3(0, 0, 0.1f);
        }
    }
}
