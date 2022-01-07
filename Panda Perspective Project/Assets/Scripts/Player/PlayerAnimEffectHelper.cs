using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEffectHelper : MonoBehaviour
{
    private PlayerAnimEventHandler handler;




    private void Awake()
    {
        handler = GetComponentInParent<PlayerAnimEventHandler>();
    }
    public void spawnEffectEvent()
    {
        handler.playSpawnEffects();
    }

    public void landEffectEvent()
    {
        handler.playLandEffects();
    }

    public void despawnEffectEvent()
    {
        handler.playDespawnEffects();

    }
}
