using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NoiseGenerator))]
public class HexChunkGroup : MonoBehaviour
{
    public int xChunks;
    public int zChunks;
    public Gradient landGradient;
    public float waterLevel;
    public float noiseScale;
    public float noiseHeight;
    public int noiseSeed;
    public int noiseOctaves;
    public Vector2 noiseOffset;
    public AnimationCurve noiseCurve;
    public float noisePersistence = 0.4f;
    public float lacunarity = 1.3f;
    public bool autoUpdate;
    private int numChunks;
    public GameObject chunk;
    private List<HexChunkDisplay> chunkDisplays;
    private List<HexChunk> chunks;
    private HexChunk[,] chunkMap;
    private HexTileData[,] allTiles;
    private NoiseGenerator noiseGen;
    public HexTileData[,] GetTiles()
    {
        return allTiles;
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
                allTiles[iX, iZ] = input[x, z];
            }
        }
    }
    private void CreateChunk(int x, int z)
    {
        
        GameObject[] allChunks = GameObject.FindGameObjectsWithTag("HexChunk");
        numChunks = allChunks.Length;
        if((x * zChunks) + z < numChunks) //don't make a chunk if it already exists
        {
            return;
        }
        GameObject newChunkObject = Instantiate(chunk);
        newChunkObject.tag = "HexChunk";
        HexChunkDisplay newDisplay = newChunkObject.GetComponent<HexChunkDisplay>();
        HexChunk newChunk = newDisplay.Chunk;
        chunkDisplays.Add(newDisplay);
        chunks.Add(newChunk);
        newDisplay.OffsetX = x;
        newDisplay.OffsetZ = z;
        chunkMap[x, z] = newChunk;
        Vector3 tVector = new Vector3(x * HexMetrics.chunkWidth, 0f, z * HexMetrics.chunkHeight);
        tVector += newChunkObject.transform.position;
        newChunk.CreateGrid(HexMetrics.chunkSize,
        HexMetrics.chunkSize,
        noiseGen.GetSubMap(HexMetrics.chunkSize, HexMetrics.chunkSize, x, z, noiseCurve),
        noiseHeight);
        newChunk.Translate(tVector);
        AddChunkToTiles(x, z, newChunk.GetTileData(x, z));
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
        noiseGen = GetComponent<NoiseGenerator>();
        noiseGen.CreateNoiseMap(xChunks * HexMetrics.chunkSize, zChunks * HexMetrics.chunkSize,
            noiseSeed,
            noiseOctaves,
            noiseScale,
            noiseOffset,
            noisePersistence,
            lacunarity,
            waterLevel);
        GameObject[] allChunks = GameObject.FindGameObjectsWithTag("HexChunk");
        for (int i = 0; i < allChunks.Length; ++i)
        {
            DestroyImmediate(allChunks[i]);
        }
        chunkDisplays.Clear();
        chunks.Clear();
        chunkMap = new HexChunk[xChunks, zChunks];
        allTiles = new HexTileData[xChunks * HexMetrics.chunkSize, zChunks * HexMetrics.chunkSize];
        for (int x = 0; x < xChunks; ++x)
        {
            for (int z = 0; z < zChunks; ++z)
            {
                CreateChunk(x, z);
            }
        }
    }

}
