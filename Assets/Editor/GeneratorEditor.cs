using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Generator), true)]
public class GeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Generator gen = (Generator)target;
        if(GUILayout.Button("Spawn Items In Square"))
        {
            gen.ClearAllItems();
            if (gen is WallGenerator generator)
                generator.SpawnClusterInSquare(new Vector3(-50, -50, 0), new Vector3(50, 50, 0), 5);
            else
                gen.SpawnClusterInSquare(new Vector3(-10, -10, 0), new Vector3(10, 10, 0), 10);
        }
        if(GUILayout.Button("Spawn Items In Radius"))
        {
            gen.ClearAllItems();
            if (gen is WallGenerator wallGen)
                wallGen.SpawnClusterInRadius(new Vector3(0, 0, 0), 10, 5);
            else
                gen.SpawnClusterInRadius(new Vector3(0, 0, 0), 10, 10);
        }
        if(GUILayout.Button("Clear"))
        {
            gen.ClearAllItems();
        }
    }
}
