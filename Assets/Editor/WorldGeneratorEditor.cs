using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldGenerator))]
public class WorldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldGenerator worldGen = (WorldGenerator)target;
        if(GUILayout.Button("Spawn All"))
        {
            worldGen.Clear();
            worldGen.SetupBorder();
            worldGen.SetupRegions();
            worldGen.SpawnAll();
        }
        if(GUILayout.Button("Clear"))
        {
            worldGen.Clear();
        }
    }
}
