using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGen))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        TerrainGen t = (TerrainGen)target;

        if (GUILayout.Button("Generate Terrain"))
        {
            t.GenerateTerrain();
        }
        if (GUILayout.Button("Paint Terrain"))
        {
            t.GenerateColors();
            t.MoveWaterLevel();
        }

        if (GUILayout.Button("Optimize"))
        {
            t.Optimize();
        }

        if (GUILayout.Button("Clear Data"))
        {
            t.Clear();
        }

        base.OnInspectorGUI();
    }
}
