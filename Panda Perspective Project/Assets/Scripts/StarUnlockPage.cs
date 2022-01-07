using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StarUnlockPage : MonoBehaviour
{
   
    [System.Serializable]
    public struct Skin
    {
        public int starsRequired;
        public GameObject animalPrefab;
        public Sprite animalFace;
    }

    public GameObject starPrefab;
    public RectTransform starArea;
    public GameObject animalPreviewImage;
    public List<Skin> skinsToUnlock;
    public float unlockInterval = 1f;

    private Dictionary<int, Skin> skinDictionary;
    private List<StarUnlockUIElement> stars;
    private bool unlocking = false;
    private float lastUnlockTime;


    private void Awake()
    {
        //dictionary allows easier reference during star generation
        skinDictionary = new Dictionary<int, Skin>();

        foreach (Skin s in skinsToUnlock)
            skinDictionary.Add(s.starsRequired, s);
    }

    public void generateStars()
    {
        deleteStars();
        StarUnlockUIElement newStar;
        //creates stars
        for (int i = 0; i < ProgressSystem.CurrentProgressData.numOfLevels * 3; i++)
        {
            //no 0th star, but there may be skins that require 0 stars
            if (i != 0)
            {
                newStar = Instantiate(starPrefab, starArea.transform).GetComponent<StarUnlockUIElement>();
                if (i <= ProgressSystem.CurrentProgressData.starsAcquiredAndSeen)
                    newStar.unlock(false);
                newStar.gameObject.SetActive(true);
                stars.Add(newStar);
            }

            if (skinDictionary.ContainsKey(i))
            {
                GameObject animalPreview = Instantiate(animalPreviewImage, starArea.transform);
                animalPreview.GetComponent<Image>().sprite = skinDictionary[i].animalFace;
            }
        }

        if (ProgressSystem.CurrentProgressData.GetTotalCollectedStars() > ProgressSystem.CurrentProgressData.starsAcquiredAndSeen)
        {
            unlocking = true;
            lastUnlockTime = Time.time;
        }
    }

    private void Update()
    {
        if (unlocking)
            if (Time.time > lastUnlockTime + unlockInterval)
            {
                lastUnlockTime = Time.time;
                stars[ProgressSystem.CurrentProgressData.starsAcquiredAndSeen].unlock(true);
                ProgressSystem.CurrentProgressData.starsAcquiredAndSeen++;
                ProgressSystem.saveProgressData();
                if (ProgressSystem.CurrentProgressData.starsAcquiredAndSeen == ProgressSystem.CurrentProgressData.GetTotalCollectedStars())
                    unlocking = false;
            }
    }

    public void deleteStars()
    {
        foreach (Transform t in starArea.GetComponentInChildren<Transform>())
            GameObject.Destroy(t.gameObject);

        stars = new List<StarUnlockUIElement>();
    }
}
