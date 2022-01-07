using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathfindingManager))]
public class PathfindingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PathfindingManager pathfindingManager = (PathfindingManager)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Update Debug Pathfinding"))
        {
            pathfindingManager.updateDebugPathfinding();
        }
    }
}
