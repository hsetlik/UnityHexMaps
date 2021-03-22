using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMesh
{
    public HexMesh(int xOffset, int zOffset, int vertexOffset, float elevation = 0f)
    {
        zOff = zOffset;
        xOff = xOffset;
        corners = new List<Vector3>();
        neighbors = new HexMesh[6];
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();
        float centerX = ((float)xOffset * HexMetrics.innerRadius * 2f);
        float centerY = (float)zOffset * (HexMetrics.outerRadius + (HexMetrics.innerRadius / 2f));
        if ((zOffset & 1) == 1)
        {
            centerX += HexMetrics.innerRadius;
        }
        Vector3 center = new Vector3(centerX, elevation, centerY);
        vertices.Add(center);
        int triangleValues = 0;
        int numVertices = 1;
        for (int i = 0; i < 6; ++i)
        {
            vertices.Add(center + HexMetrics.corners[i] * HexMetrics.innerRatio);
            corners.Add(center + HexMetrics.corners[i] * HexMetrics.innerRatio);
            if (i != 5)
            {
                triangles.Add(vertexOffset);
                triangles.Add(vertexOffset + numVertices);
                triangles.Add(vertexOffset + numVertices + 1);
            }
            else if (i == 5)
            {
                triangles.Add(vertexOffset);
                triangles.Add(vertexOffset + numVertices);
                triangles.Add(vertexOffset + 1);
            }
            triangleValues += 3;
            ++numVertices;
        }
    }
    public void AddNeighbor(HexDirection dir, HexMesh n)
    {
        if(n != null)
        {
            this.neighbors[(int)dir] = n;
            n.neighbors[(int)dir.Opposite()] = this;
        }
    }
    public Vector3 ClosestCorner(Vector3 point)
    {
        Vector3 closest = new Vector3();
        float minDistance = float.MaxValue;
        for(int i = 0; i < 6; ++i)
        {
            float distance = Vector3.Distance(point, corners[i]);
            if(distance < minDistance)
            {
                closest = corners[i];
            }
        }
        return closest;
    }
    public Vector3 GetCorner(HexDirection dir)
    {
        return corners[(int)dir];
    }
    public Vector3[] GetVertices()
    {
        return vertices.ToArray();
    }
    public int[] GetTriangles()
    {
        return triangles.ToArray();
    }
    public Color[] GetColors()
    {
        return colors.ToArray();
    }
    public string GetCenterString()
    {
        return ("X: " + vertices[0].x.ToString() + ",Z: " + vertices[0].z.ToString());
    }
    public void SetElevation(float elev)
    {
        for(int i = 0; i < vertices.Count; ++i)
        {
            Vector3 newVec = vertices[i];
            newVec.y = elev;
            vertices[i] = newVec;
            if(i > 0)
            {
                corners[i - 1] = newVec;
            }
        }
    }
    HexMesh[] neighbors;
    public int xOff;
    public int zOff;
    private int xPos;
    public int CoordX
    {
        get { return xOff + xPos; }
        set { xPos = value; }
    }
    private int zPos;
    public int CoordZ
    {
        get { return zOff + zPos; }
        set { zPos = value; }
    }
    public List<Vector3> vertices;
    public List<Vector3> corners;
    public List<int> triangles;
    List<Color> colors;
}

public class HexChunk : MonoBehaviour
{
    public HexMesh[] hexMeshes;
    int mWidth;
    int mHeight;
    private int xOff;
    public int OffsetX
    {
        get { return xOff; }
        set {
            xOff = value;
            for(int i = 0; i < hexMeshes.Length; ++i)
            {
                hexMeshes[i].CoordX = value;
            }
        }
    }
    private int zOff;
    public int OffsetZ
    {
        get { return zOff; }
        set
        {
            zOff = value;
            for (int i = 0; i < hexMeshes.Length; ++i)
            {
                hexMeshes[i].CoordZ = value;
            }
        }
    }

