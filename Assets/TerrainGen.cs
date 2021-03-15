using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

public class River
{
    public River(Vector3 start)
    {
        riverPoints = new List<Vector3>();
        riverPoints.Add(start);
    }
    public List<Vector3> riverPoints;
    public float size = 4.0f;
    public float flow = 1.0f;
    public float length = 0;

    public void ClearPoints()
    {
        Vector3 temp = riverPoints[0];
        riverPoints.Clear();
        riverPoints.Add(temp);
    }
}

public class TerrainGen : MonoBehaviour
{
    Vector2[] poissonDisk =
    {
        new Vector2(0.2770745f, 0.6951455f),
        new Vector2(0.1874257f, -0.02561589f),
        new Vector2(-0.3381929f, 0.8713168f),
        new Vector2(0.5867746f, 0.1087471f),
        new Vector2(-0.3078699f, 0.188545f),
        new Vector2(0.7993396f, 0.4595091f),
        new Vector2(-0.09242552f, 0.5260149f),
        new Vector2(0.3657553f, -0.5329605f),
        new Vector2(-0.3829718f, -0.2476171f),
        new Vector2(-0.01085108f, -0.6966301f),
        new Vector2(0.8404155f, -0.3543923f),
        new Vector2(-0.5186161f, -0.7624033f),
        new Vector2(-0.8135794f, 0.2328489f),
        new Vector2(-0.784665f, -0.2434929f),
        new Vector2(0.9920505f, 0.0855163f),
        new Vector2(-0.687256f, 0.6711345f)
};

    public List<int> savedSeeds;
    public bool randomSeed = true;
    [Range(2, 11)]
    public int resolutionPower = 2;
    [Range(1, 10)]
    public int terrainFrequency = 2;
    [Range(.1f, 100)]
    public float terrainHeight = 256;
    [Range(1.5f, 2.5f)]
    public float terrainNoiseDivider = 2;
    public int seed = 0;
    private int res = 257;
    public float mapSize = 2048;
    // Color Settings
    public float beachHeight = 5;
    public float waterHeight = -5;
    public float beachHeightLow = -3;
    public float mountainAngle = 20;
    public Color32 groundColor;
    public Color32 beachColor;
    public Color32 mountainColor;
    public Color32 waterBottomColor;
    public Transform waterPlane;
    private Mesh terrainMesh;
    private float[,] heightmap;
    private float[,] waterHeightmap;
    private bool wrap = false;

    // River settings
    public List<RiverStart> riverStartPoints;
    public bool modifyWaterHeight = true;
    public Color32 riverColor;
    public float riverSpawnLowestAboveWater = 3.0f;
    public int riversAmount = 50;
    public float riverMinLength = 2.0f;
    public int riverGenerations = 10;
    public float riverMaxStartSize = 4;
    [Range(1, 100)]
    public float riverMinStartSize = 1;
    public float riverStepSize = 1.0f;
    [Range(1, 50)]
    public float riverMaxWaterDepth = 2.0f;
    [Range(.1f, 5)]
    public float riverErosionMultiplier = 1.0f;
    public float riverSearchDist = 5.0f;
    public float underWaterSmoothingRange = 6.0f;


    // Measurements
    //General
    private float averageHeight = 0;
    private float maxHeight = 0;
    private float minHeight = 0;    

    //River measurements
    private float riverLengthTotal = 0;
    private float riverLengthsFinal = 0;
    private int riversTotal = 0;
    // Water measurements
    private float waterPercentage = 0;

