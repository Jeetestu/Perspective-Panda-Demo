using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUtils;
/// <summary>
/// Responsible for tracking all grid points, and querying specific blocks
/// </summary>
public class GridManager : MonoBehaviour
{

    private static GridManager instance;

    private Grid grid;

    public Grid Grid { get => grid;}
    public static GridManager Instance {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GridManager>();
                instance.grid = new Grid(FindObjectsOfType<GroundType>());
            }
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
        grid = new Grid(FindObjectsOfType<GroundType>());
    }

    public void refreshGrid()
    {
        instance.grid = new Grid(FindObjectsOfType<GroundType>());
    }


    #region Basic accessors

    public Vector3 getWorldPosition(Vector3Int coord)
    {
        return getWorldPosition((Vector3)coord);
    }

    public Vector3 getWorldPosition(Vector3 coord)
    {
        coord.Scale(ScaleManager.Instance.gridToWorldScale);
        return coord;
    }

    public Vector3 getWorldPosition (GameObject ob)
    {
        return getWorldPosition(Grid.getCoord(ob));
    }

    #endregion

    #region Advanced accessors

    /// <summary>
    /// Gets the block at the given origin and offset
    /// </summary>
    /// <param name="origin">Starting grid position</param>
    /// <param name="dir">The direction of offset (relative to camera angle)</param>
    /// <param name="groundOffset">Will offset by 1 towards the gravity direction (unless it is a top-down view)</param>
    /// <param name="forwardMoveDirection">The forward move direction based on camera angle</param>
    /// <param name="perspectiveVector">Perspective vector to use</param>
    /// <param name="gravityUp">Gravity vector to use</param>
    /// <returns></returns>
    public GameObject getBlockInDirection(Vector3Int origin, Direction dir, bool groundOffset, Vector3Int forwardMoveDirection, Vector3Int perspectiveVector, Vector3Int gravityUp)
    {
        ViewType viewType = JUtilsClass.getView(perspectiveVector, gravityUp);

        Vector3Int groundOffsetVector = Vector3Int.zero;
        //if groundOffset and the viewType is not vertical (top-down), then will decrement offset by gravity to get the ground block
        if (groundOffset && viewType != ViewType.vertical) groundOffsetVector = -gravityUp;

        Vector3Int coord = origin + groundOffsetVector;

        switch (dir)
        {
            case Direction.forward:
                coord += forwardMoveDirection;
                break;
            case Direction.backward:
                coord -= forwardMoveDirection;
                break;
            case Direction.left:
                coord -= Vector3Int.FloorToInt(Vector3.Cross(gravityUp, forwardMoveDirection));
                break;
            case Direction.right:
                coord+= Vector3Int.FloorToInt(Vector3.Cross(gravityUp, forwardMoveDirection));
                break;
            case Direction.none:
                break;
            default:
                Debug.LogError("Invalid move direction provided");
                break;
            
        }

        return getClosestAlongAxis(coord, perspectiveVector, gravityUp);
    }

    /// <summary>
    /// Gets the block at the given origin and offset
    /// </summary>
    /// <param name="origin">Starting grid position</param>
    /// <param name="directionVector">The direction of offset (as a vector)</param>
    /// <param name="groundOffset">Will offset by 1 towards the gravity direction (unless it is a top-down view)</param>
    /// <param name="perspectiveVector">Perspective vector to use</param>
    /// <param name="gravityUp">Gravity vector to use</param>
    /// <returns></returns>

    public GameObject getBlockInDirection(Vector3Int origin, Vector3Int directionVector, bool groundOffset, Vector3Int perspectiveVector, Vector3Int gravityUp)
    {
        ViewType viewType = JUtilsClass.getView(perspectiveVector, gravityUp);

        Vector3Int groundOffsetVector = Vector3Int.zero;
        //if groundOffset and the viewType is not vertical (top-down), then will decrement offset by gravity to get the ground block
        if (groundOffset && viewType != ViewType.vertical) groundOffsetVector = -gravityUp;

        Vector3Int coord = origin + groundOffsetVector;

        coord += directionVector;

        return getClosestAlongAxis(coord, perspectiveVector, gravityUp);
    }

    #endregion

    /// <summary>
    /// Checks all the ground objects and gets the closest along the axis going "direction" through "origin". Ignores flat blocks that aren't oriented correctly
    /// </summary>
    /// <param name="origin">Coordinate to look at</param>
    /// <param name="perspectiveVector">lookDirection (will return the closest valid block in this direction). A value of zero will return the origin</param>
    /// <returns></returns>
    private GameObject getClosestAlongAxis(Vector3Int origin, Vector3Int perspectiveVector, Vector3Int gravityUp)
    {


        //if in 3D space will only keep flat blocks that match gravityUp
        if (perspectiveVector == Vector3Int.zero)
        {
            GameObject block = grid.getBlock(origin);
            if (block == null || block.GetComponent<GroundType>() == null)
                return null;
            else if ((block.GetComponent<GroundType>().typeOfGround == GroundType.Type.flat || block.GetComponent<GroundType>().typeOfGround == GroundType.Type.flatIce) && block.transform.up != gravityUp)
                return null;
            else
                return block;
        }

        List<Vector3Int> coords = new List<Vector3Int>(Grid.ObjectDictionary.Keys);
        coords.RemoveAll(item => isNonalignedFlatBlock(item, perspectiveVector, false));
        if (perspectiveVector.x == 1)
        {
            //remove everything that doesn't match the y and z coordinate of the origin
            coords.RemoveAll(item => !(item.y == origin.y && item.z == origin.z));
            //sort so that the closest can be pulled
            coords.Sort((a, b) => a.x.CompareTo(b.x));
            coords.Reverse();
        }
        else if (perspectiveVector.x == -1)
        {
            coords.RemoveAll(item => !(item.y == origin.y && item.z == origin.z));
            coords.Sort((a, b) => a.x.CompareTo(b.x));
        }
        else if (perspectiveVector.y == 1)
        {
            coords.RemoveAll(item => !(item.x == origin.x && item.z == origin.z));
            coords.Sort((a, b) => a.y.CompareTo(b.y));
            coords.Reverse();
        }
        else if (perspectiveVector.y == -1)
        {
            coords.RemoveAll(item => !(item.x == origin.x && item.z == origin.z));
            coords.Sort((a, b) => a.y.CompareTo(b.y));
        }
        else if (perspectiveVector.z == 1)
        {
            coords.RemoveAll(item => !(item.x == origin.x && item.y == origin.y));
            coords.Sort((a, b) => a.z.CompareTo(b.z));
            coords.Reverse();
        }
        else if (perspectiveVector.z == -1)
        {
            coords.RemoveAll(item => !(item.x == origin.x && item.y == origin.y));
            coords.Sort((a, b) => a.z.CompareTo(b.z));
        }
        GameObject ob = null;
        if (coords.Count>0)
        {
            ob = grid.getBlock(coords[coords.Count - 1]);
        }
        return ob;
    }

    private bool isNonalignedFlatBlock (Vector3Int coord, Vector3Int lookDirection, bool exactDirection)
    {
        GroundType blockGroundType = grid.getBlock(coord).GetComponent<GroundType>();
        //confirm it's a flat block
        if ((blockGroundType.typeOfGround == GroundType.Type.flat || blockGroundType.typeOfGround == GroundType.Type.flatImpassable || blockGroundType.typeOfGround == GroundType.Type.flatIce))
        {
            //if correct alignment return true
            if (!exactDirection)
            {
                if (JUtilsClass.alignedAxis(blockGroundType.transform.up, lookDirection))
                    return false;
            }
            else
                if (blockGroundType.transform.up == lookDirection)
                    return false;

            return true;
        }
        return false;
    }
    
}
