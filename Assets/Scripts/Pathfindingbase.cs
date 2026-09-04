using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Node
{
    public int NodeId { get; set; }
    public int X { get; set; }
    public int Z { get; set; }
    public Dictionary<int, double> Neighbours { get; set; } = new Dictionary<int, double>();  // node id and distance
}


public class Map
{
    public HashSet<Node> Nodes { get; set; } = new HashSet<Node> { };

    public Node GetNodeById(int nodeId) // needed to get neighbour nodes based on id/index
    {
        return Nodes.FirstOrDefault(node => node.NodeId == nodeId);
    }
}

public class Pathfindingbase : MonoBehaviour
{

    private int[,] customGrid;

    [SerializeField] private GridData gridData;

    public Map gridMap { get; set; } = new Map();

    private int noOfRows;
    private int noOfColumns;

    private void Awake()
    {
        //noOfRows = gridData.NoOfRows;
        //noOfColumns = gridData.NoOfColumns;
        //customGrid = new int[noOfRows, noOfColumns];
        //GetGridData();
        //BuildGraphConnections();
        RebuildGraph();
    }

    public void RebuildGraph()
    {
        noOfRows = gridData.NoOfRows;
        noOfColumns = gridData.NoOfColumns;
        gridMap = new Map();
        customGrid = new int[gridData.NoOfRows,gridData.NoOfColumns];
        GetGridData();
        BuildGraphConnections();
    }

    private void BuildGraphConnections() // for each node in map draw its map
    {
        int[] dx = { -1, 1, 0, 0};
        int[] dz = { 0, 0, -1, 1 };

        foreach (Node node in gridMap.Nodes)
        {
            for(int i = 0; i < 4; i++)
            {
                int neighbourX = node.X + dx[i];
                int neighbourZ = node.Z + dz[i];
                if(neighbourX >= 0 && neighbourZ >= 0 && neighbourX < noOfRows && neighbourZ < noOfColumns) 
                {
                    if (customGrid[neighbourX, neighbourZ] == 0) // meaning no obstacle present
                    {
                        int neighbourId = neighbourX * noOfColumns + neighbourZ;
                        node.Neighbours[neighbourId] = 1.0; // cost noted as 1
                    }
                }
            }
        }
    }

    private void GetGridData()
    {
        Debug.Log(gridData.ObstaclePresent.Length);
        for (int index = 0; index < gridData.ObstaclePresent.Length; index++)
        {
            int x = index / noOfColumns;
            int z = index % noOfColumns;
            //Debug.Log(index);
            //Debug.Log(x + " " + z);
            customGrid[x, z] = gridData.ObstaclePresent[index] ? 1 : 0;

            // generate a node only if its not blocked by obstacle
            if (!gridData.ObstaclePresent[index])
            {
                Node node = new Node
                {
                    NodeId = index, 
                    X = x, 
                    Z = z 
                };
                gridMap.Nodes.Add(node);
            }
        }
    }

    //distance calculation
    public double Heuristic(Node current, Node goal)
    {
        return Math.Abs(current.X - goal.X) + Math.Abs(current.Z - goal.Z);
    }

    public List<Node> AStar(Node start, Node goal, Map map, HashSet<int> blockedNodeIds = null)
    {
        var openList = new List<Node>() { start }; // queued for exploration
        var closedList = new HashSet<Node>(); // already explored
        blockedNodeIds ??= new HashSet<int>(); 

        // Dictionaries to hold g(n), h(n), and parent pointers
        var gScore = new Dictionary<int, double>() { [start.NodeId] = 0 };
        var hScore = new Dictionary<int, double>() { [start.NodeId] = Heuristic(start, goal) };
        var parentMap = new Dictionary<int, Node>();

        while(openList.Count > 0)
        {
            // Find node in open list with the lowest F score
            var current = openList.OrderBy(node => gScore[node.NodeId] + hScore[node.NodeId]).First();

            if (current.NodeId == goal.NodeId)
            {
                return ReconstructPath(parentMap, current);
            }

            openList.Remove(current); 
            closedList.Add(current); 

            foreach(var neighbourId in current.Neighbours.Keys)
            {
                //avoids clicking tile where enemy is standing on or avoids player movement through enemy
                //if a single path to goal(clicked tile) is blocked by enemy a star fails to find path
                if (blockedNodeIds.Contains(neighbourId)) continue; 
                
                var neighbour = map.GetNodeById(neighbourId);
                if (neighbour == null || closedList.Contains(neighbour)) continue; // if node is already evavulated return

                // Tentative gScore (current gScore + distance to neighbour)
                double tentativeGScore = gScore[current.NodeId] + current.Neighbours[neighbourId];

                // if neighbour is not in open list or new path to neighbour is shorter ->
                if(!gScore.ContainsKey(neighbour.NodeId) || tentativeGScore < gScore[neighbour.NodeId])
                {
                    //update g and h of neighbour
                    gScore[neighbour.NodeId] = tentativeGScore;
                    hScore[neighbour.NodeId] = Heuristic(neighbour, goal);

                    // Set the current node as the parent of the neighbour(for backtrack)
                    parentMap[neighbour.NodeId] = current;

                    if(!openList.Contains(neighbour)) // if the neighbour is not queued for exploration add it
                    {
                        openList.Add(neighbour); 
                    }
                }
            }
        }
        return null; // no path 

    }

    //backtrack from goal to current and return the path
    private List<Node> ReconstructPath(Dictionary<int, Node> parentMap, Node current)
    {
        var path = new List<Node> { current };

        while (parentMap.ContainsKey(current.NodeId))
        {
            current = parentMap[current.NodeId];    
            path.Add(current);
        }
        path.Reverse();
        return path;
    }
}
