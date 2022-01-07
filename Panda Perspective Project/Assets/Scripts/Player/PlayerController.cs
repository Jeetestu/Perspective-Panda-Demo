using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUtils;

public class PlayerController : MonoBehaviour
{
    public GameObject playerPrefab;
    public Vector3Int gridPos;
    public int steps;

    public Vector3Int startGridPos;
    public Quaternion startRotation;

    public float moveSpeed = 2f;

    [HideInInspector]
    public bool isRotatingOnRotatorBlock;

    [HideInInspector]
    public bool isInvalidPositionAndPerspective;

    [HideInInspector]
    public GameObject playerAvatar;
    private Animator playerAnimator;


    private Vector3 targetWorldPos;
    private bool isWalking;
    private bool allowMovementInput;
    private GameObject targetGroundBlock;
    private bool hasRunNearDestinationFunction = false;
    private Vector3Int moveDirection;

    private Triggerable[] triggerableObjects;

    public delegate void MovementComplete();
    public event MovementComplete OnMovementComplete;

    public bool IsMoving { get => isWalking; }
    public bool AllowMovementInput { get => allowMovementInput; set => allowMovementInput = value; }
    public Vector3 TargetWorldPos { get => targetWorldPos; set => targetWorldPos = value; }

    private void Awake()
    {
        isRotatingOnRotatorBlock = false;
        isInvalidPositionAndPerspective = false;
        this.GetComponent<MeshRenderer>().enabled = false;
        steps = 0;
        triggerableObjects = GameObject.FindObjectsOfType<Triggerable>();
        playerAvatar = GameObject.Instantiate(playerPrefab, transform.position - transform.up/2, transform.rotation, transform);
        playerAnimator = playerAvatar.GetComponent<Animator>();
        gridPos = Vector3Int.FloorToInt(transform.position);
        targetWorldPos = transform.position;
        hasRunNearDestinationFunction = true;

        startGridPos = gridPos;
        startRotation = transform.rotation;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            stun();
        }

        //if at targetWorldPosition, stop moving
        if (transform.position == targetWorldPos /*|| JUtilsClass.isAlongAxis(transform.position, PerspectiveManager.Instance.perspectiveVector, targetWorldPos)*/)
        {
            isWalking = false;
            playerAnimator.SetBool("Walking", false);
            transform.position = targetWorldPos;
        }

        //if in the process of movement
        if (isWalking)
        {
            //move towards the targetWorldPosition
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
            //look at the targetWorldPosition

            transform.LookAt(targetWorldPos, PerspectiveManager.Instance.gravityUp);
        }

        //when getting close to the target, allow movementInput, and activate triggers
        //essentially does a lot of the things that should trigger 'on' the block, slightly before arriving (to prevent stuttering)
        if (Vector3.Distance(transform.position, targetWorldPos) < 0.1f && !hasRunNearDestinationFunction)
        {
            //activate triggers
            hasRunNearDestinationFunction = true;
            allowMovementInput = true;
            activateTriggers(targetGroundBlock);
            steps++;
            OnMovementComplete?.Invoke();
            if (JUtilsClass.isIceBlock(targetGroundBlock))
                attemptSlide(moveDirection);
        }

