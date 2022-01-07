using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundFolder : MonoBehaviour
{
    private void Awake()
    {
        transform.parent = ScaleManager.Instance.transform;
    }

    public void setupGroundHierarchy()
    {
        foreach (GroundType gt in FindObjectsOfType<GroundType>())
            gt.transform.parent = this.transform;

        FindObjectOfType<PlayerController>().transform.parent = this.transform;

        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;
        this.transform.localScale = Vector3.one;
    }
}
