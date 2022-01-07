using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUtils;
public class CameraManager : MonoBehaviour
{

    public static CameraManager instance;

    [Header("Target follow parameters")]
    private Transform target;
    public float speed = 2f;
    private Vector3 nextPosition;
    private Transform cameraParent;

    [Header("Rotation parameters")]
    public float speedH = 2000.0f;
    public float speedV = 2000.0f;
    

    [Header("Snapping parameters")]
    //If the camera is within this amount from a 90 degree angle, it will snap to the exact angle
    public float snapAnglePitch = 5f;
    public float snapAngleYaw = 10f;

    //the angular speed to snap (degrees per frame)
    public float snapSpeed = 10f;

    private Transform camRotTransform;
    private PlayerController playerScript;

    [Header("Current rotation parameters")]
    [Tooltip("Yaw represents the rotation horizontally around the player")]
    public float yaw = 0.0f;
    [Tooltip("Pitch represents the angle along the vertical direction (e.g. all horizontal vectors will have a pitch of 0, looking down will have a pitch of 90)")]
    public float pitch = 0.0f;

    public bool canRotateCamera = true;
    [Tooltip("Camera Zoom Variables")]
    public float distanceFromPlayer = 4f;
    public float minDistanceFromPlayer = 4f;
    public float maxDistanceFromPlayer = 25f;
    public float zoomSensitivity = 1f;

    private bool isSnapping = false;
    private bool canSnap = true;

    public bool IsSnapping { get => isSnapping; }

    private void Awake()
    {
        instance = this;
        cameraParent = this.transform.parent;
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        this.transform.localPosition = new Vector3(0, 0, -distanceFromPlayer);

        camRotTransform = GameObject.FindGameObjectWithTag("CameraRotator").transform;
        this.GetComponent<Camera>().orthographicSize = distanceFromPlayer;
    }

    public void setTarget(Transform target, bool instantlyFocus = false)
    {
        this.target = target;
        if (instantlyFocus)
            cameraParent.position = target.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
            followTargetHandler();
        bool camRotated = false;
        camRotated = rotateCameraHandler();

        zoomCameraHandler();

        camRotated = snapCameraHandler(snapAngleYaw, snapAnglePitch) || camRotated;

        if (camRotated || playerScript.isRotatingOnRotatorBlock)
        {
            //applies the camera rotation as a lerp
            camRotTransform.localEulerAngles = Vector3.Slerp(camRotTransform.localEulerAngles, new Vector3(pitch, yaw, 0.0f), 5f);
            //camRotTransform.localEulerAngles = new Vector3(pitch, yaw, 0.0f);

        }

        PerspectiveManager.Instance.updatePerspectiveVector();
        ScaleManager.Instance.updateScaling();
    }

    private void followTargetHandler()
    {
        nextPosition = cameraParent.position;
        float interpolation = speed * Time.deltaTime;
        if (PerspectiveManager.Instance.perspectiveVector.x == 0)
        {
            nextPosition.x = Mathf.Lerp(cameraParent.position.x, target.position.x, interpolation);
        }
        if (PerspectiveManager.Instance.perspectiveVector.y == 0)
        {
            nextPosition.y = Mathf.Lerp(cameraParent.position.y, target.position.y, interpolation);
        }
        if (PerspectiveManager.Instance.perspectiveVector.z == 0)
        {
            nextPosition.z = Mathf.Lerp(cameraParent.position.z, target.position.z, interpolation);
        }
        cameraParent.position = nextPosition;
    }

    //returns true if camera was moved
    //only runs if the right mouse button was held, canRotateCamera is enabled and the camera is not currently snapping
    private bool rotateCameraHandler()
    {
        if (Input.GetMouseButton(1) && !playerScript.IsMoving && !playerScript.isRotatingOnRotatorBlock && canRotateCamera && !IsSnapping)
        {
            //determines new rotation
            float h = speedH * Input.GetAxis("Mouse X") * Time.deltaTime;
            float v = speedV * Input.GetAxis("Mouse Y") * Time.deltaTime;
            yaw += h;
            pitch -= v;
            pitch = Mathf.Clamp(pitch, -2, 92);
            return true;
        }
        return false;
    }

