using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileData
{
    private int globalOffX;
    private int globalOffZ;
    private int hexX;
    private int hexY;
    private int hexZ;
    private Vector3 centerPos;
    private HexMesh parentMesh;
    public float wetness;
    public float forestation;
    private Vector3[] corners;
    public HexTileData()
    {
        corners = new Vector3[6];
        globalOffX = 0;
        globalOffZ = 0;
        centerPos = new Vector3(0f, 0f, 0f);
        parentMesh = null;
        wetness = 0.0f;
        forestation = 0.0f;
    }
    public HexTileData(int xOff, int zOff, Vector3 centerPoint, HexMesh parent)
    {
        corners = new Vector3[6];
        globalOffX = xOff;
        globalOffZ = zOff;
        centerPos = centerPoint;
        parentMesh = parent;
        wetness = 0.0f;
        forestation = 0.0f;
        if ((zOff & 1) == 0)
            hexX = xOff - (zOff) / 2;
        else
            hexX = xOff - (zOff - 1) / 2;
        hexZ = zOff;
        hexY = -hexX - hexZ;
        int sum = hexX + hexY + hexZ;
        if(sum != 0)
        {
            Debug.Log("Error! Cube coordinates for tile: " + xOff + ", " + zOff + " have nonzero sum: " + sum + "\n" +
                "HexX: " + hexX + "\n" +
                "HexZ: " + hexZ + "\n" +
                "HexY: " + hexY + "\n");
        }
        for(int i = 0; i < 6; ++i)
        {
            corners[i] = parent.GetCorner((HexDirection)i) + centerPoint;
        }
    }
    public Vector3 Corner(HexDirection dir) { return corners[(int)dir]; }
    public Vector3 Corner(int dir) { return corners[dir]; }
    public Vector3 RandomWithin() //returns a random point on the surface of the hex somewhere within its corners
    {
        System.Random prng = new System.Random(hexY);
        float dxMax = Mathf.Abs(Corner(1).x - centerPos.x);
        float dZMax = Mathf.Abs(Corner(0).z - centerPos.z);
        double xRand = (prng.NextDouble() * 2.0f) - 1.0f;
        double zRand = (prng.NextDouble() * 2.0f) - 1.0f;
        float dX = (float)xRand * dxMax;
        float dZ = (float)zRand * dZMax;
        Vector3 offset = new Vector3(dX, 0.0f, dZ);
        return (centerPos + offset);
    }
    public int HexX { get { return hexX; } }
    public int HexY { get { return hexY; } }
    public int HexZ { get { return hexZ; } }
    public int OffsetX
    {
        get { return globalOffX; }
    }
    public int OffsetZ
    {
        get { return globalOffZ; }
    }
    public Vector3 Center3D
    {
        get { return centerPos; }
    }
    public Vector2 CenterXZ
    {
        get { return new Vector2(centerPos.x, centerPos.z); }
    }
    public float Elevation
    {
        get { return centerPos.y; }
    }
}
