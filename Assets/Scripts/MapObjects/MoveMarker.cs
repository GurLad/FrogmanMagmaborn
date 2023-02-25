using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMarker : Marker
{
    // Movement arrow code
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

    public void HideArrow()
    {
        ArrowRenderer.sprite = null;
    }

    public void ShowArrow(Vector2Int previous, Vector2Int next)
    {
        if (previous == -Vector2Int.one)
        {
            ArrowRenderer.sprite = ArrowStart;
            RotateArrow(next - Pos);
        }
        else if (next == -Vector2Int.one)
        {
            ArrowRenderer.sprite = ArrowEnd;
            RotateArrow(next - Pos);
        }
        else
        {
            Vector2Int diff = next - previous;
            if (diff.x == 0 || diff.y == 0)
            {
                ArrowRenderer.sprite = ArrowLine;
                RotateArrow(diff);
            }
            else
            {
                ArrowRenderer.sprite = ArrowCorner;
                RotateArrow(next - Pos);
                RotateArrow(Pos - previous);
            }
        }
    }

    private void RotateArrow(Vector2Int diff)
    {
        float zRot = (diff.x != 0 ? (1 - Mathf.Sign(diff.x)) * 90 : 0) + (diff.y != 0 ? (2 - Mathf.Sign(diff.x)) * 90 : 0);
        ArrowRenderer.transform.Rotate(0, 0, zRot);
    }

    public override void Hover(InteractState interactState)
    {
        if (interactState == InteractState.Move && Origin.TheTeam == GameController.Current.CurrentPhase)
        {
            GameController.Current.RemoveArrowMarkers();
            List<Vector2Int> path = Origin.FindPath(Pos);
            path.Insert(0, Pos);
            for (int i = 0; i < path.Count; i++)
            {
                MoveMarker marker = GameController.Current.GetMarkerAtPos<MoveMarker>(path[i]);
                marker.ShowArrow(i > 0 ? path[i - 1] : -Vector2Int.one, i < path.Count - 1 ? path[i + 1] : -Vector2Int.one);
            }
        }
    }

    public override void Interact(InteractState interactState)
    {
        if (interactState == InteractState.Move && Origin.TheTeam == GameController.Current.CurrentPhase)
        {
            Origin.MoveOrder(Pos);
        }
        else if (interactState == InteractState.None)
        {
            GameController.Current.RemoveMarkers();
        }
    }
}
