using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RiverCorners
{
    private bool[] corners;
    public RiverCorners()
    {
        corners = new bool[6];
        for(int i = 0; i < 6; ++i)
        {
            corners[i] = false;
        }
    }
    public bool this[int index]
    {
        get { return corners[index]; }
        set { corners[index] = value; }
    }
}


public class RiverSides
{
    private bool[] sides;
    private RiverCorners corners;
    public RiverSides()
    {
        corners = new RiverCorners();
        sides = new bool[6];
        for (int i = 0; i < 6; ++i)
        {
            sides[i] = false;
        }
    }
    public bool this[int index]
    {
        get { return sides[index]; }
        set
        {
            sides[index] = value;
            if (sides[Next(index)] == value)
                corners[index] = value;
        } //this needs to recalculate the corners as well;
    }
    private int Next(int index)
    {
        int value = index + 1;
        if (value >= 6)
            value -= 6;
        return value;
    }
}


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
    private RiverSides riverSides;
    public HexTileData()
    {
        corners = new Vector3[6];
        riverSides = new RiverSides();
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
        riverSides = new RiverSides();
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
        float cushion = 0.85f;
        var xRand = UnityEngine.Random.value;
        var zRand = UnityEngine.Random.value;
        var xMin = Center3D.x - (HexMetrics.innerRadius * HexMetrics.innerRatio * cushion);
        var xMax = Center3D.x + (HexMetrics.innerRadius * HexMetrics.innerRatio * cushion);
        var zMin = Center3D.z - (HexMetrics.outerRadius * HexMetrics.innerRatio * cushion);
        var zMax = Center3D.z + (HexMetrics.outerRadius * HexMetrics.innerRatio * cushion);
        var x = Mathf.Lerp(xMin, xMax, xRand);
        var z = Mathf.Lerp(zMin, zMax, zRand);
        return new Vector3(x, Center3D.y, z);
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
