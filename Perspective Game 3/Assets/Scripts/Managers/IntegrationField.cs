using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUtils;
//This is a custom flowfield algorithm, which takes typical flowfield methods, and builds on them for the purposes of the perspective game
//for example, instead of cost being computed as just 'steps', it is a function of 'steps', 'perspective shifts' and 'rotations' required
//there is no cost field created. This process goes right to an integration field, and computes cost as it goes along
//The integration field is not stored in an array, due to the non-linear way that nodes are connected. Instead, it is store in a dictionary
public class IntegrationField
{

    private List<PathfindingManager.PathfindingNode> goalNodes;
    private Dictionary<PathfindingManager.PathfindingNode, int> integrationField;
    //represents positions you can get to, but can't get to the goal from (i.e. monodirectional traps)
    private List<PathfindingManager.PathfindingNode> trapPositions;

    //the "TravelFromToLookup" tracks what nodes 'value' that can BE MOVED FROM the node 'key'
    private Dictionary<PathfindingManager.PathfindingNode, List<PathfindingManager.PathfindingNode>> travelFromToLookup;
    //this table represents all the nodes 'value' that are ABLE TO MOVE TO the node 'key'
    private Dictionary<PathfindingManager.PathfindingNode, List<PathfindingManager.PathfindingNode>> travelToFromLookup;


    private bool isValid;

    public List<PathfindingManager.PathfindingNode> TrapPositions { get => trapPositions; }
    public bool IsValid { get => isValid;}
    public Dictionary<PathfindingManager.PathfindingNode, List<PathfindingManager.PathfindingNode>> TravelFromToLookup { get => travelFromToLookup; }
    public Dictionary<PathfindingManager.PathfindingNode, List<PathfindingManager.PathfindingNode>> TravelToFromLookup { get => travelToFromLookup; }

    public IntegrationField(Vector3Int goalBlockPos, PathfindingManager.PathfindingNode startNode)
    {
        //because there could be multiple 'win' nodes (e.g. multiple faces of a single block, the goalNode is represented by a list
        goalNodes = new List<PathfindingManager.PathfindingNode>();
        foreach (Vector3Int dir in JUtilsClass.getDirections())
        {
            GameObject spaceBlock = GridManager.Instance.Grid.getBlock(goalBlockPos + dir);
            if (spaceBlock == null)
                goalNodes.Add(new PathfindingManager.PathfindingNode(goalBlockPos + dir, dir));
        }
        calculateIntegrationField(goalNodes, startNode);
    }

    //used when goal is a single node
    public IntegrationField (PathfindingManager.PathfindingNode goalNode, PathfindingManager.PathfindingNode startNode)
    {
        //because there could be multiple 'win' nodes (e.g. multiple faces of a single block, the goalNode is represented by a list
        goalNodes = new List<PathfindingManager.PathfindingNode>();
        goalNodes.Add(goalNode);
        calculateIntegrationField(goalNodes, startNode);
    }



