using UnityEngine;

public interface IAI
{
    void OnPlayerMoved(Tile playerTile); //contract that must be used in enemy/different enemy types when player moves
}
