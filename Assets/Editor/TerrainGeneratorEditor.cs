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

        if (GUILayout.Button("Generate River Erosion"))
        {
            t.GenerateRivers();
        }

        if (GUILayout.Button("Smooth Underwater"))
        {
            t.SmoothUnderwater();
        }

        if (GUILayout.Button("Optimize"))
        {
            t.Optimize();
        }

        if (GUILayout.Button("Clear Data"))
        {
            t.Clear();
        }

        if (GUILayout.Button("Generate Preset (might take a while)"))
        {
            t.seed = -1371221714;
            t.GenerateTerrain();
            t.GenerateRivers();
            t.SmoothUnderwater();
        }

        base.OnInspectorGUI();
    }
}