    private void calculateIntegrationField(List<PathfindingManager.PathfindingNode> goalNodes, PathfindingManager.PathfindingNode startNode)
    {
        Debug.Log("Calculating integration field for pathfinding...");
        
        Debug.Log("Goal nodes are:");
        foreach (PathfindingManager.PathfindingNode n in goalNodes)
            Debug.Log(n);
        
        //variables used to track nodes when populating various lookup tables
        Queue<PathfindingManager.PathfindingNode> openList = new Queue<PathfindingManager.PathfindingNode>();
        PathfindingManager.PathfindingNode currentNode;
        List<PathfindingManager.PathfindingNode> neighbors;



        //creates the travelFromToLookup table
        //starting from the StartNode, creates a lookuptable that maps from any node, to all possible moves
        travelFromToLookup = new Dictionary<PathfindingManager.PathfindingNode, List<PathfindingManager.PathfindingNode>>();
        openList.Enqueue(startNode);

        while (openList.Count > 0)
        {
            currentNode = openList.Dequeue();
            neighbors = PathfindingManager.Instance.findAllValidMoves(currentNode);
            travelFromToLookup.Add(currentNode, neighbors);
            foreach (PathfindingManager.PathfindingNode n in neighbors)
                if (!travelFromToLookup.ContainsKey(n) && !openList.Contains(n))
                    openList.Enqueue(n);
        }

        //Because there are mono-directional movements, the we create a travelToFromLookup
        travelToFromLookup = new Dictionary<PathfindingManager.PathfindingNode, List<PathfindingManager.PathfindingNode>>();
        foreach (PathfindingManager.PathfindingNode origin in travelFromToLookup.Keys)
            foreach (PathfindingManager.PathfindingNode destination in travelFromToLookup[origin])
                if (travelToFromLookup.ContainsKey(destination))
                    travelToFromLookup[destination].Add(origin);
                else
                    travelToFromLookup.Add(destination, new List<PathfindingManager.PathfindingNode> { origin });

        //now will create the integration field
        integrationField = new Dictionary<PathfindingManager.PathfindingNode, int>();
        openList = new Queue<PathfindingManager.PathfindingNode>();
        //starts with all possible goalNodes, that can be traveled to
        foreach (PathfindingManager.PathfindingNode node in goalNodes)
        {
            if (travelToFromLookup.ContainsKey(node))
            {
                integrationField.Add(node, 0);
                openList.Enqueue(node);
            }
        }

        //generates an integration list
        while (openList.Count > 0)
        {
            currentNode = openList.Dequeue();
            neighbors = travelToFromLookup[currentNode];
            foreach (PathfindingManager.PathfindingNode n in neighbors)
            {
                //if not in integration field, this is the first time we've checked this node. Add to integrationField
                if (!integrationField.ContainsKey(n))
                {
                    integrationField.Add(n, integrationField[currentNode] + 1);
                    openList.Enqueue(n);
                }
                //if it is in integration field, then check if the cost to reach it is lower than previously calculated
                else if ((integrationField[currentNode] + 1) < integrationField[n])
                {
                    integrationField[n] = integrationField[currentNode] + 1;
                    openList.Enqueue(n);
                }
            }
        }

        //any node that can be traveled to, but is not on the integration field, is a trap node
        trapPositions = new List<PathfindingManager.PathfindingNode>();
        foreach (PathfindingManager.PathfindingNode n in travelToFromLookup.Keys)
            if (!integrationField.ContainsKey(n))
                trapPositions.Add(n);

        isValid = false;

        if (integrationField.Count == 0)
            Debug.LogError("Integration field not properly created");
        else
            isValid = true;
    }

    public PathfindingManager.PathfindingNode getNextStepToGoal (PathfindingManager.PathfindingNode currentNode)
    {
        if (!integrationField.ContainsKey(currentNode))
            Debug.LogError("Not possible to reach goal node from current position");

        List<PathfindingManager.PathfindingNode> neighbors = PathfindingManager.Instance.findAllValidMoves(currentNode);

        //gets the neighbor with the smallest path cost
        PathfindingManager.PathfindingNode nextStepNode = currentNode;
        int nextStepCost= 99999;
        foreach (PathfindingManager.PathfindingNode n in neighbors)
        {
            int nCost;
            if (integrationField.TryGetValue(n, out nCost))
                if (nCost < nextStepCost)
                {
                    nextStepNode = n;
                    nextStepCost = nCost;
                }
        }

        return nextStepNode;
    }

    public List<PathfindingManager.PathfindingNode> getPathToGoal (PathfindingManager.PathfindingNode startNode)
    {
        List<PathfindingManager.PathfindingNode> path = new List<PathfindingManager.PathfindingNode>();
        PathfindingManager.PathfindingNode currentNode = startNode;

        if (!integrationField.ContainsKey(currentNode))
            Debug.LogError("Could not find node in integration field: " + currentNode.gridPos + ", " + currentNode.gravityUp);

        while (integrationField[currentNode] != 0)
        {

            path.Add(currentNode);
            currentNode = getNextStepToGoal(currentNode);
        }
        path.Add(currentNode);
        Debug.Log("Path calculated of distance: " + path.Count);
        return path;       
    }

}
