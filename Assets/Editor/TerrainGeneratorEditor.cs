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
            //General settings
            t.randomSeed = false;

            //Terrain settings
            t.resolutionPower = 10;
            t.terrainFrequency = 1;
            t.terrainHeight = 31;
            t.terrainNoiseDivider = 2;
            t.seed = 257966827;
            t.mapSize = 250;
            t.beachHeight = .3f;
            t.waterHeight = 0;
            t.beachHeightLow = -.2f;
            t.mountainAngle = 30;

            // Colors
            //??

            //River settings
            t.riverSpawnLowestAboveWater = 3;
            t.riversAmount = 500;
            t.riverGenerations = 3;
            t.riverMaxStartSize = 8;
            t.riverMinStartSize = 2;
            t.riverStepSize = 4;
            t.riverMaxWaterDepth = 1.5f;
            t.riverErosionMultiplier = 1.0f;
            t.riverSearchDist = 4;
            t.underWaterSmoothingRange = 6;

            //Generate
            t.GenerateTerrain();
            t.GenerateRivers();
            t.SmoothUnderwater();
        }

        base.OnInspectorGUI();
    }
}