        if (allowMovementInput)
            movementInputHandler();


    }

    //used when being moved by a triggered block (e.g. rotator)
    public void instantlyFinishMoving()
    {
        transform.position = targetWorldPos;
        isWalking = false;
        playerAnimator.SetBool("Walking", false);
    }

    private void movementInputHandler()
    {
        Direction moveDirection = Direction.none;
        if ((Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow)))
            moveDirection = Direction.forward;
        else if ((Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow)))
            moveDirection = Direction.backward;
        else if ((Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow)))
            moveDirection = Direction.right;
        else if ((Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow)))
            moveDirection = Direction.left;

        if (moveDirection != Direction.none)
        {
            attemptMovement(moveDirection);
        }
    }

    private void attemptMovement(Direction moveDirection)
    {
        GameObject spaceBlock = GridManager.Instance.getBlockInDirection(gridPos, moveDirection, false,
            PerspectiveManager.Instance.perspectiveForwardMovePosition,
            PerspectiveManager.Instance.perspectiveVector,
            PerspectiveManager.Instance.gravityUp);
        GameObject groundBlock = GridManager.Instance.getBlockInDirection(gridPos, moveDirection, true,
            PerspectiveManager.Instance.perspectiveForwardMovePosition,
            PerspectiveManager.Instance.perspectiveVector,
            PerspectiveManager.Instance.gravityUp);

        if (PathfindingManager.Instance.checkValidPositionAndPerspective(PerspectiveManager.Instance.perspectiveVector, gridPos, PerspectiveManager.Instance.gravityUp) &&
            PathfindingManager.Instance.canMoveToBlock(PerspectiveManager.Instance.currentView, spaceBlock, groundBlock))
        {
            //move(groundBlock);
            startMovement(GridManager.Instance.Grid.getCoord(groundBlock) + PerspectiveManager.Instance.gravityUp);

        }
    }

    private void startMovement(Vector3Int targetGridPos, bool walkingAnimation = true)
    {


        moveDirection = targetGridPos - gridPos;
        gridPos = targetGridPos;
        targetWorldPos = GridManager.Instance.getWorldPosition(gridPos);
        targetGroundBlock = GridManager.Instance.Grid.getBlock(gridPos - PerspectiveManager.Instance.gravityUp);
        playerAnimator.SetBool("Walking", walkingAnimation);
        isWalking = true;
        allowMovementInput = false;
        hasRunNearDestinationFunction = false;

        //instantly teleports along the perspective vector to prevent clipping
        transform.position = transform.position + (Vector3.Scale(targetWorldPos, JUtilsClass.vecAbs(PerspectiveManager.Instance.perspectiveVector)));
        //transform.position = JUtilsClass.closerAlongAxis(transform.position, targetWorldPos, PerspectiveManager.Instance.perspectiveVector) ? transform.position : transform.position + (Vector3.Scale(targetWorldPos, JUtilsClass.vecAbs(PerspectiveManager.Instance.perspectiveVector)));
    }

    private void attemptSlide(Vector3Int slideDirection)
    {
        GameObject spaceBlock = GridManager.Instance.getBlockInDirection(gridPos, slideDirection, false,
            PerspectiveManager.Instance.perspectiveVector,
            PerspectiveManager.Instance.gravityUp);
        GameObject groundBlock = GridManager.Instance.getBlockInDirection(gridPos, slideDirection, true,
            PerspectiveManager.Instance.perspectiveVector,
            PerspectiveManager.Instance.gravityUp);

        if (PathfindingManager.Instance.canMoveToBlock(PerspectiveManager.Instance.currentView, spaceBlock, groundBlock)
            && (groundBlock.GetComponent<GroundType>().typeOfGround == GroundType.Type.flatIce || groundBlock.GetComponent<GroundType>().typeOfGround == GroundType.Type.ice))
        {
            startMovement(GridManager.Instance.Grid.getCoord(groundBlock) + PerspectiveManager.Instance.gravityUp, false);
        }
    }

    public void stun()
    {
        StartCoroutine(stunned());
    }

    IEnumerator stunned()
    {
        //float animLength;
        allowMovementInput = false;
        playerAnimator.SetTrigger("Hit");
        //gets length of Hit animation
        //animLength = playerAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        yield return new WaitForSeconds(playerAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        allowMovementInput = true;
    }

    private void activateTriggers(GameObject block)
    {
        foreach (Triggerable current in triggerableObjects)
        {
            if (block == current.gameObject)
            {
                current.activate();
            }
        }
    }

    private bool isUpsideDownFlatWalkableBlock(GameObject block)
    {
        return block != null && 
            (block.GetComponent<GroundType>().typeOfGround == GroundType.Type.flatIce || block.GetComponent<GroundType>().typeOfGround == GroundType.Type.flat)
            && block.transform.up == -PerspectiveManager.Instance.gravityUp;
    }

}
