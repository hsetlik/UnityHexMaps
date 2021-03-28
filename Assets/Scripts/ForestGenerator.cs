using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(NoiseGenerator))]
public class ForestGenerator : MonoBehaviour
{
    public HexChunkGroup map;
    public GameObject maplePrefab;
    public GameObject pinePrefab;
    public float maxElevation = 1.0f;
    public float minElevation;
    public int noiseOctaves = 3;
    public int treeDensity = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2.0f;
    private HexTileData[,] mapTiles;
    private NoiseGenerator noiseGen;
    private float[,] noiseMap;
    public void Init()
    {
        mapTiles = new HexTileData[map.xChunks * HexMetrics.chunkSize, map.zChunks * HexMetrics.chunkSize];
        noiseMap = new float[map.xChunks * HexMetrics.chunkSize, map.zChunks * HexMetrics.chunkSize];
        noiseGen = GetComponent<NoiseGenerator>();
        mapTiles = map.GetTiles();
        if(minElevation < map.waterLevel) //no underwater trees
        {
            minElevation = map.waterLevel; 
        }
        //Debug.Log("Setting forestation for: " + mapTiles.GetLength(0) + " by " + mapTiles.GetLength(1) + " tiles");
    }
    private void PrepareNoise()
    {
        int nWidth = map.xChunks * HexMetrics.chunkSize;
        int nHeight = map.zChunks* HexMetrics.chunkSize;
        int seed = map.noiseSeed;
        float scale = map.noiseScale;
        float water = map.waterLevel;
        noiseGen.CreateNoiseMap(nWidth, nHeight, seed, map.noiseOctaves, scale, map.noiseOffset, map.noisePersistence, map.lacunarity, water);
        noiseMap = noiseGen.GetFullMap(map.noiseCurve);
        //Debug.Log("Initialized forestation for: " + mapTiles.GetLength(0) + " by " + mapTiles.GetLength(1) + " tiles");
    }
    public void SetForestation(int x, int z)
    {
        float centerElev = minElevation + ((maxElevation - minElevation) / 2.0f);
        float elevation = mapTiles[x, z].Elevation;
        elevation /= map.noiseHeight;
        if(elevation < minElevation || elevation > maxElevation)
        {
            mapTiles[x, z].forestation = 0.0f;
            return;
        }
        float forestation = centerElev * (2.0f * noiseMap[x, z]);
        mapTiles[x, z].forestation = forestation;
        //Debug.Log("Forestation at: " + x + ", " + z + " is: " + mapTiles[x, z].forestation);
    }
    private void SetForestation()
    {
        
        for(int x = 0; x < mapTiles.GetLength(0); ++x)
        {
            for(int z = 0; z < mapTiles.GetLength(1); ++z)
            {
                SetForestation(x, z);
            }
        }
    }
    public void SpawnTrees(int x, int z)
    {
        HexTileData tile = mapTiles[x, z];
        if(tile.forestation == 0.0f)
        {
            return;
        }
        int numTrees = 1;
        for(int i = 0; i < numTrees; ++i) //create each tree;
        {
            Vector3 position = tile.RandomWithin(map.noiseSeed);
            Vector3 tileCenter = tile.Center3D;
            GameObject newTree = Instantiate(maplePrefab);
            newTree.tag = "Tree";
            newTree.transform.position = tileCenter;
        }
    }
    public void ClearTrees()
    {
        GameObject[] allTrees = GameObject.FindGameObjectsWithTag("Tree");
        for (int i = 0; i < allTrees.Length; ++i) //every time the forests are regenerated, start by destroying all existing trees
        {
            DestroyImmediate(allTrees[i]);
        }
    }
    public void GenerateForest()
    {
        ClearTrees();
        Init();
        PrepareNoise();
        SetForestation();
        for(int x = 0; x < mapTiles.GetLength(0); ++x)
        {
            for(int z = 0; z < mapTiles.GetLength(1); ++z)
            {
                SpawnTrees(x, z);
            }
        }

    }
}
