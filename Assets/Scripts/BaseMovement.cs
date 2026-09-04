using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class BaseMovement : MonoBehaviour
{
    private float tileXWidth;
    private float tileZWidth;
    public LayerMask layerMask;
    public CinemachineCamera virtualCamera;
    private Pathfindingbase pathfindingBase;
    [HideInInspector] public Tile currentTile;
    private Animator animator;
    private int noOfRows;
    private int noOfCols;
    protected virtual void Start()
    {
        tileXWidth = GridManager.Instance.tileXWidth;
        tileZWidth = GridManager.Instance.tileZWidth;
        pathfindingBase = GridManager.Instance.pathFindingBase;
        noOfRows = GridManager.Instance.noOfRows;
        noOfCols = GridManager.Instance.noOfColumns;
        animator = GetComponentInChildren<Animator>();
    }

    // starting tile of player and enemy is initialized here
    public void InitializeTile(Tile tile)
    {
        currentTile = tile;
    }

    //gets the tile where player and enemy is sitting on
    private void GetCurrentTile()
    {
        RaycastHit hitInfo;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hitInfo, Mathf.Infinity, layerMask))
        {
            currentTile = hitInfo.collider.gameObject.GetComponent<Tile>();
            //Debug.Log("current tile = " + currentTile.tileX + "  "+currentTile.tileZ);
        }
    }
    
    //player subscribes to this method to move itself to clicked position/tile
    protected void MoveToTarget(Tile goalTile)
    {
        if (GameManager.Instance.UnitMoving)
        {
            Debug.Log("please wait for the current unit move to complete");
            return;
        }
        int index = goalTile.tileX * noOfCols + goalTile.tileZ;
        if (GridManager.Instance.gridData.ObstaclePresent[index])
        {
            Debug.Log("obstacle present click somewhere else");
            return; // if its obstacle ignore click
        }
        var blocked = GameManager.Instance.GetOccupiedNodeIds(currentTile);
        //Debug.Log(blocked.Count);
        List<Node> pathNodes = GetPathNodes(goalTile,blocked);
        RunPath(pathNodes);
    }

    protected void RunPath(List<Node> pathNodes)
    {
        if (pathNodes == null || pathNodes.Count <= 1)//clicking on same tile as current tile should return
        {
            Debug.Log("No Path Exist");
            return;
        }
        
        GameManager.Instance.UnitMoving = true;
        animator.SetBool("walk", true);
        StartCoroutine(Move(pathNodes));
    }

    //moves player and enemy tile by tile
    private IEnumerator Move(List<Node> pathNodes)
    {
        for (int i = 1; i < pathNodes.Count; i++) 
        {
            Node nextNode = pathNodes[i];
            Vector3 nextNodePosition = new Vector3(nextNode.X * tileXWidth + 0.1f, 0.045f, nextNode.Z * tileZWidth + 0.1f);
            Debug.Log("nextnodepostion = " + nextNodePosition);
            while (transform.position != nextNodePosition)
            {
                transform.LookAt(nextNodePosition);
                transform.position = Vector3.MoveTowards(transform.position, nextNodePosition, Time.deltaTime);
                yield return null;
            }
        }
        GetCurrentTile();
        GameManager.Instance.UnitMoving = false;
        animator.SetBool("walk", false);
        OnMoveFinished();
        GameEvents.Instance.PlayerMovementComplete();
    }

    protected virtual void OnMoveFinished() { }

    // responsible for calling a* for source and destination node/tile
    protected List<Node> GetPathNodes(Tile goalTile, HashSet<int> blockedNodeIds = null)
    {
        int index1 = currentTile.tileX * noOfCols + currentTile.tileZ;
        int index2 = goalTile.tileX * noOfCols + goalTile.tileZ;
        Node currentNode = pathfindingBase.gridMap.GetNodeById(index1);
        Node targetNode = pathfindingBase.gridMap.GetNodeById(index2);

        if (currentNode == null || targetNode == null) return null;

        return pathfindingBase.AStar(currentNode, targetNode, pathfindingBase.gridMap,blockedNodeIds);
    }
    
}
