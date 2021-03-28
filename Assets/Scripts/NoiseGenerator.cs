using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public void CreateNoiseMap(int width, int height, int seed, int octaves,
        float noiseScale, Vector2 offset, float persistence, float lacunarity, float waterLevel)
    {
        fullMap = new float[width, height];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; ++i)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        float halfWidth = (width) / 2.0f;
        float halfHeight = (height) / 2.0f;
        for (int z = 0; z < height; ++z)
        {
            for(int x = 0; x < width; ++x)
            {
                float amplitude = 1.0f;
                float frequency = 1.0f;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; ++i)
                {
                    float halfX = x - halfWidth;
                    float halfY = z - halfHeight;
                    float sampleX = halfX / noiseScale * frequency + octaveOffsets[i].x;
                    float sampleY = halfY / noiseScale * frequency + octaveOffsets[i].y;
                    float pValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += pValue * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                
                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;
                fullMap[x, z] = noiseHeight;
            }
        }
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                fullMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, fullMap[x, y]);
                if(fullMap[x, y] < waterLevel)
                {
                    fullMap[x, y] = waterLevel;
                }
            }
        }
    }
    private float[,] fullMap;
    public float[,] GetSubMap(int width, int height, int xOffset, int zOffset, AnimationCurve curve)
    {
        float[,] subMap = new float[width, height];
        for(int x = 0; x < width; ++x)
        {
           for(int z = 0; z < height; ++z)
            {
                float value = fullMap[(xOffset * width) + x, (zOffset * height) + z];
                subMap[x, z] = curve.Evaluate(value);
            }
        }
        return subMap;
    }
}