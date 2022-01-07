using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _i;



    public static GameAssets i
    {
        get
        {
            if (_i == null) _i = (Instantiate(Resources.Load("GameAssets")) as GameObject).GetComponent<GameAssets>();
            return _i;
        }
    }

    [Header("Star Assets")]
    public GameObject starPlanePrefab;
    public Sprite starLight;
    public Sprite starDark;
    public Color starDarkUIColor;
    public Color starLightUIColor;
    public GameObject starUnlockEffect;

    [Header("Manager pre-fabs")]
    public GameObject groundFolder;
    public GameObject perspectiveManager;
    public GameObject scaleManager;
    public GameObject audioManager;
    public GameObject levelManager;
    public GameObject transitionManager;
    public GameObject pathfindingManager;
    public GameObject MainLevelCanvas;
    public GameObject cameraParent;

    [Header("Other")]
    public GameObject warningCubePrefab;
    public GameObject cameraFollower;



}
