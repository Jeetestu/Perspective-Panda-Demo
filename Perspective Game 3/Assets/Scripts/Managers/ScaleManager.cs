using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleManager : MonoBehaviour
{
    private static ScaleManager instance;

    private void Awake()
    {
        instance = this;
    }
    public Vector3 gridToWorldScale = new Vector3(1, 1, 1);

    private Transform cam;
    private Light[] lights;


    //minimum and maximum scales when scaling the world
    public float minWorldScale = 0.03f;
    public float maxWorldScale = 1f;

    public static ScaleManager Instance { 
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ScaleManager>();
            return instance;
        }
            }

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        lights = FindObjectsOfType<Light>();
    }

    public void updateScaling()
    {
        #region Update the grid size based on the camera position

        float xScale = Mathf.Min(Vector3.Angle(cam.forward, new Vector3(1, 0, 0)), Vector3.Angle(cam.forward, new Vector3(-1, 0, 0)))/90;
        float yScale = Mathf.Min(Vector3.Angle(cam.forward, new Vector3(0, 1, 0)), Vector3.Angle(cam.forward, new Vector3(0, -1, 0))) / 90;
        float zScale = Mathf.Min(Vector3.Angle(cam.forward, new Vector3(0, 0, 1)), Vector3.Angle(cam.forward, new Vector3(0, 0, -1))) / 90;

        xScale = Mathf.Log(xScale, 60) + 1.3f;
        yScale = Mathf.Log(yScale, 60) + 1.3f;
        zScale = Mathf.Log(zScale, 60) + 1.3f;


        xScale = Mathf.Clamp(xScale, minWorldScale, maxWorldScale);
        yScale = Mathf.Clamp(yScale, minWorldScale, maxWorldScale);
        zScale = Mathf.Clamp(zScale, minWorldScale, maxWorldScale);
        gridToWorldScale = new Vector3(xScale, yScale, zScale);

        #endregion
        // Applies scaling to all objects
        transform.localScale = gridToWorldScale;


        #region updateShadows
        float distanceToFlatView = 1 - Mathf.Min(gridToWorldScale.x, gridToWorldScale.y, gridToWorldScale.z) + minWorldScale;
        LightShadows shadowSetting = LightShadows.Soft;
        if (distanceToFlatView == 1)
        {
            shadowSetting = LightShadows.None;
        }
        foreach (Light item in lights)
        {
            item.shadowStrength = 1 - distanceToFlatView;
            item.shadows = shadowSetting;
        }
        #endregion
    }
}
