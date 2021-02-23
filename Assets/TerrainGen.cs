using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class TerrainGen : MonoBehaviour
{
    public bool randomSeed = true;
    [Range(2, 11)]
    public int resolutionPower = 2;
    [Range(1, 10)]
    public int frequency = 2;
    [Range(.1f, 100)]
    public float height = 256;
    [Range(1.5f,2.5f)]
    public float noiseDivider = 2;
    public int seed = 0;
    private int res = 257;
    public float mapSize = 2048;
    //ColorSettings
    public float beachHeight = 5;
    public float waterHeight = -5;
    public float beachHeightLow = -3;
    public float mountainAngle = 20;
    public Color32 groundColor;
    public Color32 beachColor;
    public Color32 mountainColor;
    public Color32 waterBottomColor;
    public Transform waterPlane;
    private Mesh mesh;
    //public float waterHeight = -50;
    private float[,] heightmap;
    private bool wrap = false;
    private Mesh[] meshes;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
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
        DiamondSquare(wrap);
        //Make model here
        MakeModels();
        GenerateColors();
        MoveWaterLevel();
    }

    public void MoveWaterLevel()
    {
        waterPlane.position = new Vector3(waterPlane.position.x, waterHeight, waterPlane.position.z);
        MeshRenderer m = GetComponent<MeshRenderer>();
        m.sharedMaterial.SetFloat("Vector1_F47479BC", waterHeight);
    }
    //Eventually make this divide into sections? LOD models by generating lower res?
    public void MakeModels()
    {
        MakeModel(new Vector2Int(0, 0), new Vector2Int(res, res), 0);
    }

    public Vector3[] GetVec3Stream(Vector2Int start, Vector2Int end)
    {
        float mult = mapSize/(res-1);
        Vector3[] map = new Vector3[res * res];
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                map[res * y + x] = new Vector3(x*mult, heightmap[x, y], y*mult);
            }
        }
        return map;
    }

    private void DiamondSquare(bool wrap)
    {
        if (!wrap)
        {
            //Initialize corners?? Or not if wrap!
            heightmap[0, 0] = Random.Range(-height / 2, height / 2);
            heightmap[0, res - 1] = Random.Range(-height / 2, height / 2);
            heightmap[res - 1, 0] = Random.Range(-height / 2, height / 2);
            heightmap[res - 1, res - 1] = Random.Range(-height / 2, height / 2);
        }
        //Find center &Move
        int step = (res - 1); ///(int)Mathf.Pow(2, frequency);
        int halfStep = step / 2;
        int divisions = 0;
        float randomRange = height/2;
        float noiseDivLocal = noiseDivider;
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
            if (divisions >= frequency)
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

    private void MakeModel(Vector2Int start, Vector2Int end, int subMesh)
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Vector2Int size = end - start;
        //Make vertices
        Vector3[] vertices = GetVec3Stream(start, end);
        mesh.vertices = vertices;

        //Make tris
        int[] indices = new int[(size.x-1)*(size.y-1)*6];
        for (int y = start.y; y < end.y -1; y++)
        {
            for (int x = start.x; x < end.x - 1; x++)
            {
                int initial = y * (res - 1) * 6 + x * 6;
                //First triangle
                indices[initial] = y * res + x; //0 at beginnning
                indices[initial + 1] = (y+1) * res + x; //2 at beginnning
                indices[initial + 2] = y * res + x + 1; //1 at beginnning
                //Second triangle
                indices[initial + 3] = (y+1) * res + x; //2 at beginnning
                indices[initial + 4] = (y+1) * res + x + 1; //3 at beginnning
                indices[initial + 5] = y * res + x + 1; //1 at beginnning
            }
        }
        mesh.SetIndices(indices, MeshTopology.Triangles, subMesh);
        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        int uvLength = uvs.Length;

        for (int i = 0; i < uvLength; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z); //TODO: Test
        }
        //Add UVs
        mesh.SetUVs(0, uvs);
        //GenerateColors();
        mesh.RecalculateNormals();
        //Add colors

        var mf = GetComponent<MeshFilter>();
        mf.mesh = mesh;
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
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        Color32[] colors = new Color32[mesh.vertices.Length];
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
        mesh.colors32 = colors;
    }
}
