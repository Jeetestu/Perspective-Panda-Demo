using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GroundFolder))]
public class GroundFolderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GroundFolder groundFolder = (GroundFolder)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Setup Ground Hierarchy"))
        {
            groundFolder.setupGroundHierarchy();
        }
    }
}
