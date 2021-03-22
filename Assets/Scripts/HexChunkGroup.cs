using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexChunkGroup : MonoBehaviour
{
    public int xChunks;
    public int zChunks;
    public bool autoUpdate;
    int xCreated;
    int zCreated;
    private int numChunks;
    public GameObject chunk;
    private List<HexChunkDisplay> chunkDisplays;
    private List<HexChunk> chunks;
    private HexChunk[,] chunkMap;
    public HexChunkGroup()
    {
        xCreated = 0;
        zCreated = 0;
        chunkDisplays = new List<HexChunkDisplay>();
        chunks = new List<HexChunk>();
        numChunks = 0;
    }
    private void CreateChunk(int x, int z)
    {
        bool hasNeighborBelow = (z > 0);
        bool hasNeighborToRight = (x > 0);
        GameObject[] allChunks = GameObject.FindGameObjectsWithTag("HexChunk");
        numChunks = allChunks.Length;
        //Debug.Log(numChunks + " existing chunks");
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
        newChunk.CreateGrid(HexMetrics.chunkSize, HexMetrics.chunkSize);
        newChunk.CompleteGrid();
        if(hasNeighborBelow)
        {
            newChunk.AddNeighborChunk(true, chunkMap[x, z - 1]);
        }
        if(hasNeighborToRight)
        {
            //Debug.Log("Adding righthand neighbor of: " + (x - 1) + ", " + z + ". ChunkMap size is: "+ chunkMap.GetLength(0) + ", " + chunkMap.GetLength(1));
            newChunk.AddNeighborChunk(false, chunkMap[x - 1, z]);
        }
         //don't do this until after the edges are stitched

        Vector3 tVector = new Vector3(x * HexMetrics.chunkWidth, 0f, z * HexMetrics.chunkHeight);
        tVector += newChunkObject.transform.position;
        newChunkObject.transform.position = tVector;
        newDisplay.CreateMap();
    }
    public void Generate()
    {
        GameObject[] allChunks = GameObject.FindGameObjectsWithTag("HexChunk");
        for(int i = 0; i < allChunks.Length; ++i)
        {
            DestroyImmediate(allChunks[i]);
        }
        chunkDisplays.Clear();
        chunks.Clear();
        chunkMap = new HexChunk[xChunks, zChunks];
        for(int x = 0; x < xChunks; ++x)
        {
            for(int z = 0; z < zChunks; ++z)
            {
                //Debug.Log("Creating chunk: " + x + ", " + z);
                CreateChunk(x, z);
                zCreated++;
            }
            xCreated++;
        }
    }
    public void StitchEdges()
    {

    }

}