    public void MeasureAndPrintStats()
    {
        float heights = 0;
        maxHeight = 0;
        minHeight = 0;
        float waterCount = 0;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                if (heightmap[x, y] < minHeight) minHeight = heightmap[x, y];
                if (heightmap[x, y] > maxHeight) maxHeight = heightmap[x, y];
                if (heightmap[x, y] < waterHeight) waterCount++;
                heights += heightmap[x, y];
            }
        }
        //Get percentage
        waterPercentage = waterCount / (res * res) * 100;
        averageHeight = heights / (res * res);

        Debug.Log("Terrain Statistics: " +
            "\nTerrain Average Height: " + averageHeight +
            "\nTerrain Max Height: " + maxHeight +
            "\nTerrain Min Height: " + minHeight +
            "\nWater Percentage: " + waterPercentage + "%" +
            "\nTotal Rivers: " + riversTotal +
            "\nTotal River Lenght(km): " + riverLengthTotal/1000 +
            "\nFinal River Length(km): " + riverLengthsFinal/1000);
    }


    public void GenerateTerrain()
    {
        if (resolutionPower < 2) resolutionPower = 2;
        if (resolutionPower > 12) resolutionPower = 12;

        res = (int)Mathf.Pow(2, resolutionPower) + 1;

        if (randomSeed)
        {
            seed = (int)System.DateTime.Now.Ticks;
            Random.InitState(seed);
        }
        else Random.InitState(seed);
        heightmap = new float[res, res];
        waterHeightmap = new float[res, res];
        DiamondSquare(wrap);
        //Make model here
        MakeModels(false);
        GenerateColors();
        //MoveWaterLevel();
        MeasureAndPrintStats();
    }

    public void GenerateRivers(bool moveWater = false, bool onlyPrelocated = false)
    {
        // Reset measurements
        riverLengthsFinal = 0;
        riverLengthTotal = 0;
        riversTotal = 0;
        //Check if there is any terrain
        if (heightmap == null)
        {
            randomSeed = false;
            GenerateTerrain();
        }
        //Check how much is above water (NOT WORKING RIGHT NOW)
        //float aboveWater = TerrainAboveWaterAmount();
        int riversPlaced = 0;
        int riverTarget = (int)(riversAmount);
        //1000 tries per river
        List<River> rivers = new List<River>();

        if (!onlyPrelocated)
        {
            for (int i = 0; i < riversAmount * 1000; i++)
            {
                Vector3 pos;
                if (TryPlacingRiverStart(out pos))
                {
                    River r = new River(pos);
                    r.size = Random.Range(riverMinStartSize, riverMaxStartSize);
                    r.flow = Random.Range(0.1f, 2);
                    rivers.Add(r);
                    riversPlaced++;
                }
                if (riversPlaced > riverTarget) break;
            }
        }
        else
        {
            for (int i = 0; i < riverStartPoints.Count; i++)
            {
                //Find relative position
                Vector3 riverPos = WorldToGenerationSpace(riverStartPoints[i].transform.position);
                // Check if within terrain borders
                if (!(riverPos.x < 0 || riverPos.y < 0 || riverPos.x > res || riverPos.y > res))
                {
                    // Move to height
                    float h = HeightAt(new Vector2(riverPos.x, riverPos.y));
                    River r = new River(new Vector3(riverPos.x, riverPos.y, h));
                    r.size = riverStartPoints[i].riverSize;
                    r.flow = riverStartPoints[i].riverFlow;
                    rivers.Add(r);
                    riversPlaced++;
                }
                else Debug.Log("Transform " + i + " not within borders, river not added!");
            }
        }


        for (int gen = 0; gen < riverGenerations; gen++)
        {
            Parallel.For(0, rivers.Count, index =>
            {
                FindRiverPath(rivers[index]);
            });

            //Remove any river too short
            for (int i = rivers.Count-1; i >= 0; i--)
            {
                float l = rivers[i].length;
                Vector3 dist = rivers[i].riverPoints[0] - rivers[i].riverPoints[rivers[i].riverPoints.Count - 1];
                //2D Distance
                float l2 = new Vector2(dist.x, dist.y).magnitude;
                if (l < riverMinLength || l2 < riverMinLength) rivers.RemoveAt(i);
            }

            //Push terrain
            for (int i = 0; i < rivers.Count; i++)
            {
                PushRiverPath(rivers[i], riverErosionMultiplier, 50.0f);
            }

            if (moveWater)
            {
                // Fill rivers with water (using heightmap)
                if (gen == riverGenerations - 1)
                {
                    //Move water heightmap
                    for (int i = 0; i < rivers.Count; i++)
                    {
                        PushRiverWaterPath(rivers[i], riverErosionMultiplier);
                    }
                }
            }
            for (int i = 0; i < rivers.Count; i++)
            {
                riverLengthTotal += rivers[i].length;
                rivers[i].ClearPoints();
            }
        }

        for (int i = 0; i < rivers.Count; i++)
        {
            riverLengthsFinal += rivers[i].length;
        }
        riversTotal = rivers.Count;


        if (moveWater) SmoothRivers();
        rivers.Clear();
        MakeModels(true);
        GenerateColors();
        MeasureAndPrintStats();
    }

    public void SmoothRivers()
    {
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                    float height = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        height += WaterHeightAt(new Vector2(x, y) + poissonDisk[i] * 2);
                    }
                    waterHeightmap[x, y] = height / 8;
            }
        }
    }

    public void SmoothUnderwater(bool makeModel = true)
    {
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                if (heightmap[x, y] < waterHeight)
                {
                    float height = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        height += HeightAt(new Vector2(x, y) + poissonDisk[i] * underWaterSmoothingRange);
                    }
                    heightmap[x, y] = height / 8;
                }
            }
        }
        if (makeModel)
        {
            MakeModels(false);
            GenerateColors();
        }
    }



    private void FindRiverPath(River r)
    {
        Vector3 activePos = r.riverPoints[0];
        Vector3 lastPos;
        int steps = 0;
        while (activePos.z > waterHeight - riverMaxWaterDepth)
        {
            lastPos = activePos;
            float actualDistance = 0;
            //Default search radius
            float searchRadius = 2;            
            //Check previous moves to see if any significant progress has been made
            if (r.riverPoints.Count > 2)
            {
                for (int i = r.riverPoints.Count - 2; i > 0 && i > r.riverPoints.Count - 10; i--)
                {
                    float distanceBack = (r.riverPoints[r.riverPoints.Count - 1] - r.riverPoints[i]).magnitude;
                    if (distanceBack > 10)
                    {
                        break;
                    }
                    actualDistance = distanceBack;
                }
                // If progress has been mediocre, increase search distance
                if (actualDistance < 8)
                {
                    searchRadius = riverSearchDist;
                }
            }


            //Check where is forwards, radius increases based on previous movement
            Vector3 dir = GetFlowDirection(activePos, searchRadius, 12);
           
            if (steps == 0) activePos += dir * riverStepSize;
            //Mix between flowDir & previousDir
            //dir = ((activePos - lastPos).normalized + dir) / 2;
            dir = Vector3.Lerp((activePos - lastPos).normalized, dir, 0.85f);

            activePos += dir * riverStepSize;
            float h = HeightAt(new Vector2(activePos.x, activePos.y));
            //Make sure height is correct
            activePos.z = h;
            //Add point
            r.riverPoints.Add(activePos);
            //Count steps
            steps++;
            r.length += (lastPos - activePos).magnitude;
            if (steps > 2000) break;
        }
        //Debug.Log("Steps: " + steps);
    }

    private void PushRiverPath(River r, float strengthMultiplier, float smoothInDistance)
    {
        float dist = 0;
        float smoothing = 0;
        for (int i = 0; i < r.riverPoints.Count; i++)
        {
            if (i != 0)
            {
                dist += (r.riverPoints[i] - r.riverPoints[i - 1]).magnitude;
                smoothing = dist / smoothInDistance;
            }
            float strength = 1.0f;
            float waterStrength = 1.0f;
            float waterStrengthZeroStart = 0.0f;
            //Push heightmap here
            if (dist < smoothInDistance)
            {
                strength *= smoothing;
            }
            if (r.riverPoints[i].z < waterHeight)
            {
                waterStrengthZeroStart = (Mathf.Abs(r.riverPoints[i].z) / riverMaxWaterDepth);
                waterStrength *= 1 - waterStrengthZeroStart;
            }
            float finalRadius = r.size * strength;
            float finalStrength = -strength * strengthMultiplier * waterStrength * r.flow;
            if (finalRadius < 1.0f) finalRadius = 1.0f;
            if (finalStrength > -.2f) finalStrength = -.2f;
            PushHeightAt(r.riverPoints[i], finalRadius, finalStrength);
        }
    }

    private void PushRiverWaterPath(River r, float smoothInAmount)
    {
        int smoothing = (int)(r.riverPoints.Count * smoothInAmount);
        for (int i = 0; i < r.riverPoints.Count; i++)
        {
            //Move height down to lowest point
            float strength = 1.0f;
            //Push heightmap here
            if (i < smoothing)
            {
                strength *= (float)i / smoothing;
            }
            PushWaterHeightAt(r.riverPoints[i], r.size * strength);
        }
    }

    private Vector3 GenerationSpaceToWorld(Vector3 genSpacePos)
    {
        float mult = mapSize / (res - 1);
        return transform.position + 
            new Vector3(genSpacePos.x * mult, genSpacePos.z, genSpacePos.y * mult);
    }
    
    private Vector3 WorldToGenerationSpace(Vector3 worldPos)
    {
        Vector3 fromOrigin = worldPos;
        float div = (res - 1) / mapSize;
        return new Vector3(fromOrigin.x*div, fromOrigin.z*div, fromOrigin.y);
    }


    private Vector3 GetFlowDirection(Vector3 pos, float radius, int samples)
    {
        if (samples > 15) samples = 15;
        Vector3[] directions = new Vector3[samples];
        Vector3 gather = Vector3.zero;
        for (int i = 0; i < samples; i++)
        {
            Vector2 newPos2D = new Vector2(pos.x, pos.y) + new Vector2(poissonDisk[i].x * radius * 2, poissonDisk[i].y * radius * 2);
            float h = HeightAt(newPos2D);
            Vector3 newPos = new Vector3(newPos2D.x, newPos2D.y, h);
            Vector3 rel = newPos - pos;
            if (rel.z >= 0) rel *= -1;
            rel.Normalize();
            rel *= Mathf.Abs(rel.z);
            gather += rel;
        }
        gather /= samples;
        gather.Normalize();
        return gather;
    }

    private void PushHeightAt(Vector3 pos, float radius, float strength)
    {

        for (int x = -(int)radius; x < (int)radius; x++)
        {
            for (int y = (int)-radius; y < (int)radius; y++)
            {
                //Get world pos                
                int xPos = (int)pos.x + x;
                int yPos = (int)pos.y + y;
                //Get distance
                Vector2 posXY = new Vector2(pos.x, pos.y);
                float dist = (new Vector2(xPos, yPos) - posXY).magnitude;
                float distMult = 1 - (dist / radius);
                Mathf.Clamp(distMult, 0, 1);

                if (Inside(xPos, yPos))
                {
                    float targetHeight = pos.z + (strength * distMult);
                    if (heightmap[xPos, yPos] > targetHeight)
                        heightmap[xPos, yPos] = targetHeight;
                }

            }
        }
    }

    private void PushWaterHeightAt(Vector3 pos, float radius)
    {
        //Make sure height is low enough
        pos.z = HeightAt(new Vector2(pos.x, pos.y));
        for (int x = -(int)radius; x < (int)radius; x++)
        {
            for (int y = (int)-radius; y < (int)radius; y++)
            {
                //Get world pos                
                int xPos = (int)pos.x + x;
                int yPos = (int)pos.y + y;
                //Get distance
                Vector2 posXY = new Vector2(pos.x, pos.y);
                float dist = (new Vector2(xPos, yPos) - posXY).magnitude;
                float distMult = 1 - (dist / radius);
                Mathf.Clamp(distMult, 0, 1);

                if (Inside(xPos, yPos))
                {
                    float targetHeight = pos.z;
                    if (targetHeight > waterHeight)
                        waterHeightmap[xPos, yPos] = targetHeight;
                }

            }
        }
    }


    private bool TryPlacingRiverStart(out Vector3 pos)
    {
        float rndX = Random.Range(0, res);
        float rndY = Random.Range(0, res);
        //Check if below water
        float height = HeightAt(new Vector2(rndX, rndY));
        pos = new Vector3(rndX, rndY, height);

        if (height < waterHeight + riverSpawnLowestAboveWater) return false;
        float size = Random.Range(riverMinStartSize, riverMaxStartSize);

        return true;
    }

    private float HeightAt(Vector2 pos)
    {
        Vector4 heights = GetHeights(pos);
        Vector2 localPos = new Vector2(pos.x % 1, pos.y % 1);
        float height = Mathf.Lerp(Mathf.Lerp(heights.z, heights.w, pos.x), Mathf.Lerp(heights.y, heights.x, pos.x), pos.y);
        return height;
    }

    private float WaterHeightAt(Vector2 pos)
    {
        Vector4 heights = GetWaterHeights(pos);
        Vector2 localPos = new Vector2(pos.x % 1, pos.y % 1);
        float height = Mathf.Lerp(Mathf.Lerp(heights.z, heights.w, pos.x), Mathf.Lerp(heights.y, heights.x, pos.x), pos.y);
        return height;
    }

    private Vector4 GetHeights(Vector2 pos)
    {
        int xInt = Mathf.CeilToInt(pos.x);
        int yInt = Mathf.CeilToInt(pos.y);

        //Edge case 0,0 fix
        if (xInt - 1 <= 0) xInt = 1;
        if (yInt - 1 <= 0) yInt = 1;
        if (xInt >= res) xInt = res - 2;
        if (yInt >= res) yInt = res - 2;
        float h0 = heightmap[xInt, yInt];
        float h1 = heightmap[xInt - 1, yInt];
        float h2 = heightmap[xInt - 1, yInt - 1];
        float h3 = heightmap[xInt, yInt - 1];
        return new Vector4(h0, h1, h2, h3);
    }

    private Vector4 GetWaterHeights(Vector2 pos)
    {
        int xInt = Mathf.CeilToInt(pos.x);
        int yInt = Mathf.CeilToInt(pos.y);

        //Edge case 0,0 fix
        if (xInt - 1 <= 0) xInt = 1;
        if (yInt - 1 <= 0) yInt = 1;
        if (xInt >= res) xInt = res - 2;
        if (yInt >= res) yInt = res - 2;
        float h0 = waterHeightmap[xInt, yInt];
        float h1 = waterHeightmap[xInt - 1, yInt];
        float h2 = waterHeightmap[xInt - 1, yInt - 1];
        float h3 = waterHeightmap[xInt, yInt - 1];
        return new Vector4(h0, h1, h2, h3);
    }

    public void MoveWaterLevel()
    {
        waterPlane.position = new Vector3(waterPlane.position.x, waterHeight, waterPlane.position.z);
    }
    //Eventually make this divide into sections? LOD models by generating lower res?
    public void MakeModels(bool makeWater)
    {
        MakeModel(new Vector2Int(0, 0), new Vector2Int(res, res), 0, GetComponent<MeshFilter>(), heightmap);
        terrainMesh = GetComponent<MeshFilter>().sharedMesh;
        if (makeWater) MakeModel(new Vector2Int(0, 0), new Vector2Int(res, res), 0, waterPlane.gameObject.GetComponent<MeshFilter>(), waterHeightmap);
    }

    public Vector3[] GetVec3Stream(Vector2Int start, Vector2Int end, float[,] heights)
    {
        float mult = mapSize / (res - 1);
        Vector3[] map = new Vector3[res * res];
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                map[res * y + x] = new Vector3(x * mult, heights[x, y], y * mult);
            }
        }
        return map;
    }

    private void DiamondSquare(bool wrap)
    {
        if (!wrap)
        {
            //Initialize corners?? Or not if wrap!
            heightmap[0, 0] = Random.Range(-terrainHeight / 2, terrainHeight / 2);
            heightmap[0, res - 1] = Random.Range(-terrainHeight / 2, terrainHeight / 2);
            heightmap[res - 1, 0] = Random.Range(-terrainHeight / 2, terrainHeight / 2);
            heightmap[res - 1, res - 1] = Random.Range(-terrainHeight / 2, terrainHeight / 2);
        }
        //Find center &Move
        int step = (res - 1); ///(int)Mathf.Pow(2, frequency);
        int halfStep = step / 2;
        int divisions = 0;
        float randomRange = terrainHeight / 2;
        float noiseDivLocal = terrainNoiseDivider;
        //TODO: At frequencies higher than 0, do steps but at much lower values (at first step values)

        while (step > 1)
        {
            //Sample square
            for (int y = halfStep; y < res; y += step)
            {
                for (int x = halfStep; x < res; x += step)
                {
                    heightmap[x, y] = SampleSquare(x, y, halfStep) + Random.Range(-randomRange, randomRange);
                }
            }

            //Sample diamond
            for (int y = 0; y < res; y += halfStep)
            {
                for (int x = y % step == 0 ? halfStep : 0; x < res; x += step)
                {
                    heightmap[x, y] = SampleDiamond(x, y, halfStep) + Random.Range(-randomRange, randomRange);
                }
            }

            step /= 2;
            halfStep = step / 2;
            divisions += 1;
            if (divisions >= terrainFrequency)
            {
                randomRange /= noiseDivLocal;
            }
        }
    }

    private float SampleDiamond(int x, int y, int distance)
    {
        float value = 0;
        int div = 0;
        //Sample left, right, up down
        if (Inside(x - distance, y))
        {
            value += Sample(x - distance, y);
            div++;
        }
        if (Inside(x + distance, y))
        {
            value += Sample(x + distance, y);
            div++;
        }
        if (Inside(x, y - distance))
        {
            value += Sample(x, y - distance);
            div++;
        }
        if (Inside(x, y + distance))
        {
            value += Sample(x, y + distance);
            div++;
        }
        value /= div;
        return value;
    }

    private float SampleSquare(int x, int y, int distance)
    {
        float value = 0;
        int div = 0;
        //Sample left up, right up, left down, right down
        if (Inside(x - distance, y - distance))
        {
            value += Sample(x - distance, y - distance);
            div++;
        }
        if (Inside(x + distance, y - distance))
        {
            value += Sample(x + distance, y - distance);
            div++;
        }
        if (Inside(x - distance, y + distance))
        {
            value += Sample(x - distance, y + distance);
            div++;
        }
        if (Inside(x + distance, y + distance))
        {
            value += Sample(x + distance, y + distance);
            div++;
        }
        value /= div;
        return value;
    }

    private float Sample(int x, int y)
    {
        int xPos = WrapValue(x);
        int yPos = WrapValue(y);
        return heightmap[xPos, yPos];
    }

    private bool Inside(int x, int y)
    {
        if (wrap) return true;
        if (x < 0 || x >= res) return false;
        if (y < 0 || y >= res) return false;
        return true;
    }

    private int WrapValue(int val)
    {
        if (val < 0) return res + val;
        if (val >= res) return val - res;
        return val;
    }

    public void Clear()
    {
        var mf = GetComponent<MeshFilter>();
        mf.sharedMesh.Clear();
    }

    private void MakeModel(Vector2Int start, Vector2Int end, int subMesh, MeshFilter mf, float[,] heights)
    {
        Mesh m = new Mesh();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Vector2Int size = end - start;
        //Make vertices
        Vector3[] vertices = GetVec3Stream(start, end, heights);
        m.vertices = vertices;

        //Make tris
        int[] indices = new int[(size.x - 1) * (size.y - 1) * 6];
        for (int y = start.y; y < end.y - 1; y++)
        {
            for (int x = start.x; x < end.x - 1; x++)
            {
                int initial = y * (res - 1) * 6 + x * 6;
                //First triangle
                indices[initial] = y * res + x; //0 at beginnning
                indices[initial + 1] = (y + 1) * res + x; //2 at beginnning
                indices[initial + 2] = y * res + x + 1; //1 at beginnning
                //Second triangle
                indices[initial + 3] = (y + 1) * res + x; //2 at beginnning
                indices[initial + 4] = (y + 1) * res + x + 1; //3 at beginnning
                indices[initial + 5] = y * res + x + 1; //1 at beginnning
            }
        }
        m.SetIndices(indices, MeshTopology.Triangles, subMesh);
        Vector2[] uvs = new Vector2[m.vertices.Length];
        int uvLength = uvs.Length;
        Vector3[] normals = new Vector3[m.vertices.Length];
        for (int i = 0; i < uvLength; i++)
        {
            uvs[i] = -new Vector2(vertices[i].x, vertices[i].z)/(res/4);
            normals[i] = Vector3.down;
        }
        //Add UVs
        m.SetUVs(0, uvs);
        //GenerateColors();
        m.RecalculateNormals();
        m.RecalculateTangents();
        //Add colors

        //var mf = GetComponent<MeshFilter>();
        mf.mesh = m;
    }

    public void Optimize()
    {
        var mf = GetComponent<MeshFilter>();
        mf.sharedMesh.OptimizeIndexBuffers();
        mf.sharedMesh.OptimizeReorderVertexBuffer();
        mf.sharedMesh.Optimize();
    }

    public void GenerateColors()
    {
        //Would be cool if some kind of noise-bias could be incorporated as well.
        Vector3[] vertices = terrainMesh.vertices;
        Vector3[] normals = terrainMesh.normals;

        Color32[] colors = new Color32[terrainMesh.vertices.Length];
        int colLength = colors.Length;
        for (int i = 0; i < colLength; i++)
        {
            //Cliff Colors
            if (Vector3.Angle(Vector3.up, normals[i]) > mountainAngle)
            {
                colors[i] = mountainColor;
            }
            else if (vertices[i].y < beachHeight + waterHeight)
            {
                //Beach
                if (vertices[i].y > beachHeightLow + waterHeight)
                {
                    colors[i] = beachColor;
                }
                //Bottom
                else
                {
                    colors[i] = waterBottomColor;
                }
            }
            //Grass
            else
            {
                colors[i] = groundColor;
            }
        }
        terrainMesh.colors32 = colors;
    }
}
