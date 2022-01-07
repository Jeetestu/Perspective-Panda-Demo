using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStarController : MonoBehaviour
{
    public GameObject[] stars;
    public TMP_Text[] counters;
    public float shootingStarSpeed = 2f;

    public void Awake()
    {
        counters = new TMP_Text[stars.Length];
        for (int i = 0; i < stars.Length; i++)
        {
            counters[i] = stars[i].transform.GetChild(0).GetComponent<TMP_Text>();
            counters[i].gameObject.SetActive(false);
        }

        
    }
    public void UIInstantLightUpStar(int starIndex)
    {
        stars[starIndex].GetComponent<Image>().color = GameAssets.i.starLightUIColor;
    }

    public void UIToggleCounter(int starIndex, bool toggle)
    {
        counters[starIndex].gameObject.SetActive(toggle);
    }

    public void UIUpdateCounter(int starIndex, int val)
    {
        counters[starIndex].SetText(val.ToString());
    }

    public void UILightUpStarEffect(int starIndex, Vector3 worldStartPos)
    {
        GameObject starFinal = stars[starIndex];
        counters[starIndex].gameObject.SetActive(false);
        Vector3 screenStartPos = Camera.main.WorldToScreenPoint(worldStartPos);
        GameObject shootingStar = Instantiate(starFinal, screenStartPos, Quaternion.identity, this.transform);
        StartCoroutine(runShootingStarEffect(shootingStar, starFinal));
    }

    IEnumerator runShootingStarEffect(GameObject shootingStar, GameObject starFinal)
    {
        Image shootingStarImage = shootingStar.GetComponent<Image>();
        Image starFinalImage = starFinal.GetComponent<Image>();
        RectTransform shootingStarRect = shootingStar.GetComponent<RectTransform>();
        RectTransform starFinalRect = starFinal.GetComponent<RectTransform>();

        //the shooting star starts tiny and grows as it gets closer to it's position in the UI
        shootingStarRect.sizeDelta = new Vector2(0f, 0f);
        shootingStarImage.color = GameAssets.i.starLightUIColor;

        //shootingStar.transform.localPosition = new Vector3(shootingStar.transform.localPosition.x, shootingStar.transform.localPosition.y, 0f);
        //shootingStar.transform.localRotation = Quaternion.identity;

        while (Vector2.Distance(shootingStarRect.anchoredPosition, starFinalRect.anchoredPosition) > 0.2f)
        //while (shootingStarRect.anchoredPosition != starFinalRect.anchoredPosition) 
        {
            shootingStarRect.anchoredPosition = Vector2.Lerp(shootingStarRect.anchoredPosition, starFinalRect.anchoredPosition, shootingStarSpeed * Time.deltaTime);
            shootingStarRect.sizeDelta = Vector2.Lerp(shootingStarRect.sizeDelta, starFinalRect.sizeDelta, shootingStarSpeed * Time.deltaTime);

            yield return null;
        }

        starFinalImage.color = GameAssets.i.starLightUIColor;
        Destroy(shootingStar);
    }
}
