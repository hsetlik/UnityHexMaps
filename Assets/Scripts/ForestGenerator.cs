using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestGenerator : MonoBehaviour
{
    public HexChunkGroup map;
    public GameObject maplePrefab;
    public GameObject pinePrefab;
    public float maxElevation = 1.0f;
    public float minElevation;
    private HexTileData[,] mapTiles;
    public void Init()
    {
        mapTiles = new HexTileData[map.xChunks * HexMetrics.chunkSize, map.zChunks * HexMetrics.chunkSize];
        mapTiles = map.GetTiles();
    }
    public void SetForestation()
    {
        for(int x = 0; x < mapTiles.GetLength(0); ++x)
        {
            for(int z = 0; z < mapTiles.GetLength(1); ++z)
            {

            }
        }
    }


}
