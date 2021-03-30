using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RiverPieceData
{
    RiverPieceData()
    {

    }
    RiverPieceData(int _x, int _z, HexDirection direction)
    {
        x = _x;
        z = _z;
        dir = direction;
    }
    public int x;
    public int z;
    public HexDirection dir;
}

public class River
{
    private List<Vector3> lVertices;
    private List<int> lTriangles;
    private List<Vector2> lUvs;
    private List<RiverPieceData> pieces;
    public void Init()
    {
        pieces = new List<RiverPieceData>();
        lVertices = new List<Vector3>();
        lTriangles = new List<int>();
        lUvs = new List<Vector2>();
    }
    public void CalculateRiver(HexChunkGroup map, int startX, int startZ)
    {
        //TODO: this needs to go through all the elevations and stuff to fill out the list of pieces;
    }
    public Mesh GetMeshOnMap(HexChunkGroup map)
    {
        Mesh mesh = new Mesh();
        //TODO: have this parse the pieces into their relevant vertices and triangles, then parse that into a mesh
        return mesh;
    }
    public void AddSideToRiver(int x, int z, HexDirection dir, HexChunkGroup map)
    {

        HexMesh mesh = map.GetMeshAt(x, z);
        HexChunk chunk = map.GetChunkAt(x, z);
        if (dir < HexDirection.SE)
        {
            //TODO: reset chunk and mesh such that same quad and triangle are added from a neighboring hex in the opposite direction
        }
        AddQuadTriangles();
        int vStart = mesh.quadVertexIndeces[dir];
        for (int i = 0; i < 4; ++i)
        {
            lVertices.Add(chunk.lVertices[vStart + i]);
            AddUV(chunk.lVertices[vStart + i]);
        }
        AddTriTriangles();
        vStart = mesh.triVertexIndeces[dir];
        for (int i = 0; i < 3; ++i)
        {
            lVertices.Add(chunk.lVertices[vStart + i]);
            AddUV(chunk.lVertices[vStart + i]);
        }
    }
    private void AddUV(Vector3 vec)
    {
        var newPoint = new Vector2(vec.x, vec.z);
        lUvs.Add(newPoint);
    }
    private void AddTriTriangles()
    {
        int start = lVertices.Count;
        int[] tris =
        {
            start,
            start + 1,
            start + 2
        };
        lTriangles.AddRange(tris);
    }
    private void AddQuadTriangles()
    {
        int start = lVertices.Count;
        int[] tris =
        {
            start,
            start + 1,
            start + 2,
            start,
            start + 2,
            start + 3
        };
        lTriangles.AddRange(tris);
    }
    public Vector2[] GetUVs()
    {
        int size = lUvs.Count;
        Vector2[] arr = new Vector2[size];
        float maxX = float.MinValue;
        float minX = float.MaxValue;
        float maxZ = float.MinValue;
        float minZ = float.MaxValue;
        for (int i = 0; i < size; ++i)
        {
            float x = lUvs[i].x;
            float z = lUvs[i].y;
            if (x > maxX)
                maxX = x;
            if (x < minX)
                minX = x;
            if (z > maxZ)
                maxZ = z;
            if (z < minZ)
                minZ = z;
        }
        for (int i = 0; i < size; ++i)
        {
            float x = maxX / lUvs[i].x;
            float z = maxZ / lUvs[i].y;
            arr[i] = new Vector2(x, z);
        }
        return arr;
    }

}

public class RiverGenerator : MonoBehaviour
{
    public HexChunkGroup map;
    public GameObject riverPrefab;
    
    public void StartRiverAt(int x, int z, HexDirection dir)
    {
        if(dir < HexDirection.SE)
        {
            return;
        }
        HexMesh startingMesh = map.GetMeshAt(x, z);
        HexChunk startingChunk = map.GetChunkAt(x, z);

    }
    //adds a corner triangle's triangles and verices as well as those of the quad in the same direction
    //should add a total of seven vertices and three triangles
    
}
