using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public static class ProgressSystem
{
    [System.Serializable]
    public class ProgressData
    {
        public int numOfLevels;
        public bool[] levelUnlocked;
        //the first index represents the level
        //the second index represents the three stars in the level (true means they've been collected, false means they haven't)
        public bool[,] starsAcquired;
        //this variable shows how many stars have been acquired since the last time the 'skin' screen has been visited
        //it drives the animation to show skins being unlocked
        public int starsAcquiredAndSeen;
        public int selectedSkin;

        public int GetTotalCollectedStars()
        {
            int count = 0;
            for (int i = 0; i < numOfLevels+1; i++)
                for (int j = 0; j < 3; j++)
                    if (starsAcquired[i,j])
                        count++;
            return count;
        }
    }

    private static ProgressData currentProgressData;

    public static ProgressData CurrentProgressData
    {
        get
        {
            if (currentProgressData == null) ProgressSystem.loadProgressData();
            return currentProgressData;
        }
    }

    public static void saveProgressData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/progress.gd");
        bf.Serialize(file, ProgressSystem.currentProgressData);
        file.Close();
    }

    public static void loadProgressData()
    {
        //saved game data exists
        if (File.Exists(Application.persistentDataPath + "/progress.gd"))
        {
            Debug.Log("Loading pre-existing data at path: " + Application.persistentDataPath);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/progress.gd", FileMode.Open);
            ProgressSystem.currentProgressData = (ProgressData)bf.Deserialize(file);
            file.Close();
            updateLevelArrays();
        }
        //no saved data - Creates new data file
        else
        {

            currentProgressData = new ProgressData();
            currentProgressData.numOfLevels = SceneManager.sceneCountInBuildSettings - 1;
            Debug.Log("No save data, making new data file");
            Debug.Log("There are " + currentProgressData.numOfLevels + " levels");

            currentProgressData.levelUnlocked = new bool[currentProgressData.numOfLevels + 1];
            currentProgressData.starsAcquired = new bool[currentProgressData.numOfLevels + 1, 3];
            currentProgressData.levelUnlocked[1] = true;
        }
    }


    //used in case the number of levels has changed since the previous ProgressSystem save ocurred
    //will update the array sizes
    public static void updateLevelArrays()
    {
        int actualNumOfLevels = SceneManager.sceneCountInBuildSettings - 1;
        if (actualNumOfLevels != currentProgressData.numOfLevels)
        {
            bool[] newLevelUnlockedArray = new bool[actualNumOfLevels + 1];
            bool[,] newStarsAcquiredArray = new bool[actualNumOfLevels + 1, 3];

            for (int i = 0; i < Mathf.Min(actualNumOfLevels, currentProgressData.numOfLevels); i++)
            {
                newLevelUnlockedArray[i] = CurrentProgressData.levelUnlocked[i];
                newStarsAcquiredArray[i, 0] = CurrentProgressData.starsAcquired[i, 0];
                newStarsAcquiredArray[i, 1] = CurrentProgressData.starsAcquired[i, 1];
                newStarsAcquiredArray[i, 2] = CurrentProgressData.starsAcquired[i, 2];
            }

            CurrentProgressData.numOfLevels = actualNumOfLevels;
            CurrentProgressData.starsAcquired = newStarsAcquiredArray;
            CurrentProgressData.levelUnlocked = newLevelUnlockedArray;
        }
    }
}


