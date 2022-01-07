using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private Dictionary<Vector3Int, GameObject> objectDictionary;
    private Dictionary<GameObject, Vector3Int> reversedObjectDictionary;

    private Vector3Int minGridBounds, maxGridBounds;

    public Dictionary<Vector3Int, GameObject> ObjectDictionary { get => objectDictionary; }
    public Vector3Int MinGridBounds { get => minGridBounds; }
    public Vector3Int MaxGridBounds { get => maxGridBounds; }

    public Grid(GroundType[] groundBlocks)
    {
        objectDictionary = new Dictionary<Vector3Int, GameObject>();
        reversedObjectDictionary = new Dictionary<GameObject, Vector3Int>();
        minGridBounds = new Vector3Int(999, 999, 999);
        maxGridBounds = new Vector3Int(-999, -999, -999);
        foreach (GroundType block in groundBlocks)
        {
            addBlock(block.gameObject);
        }
    }

    public GameObject getBlock(Vector3Int coord)
    {
        GameObject ob;
        ObjectDictionary.TryGetValue(coord, out ob);
        return ob;
    }

    public Vector3Int getCoord(GameObject ob)
    {
        Vector3Int coord;
        if (!reversedObjectDictionary.TryGetValue(ob, out coord))
            Debug.LogWarning("Could not find block: " + ob.name);
        return coord;
    }

    public void addBlock(GameObject block)
    {
        Vector3Int gridPos = Vector3Int.RoundToInt(block.transform.position);
        if (ObjectDictionary.ContainsKey(gridPos))
            Debug.LogWarning("Duplicate block found at position: " + gridPos);
        else
        {
            ObjectDictionary.Add(gridPos, block.gameObject);
            reversedObjectDictionary.Add(block.gameObject, gridPos);

            //updates the grid bounds if relevant (if any component of the given vector exceeds the min/max bounds, will update the min/max bounds)
            minGridBounds = Vector3Int.Min(gridPos, MinGridBounds);
            maxGridBounds = Vector3Int.Max(gridPos, maxGridBounds);
        }
    }

    public void removeBlock(GameObject block)
    {
        Vector3Int gridPos = getCoord(block);
        ObjectDictionary.Remove(gridPos);
        reversedObjectDictionary.Remove(block);
    }

    /// <summary>
    /// Call if object has moved positions. Will update the Grid lookup reference for the object
    /// </summary>
    /// <param name="ob"></param>
    public void updateBlockPos(GameObject ob)
    {
        removeBlock(ob);
        addBlock(ob);
    }

    

}
