using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StarBlock))]
public class StarBlockEditor : Editor
{
    public override void OnInspectorGUI()
    {
        StarBlock starBlock = (StarBlock)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Star"))
        {
            starBlock.setupStarPrefab();
        }
    }
}
