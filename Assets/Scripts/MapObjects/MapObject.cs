using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum InteractState { None, Move, Attack }
public abstract class MapObject : MonoBehaviour
{
    private Vector2Int pos;
    public Vector2Int Pos
    {
        get
        {
            return pos;
        }
        set
        {
            pos = value;
            transform.position = new Vector3(pos.x * GameController.Current.TileSize, pos.y * GameController.Current.TileSize, transform.position.z);
        }
    }
    public abstract void Interact(InteractState interactState);
    private void Start()
    {
        GameController.Current.MapObjects.Add(this);
    }
}
