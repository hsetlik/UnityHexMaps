using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float noisePersistence = 0.4f;
    public float lacunarity = 1.3f;
    public bool autoUpdate;
    
    private int numChunks;
    
    public GameObject chunk;
    private List<HexChunkDisplay> chunkDisplays;
    private List<HexChunk> chunks;
    private HexChunk[,] chunkMap;
    private float maxElev;
    private float minElev;
    public HexChunkGroup()
    {
        maxElev = float.MinValue;
        minElev = float.MaxValue;
        chunkDisplays = new List<HexChunkDisplay>();
        chunks = new List<HexChunk>();
        //landGradient = new Gradient();
        numChunks = 0;
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
        NoiseGenerator.CreateNoiseMap(HexMetrics.chunkSize, x, HexMetrics.chunkSize, z, noiseSeed, noiseOctaves, noiseScale, noiseHeight, noiseOffset, noisePersistence, lacunarity));
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
        newChunk.SetColors(landGradient, newChunk.GetMaxHeight(), newChunk.GetMinHeight());
        //don't do this until after the edges are stitched
        newDisplay.CreateMap();
    }
    public void Generate()
    {
        GameObject[] allChunks = GameObject.FindGameObjectsWithTag("HexChunk");
        for (int i = 0; i < allChunks.Length; ++i)
        {
            DestroyImmediate(allChunks[i]);
        }
        chunkDisplays.Clear();
        chunks.Clear();
        chunkMap = new HexChunk[xChunks, zChunks];
        for (int x = 0; x < xChunks; ++x)
        {
            for (int z = 0; z < zChunks; ++z)
            {
                //Debug.Log("Creating chunk: " + x + ", " + z);
                CreateChunk(x, z);
            }
        }
    }

}
