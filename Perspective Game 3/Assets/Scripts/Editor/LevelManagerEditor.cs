using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LevelManager levelManager = (LevelManager)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Check Manager Integrity"))
        {
            levelManager.checkForAllManagers();
        }
        if (GUILayout.Button("Regenerate Star Blocks"))
        {
            levelManager.regenerateStarBlocks();
        }
    }
}
