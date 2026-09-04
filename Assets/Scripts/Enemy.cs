
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BaseMovement, IAI
{
    private Transform playerTransform;

    protected override void Start()
    {
        base.Start();
        //GameEvents.Instance.onPlayerMoved += OnPlayerMoved;
        GameEvents.Instance.onPlayerMovementComplete += FacePlayer;
    }

    // just to see the direction of player once it reaches destination
    private void FacePlayer()
    {
        playerTransform = GameManager.Instance.playerUnit.transform;
        transform.LookAt(playerTransform.position); 
    }

    private void OnDisable()
    {
        //GameEvents.Instance.onPlayerMoved -= OnPlayerMoved;
        GameEvents.Instance.onPlayerMovementComplete -= FacePlayer;
    }

    public void OnPlayerMoved(Tile playerTile) 
    {
        List<Node> pathNodes = GetPathNodes(playerTile);
        if (pathNodes == null || pathNodes.Count <= 2) return; // already adjacent node of player then return

        List<Node> pathToAdjacentTileOfPlayer = pathNodes.GetRange(0, pathNodes.Count - 1); // to reach until (last - 1) tile/node
        RunPath(pathToAdjacentTileOfPlayer);
    }

}
