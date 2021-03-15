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

        if (GUILayout.Button("Generate Rivers At Transforms"))
        {
            t.GenerateRivers(t.modifyWaterHeight, true);
        }

        if (GUILayout.Button("Generate River Erosion (slow!)"))
        {
            t.GenerateRivers(t.modifyWaterHeight);
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

        if (GUILayout.Button("Generate Preset (slow!)"))
        {
            //General settings
            t.randomSeed = false;

            //Terrain settings
            t.resolutionPower = 10;
            t.terrainFrequency = 1;
            t.terrainHeight = 31;
            t.terrainNoiseDivider = 2;
            t.seed = 1621675866;
            t.mapSize = 250;
            t.beachHeight = .3f;
            t.waterHeight = 0;
            t.beachHeightLow = -.2f;
            t.mountainAngle = 30;

            // Colors
            //??

            //River settings
            t.riverSpawnLowestAboveWater = 5;
            t.riversAmount = 500;
            t.riverGenerations = 1;
            t.riverMaxStartSize = 8;
            t.riverMinStartSize = 2;
            t.riverStepSize = 2;
            t.riverMaxWaterDepth = 1.5f;
            t.riverErosionMultiplier = 0.75f;
            t.riverSearchDist = 20;
            t.underWaterSmoothingRange = 6;

            //Generate
            t.GenerateTerrain();
            t.GenerateRivers(t.modifyWaterHeight);
            t.SmoothUnderwater();
        }

        base.OnInspectorGUI();
    }
}