    public List<Vector3> lVertices;
    public List<int> lTriangles;
    public void CreateGrid(int width, int height)
    {
        mWidth = width;
        mHeight = height;
        lVertices = new List<Vector3>();
        lTriangles = new List<int>();
        hexMeshes = new HexMesh[width * height];
        int hexIndex = 0;
        int vOffset = 0;
        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                hexMeshes[hexIndex] = new HexMesh(x, z, hexIndex * 7 + vOffset);
                lVertices.AddRange(hexMeshes[hexIndex].GetVertices());
                lTriangles.AddRange(hexMeshes[hexIndex].GetTriangles());
                ++hexIndex;
            }
        }
        Debug.Log("Grid Created. Vertices: " + lVertices.Count + " Triangles: " + lTriangles.Count);
    }
    public bool ExistsInList(int[] array, List<int> list)
    {
        int aLength = array.Length;
        int lLength = list.Count;
        for(int lIdx = 0; lIdx < lLength - aLength; ++lIdx)
        {
            bool allMatch = true;
            List<int> checkList = list.GetRange(lIdx, aLength);
            for (int aIdx = 0; aIdx < aLength; ++aIdx)
            {
                if(checkList[aIdx] != array[aIdx])
                {
                    allMatch = false;
                    break;
                }
            }
            if(allMatch)
            {
                return true;
            }
        }
        return false;
    }
    public void AddHexMesh(Mesh mesh, int width, int height)
    {
        CreateGrid(width, height);
        CompleteGrid();
        ApplyToMesh(mesh);
        //mesh.RecalculateNormals();
    }
    public void CompleteGrid()
    {
        for (int x = 0; x < mWidth; ++x)
        {
            for (int z = 0; z < mHeight; ++z)
            {
                for (HexDirection dir = HexDirection.NE; dir < HexDirection.SW; ++dir)
                {
                    HexMesh currentHex = hexMeshes[(mHeight * x) + z];
                    if (GetNeighbor(dir, currentHex) != null)
                    {
                        AddBridge(dir, currentHex);
                        AddTriangle(dir, currentHex);
                    }
                }
            }
        }
        Debug.Log("Grid Completed. Vertices: " + lVertices.Count + " Triangles: " + lTriangles.Count);
    }
    public void ApplyToMesh(Mesh mesh)
    {
        mesh.vertices = lVertices.ToArray();
        mesh.triangles = lTriangles.ToArray();
    }
    HexMesh GetNeighbor(HexDirection direction, HexMesh center)
    {
        int x = center.xOff;
        int z = center.zOff;
        int checkIndex = 0;
        bool rowIsEven = false;
        if ((center.zOff & 1) != 1)
        {
            rowIsEven = true;
        }
        switch (direction)
        {
            case HexDirection.NE:
                {
                    checkIndex = (rowIsEven) ? ((x * mHeight) + z + 1) : (((x + 1) * mHeight) + z + 1);
                    break;
                }
            case HexDirection.E:
                {
                    checkIndex = ((x + 1) * mHeight) + z;
                    break;
                }
            case HexDirection.SE:
                {
                    checkIndex = (rowIsEven) ? ((x * mHeight) + z - 1) : (((x + 1) * mHeight) + z - 1);
                    break;
                }
            case HexDirection.SW:
                {
                    checkIndex = (rowIsEven) ? (((x - 1) * mHeight) + z - 1) : ((x * mHeight) + z - 1);
                    break;
                }
            case HexDirection.W:
                {
                    checkIndex = ((x - 1) * mHeight) + z;
                    break;
                }
            case HexDirection.NW:
                {
                    checkIndex = (rowIsEven) ? (((x - 1) * mHeight) + z + 1) : ((x * mHeight) + z + 1);
                    break;
                }
        }

        if (checkIndex >= 0 && checkIndex < hexMeshes.Length)
        {
            HexMesh neighbor = hexMeshes[checkIndex];
            int dZ = Mathf.Abs(neighbor.zOff - center.zOff);
            int dX = Mathf.Abs(neighbor.xOff - center.xOff);
            if (dZ > 1 || dX > 1)
            {
                return null;
            }
            else
            {
                return neighbor;
            }

        }
        return null;
    }

    public void AddBridgeOutside(HexDirection dir, HexMesh from, HexMesh to)
    {
        from.AddNeighbor(dir, to);
        Vector3[] vertices = new Vector3[4];
        vertices[0] = from.GetCorner(dir);
        vertices[1] = from.GetCorner(dir.Next());
        vertices[2] = to.ClosestCorner(vertices[0]);
        vertices[3] = to.ClosestCorner(vertices[1]);
        lVertices.AddRange(vertices);
        int[] triangles = {
                lVertices.IndexOf(vertices[0]),
                lVertices.IndexOf(vertices[2]),
                lVertices.IndexOf(vertices[1]),
                lVertices.IndexOf(vertices[2]),
                lVertices.IndexOf(vertices[0]),
                lVertices.IndexOf(vertices[3])
        };
        if (!ExistsInList(triangles, lTriangles))
        {
            lTriangles.AddRange(triangles);
        }
        Debug.Log("Outside bridge added. Vertices: " + lVertices.Count + " Triangles: " + lTriangles.Count);
    }

    public void AddBridge(HexDirection direction, HexMesh hex)
    {
        HexMesh neighbor = GetNeighbor(direction, hex);
        if(neighbor == null)
        {
            return;
        }
        hex.AddNeighbor(direction, neighbor);
        Vector3[] vertices = new Vector3[4];
        vertices[0] = hex.GetCorner(direction);
        vertices[1] = hex.GetCorner(direction.Next());
        vertices[2] = neighbor.GetCorner(direction.Opposite());
        vertices[3] = neighbor.GetCorner(direction.Opposite().Next());
        int[] triangles = {
                lVertices.IndexOf(vertices[0]),
                lVertices.IndexOf(vertices[2]),
                lVertices.IndexOf(vertices[1]),
                lVertices.IndexOf(vertices[2]),
                lVertices.IndexOf(vertices[0]),
                lVertices.IndexOf(vertices[3])
        };
        if(!ExistsInList(triangles, lTriangles)) //double check to not add duplicate triangles
        {
            lTriangles.AddRange(triangles);
        }
    }

    public void AddTriangle(HexDirection direction, HexMesh hex)
    {
        if(direction != HexDirection.NE && direction != HexDirection.E)
        {
            return;
        }
        switch(direction)
        {
            case HexDirection.NE:
                {
                    HexDirection dir = direction.Next();
                    HexMesh n1 = GetNeighbor(HexDirection.NE, hex);
                    HexMesh n2 = GetNeighbor(HexDirection.E, hex);
                    if (n1 == null || n2 == null)
                    {
                        return;
                    }
                    Vector3[] vertices = new Vector3[3];
                    vertices[0] = hex.GetCorner(dir);
                    vertices[1] = n1.GetCorner(HexDirection.SW);
                    vertices[2] = n2.GetCorner(HexDirection.NW);
                    int[] triangles =
                    {
                        lVertices.IndexOf(vertices[0]),
                        lVertices.IndexOf(vertices[1]),
                        lVertices.IndexOf(vertices[2])
                    };
                    if (!ExistsInList(triangles, lTriangles))
                    {
                        lTriangles.AddRange(triangles);
                    }
                    break;
                }
            case HexDirection.E:
                {
                    HexDirection dir = direction.Next();
                    HexMesh n1 = GetNeighbor(HexDirection.E, hex);
                    HexMesh n2 = GetNeighbor(HexDirection.SE, hex);
                    if(n1 == null || n2 == null)
                    {
                        return;
                    }
                    Vector3[] vertices = new Vector3[3];
                    vertices[0] = hex.GetCorner(dir);
                    vertices[1] = n1.GetCorner(HexDirection.W);
                    vertices[2] = n2.GetCorner(HexDirection.NE);
                    int[] triangles =
                    {
                        lVertices.IndexOf(vertices[0]),
                        lVertices.IndexOf(vertices[1]),
                        lVertices.IndexOf(vertices[2])
                    };
                    if (!ExistsInList(triangles, lTriangles))
                    {
                        lTriangles.AddRange(triangles);
                    }
                    break;
                }
        }
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        AddHexMesh(mesh, HexMetrics.chunkSize, HexMetrics.chunkSize);
        return mesh;
    }

    public void AddNeighborChunk(bool isAbove, HexChunk neighbor)
    {
        if(isAbove)
        {
            for(int i = 0; i < HexMetrics.chunkSize; ++i)
            {
                HexMesh from = hexMeshes[HexMetrics.chunkSize * i];
                HexMesh to = neighbor.hexMeshes[(HexMetrics.chunkSize * i) + (HexMetrics.chunkSize - 1)];
                AddBridgeOutside(HexDirection.SE, from, to);
                /*
                if(i > 0)
                {
                    HexMesh toSW = neighbor.hexMeshes[(HexMetrics.chunkSize * (i + 1)) + (HexMetrics.chunkSize - 1)];
                    AddBridgeOutside(HexDirection.SW, from, toSW);
                }
                */
            }
        }

    }

    public void SetElevation(float[,] heightMap)
    {
        for (int x = 0; x < mWidth; ++x)
        {
            for(int z = 0; z < mHeight; ++z)
            {
                hexMeshes[(mHeight * x) + z].SetElevation(heightMap[x, z]);
            }
        }
    }
}
