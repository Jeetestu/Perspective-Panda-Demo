using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private StarBlock[] starBlocks;
    private PlayerController playerScript;
    private LevelStartAnimation levelStartAnimation;
    private UIStarController UIStarController;
    public int levelIndex;


    public StarBlock[] StarBlocks { get => starBlocks;}

    private void Awake()
    {
        Instance = this;
        starBlocks = getOrderedArrayOfStarBlocks();
        levelStartAnimation = GetComponent<LevelStartAnimation>();
        playerScript = FindObjectOfType<PlayerController>();
        levelIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void Start()
    {
        UIStarController = FindObjectOfType<UIStarController>();
        playerScript.gameObject.SetActive(false);
        CameraManager.instance.setTarget(FindObjectOfType<FinishZone>().transform, true);
        Transitioner.Instance.OnTransitionComplete += startLevelLoadAnimation;
        Transitioner.Instance.OnTransitionComplete += setCanvasToScreenOverlay;
        setupStarsFromSavedData();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            restartLevel();
    }

    //restarts the level without reloading the scene
    //essentially teleporting the player back to their spawn
    //TODO
    //reset gravity and player orientation
    public void restartLevel()
    {
        //saves data
        ProgressSystem.saveProgressData();
        setupStarsFromSavedData();
        MoveToPoint cameraFollower = GameObject.Instantiate(GameAssets.i.cameraFollower, playerScript.transform.position, Quaternion.identity).GetComponent<MoveToPoint>();
        CameraManager.instance.canRotateCamera = false;
        CameraManager.instance.setTarget(cameraFollower.transform);
        playerScript.GetComponentInChildren<Animator>().SetTrigger("Despawn");
        playerScript.AllowMovementInput = false;
        cameraFollower.startMovement(playerScript.transform.position, GridManager.Instance.getWorldPosition(playerScript.startGridPos), false, true, 3f);

        foreach (StarBlock star in starBlocks)
            star.resetCountdown();

        //when the camera follower is done moving, the camera is reset to follow the player, and the player is teleported and spawned back in
        cameraFollower.OnMovementComplete += () => CameraManager.instance.setTarget(playerScript.transform);
        cameraFollower.OnMovementComplete += () => CameraManager.instance.canRotateCamera = true;
        cameraFollower.OnMovementComplete += () => playerScript.transform.position = GridManager.Instance.getWorldPosition(playerScript.startGridPos);
        cameraFollower.OnMovementComplete += () => playerScript.gridPos = playerScript.startGridPos;
        cameraFollower.OnMovementComplete += () => playerScript.transform.localRotation = playerScript.startRotation;
        cameraFollower.OnMovementComplete += () => playerScript.GetComponentInChildren<Animator>().SetTrigger("Spawn");
        cameraFollower.OnMovementComplete += () => Destroy(cameraFollower);
        cameraFollower.OnMovementComplete += () => PerspectiveManager.Instance.gravityUp = new Vector3Int(0, 1, 0);
    }

    #region camera code

    public void setCanvasToCameraOverlay()
    {
        Canvas mainCanvas = GameObject.FindGameObjectWithTag("MainLevelCanvas").GetComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        mainCanvas.worldCamera = Camera.main;
    }

    public void setCanvasToScreenOverlay()
    {
        if (GameObject.FindGameObjectWithTag("MainLevelCanvas")!= null)
        {
            Canvas mainCanvas = GameObject.FindGameObjectWithTag("MainLevelCanvas").GetComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

    }

    #endregion

    #region transition code
    public void startLevelLoadAnimation()
    {
        //start loading blocks from the finish block
        levelStartAnimation.playLevelStartAnimations(GridManager.Instance.Grid.getCoord(FindObjectOfType<FinishZone>().gameObject));
        CameraManager.instance.canRotateCamera = false;

        //setup the camera follower to move from the finish block to the player spawn point
        MoveToPoint cameraFollower = GameObject.FindGameObjectWithTag("InitialCameraFollower").GetComponent<MoveToPoint>();
        cameraFollower.startMovement(FindObjectOfType<FinishZone>().transform.position, playerScript.transform.position, true);

        //have camera track the camera follower
        CameraManager.instance.setTarget(cameraFollower.transform);

        //on camera movement complete, spawn the player, and set camera to follow the player
        cameraFollower.OnMovementComplete += () => playerScript.gameObject.SetActive(true);
        cameraFollower.OnMovementComplete += () => CameraManager.instance.setTarget(playerScript.transform);
        cameraFollower.OnMovementComplete += () => CameraManager.instance.canRotateCamera = true;
        playerScript.GetComponent<PlayerAnimEventHandler>().OnSpawnLand += () => CameraManager.instance.speed = 10f;
        Transitioner.Instance.OnTransitionComplete -= startLevelLoadAnimation;
    }

    public void startLevelCompleteAnimation()
    {
        FindObjectOfType<AudioManager>().Play("LevelWon");
        playerScript.GetComponentInChildren<Animator>().SetTrigger("Despawn");
        playerScript.AllowMovementInput = false;
        playerScript.GetComponent<PlayerAnimEventHandler>().OnDespawn += startLevelCompleteTransition;

        //saves the level data
        if (levelIndex < ProgressSystem.CurrentProgressData.numOfLevels)
        {
            ProgressSystem.CurrentProgressData.levelUnlocked[levelIndex + 1] = true;
            ProgressSystem.saveProgressData();
        }

    }

    public void startLevelCompleteTransition()
    {
        setCanvasToCameraOverlay();
        //if at last level go back to the menu
        if (SceneManager.GetActiveScene().buildIndex + 1 == SceneManager.sceneCountInBuildSettings)
            Transitioner.Instance.TransitionToScene(0);
        else
            Transitioner.Instance.TransitionToScene(SceneManager.GetActiveScene().buildIndex + 1);
        playerScript.GetComponent<PlayerAnimEventHandler>().OnDespawn -= startLevelCompleteTransition;
    }

    #endregion

    #region star code
    public StarBlock[] getOrderedArrayOfStarBlocks()
    {
        StarBlock[] starBlocks = FindObjectsOfType<StarBlock>();
        //sorts by x, then y, then z
        System.Array.Sort(starBlocks, (a, b) => {return a.transform.position.x.CompareTo(b.transform.position.x);});
        System.Array.Sort(starBlocks, (a, b) => { return a.transform.position.y.CompareTo(b.transform.position.y); });
        System.Array.Sort(starBlocks, (a, b) => { return a.transform.position.z.CompareTo(b.transform.position.z); });

        if (starBlocks.Length != 3)
            Debug.LogWarning("There are not three star blocks on the level");

        return starBlocks;
    }

    public int getStarBlockIndex(StarBlock starBlock)
    {
        for (int i = 0; i < starBlocks.Length; i++)
            if (starBlocks[i] == starBlock)
                return i;

        Debug.LogError("Could not find star block");
        return -1;
    }

    public void setupStarsFromSavedData()
    {
        for (int i = 0; i < starBlocks.Length; i++)
        {
            if (ProgressSystem.CurrentProgressData.starsAcquired[levelIndex, i])
            {
                starBlocks[i].disableStar(true);
                UIStarController.UIInstantLightUpStar(i);
                UIStarController.UIToggleCounter(i, false);
            }
        }


    }

    #endregion

    #region LevelSetupCode (for editor)

    public void checkForAllManagers()
    {
        bool good = true;
        if (FindObjectOfType<GroundFolder>() == null)
        {
            good = false;
            GameObject.Instantiate(GameAssets.i.groundFolder);
            Debug.Log("Instantiating Ground Folder");
        }
        if (FindObjectOfType<PerspectiveManager>() == null)
        {
            good = false;
            GameObject.Instantiate(GameAssets.i.perspectiveManager);
            Debug.Log("Instantiating PerspectiveManager");
        }
        if (FindObjectOfType<ScaleManager>() == null)
        {
            good = false;
            GameObject.Instantiate(GameAssets.i.scaleManager);
            Debug.Log("Instantiating Scale Manager");
        }
        if (FindObjectOfType<AudioManager>() == null)
        {
            good = false;
            GameObject.Instantiate(GameAssets.i.audioManager);
            Debug.Log("Instantiating Audio Manager");
        }
        if (FindObjectOfType<Transitioner>() == null)
        {
            good = false;
            GameObject.Instantiate(GameAssets.i.transitionManager);
            Debug.Log("Instantiating Transition Manager");
        }
        if (FindObjectOfType<PathfindingManager>() == null)
        {
            good = false;
            GameObject.Instantiate(GameAssets.i.pathfindingManager);
            Debug.Log("Instantiating Pathfinding Manager");
        }
        if (FindObjectOfType<CameraManager>() == null)
        {
            good = false;
            GameObject.Instantiate(GameAssets.i.cameraParent);
            Debug.Log("Instantiating Camera Parent");
        }
        if (good)
            Debug.Log("Manager integrity passed check");
    }

    public void regenerateStarBlocks()
    {
        foreach (StarBlock star in FindObjectsOfType<StarBlock>())
            star.setupStarPrefab();
    }


    #endregion
}
