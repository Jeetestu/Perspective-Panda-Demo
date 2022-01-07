using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingTransparency : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float maxTransparency;
    [Range(0.0f, 1.0f)]
    public float minTransparency;
    public float intervalSpeed;

    private bool ToMax = true;
    private Color col;
    private Renderer ren;
    void Start()
    {
        ren = GetComponent<Renderer>();
        col = ren.material.color;
        col.a = maxTransparency;
        ren.material.color = col;
    }
    // Update is called once per frame
    void Update()
    {
        if (col.a >= maxTransparency)
        {
            ToMax = false;
        }
        else if (col.a <= minTransparency)
        {
            ToMax = true;
        }

        if (ToMax)
        {
            col.a = Mathf.MoveTowards(col.a, maxTransparency, intervalSpeed * Time.deltaTime);
        }
        else
        {
            col.a = Mathf.MoveTowards(col.a, minTransparency, intervalSpeed * Time.deltaTime);
        }

        ren.material.color = col;
    }
}
