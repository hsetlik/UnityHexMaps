using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoistureMapCreator : MonoBehaviour
{

    public NoiseGenerator noiseGen;
    public AnimationCurve moistureCurve;
    public int noiseOctaves = 3;
    public int noiseSeed = 6;
    public Vector2 noiseOffset;
    public float persistence = 0.5f;
    public float lacunarity = 2.0f;
    public void Init()
    {
        noiseGen = new NoiseGenerator();
    }
    public float[,] GetMoistureMap(int width, int height, float waterLevel, float noiseScale)
    {
        Init();
        float[,] map = new float[width, height];
        noiseGen.CreateNoiseMap(width, height, noiseSeed, noiseOctaves, noiseScale, noiseOffset, persistence, lacunarity, waterLevel);
        map = noiseGen.GetFullMap();
        return map;
    }
    public void CreateMoistureMap(int width, int height, float waterLevel, float noiseScale)
    {
        Init();
        noiseGen.CreateNoiseMap(width, height, noiseSeed, noiseOctaves, noiseScale, noiseOffset, persistence, lacunarity, 0.0f);
    }
}
