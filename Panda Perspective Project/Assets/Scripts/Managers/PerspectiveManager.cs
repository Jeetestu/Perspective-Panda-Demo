using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUtils;
/// <summary>
/// Responsible for tracking the current look direction, and associated movements (i.e. which way is up, forward and what movement type is the player in)
/// </summary>
public class PerspectiveManager : MonoBehaviour
{
    private static PerspectiveManager instance;

    //When this vector becomes 0,0,0 represents an isometric view. Otherwise it represents the forward view of the camera
    public Vector3Int perspectiveVector;
    public ViewType currentView;
    public Vector3Int perspectiveForwardMovePosition;
    public Vector3Int gravityUp;

    private PlayerController playerScript;
    private GameObject warningCube;

    public static PerspectiveManager Instance { 
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PerspectiveManager>();
                instance.gravityUp = Vector3Int.up;
            }
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
        gravityUp = Vector3Int.up;
        perspectiveForwardMovePosition = Vector3Int.RoundToInt(Vector3.forward);
        playerScript = FindObjectOfType<PlayerController>();
    }

    //refreshes the perspective vector if the camera has rotated, in case a new form of movement is now needed
    public void updatePerspectiveVector()
    {

        #region gets the new perspective vector based on camera look direction
        //default perspective vector is zero (represents standard 3D space)
        Vector3Int newPerspectiveVector = Vector3Int.zero;

        //to avoid floating point errors, rounds to nearest 50th decimal
        Vector3 roundedForward = JUtilsClass.roundVector(CameraManager.instance.transform.forward, 50);
        //if the current forward looking vector is at a right angle (Looking down, or horizontally)
        if (roundedForward == -PerspectiveManager.Instance.gravityUp || roundedForward == JUtilsClass.getClosestHorizontalDirection(roundedForward, PerspectiveManager.Instance.gravityUp))
        {
            //gets the new perspective vector
            newPerspectiveVector = Vector3Int.RoundToInt(Vector3.Normalize(roundedForward));
        }

        perspectiveVector = newPerspectiveVector;

        #endregion

        currentView = JUtilsClass.getView(perspectiveVector, gravityUp);

        #region Adjusts perspective up and forward directions

        if (currentView == ViewType.normal)
        {
            perspectiveForwardMovePosition = JUtilsClass.getClosestHorizontalDirection(CameraManager.instance.transform.forward, gravityUp);
        }
        else if (currentView == ViewType.vertical)
        {
            perspectiveForwardMovePosition = JUtilsClass.getClosestHorizontalDirection(CameraManager.instance.transform.up, gravityUp);
        }
        else
        {
            perspectiveForwardMovePosition = perspectiveVector;
        }

        #endregion

        #region check if movement is valid from current position

        //if new angle is valid
        Vector3Int invalidCoord;
        if (PathfindingManager.Instance.checkValidPositionAndPerspective(newPerspectiveVector, playerScript.gridPos, gravityUp, out invalidCoord))
        {
            Time.timeScale = 1;
            playerScript.isInvalidPositionAndPerspective = false;
            if (warningCube != null)
            {
                WarningCube.destroyWarningCube(warningCube);
                warningCube = null;
            }
        }
        //current perspective is invalid
        else
        {
            //this adjustment to the invalid coordinate ensures the warningCube is drawn at the forefront of all cubes
            invalidCoord = invalidCoord - (newPerspectiveVector * 1000);
            if (warningCube != null)
            {
                warningCube.transform.position = GridManager.Instance.getWorldPosition(invalidCoord);
            }
            else if (warningCube == null)
            {
                warningCube = WarningCube.createWarningCube(invalidCoord);
            }
            if (CameraManager.instance.IsSnapping)
                Time.timeScale = 1;
            else
                Time.timeScale = 0.1f;
            playerScript.isInvalidPositionAndPerspective = true;
        }
        #endregion

    }


}
