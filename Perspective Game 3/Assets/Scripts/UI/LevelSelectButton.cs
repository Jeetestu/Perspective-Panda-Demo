using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelSelectButton : MonoBehaviour
{
    public int level;

    public Image[] numberImages;
    public Image[] starImages;

    public Sprite[] numberSprites;


    public void setupNumber(int level)
    {
        this.level = level;
        //disables all number images
        foreach (Image i in numberImages)
            i.gameObject.SetActive(false);

        char[] levelString = level.ToString().ToCharArray();
        for (int i =0; i < levelString.Length; i++)
        {
            numberImages[i].gameObject.SetActive(true);
            numberImages[i].sprite = numberSprites[(int)char.GetNumericValue(levelString[i])];
        }
    }
    
    public void lightUpStar(int index)
    {
        starImages[index].color = GameAssets.i.starLightUIColor;
    }
    public void loadLevel()
    {
        Debug.Log("Loading level: " + level);
        Transitioner.Instance.TransitionToScene(level, true);
    }
}
