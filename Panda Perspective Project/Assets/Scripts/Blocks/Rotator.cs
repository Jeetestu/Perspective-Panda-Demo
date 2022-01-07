using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public enum RotateDistance { quarterRotation, halfRotation }
    public enum RotateDirection { clockwise, counterClockwise }

    //relative position of the player to the rotator
    private enum RelativePositioning { up, down, left, right }


    //angle to rotate
    public RotateDistance rotateDistance = RotateDistance.quarterRotation;
    //direction to rotate
    public RotateDirection rotateDirection = RotateDirection.clockwise;
    //time (in seconds) to rotate
    public float rotateDuration = 3;

    //direction to rotate
    private Vector3 rotationAxis;
    private PlayerController playerScript;
    private Transform cam;
    private bool rotating;
    //The relative grid position of where the player will be standing on the rotator block for the first rotation
    private Vector3Int rotatePointOne;
    //The relative grid position of where the player will be standing on the rotator block for the second rotation
    private Vector3Int rotatePointTwo;
    private float angularVelocity;

    [HideInInspector]
    public Vector3Int displacementOne;
    [HideInInspector]
    public Vector3Int displacementTwo;

    private void Awake()
    {
        if (rotateDirection == RotateDirection.clockwise)
        {
            rotationAxis = -transform.forward;
        }
        else
        {
            rotationAxis = transform.forward;
        }

        //The first rotation point is always directly 'relative' up on the rotator block
        rotatePointOne = Vector3Int.RoundToInt(transform.up);

        //calculate angular velocity
        if (rotateDistance == RotateDistance.halfRotation)
            angularVelocity = 180f / rotateDuration;
        else if (rotateDirection == RotateDirection.clockwise)
            angularVelocity = 90f / rotateDuration;
        else
            angularVelocity = 90f / rotateDuration;

        rotatePointTwo = calculateRotatePointTwo();
        playerScript = FindObjectOfType<PlayerController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        this.GetComponent<Triggerable>().scripts.AddListener(startRotation);
    }

    //broken out into a seperate function so it can be called from editor (by pathfinding manager)
    public Vector3Int calculateRotatePointTwo()
    {
        if (rotateDistance == RotateDistance.halfRotation)
            return Vector3Int.RoundToInt(-transform.up);
        else if (rotateDirection == RotateDirection.clockwise)
            return Vector3Int.RoundToInt(transform.right);
        else
            return Vector3Int.RoundToInt(-transform.right);
    }

    void startRotation()
    {
        if (!rotating)
        {
            playerScript.instantlyFinishMoving();
            playerScript.AllowMovementInput = false;
            Vector3 stepPosition = playerScript.gridPos - GridManager.Instance.Grid.getCoord(this.gameObject);
            if (stepPosition == rotatePointOne)
            {
                //Debug.Log("Rotate Point 1");
                StartCoroutine(rotate(rotationAxis, rotatePointOne, rotatePointTwo));
            }
            else if (stepPosition == rotatePointTwo)
            {
                //Debug.Log("Rotate Point 2");
                StartCoroutine(rotate(-rotationAxis, rotatePointTwo, rotatePointOne));
            }

        }
        
    }
    /// <summary>
    /// Rotates from the start position to the end position 
    /// </summary>
    /// <param name="dir">The axis of rotation</param>
    /// <param name="startOrientation">The relative position to start rotation from (Would be a vector of magnitude one)</param>
    /// <param name="endOrientation">The relative position to end the rotation (Would be a vector of magnitude one). Also corresponds to the new gravityUp</param>
    /// <returns></returns>
    IEnumerator rotate (Vector3 dir, Vector3Int startOrientation, Vector3Int endOrientation)
    {
        Quaternion originalRotation = transform.rotation;
        playerScript.isRotatingOnRotatorBlock = true;
        rotating = true;


        float t = 0;

        Vector3 relativePosition = transform.InverseTransformPoint(GridManager.Instance.Grid.getCoord(this.gameObject) + startOrientation);

        while (t < rotateDuration)
        {
            t += Time.deltaTime;
            transform.RotateAround(transform.position, dir, angularVelocity * Time.deltaTime);
            playerScript.transform.RotateAround(transform.position, dir, angularVelocity * Time.deltaTime);
            cam.parent.parent.transform.Rotate(dir, angularVelocity * Time.deltaTime);
            // Debug.Log(gridManager.getWorldPosition(GridManager.instance.Grid.getCoord(this.gameObject)) + " + " + gridManager.getWorldPosition(transform.TransformDirection(relativePosition).normalized));
            playerScript.transform.position = GridManager.Instance.getWorldPosition(GridManager.Instance.Grid.getCoord(this.gameObject)) + GridManager.Instance.getWorldPosition(transform.TransformDirection(relativePosition).normalized);

            //Debug.Log(playerScript.transform.position);
            yield return null;
        }
        playerScript.isRotatingOnRotatorBlock = false;
        rotating = false;
        PerspectiveManager.Instance.gravityUp = endOrientation;
        playerScript.gridPos = GridManager.Instance.Grid.getCoord(this.gameObject) + PerspectiveManager.Instance.gravityUp;
        this.transform.rotation = originalRotation;
        playerScript.AllowMovementInput = true;
        playerScript.TargetWorldPos = playerScript.transform.position;
        PerspectiveManager.Instance.updatePerspectiveVector();
    }

    public PathfindingManager.PathfindingNode getDestination(PathfindingManager.PathfindingNode startPos)
    {
        Vector3Int rotatePointOne = Vector3Int.RoundToInt(transform.up);
        Vector3Int rotatePointTwo = calculateRotatePointTwo();
        Vector3Int startOrientation = startPos.gridPos - GridManager.Instance.Grid.getCoord(this.gameObject);
        //if the startPos is onto the first rotationPoint, then return the second rotation point, and vice-versa
        if (startOrientation == rotatePointOne) return new PathfindingManager.PathfindingNode(rotatePointTwo + GridManager.Instance.Grid.getCoord(this.gameObject), rotatePointTwo);
        if (startOrientation == rotatePointTwo) return new PathfindingManager.PathfindingNode(rotatePointOne + GridManager.Instance.Grid.getCoord(this.gameObject), rotatePointOne);
        //otherwise there is no rotation ocurring
        return startPos;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(GridManager.Instance.getWorldPosition(GridManager.Instance.Grid.getCoord(this.gameObject) + rotatePointOne), GridManager.Instance.getWorldPosition(GridManager.Instance.Grid.getCoord(this.gameObject) + rotatePointTwo));
    }

}
