using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUtils;
using TMPro;
public class StarBlock : MonoBehaviour
{
    public GameObject starPlane;
    public Vector3Int starPlaneDirection = new Vector3Int (0,1,0);
    public bool hasCountdown = false;
    public int countdownStartValue = 10;
    public int countdownValue;
    public bool canPickup = true;

    public GameObject textCanvas;
    public TMP_Text countdownText;

    bool starPickedUp;

    StarPlaneController starPlaneController;
    UIStarController UIStarController;
    PathfindingManager.PathfindingNode pickupNode;

    PlayerController playerScript;
    int starIndex;

    public void Awake()
    {
        //need to update based on saved Data
        canPickup = true;
        UIStarController = FindObjectOfType<UIStarController>();
        starPlaneController = GetComponentInChildren<StarPlaneController>();
        starPlaneController.toggleCanPickupEffects(canPickup);
        GetComponent<Triggerable>().scripts.AddListener(pickupStar);
        playerScript = FindObjectOfType<PlayerController>();

    }

    public void Start()
    {
        starIndex = LevelManager.Instance.getStarBlockIndex(this);
        pickupNode = new PathfindingManager.PathfindingNode(GridManager.Instance.Grid.getCoord(this.gameObject) + starPlaneDirection, starPlaneDirection);

        if (hasCountdown)
        {
            UIStarController.UIUpdateCounter(starIndex, countdownStartValue);
            UIStarController.UIToggleCounter(starIndex, true);
            playerScript.OnMovementComplete += decrementCountdown;
        }


            
    }

    public void decrementCountdown()
    {
        if (!hasCountdown) return;
        if (countdownValue > 0)
        {
            countdownValue--;
            UIStarController.UIUpdateCounter(starIndex, countdownValue);
            countdownText.text = countdownValue.ToString();
        }

        if (countdownValue == 0)
        {
            disableStar(false);
        }

    }

    public void resetCountdown()
    {
        if (!hasCountdown) return;

        if (!starPickedUp)
        {
            canPickup = true;
            starPlaneController.toggleCanPickupEffects(true);
        }

        countdownValue = countdownStartValue;
        UIStarController.UIUpdateCounter(starIndex, countdownStartValue);
    }

    //called from editor
    public void setupStarPrefab()
    {
        if (starPlane != null)
            DestroyImmediate(starPlane);
        starPlane = Instantiate(GameAssets.i.starPlanePrefab, transform.GetChild(0), false);

        //move starPlane to space
        JUtilsClass.getFaceCoordinates(starPlaneDirection, out Vector3 position, out Vector3 rotation);
        starPlane.transform.localPosition = position;
        starPlane.transform.rotation = Quaternion.Euler(rotation);

        //add a Triggerable script if it doesn't exist
        if (GetComponent<Triggerable>() == null)
            this.gameObject.AddComponent<Triggerable>();



        //countdown related code

        textCanvas = starPlane.transform.Find("TextCanvas").gameObject;
        countdownText = textCanvas.transform.GetChild(0).GetComponent<TMP_Text>();

        if (hasCountdown)
        {
            //creates integration field with this star as the goal
            PathfindingManager.PathfindingNode pickupNode = new PathfindingManager.PathfindingNode(GridManager.Instance.Grid.getCoord(this.gameObject) + starPlaneDirection, starPlaneDirection);
            IntegrationField starBlockIntegrationField = new IntegrationField(pickupNode, PathfindingManager.Instance.getCurrentPlayerPathfindingNode());

            countdownStartValue = starBlockIntegrationField.getPathToGoal(PathfindingManager.Instance.getCurrentPlayerPathfindingNode()).Count - 1;
            countdownValue = countdownStartValue;
            countdownText.text = countdownStartValue.ToString();
            textCanvas.SetActive(true);
        }
        else
            textCanvas.SetActive(false);


    }

    //does not run animations
    public void disableStar(bool pickedUp)
    {
        starPickedUp = pickedUp;
        canPickup = false;
        starPlaneController.toggleCanPickupEffects(false);
        if (pickedUp)
        {
            hasCountdown = false;
            countdownText.gameObject.SetActive(false);
        }
    }

    public void pickupStar()
    {
        //if can't pickup star, then exit
        if (!canPickup) return;

        //checks if player is in the correct position
        if (PathfindingManager.Instance.getCurrentPlayerPathfindingNode().Equals(pickupNode))
        {
            int starIndex = LevelManager.Instance.getStarBlockIndex(this);
            ProgressSystem.CurrentProgressData.starsAcquired[LevelManager.Instance.levelIndex, starIndex] = true;
            UIStarController.UILightUpStarEffect(starIndex, this.transform.position);
            starPlaneController.playPickupEffects();
            canPickup = false;
            starPickedUp = true;
        }
    }

}
