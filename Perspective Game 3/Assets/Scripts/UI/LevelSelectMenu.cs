using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelSelectMenu : MonoBehaviour
{
    public RectTransform buttonTemplate;
    public RectTransform levelButtonArea;
    public Button nextPageButton;
    public Button prevPageButton;

    List<GameObject> buttons;
    int lowestLevelOfPage;
    //i.e. if there are only 7 levels in the game, with 10 fitting on a page, this value will be 7
    int highestValidLevelOfPage;
    //i.e. if there are only 7 levels in the game, with 10 fitting on a page, this value will be 10
    int highestViewableLevelOfPage;

    private void Awake()
    {
        buttons = new List<GameObject>();
    }
    public void generateLevelButtons(int startingLevel)
    {
        int level = startingLevel;
        lowestLevelOfPage = level;
        highestValidLevelOfPage = level;
        highestViewableLevelOfPage = level;
        deleteButtons();
        for (float y = 0; y > -levelButtonArea.rect.height + buttonTemplate.rect.height; y = y - buttonTemplate.rect.height)
            for (float x = 0; x < levelButtonArea.rect.width - buttonTemplate.rect.width; x = x + buttonTemplate.rect.width)
            {
                if (level <= ProgressSystem.CurrentProgressData.numOfLevels)
                {
                    GameObject newButton = Instantiate(buttonTemplate.gameObject, new Vector3(x, y, 0f), Quaternion.identity, levelButtonArea.transform);

                    newButton.transform.localPosition = new Vector3(x, y, 0f);
                    newButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0f);

                    buttons.Add(newButton);

                    LevelSelectButton newButtonLogic = newButton.GetComponent<LevelSelectButton>();
                    newButtonLogic.setupNumber(level);
                    for (int i = 0; i < 3; i++)
                        if (ProgressSystem.CurrentProgressData.starsAcquired[level, i])
                            newButtonLogic.lightUpStar(i);
                    newButton.GetComponent<Button>().interactable = ProgressSystem.CurrentProgressData.levelUnlocked[level];


                    newButton.SetActive(true);
                    level++;
                    highestValidLevelOfPage++;
                }
                highestViewableLevelOfPage++;

            }

        highestValidLevelOfPage--;
        highestViewableLevelOfPage--;

        //disable & enable the page toggle buttons as necessary
        if (lowestLevelOfPage == 1)
            prevPageButton.interactable = false;
        else
            prevPageButton.interactable = true;

        if (highestValidLevelOfPage >= ProgressSystem.CurrentProgressData.numOfLevels)
            nextPageButton.interactable = false;
        else
            nextPageButton.interactable = true;

        buttonTemplate.gameObject.SetActive(false);

        //Debug.Log(highestValidLevelOfPage);
        //Debug.Log(highestViewableLevelOfPage);
        //Debug.Log(lowestLevelOfPage);
    }

    public void nextPage()
    {
        generateLevelButtons(highestValidLevelOfPage + 1);
    }

    public void previousPage()
    {
        generateLevelButtons(Mathf.Max(1, lowestLevelOfPage - 1 - (highestViewableLevelOfPage - lowestLevelOfPage)));
    }

    private void deleteButtons()
    {
        foreach (GameObject button in buttons)
            GameObject.Destroy(button);

        buttons = new List<GameObject>();
    }

}
