using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningCube
{
    private static List<GameObject> warningCubes;

    public static GameObject createWarningCube(Vector3Int gridPos)
    {
        if (warningCubes == null)
            warningCubes = new List<GameObject>();

        GameObject newWarningCube = GameObject.Instantiate(GameAssets.i.warningCubePrefab, GridManager.Instance.getWorldPosition(gridPos), Quaternion.identity, ScaleManager.Instance.transform);

        warningCubes.Add(newWarningCube);

        return newWarningCube;
    }

    public static void destroyWarningCube(GameObject cube)
    {
        bool cubeFound = false;
        cubeFound = warningCubes.Contains(cube);

        if (cubeFound)
        {
            warningCubes.Remove(cube);
            GameObject.Destroy(cube);
        }
        else
        {
            Debug.LogWarning("Cube to be destroyed not found : " + cube.name);
        }
    }

    public static void destroyAllActiveWarningCubes()
    {
        if (warningCubes == null)  return;

        foreach (GameObject c in warningCubes)
            GameObject.Destroy(c);

        warningCubes = new List<GameObject>();
    }
}
