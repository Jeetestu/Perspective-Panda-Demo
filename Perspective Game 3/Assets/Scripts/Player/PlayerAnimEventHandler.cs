using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEventHandler : MonoBehaviour
{
    public ParticleSystem[] spawnEffects;
    public ParticleSystem[] landEffects;
    public ParticleSystem[] despawnEffects;

    public delegate void PlayerEvent();
    public event PlayerEvent OnDespawn;
    public event PlayerEvent OnSpawnLand;

    public void playSpawnEffects()
    {
        foreach (ParticleSystem ps in spawnEffects)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }

    }

    public void playLandEffects()
    {
        foreach (ParticleSystem ps in landEffects)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }
        GetComponent<PlayerController>().AllowMovementInput = true;
        OnSpawnLand?.Invoke();
    }

    public void playDespawnEffects()
    {
        foreach (ParticleSystem ps in despawnEffects)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }
        OnDespawn?.Invoke();
    }
}
