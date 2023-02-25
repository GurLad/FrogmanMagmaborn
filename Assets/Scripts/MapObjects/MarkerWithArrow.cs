using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MarkerWithArrow : Marker
{
    public SpriteRenderer ArrowRenderer;
    public Sprite ArrowStart;
    public Sprite ArrowLine;
    public Sprite ArrowCorner;
    public Sprite ArrowEnd;

    protected override void Start()
    {
        base.Start();
        PalettedSprite arrowRendererPalettedSprite = ArrowRenderer.GetComponent<PalettedSprite>();
        arrowRendererPalettedSprite.Awake();
        arrowRendererPalettedSprite.Palette = (int)Origin.TheTeam;
        HideArrow();
    }

    private void ShowArrow(Vector2Int previous, Vector2Int next)
    {
        if (previous == -Vector2Int.one)
        {
            ArrowRenderer.sprite = ArrowStart;
            RotateArrowLine(next - Pos);
        }
        else if (next == -Vector2Int.one)
        {
            ArrowRenderer.sprite = ArrowEnd;
            RotateArrowLine(previous - Pos);
        }
        else
        {
            Vector2Int diff = next - previous;
            if (diff.x == 0 || diff.y == 0)
            {
                ArrowRenderer.sprite = ArrowLine;
                RotateArrowLine(diff);
            }
            else
            {
                ArrowRenderer.sprite = ArrowCorner;
                RotateArrowCorner(previous, next, diff);
                //RotateArrow(Pos - previous);
            }
        }
    }

    private void RotateArrowLine(Vector2Int diff)
    {
        float zRot = (diff.x != 0 ? (1 - Mathf.Sign(diff.x)) * 90 : 0) + (diff.y != 0 ? (2 + Mathf.Sign(diff.y)) * 90 : 0);
        ArrowRenderer.transform.localEulerAngles = new Vector3(0, 0, zRot);
    }

    private void RotateArrowCorner(Vector2Int previous, Vector2Int next, Vector2Int diff)
    {
        float zRot = 42;
        Vector2Int diagonal = new Vector2Int(1, -1);
        if (
            (diff == Vector2Int.one && next - Pos == Vector2Int.right) || (diff == -Vector2Int.one && next - Pos == Vector2Int.down))
        {
            zRot = 0;
        }
        else if (
            (diff == diagonal && next - Pos == Vector2Int.down) || (diff == -diagonal && next - Pos == Vector2Int.left))
        {
            zRot = 90;
        }
        else if (
            (diff == Vector2Int.one && next - Pos == Vector2Int.up) || (diff == -Vector2Int.one && next - Pos == Vector2Int.left))
        {
            zRot = 180;
        }
        else if (
            (diff == diagonal && next - Pos == Vector2Int.right) || (diff == -diagonal && next - Pos == Vector2Int.up))
        {
            zRot = 270;
        }
        ArrowRenderer.transform.localEulerAngles = new Vector3(0, 0, zRot);
    }

    public void ShowArrowPath()
    {
        List<Vector2Int> path = Origin.FindPath(Pos);
        if (path.Count > 0)
        {
            path.Insert(0, Origin.Pos);
            for (int i = 0; i < path.Count; i++)
            {
                MarkerWithArrow marker = GameController.Current.GetMarkerAtPos<MarkerWithArrow>(path[i]);
                marker.ShowArrow(i > 0 ? path[i - 1] : -Vector2Int.one, i < path.Count - 1 ? path[i + 1] : -Vector2Int.one);
            }
        }
    }

    public void HideArrow()
    {
        ArrowRenderer.sprite = null;
    }
}
