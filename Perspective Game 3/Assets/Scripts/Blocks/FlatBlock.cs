using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatBlock : MonoBehaviour
{
    Vector3Int visibleAxis;
    // Start is called before the first frame update
    void Awake()
    {
        visibleAxis = Vector3Int.RoundToInt(transform.up);
    }
}
