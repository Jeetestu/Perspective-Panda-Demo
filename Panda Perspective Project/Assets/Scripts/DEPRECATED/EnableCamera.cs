using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableCamera : MonoBehaviour
{
    private void Awake()
    {
        this.GetComponent<Triggerable>().scripts.AddListener(turnOnCameraRotation);
    }


    public void turnOnCameraRotation()
    {
        GameObject.FindObjectOfType<CameraManager>().canRotateCamera = true;
    }
}
