using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkySettings : MonoBehaviour
{
    [Tooltip("Value of 0 is noon, 0.5 is midnight")]
    [Range(0f, 1f)]
    public float timeOfDay;
    public float timeSpeed;
    
    private Material mat;
    private Vector2 newTime;
    void Start()
    {
        mat = GetComponent<MeshRenderer>().sharedMaterial;
        newTime.x = timeOfDay;
        mat.SetTextureOffset("_MainTex", new Vector2(timeOfDay, 0f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //runs in editor
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            mat = GetComponent<MeshRenderer>().sharedMaterial;
            newTime.x = timeOfDay;
            mat.SetTextureOffset("_MainTex", new Vector2(timeOfDay, 0f));
        }

    }
}
