using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUtils;
public class LevelStartAnimation : MonoBehaviour
{

    public enum SpawnType { none, pop}

    public SpawnType spawnAnimation = SpawnType.none;
    [Space]
    public float animateSpeed = 1.5f;
    public float animateSequenceDelay = 0.8f;

    private bool[,,] startedAnimating;
    //private int gridSize = 100;
    //private int gridOffset;
    private Vector3Int gridSize;
    private Vector3Int gridOffset;

    private Dictionary<Vector3Int, bool> animationSignalSent;
    private List<Vector3Int> validGridPositions;

    public delegate void IntroAnimationComplete();
    public event IntroAnimationComplete OnIntroAnimationComplete;

    private void Awake()
    {
        //sets all ground blocks to a scale of zero (invisible) except the finish zone
        if (spawnAnimation != SpawnType.none)
            foreach (GroundType gt in FindObjectsOfType<GroundType>())
                if (gt.typeOfGround != GroundType.Type.finishZone)
                    gt.gameObject.transform.localScale = Vector3.zero;



    }

    private void Start()
    {
        //sets up the reference grid to indicate if a cell has been set to start animating
        animationSignalSent = new Dictionary<Vector3Int, bool>();
        Vector3Int minGridBounds = GridManager.Instance.Grid.MinGridBounds;
        Vector3Int maxGridBounds = GridManager.Instance.Grid.MaxGridBounds;
        for (int x = minGridBounds.x; x <= maxGridBounds.x; x++)
            for (int y = minGridBounds.y; y <= maxGridBounds.y; y++)
                for (int z = minGridBounds.z; z <= maxGridBounds.z; z++)
                    animationSignalSent.Add(new Vector3Int(x, y, z), false);

        validGridPositions = new List<Vector3Int>();
        validGridPositions.AddRange(animationSignalSent.Keys);
    }

    public void playLevelStartAnimations(Vector3Int startGridPos)
    {
        if (spawnAnimation != SpawnType.none)
            StartCoroutine(animController(startGridPos));
    }

    IEnumerator animController(Vector3Int startGridPos)
    {
        HashSet<Vector3Int> animateThisCycle = new HashSet<Vector3Int> { startGridPos };
        
        //Every 'cycle' will start animating all adjacent cells, next to the cells that were animated in the previous cycle.
        //starts the first cycle at startGridPos
        while (animateThisCycle.Count != 0)
        {
            animateThisCycle = animateSetOfBlocks(animateThisCycle);
            yield return new WaitForSeconds(animateSequenceDelay);
        }

        OnIntroAnimationComplete?.Invoke();
    }

    //starts animating all cells in the array that have not already begun animating
    //returns a Hashset (unique list) of all neighbors of the given list of cells
    private HashSet<Vector3Int> animateSetOfBlocks(HashSet<Vector3Int> GridPositions)
    {
        List<GameObject> animatedBlocksThisCycle = new List<GameObject>();
        HashSet<Vector3Int> GridPositionsForNextCycle = new HashSet<Vector3Int>();
        foreach (Vector3Int gridPosToAnimate in GridPositions)
        {
            //if this cell has not already had an animation signal sent
            //if (!animationSignalSent[gridPosToAnimate])
            animationSignalSent[gridPosToAnimate] = true;

            //get all the adjacent cells and adds to the list for the next cycle
            foreach (Vector3Int adj in JUtilsClass.getDirections())
            {
                Vector3Int newGridPos = adj + gridPosToAnimate;
                //only adds to the list if it is a valid coordinate that has not received an animation signal already
                //Because the GridPositionsForNextCycle is a hashset, all values are unique
                bool signalSent;
                bool isInGrid = animationSignalSent.TryGetValue(newGridPos, out signalSent);
                if (!signalSent && isInGrid)
                    GridPositionsForNextCycle.Add(newGridPos);
            }

            //checks if there is a block at target coordinate
            GameObject block = GridManager.Instance.Grid.getBlock(gridPosToAnimate);
            if (block != null && block.GetComponent<GroundType>().typeOfGround != GroundType.Type.finishZone)
            {
                animatedBlocksThisCycle.Add(block);
            }
        }

        //starts a coroutine to handle animation for this set of blocks
        if (animatedBlocksThisCycle.Count > 0)
        {
            if (spawnAnimation == SpawnType.pop) StartCoroutine(pop(animatedBlocksThisCycle.ToArray()));
        }

        return GridPositionsForNextCycle;
    }

    IEnumerator pop(GameObject[] blocks)
    {
        float currentScale = 0f;
        float t = 0f;
        while (currentScale != 1f)
        {
            t = t + Time.deltaTime;
            currentScale = JUtilsClass.Spring(0f, 1f, t * animateSpeed);
            foreach (GameObject block in blocks)
            {
                block.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            }
            yield return null;
        }
    }


}
