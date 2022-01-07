using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishZone : MonoBehaviour
{
    private LevelManager gm;
    public void Awake()
    {
        gm = GameObject.FindObjectOfType<LevelManager>();
        this.GetComponent<Triggerable>().scripts.AddListener(winLevel);
    }
    public void winLevel()
    {
        gm.startLevelCompleteAnimation();
    }
}
