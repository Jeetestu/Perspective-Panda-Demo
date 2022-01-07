using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StarUnlockUIElement : MonoBehaviour
{
    public Image thisImage;
    public void unlock(bool showEffect)
    {

        if (showEffect)
            Instantiate(GameAssets.i.starUnlockEffect, this.transform, false);
        thisImage.sprite = GameAssets.i.starLight;
    }
}