    private void zoomCameraHandler()
    {
        Vector2 scrollDelta = Input.mouseScrollDelta;
        if (scrollDelta.y != 0 && !playerScript.IsMoving && !playerScript.isRotatingOnRotatorBlock && canRotateCamera && !IsSnapping)
        {
            distanceFromPlayer -= (scrollDelta.y * zoomSensitivity);
            distanceFromPlayer = Mathf.Clamp(distanceFromPlayer, minDistanceFromPlayer, maxDistanceFromPlayer);
            this.transform.localPosition = new Vector3(0, 0, -distanceFromPlayer);
        }
    }

    //returns true if camera was moved
    //snaps the camera to the nearest 90 degree angle (if close within the snapAngleYaw and snapAnglePitch)
    private bool snapCameraHandler(float snapAngleYaw, float snapAnglePitch)
    {
        //rounds yaw to the nearest 90 degrees
        float nearest90Yaw = Mathf.Round(yaw / 90f) * 90f;
        //rounds pitch to the nearest 90 degrees
        float nearest90Pitch = Mathf.Round(pitch / 90f) * 90f;
        //gets the distance (in degrees) from current yaw to nearest yaw
        float distanceTo90Yaw = Mathf.Abs(yaw - nearest90Yaw);
        //gets the distance (in degrees) from current pitch to nearest pitch)
        float distanceTo90Pitch = Mathf.Abs(pitch - nearest90Pitch);

        //Debug.Log("Pitch: " + pitch + ", Yaw: " + yaw + ", distanceToPitch: " + distanceTo90Pitch + ", distanceToYaw: " + distanceTo90Yaw + ", Snapping: " + snapping + ", CanSnap: " + canSnap + ", canMove: " + playerScript.canMove + ", playerRotatingCanMoveOverride: " + playerScript.playerRotatingCanMoveOverride);

        //if already at right angle, then disable snapping (so that player can break free)
        if (distanceTo90Yaw == 0f && distanceTo90Pitch == 0f || pitch == 90f)
        {
            canSnap = false;
            if (!Input.GetMouseButton(1))
                isSnapping = false;
        }
        //if pitch AND yaw within snapping distance OR pitch within snapping distance (i.e. looking vertical or horizontal)
        if (canSnap && (distanceTo90Yaw < snapAngleYaw && distanceTo90Pitch < snapAnglePitch || Mathf.Abs(pitch - 90f) < snapAnglePitch))
        {
            isSnapping = true;
        }
        //Snap to nearest 90 degrees, for pitch and yaw that are close to 90 degrees
        if (IsSnapping && !playerScript.IsMoving && !playerScript.isRotatingOnRotatorBlock)
        {
            //move towards nearest 90 degree yaw
            if (distanceTo90Yaw < snapAngleYaw)
                yaw = Mathf.MoveTowards(yaw, nearest90Yaw, snapSpeed * Time.deltaTime);

            //move towards nearest 90 degree pitch
            if (distanceTo90Pitch < snapAnglePitch)
                pitch = Mathf.MoveTowards(pitch, nearest90Pitch, snapSpeed * Time.deltaTime);
            return true;
        }
        //otherwise, re-enable snapping when leaving a horizontal or vertical view
        else if (distanceTo90Pitch > snapAnglePitch || distanceTo90Yaw > snapAngleYaw && !(Mathf.Abs(pitch - 90f) < snapAnglePitch))
        {
            canSnap = true;
        }

        return false;


    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Camera.main.transform.parent.localEulerAngles = new Vector3(pitch, yaw, 0.0f);
            Camera.main.transform.parent.position = GameObject.FindGameObjectWithTag("Player").transform.position;
            this.transform.localPosition = new Vector3(0, 0, -distanceFromPlayer);
            this.GetComponent<Camera>().orthographicSize = distanceFromPlayer;
        }

    }


}

