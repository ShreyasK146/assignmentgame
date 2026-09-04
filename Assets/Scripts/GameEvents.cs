using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance;
    
    public Action<Tile> onTileHovered;
    public Action<Tile> onTileClicked;
    //public Action<Tile> onPlayerMoved;
    public Action onPlayerMovementComplete;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
 
    }

    //public void PlayerMoved(Tile tile)
    //{
    //    onPlayerMoved?.Invoke(tile);
    //}

    public void PlayerMovementComplete()
    {
        onPlayerMovementComplete?.Invoke();
    }

    public void TileHovered(Tile tile)
    {
        onTileHovered?.Invoke(tile);
    }

    public void TileClicked(Tile tile)
    {
        onTileClicked?.Invoke(tile);    
    }
}
