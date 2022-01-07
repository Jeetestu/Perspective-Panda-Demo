using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUtils;

/// <summary>
/// Responsible for checking if specific blocks/perspectives are valid
/// </summary>
public class PathfindingManager : MonoBehaviour
{

    private static PathfindingManager instance;

    private void Awake()
    {
        instance = this;
        playerScript = FindObjectOfType<PlayerController>();
    }
    [Header("Debug Info")]
    public bool showDebugPath = true;
    public bool showTravelLookupCalculations = false;
    public bool showTrapPositions = false;
    public int pathLength;

    private PlayerController playerScript;

    IntegrationField integrationField;

    public static PathfindingManager Instance { 
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PathfindingManager>();
                instance.playerScript = FindObjectOfType<PlayerController>();
            }
            return instance;
        }
    }

    private void Start()
    {
        integrationField = new IntegrationField(GridManager.Instance.Grid.getCoord(FindObjectOfType<FinishZone>().gameObject), new PathfindingNode(playerScript.gridPos, PerspectiveManager.Instance.gravityUp));

    }

    public struct PathfindingNode : IEqualityComparer<PathfindingNode>
    {
        public Vector3Int gridPos, gravityUp;
        public PathfindingNode (Vector3Int posInput, Vector3Int upInput)
        {
            gridPos = posInput;
            gravityUp = upInput;
        }

        bool IEqualityComparer<PathfindingNode>.Equals(PathfindingNode x, PathfindingNode y)
        {
            return x.gravityUp == y.gravityUp && x.gridPos == y.gridPos;
        }

        int IEqualityComparer<PathfindingNode>.GetHashCode(PathfindingNode obj)
        {
            return this.GetHashCode();
        }

        public override string ToString()
        {
            return gridPos + ", " + gravityUp;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Vector3Int nextPos = integrationField.getNextStepToGoal(getCurrentPlayerPathfindingNode()).gridPos;
            Debug.DrawLine(GridManager.Instance.getWorldPosition(playerScript.gridPos), GridManager.Instance.getWorldPosition(nextPos), Color.blue, 2f);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            List<PathfindingNode> path = integrationField.getPathToGoal(getCurrentPlayerPathfindingNode());
            for (int i = 0; i < path.Count - 1; i++)
                Debug.DrawLine(GridManager.Instance.getWorldPosition(path[i].gridPos), GridManager.Instance.getWorldPosition(path[i + 1].gridPos), Color.blue, 5f);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            WarningCube.destroyAllActiveWarningCubes();
            for (int i =0; i < integrationField.TrapPositions.Count; i++)
            {
                WarningCube.createWarningCube(integrationField.TrapPositions[i].gridPos);
            }
        }
    }

    public PathfindingNode getCurrentPlayerPathfindingNode()
    {
        if (Application.isPlaying)
            return new PathfindingNode(playerScript.gridPos, PerspectiveManager.Instance.gravityUp);
        else
            return new PathfindingNode(Vector3Int.FloorToInt(playerScript.transform.position), PerspectiveManager.Instance.gravityUp);
    }


    /// <summary>
    /// Returns a list of all possible move positions given position and perspectiveVector
    /// </summary>
    /// <param name="pos">Current position</param>
    /// <param name="perspectiveVector">The current perspective vector</param>
    /// <returns></returns>
    private List<PathfindingNode> findValidMovesInPerspective(PathfindingNode pos, Vector3Int perspectiveVector)
    {
        ViewType viewType = JUtilsClass.getView(perspectiveVector, pos.gravityUp);
        List<Vector3Int> directionVectorsToCheck;
        List<PathfindingNode> output = new List<PathfindingNode>();

        //if this orientation is invalid, then there are no valid moves
        if (!isValidPositionAndPerspective(perspectiveVector, pos))
            return output;

        //4 directions in 3D or vertical view, but can only move left or right in horizontal view
        directionVectorsToCheck = JUtilsClass.getAllHorizontalDirections(pos.gravityUp);
        if (viewType == ViewType.horizontal)
        {
            directionVectorsToCheck.Remove(perspectiveVector);
            directionVectorsToCheck.Remove(-perspectiveVector);
        }

        GameObject spaceBlock;
        GameObject groundBlock;

        foreach (Vector3Int directionVector in directionVectorsToCheck)
        {
            spaceBlock = GridManager.Instance.getBlockInDirection(pos.gridPos, directionVector, false, perspectiveVector, pos.gravityUp);
            groundBlock = GridManager.Instance.getBlockInDirection(pos.gridPos, directionVector, true, perspectiveVector, pos.gravityUp);


            if (canMoveToBlock(viewType, spaceBlock, groundBlock))
            {
                //the space the player would occupy
                PathfindingNode newPosition = new PathfindingNode(GridManager.Instance.Grid.getCoord(groundBlock) + pos.gravityUp, pos.gravityUp);
                //if stepping onto a rotator, return where the rotator would place you instead of the step the player is moving to
                if (groundBlock.GetComponent<GroundType>().typeOfGround == GroundType.Type.rotator)
                    newPosition = groundBlock.GetComponent<Rotator>().getDestination(newPosition);
                //else if stepping onto ice, return where the player will end up sliding to
                else if (groundBlock.GetComponent<GroundType>().typeOfGround == GroundType.Type.ice || groundBlock.GetComponent<GroundType>().typeOfGround == GroundType.Type.flatIce)
                {
                    //keep searching along this direction until the player can't move, or finds a non-ice ground
                    while (canMoveToBlock(viewType, spaceBlock, groundBlock) && 
                        (groundBlock.GetComponent<GroundType>().typeOfGround == GroundType.Type.ice || groundBlock.GetComponent<GroundType>().typeOfGround == GroundType.Type.flatIce))
                    {
                        newPosition = new PathfindingNode(GridManager.Instance.Grid.getCoord(groundBlock) + pos.gravityUp, pos.gravityUp);
                        spaceBlock = GridManager.Instance.getBlockInDirection(newPosition.gridPos, directionVector, false, perspectiveVector, newPosition.gravityUp);
                        groundBlock = GridManager.Instance.getBlockInDirection(newPosition.gridPos, directionVector, true, perspectiveVector, newPosition.gravityUp);
                    }
                }
                //else return the position you would be stepping on
                output.Add(newPosition);
            }
        }
        return output;
    }

    public List<PathfindingNode> findAllValidMoves(PathfindingNode pos)
    {
        List<PathfindingNode> output = new List<PathfindingNode>();
        List<Vector3Int> perspectiveVectorsToCheck = JUtilsClass.getAllHorizontalDirections(pos.gravityUp);
        perspectiveVectorsToCheck.Add(-pos.gravityUp);
        perspectiveVectorsToCheck.Add(Vector3Int.zero);

        foreach (Vector3Int perspectiveVector in perspectiveVectorsToCheck)
            output.AddRange(findValidMovesInPerspective(pos, perspectiveVector));

        List<PathfindingNode> uniqueOutput = new List<PathfindingNode>();
        //makes output unique
        while (output.Count > 0)
        {
            PathfindingNode p = output[0];
            uniqueOutput.Add(p);
            for (int i = output.Count - 1; i >= 0; i--)
                if (output[i].gridPos == p.gridPos && output[i].gravityUp == p.gravityUp)
                    output.RemoveAt(i);
        }

        return uniqueOutput;


    }

    
    //check if move type is valid
    public bool canMoveToBlock(ViewType viewType, GameObject spaceBlock, GameObject groundBlock)
    {
        //if there is no ground to step on return false
        if (groundBlock == null)
            return false;

        GroundType groundBlockType = groundBlock.GetComponent<GroundType>();

        //if ground is impassable return false
        if (groundBlockType.typeOfGround == GroundType.Type.impassable || groundBlockType.typeOfGround == GroundType.Type.flatImpassable)
            return false;

        //if space is occupied (3D or horizontal movement) return false
        if (viewType != ViewType.vertical && spaceBlock != null)
            return false;

        return true;
    }


    private bool isValidPositionAndPerspective(Vector3Int perspectiveVectorTest, PathfindingNode testPosition)
    {
        return checkValidPositionAndPerspective(perspectiveVectorTest, testPosition.gridPos, testPosition.gravityUp);
    }

    public bool checkValidPositionAndPerspective(Vector3Int perspectiveVectorTest, Vector3Int positionTest, Vector3Int gravityUpTest)
    {
        Vector3Int dummyCoord;
        return checkValidPositionAndPerspective(perspectiveVectorTest, positionTest, gravityUpTest, out dummyCoord);
    }

    //checks if the player is allowed to have a particular orientation if they are at a particular location
    public bool checkValidPositionAndPerspective(Vector3Int perspectiveVectorTest, Vector3Int positionTest, Vector3Int gravityUpTest, out Vector3Int invalidCoord)
    {
        GameObject spaceBlockToCheck;
        GameObject groundBlockToCheck;

        spaceBlockToCheck = GridManager.Instance.getBlockInDirection(positionTest, Direction.none, false, Vector3Int.zero, perspectiveVectorTest, gravityUpTest);
        groundBlockToCheck = GridManager.Instance.getBlockInDirection(positionTest, Direction.none, true, Vector3Int.zero, perspectiveVectorTest, gravityUpTest);

        //if the space block is going to cover the player, return false
        if (perspectiveVectorTest != Vector3Int.zero && spaceBlockToCheck != null && JUtilsClass.closerAlongAxis(GridManager.Instance.Grid.getCoord(spaceBlockToCheck), positionTest, perspectiveVectorTest))
        {
            invalidCoord = positionTest;
            return false;
        }

        //if there is no ground, return false
        if (groundBlockToCheck == null)
        {
            invalidCoord = positionTest - gravityUpTest;
            return false;
        }

        //if ground is impassable, return false
        if (JUtilsClass.isImpassableGround(groundBlockToCheck.GetComponent<GroundType>()))
        {
            invalidCoord = positionTest - gravityUpTest;
            return false;
        }

        invalidCoord = Vector3Int.zero;
        return true;
    }

    #region Level editting support

    List<PathfindingNode> debugPath;
    bool debugPathChanged;

    public void updateDebugPathfinding ()
    {
        GridManager.Instance.refreshGrid();
        integrationField = new IntegrationField(GridManager.Instance.Grid.getCoord(FindObjectOfType<FinishZone>().gameObject), new PathfindingNode(Vector3Int.FloorToInt(FindObjectOfType<PlayerController>().transform.position), PerspectiveManager.Instance.gravityUp));
        List<PathfindingNode> oldPath = debugPath;
        debugPath = integrationField.getPathToGoal(new PathfindingNode(Vector3Int.FloorToInt(FindObjectOfType<PlayerController>().transform.position), PerspectiveManager.Instance.gravityUp));
        pathLength = debugPath.Count;
        int i = 0;
        debugPathChanged = false;
        if (oldPath != null)
            while (i < pathLength && !debugPathChanged)
            {
                if (oldPath[i].gridPos != debugPath[i].gridPos || oldPath.Count != debugPath.Count)
                    debugPathChanged = true;
                i++;
            }

        Debug.Log("Path found of length: " + debugPath.Count + " and " + integrationField.TrapPositions.Count + " trap positions");
    }
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (integrationField != null)
            {
                if (showDebugPath && integrationField.IsValid)
                {
                    //draw path to goal
                    if (debugPathChanged)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.white;

                    for (int i = 0; i < debugPath.Count - 1; i++)
                        Gizmos.DrawLine(GridManager.Instance.getWorldPosition(debugPath[i].gridPos), GridManager.Instance.getWorldPosition(debugPath[i + 1].gridPos));

                }
                if (showTrapPositions && integrationField.IsValid)
                {
                    //draw trap positions
                    Gizmos.color = Color.red;

                    for (int i = 0; i < integrationField.TrapPositions.Count; i++)
                        Gizmos.DrawWireCube(GridManager.Instance.getWorldPosition(integrationField.TrapPositions[i].gridPos), ScaleManager.Instance.gridToWorldScale);
                }
                if (showTravelLookupCalculations)
                {
                    Gizmos.color = Color.green;
                    foreach (PathfindingNode n in integrationField.TravelFromToLookup.Keys)
                        Gizmos.DrawWireCube(GridManager.Instance.getWorldPosition(n.gridPos), ScaleManager.Instance.gridToWorldScale);
                }
            }

        }
        else
        {
            Gizmos.color = Color.white;
            if (playerScript != null)
            {
                foreach (PathfindingNode pos in findAllValidMoves(new PathfindingNode(playerScript.gridPos, PerspectiveManager.Instance.gravityUp)))
                {
                    Gizmos.DrawWireCube(GridManager.Instance.getWorldPosition(pos.gridPos), ScaleManager.Instance.gridToWorldScale);
                }
            }
        }

    }

    #endregion

}
