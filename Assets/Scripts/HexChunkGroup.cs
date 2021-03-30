using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(ForestGenerator), typeof(RiverGenerator))]
public class HexChunkGroup : MonoBehaviour
{
    public int xChunks;
    public int zChunks;
    public Gradient landGradient;
    public Gradient moistureGradient;
    public float waterLevel;
    public float noiseScale;
    public float noiseHeight;
    public int noiseSeed;
    public int noiseOctaves;
    public Vector2 noiseOffset;
    public AnimationCurve noiseCurve;
    public float noisePersistence = 0.4f;
    public float lacunarity = 1.3f;
    public float treeDensity = 4.0f;
    public float treeLower = 0.47f;
    public float treeUpper = 0.9f;
    public bool autoUpdate;
    private int numChunks;
    public GameObject chunk;
    private List<HexChunkDisplay> chunkDisplays;
    private List<HexChunk> chunks;
    public HexChunk[,] chunkMap;
    private HexChunkDisplay[,] displayMap;
    private HexTileData[,] allTiles;
    private NoiseGenerator landNoiseGen;
    private NoiseGenerator forestNoiseGen;
    public ForestGenerator forestGen;
    public RiverGenerator riverGen;
    private MoistureMapCreator moistureMap;
    public HexTileData[,] GetTiles()
    {
        return allTiles;
    }
    public HexChunk GetChunkAt(int globalX, int globalZ)
    {
        int chunkX = Mathf.FloorToInt((float)globalX / HexMetrics.chunkSize);
        int chunkZ = Mathf.FloorToInt((float)globalZ / HexMetrics.chunkSize);
        return chunkMap[chunkX, chunkZ];
    }
    public HexMesh GetMeshAt(int globalX, int globalZ)
    {
        int meshX = globalX % HexMetrics.chunkSize;
        int meshZ = globalZ % HexMetrics.chunkSize;
        return GetChunkAt(globalX, globalZ).hexMeshes[meshX, meshZ];
    }
    public HexChunkGroup()
    {
        chunkDisplays = new List<HexChunkDisplay>();
        chunks = new List<HexChunk>();
        //landGradient = new Gradient();
        numChunks = 0;
    }
    private void AddChunkToTiles(int xPos, int zPos, HexTileData[,] input)
    {
        int width = input.GetLength(0);
        int height = input.GetLength(1);
        for(int x = 0; x < width; ++x)
        {
            for(int z = 0; z < height; ++z)
            {
                int iX = (width * xPos) + x;
                int iZ = (height * zPos) + z;
                allTiles[iX, iZ] = new HexTileData();
                allTiles[iX, iZ] = input[x, z];
            }
        }
    }
    private void CreateChunk(int x, int z)
    {
        
        GameObject[] allChunks = GameObject.FindGameObjectsWithTag("HexChunk");
        numChunks = allChunks.Length;
        GameObject newChunkObject = Instantiate(chunk);
        newChunkObject.tag = "HexChunk";
        HexChunkDisplay newDisplay = newChunkObject.GetComponent<HexChunkDisplay>();
        HexChunk newChunk = newDisplay.Chunk;
        chunkDisplays.Add(newDisplay);
        chunks.Add(newChunk);
        newDisplay.OffsetX = x;
        newDisplay.OffsetZ = z;
        chunkMap[x, z] = newChunk;
        displayMap[x, z] = newDisplay;
        Vector3 tVector = new Vector3(x * HexMetrics.chunkWidth, 0f, z * HexMetrics.chunkHeight);
        tVector += newChunkObject.transform.position;
        newChunk.CreateGrid(HexMetrics.chunkSize,
        HexMetrics.chunkSize,
        landNoiseGen.GetSubMap(HexMetrics.chunkSize, HexMetrics.chunkSize, x, z, noiseCurve),
        noiseHeight);
        newChunk.Translate(tVector);
        
        if (x > 0)
        {
            newChunk.AddNeighborChunk(false, chunkMap[x - 1, z]);
        }
        if(z > 0)
        {
            newChunk.AddNeighborChunk(true, chunkMap[x, z - 1]);
        }
        if(x > 0 && z > 0) //stitch on the corner pieces if the chunk has an unfilled corner
        {
            HexChunk left = chunkMap[x - 1, z];
            HexChunk below = chunkMap[x, z - 1];
            HexChunk corner = chunkMap[x - 1, z - 1];
            newChunk.FillCorner(left, below, corner);
        }
        newChunk.SetColors(landGradient, noiseHeight, 0.0f);
        float[,] moisture = moistureMap.noiseGen.GetSubMap(HexMetrics.chunkSize, HexMetrics.chunkSize, x, z, noiseCurve);
        newChunk.SetColors(moistureGradient, moisture);
        AddChunkToTiles(x, z, newChunk.GetTileData(x, z));
        //don't do this until after the edges are stitched
        newDisplay.CreateMap();
    }
    public HexMesh GetMeshAt(Vector3 location)
    {
        for(int x = 0; x < chunkMap.GetLength(0); ++x)
        {
            for (int z = 0; z < chunkMap.GetLength(1); ++z)
            {
                if(chunkMap[x, z].translationVector.magnitude < location.magnitude)
                {
                    return chunkMap[x, z].GetClosestMesh(location);
                }
            }
        }
        return null;
    }
    public void Generate()
    {
        forestGen = GetComponent<ForestGenerator>();
        moistureMap = GetComponent<MoistureMapCreator>();
        landNoiseGen = new NoiseGenerator();
        forestNoiseGen = new NoiseGenerator();
        landNoiseGen.CreateNoiseMap(xChunks * HexMetrics.chunkSize, zChunks * HexMetrics.chunkSize,
            noiseSeed,
            noiseOctaves,
            noiseScale,
            noiseOffset,
            noisePersistence,
            lacunarity,
            waterLevel);
        forestNoiseGen.CreateNoiseMap(xChunks * HexMetrics.chunkSize, zChunks * HexMetrics.chunkSize,
            noiseSeed,
            noiseOctaves,
            noiseScale,
            noiseOffset,
            noisePersistence,
            lacunarity,
            waterLevel);
        moistureMap.CreateMoistureMap(xChunks * HexMetrics.chunkSize, zChunks * HexMetrics.chunkSize, waterLevel, noiseScale);
        GameObject[] allChunks = GameObject.FindGameObjectsWithTag("HexChunk");
        for (int i = 0; i < allChunks.Length; ++i)
        {
            DestroyImmediate(allChunks[i]);
        }
        chunkDisplays.Clear();
        chunks.Clear();
        chunkMap = new HexChunk[xChunks, zChunks];
        displayMap = new HexChunkDisplay[xChunks, zChunks];
        allTiles = new HexTileData[xChunks * HexMetrics.chunkSize, zChunks * HexMetrics.chunkSize];
        for (int x = 0; x < xChunks; ++x)
        {
            for (int z = 0; z < zChunks; ++z)
            {
                CreateChunk(x, z);
            }
        }
    }
    public float[,] TreeLimitedMap(float[,] input)
    {
        var width = input.GetLength(0);
        var height = input.GetLength(1);
        float[,] output = new float[width, height];
        for(int x = 0; x < width; ++x)
        {
            for(int z = 0; z < height; ++z)
            {
                if (input[x, z] > 1.0f || input[x, z] < 0.0f)
                {
                    Debug.Log("Error! Input level is: " + input[x, z]);
                }
                if(input[x, z] < treeLower || input[x, z] > treeUpper || input[x, z] < waterLevel)
                {
                    output[x, z] = 0.0f;
                }
                else
                {
                    output[x, z] = input[x, z];
                }
            }
        }
        return output;
    }
    public Vector3 NearestVertex(Mesh mesh, Vector3 point)
    {
        int vCount = mesh.vertexCount;
        Vector3 output = Vector3.zero;
        float minDistance = float.MaxValue;
        for(int i = 0; i < vCount; ++i)
        {
            float distance = Vector3.Distance(point, mesh.vertices[i]);
            if(distance < minDistance)
            {
                minDistance = distance;
                output = mesh.vertices[i];
            }
        }
        return output;
    }
    public List<Vector3> TreeLocations()
    {
        var list = new List<Vector3>();
        float[,] noiseMap = TreeLimitedMap(forestNoiseGen.GetFullMap(noiseCurve));
        for (int x = 0; x < noiseMap.GetLength(0); ++x)
        {
            for(int z = 0; z < noiseMap.GetLength(1); ++z)
            {
                int numTrees = Mathf.FloorToInt(noiseMap[x, z] * treeDensity);
                if(numTrees > 0)
                {
                    for(int i = 0; i < numTrees; ++i)
                    {
                        int displayX = Mathf.FloorToInt((float)x / HexMetrics.chunkSize);
                        int displayZ = Mathf.FloorToInt((float)z / HexMetrics.chunkSize);
                        var chunkFilter = displayMap[displayX, displayZ].Filter;
                        var fPos = chunkFilter.transform.position;
                        var cPoint = allTiles[x, z].RandomWithin();
                        var adjPoint = cPoint + fPos;
                        list.Add(adjPoint);
                    }
                }
            }
        }
        //Debug.Log(dStr);
        return list;
    }
}
