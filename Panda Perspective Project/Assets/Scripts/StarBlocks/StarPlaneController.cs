using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarPlaneController : MonoBehaviour
{
    public SpriteRenderer starRenderer;
    public ParticleSystem[] auraEffects;
    public ParticleSystem[] pickupEffects;

    public void toggleCanPickupEffects(bool val)
    {

        foreach (ParticleSystem ps in auraEffects)
            ps.gameObject.SetActive(val);

        if (val)
            starRenderer.sprite = GameAssets.i.starLight;
        else
            starRenderer.sprite = GameAssets.i.starDark;
    }

    public void playPickupEffects()
    {
        foreach (ParticleSystem ps in pickupEffects)
            ps.gameObject.SetActive(true);

        toggleCanPickupEffects(false);
    }
}
