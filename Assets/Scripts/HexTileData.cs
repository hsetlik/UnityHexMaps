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
    public HexTileData()
    {

    }
    public HexTileData(int xOff, int zOff, Vector3 centerPoint, HexMesh parent)
    {
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
    }
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
