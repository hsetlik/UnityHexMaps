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
    public void Translate(Vector3 delta)
    {
        for(int i = 0; i < 7; ++i)
        {
            vertices[i] += delta;
            if(i > 0)
            {
                corners[i] += delta;
            }
        }
    }
    public Vector3 ClosestCorner(Vector3 point) //note: this funtcion works in terms of distance on the X-Z plane
    {
        Vector3 closest = new Vector3();
        float minDistance = float.MaxValue;
        for(int i = 0; i < 6; ++i)
        {
            float dX = Mathf.Abs(corners[i].x - point.x);
            float dZ = Mathf.Abs(corners[i].z - point.z);
            float distance = Mathf.Sqrt(Mathf.Pow(dX, 2) + Mathf.Pow(dZ, 2));
            if(distance < minDistance)
            {
                minDistance = distance;
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
    public HexMesh[,] hexMeshes;
    int mWidth;
    int mHeight;
    private Vector3 translationVector;
    public int widthInChunks; //TODO: make sure these get set up correctly
    public int heightInChunks;
    private int xOff;
    public int OffsetX
    {
        get { return xOff; }
        set {
            xOff = value;
            for (int x = 0; x < HexMetrics.chunkSize; ++x)
            {
                for (int z = 0; z < HexMetrics.chunkSize; ++z)
                {
                    hexMeshes[x, z].CoordX = value;
                }
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
            for (int x = 0; x < HexMetrics.chunkSize; ++x)
            {
                for (int z = 0; z < HexMetrics.chunkSize; ++z)
                {
                    hexMeshes[x, z].CoordZ = value;
                }
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
        hexMeshes = new HexMesh[width, height];
        int hexIndex = 0;
        int vOffset = 0;
        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                hexMeshes[x, z] = new HexMesh(x, z, hexIndex * 7 + vOffset);
                lVertices.AddRange(hexMeshes[x, z].GetVertices());
                lTriangles.AddRange(hexMeshes[x, z].GetTriangles());
                ++hexIndex;
                //Debug.Log("Vertices for hex " + x + ", " + z + ": " + lVertices.Count);
                //Debug.Log("Triangles for hex " + x + ", " + z + ": " + lTriangles.Count);
                HexMesh currentHex = hexMeshes[x, z];
                for (HexDirection dir = HexDirection.SE; dir <= HexDirection.NW; ++dir)
                {
                    if (GetNeighbor(dir, currentHex) != null)
                    {
                        AddBridge(dir, currentHex);
                        AddTriangle(dir, currentHex);
                    }
                }
                //Debug.Log("Vertices after bridges and tris: " + lVertices.Count);
                //Debug.Log("Triangles after bridges and tris: " + lTriangles.Count);
            }
        }
    }
    public bool ExistsInList(int[] array, List<int> list)
    {
        int aLength = array.Length;
        int lLength = list.Count;
        for (int lIdx = 0; lIdx < lLength - aLength; ++lIdx)
        {
            bool allMatch = true;
            List<int> checkList = list.GetRange(lIdx, aLength);
            for (int aIdx = 0; aIdx < aLength; ++aIdx)
            {
                if (checkList[aIdx] != array[aIdx])
                {
                    allMatch = false;
                    break;
                }
            }
            if (allMatch)
            {
                return true;
            }
        }
        return false;
    }
    public bool ExistsInList(Vector3[] array, List<Vector3> list)
    {
        int aLength = array.Length;
        int lLength = list.Count;
        for (int lIdx = 0; lIdx < lLength - aLength; ++lIdx)
        {
            bool allMatch = true;
            List<Vector3> checkList = list.GetRange(lIdx, aLength);
            for (int aIdx = 0; aIdx < aLength; ++aIdx)
            {
                if (checkList[aIdx] != array[aIdx])
                {
                    allMatch = false;
                    break;
                }
            }
            if (allMatch)
            {
                return true;
            }
        }
        return false;
    }
    public void Translate(Vector3 delta)
    {
        translationVector = new Vector3();
        translationVector = delta;
        for (int i = 0; i < lVertices.Count; ++i)
        {
            lVertices[i] += delta;
        }
    }
    public void AddHexMesh(Mesh mesh, int width, int height)
    {
        CreateGrid(width, height);
        ApplyToMesh(mesh);
        //mesh.RecalculateNormals();
    }
    public void ApplyToMesh(Mesh mesh)
    {
        mesh.vertices = lVertices.ToArray();
        mesh.triangles = lTriangles.ToArray();
    }
    HexMesh GetNeighbor(HexDirection direction, HexMesh center)
    {
        int cX = center.xOff;
        int cZ = center.zOff;
        int dX = 0;
        int dZ = 0;
        bool evenRow = ((cZ & 1) != 1);
        switch (direction)
        {
            case HexDirection.NE:
                {
                    dX = (evenRow) ? 0 : 1;
                    dZ = 1;
                    break;
                }
            case HexDirection.E:
                {
                    dX = 1;
                    dZ = 0;
                    break;
                }
            case HexDirection.SE:
                {
                    dX = (evenRow) ? 0 : 1;
                    dZ = -1;
                    break;
                }
            case HexDirection.SW:
                {
                    dX = (evenRow) ? -1 : 0;
                    dZ = -1;
                    break;
                }
            case HexDirection.W:
                {
                    dX = -1;
                    dZ = 0;
                    break;
                }
            case HexDirection.NW:
                {
                    dX = (evenRow) ? -1 : 0;
                    dZ = 1;
                    break;
                }
        }
        int x = cX + dX;
        if (x >= hexMeshes.GetLength(0) || x < 0)
        {
            return null;
        }
        int z = cZ + dZ;
        if (z >= hexMeshes.GetLength(1) || z < 0)
        {
            return null;
        }
        return hexMeshes[x, z];
    }

    public void AddBridgeOutside(HexDirection dir, HexMesh from, HexMesh to, Vector3 fromT, Vector3 toT)
    {
        if (translationVector == null)
        {
            return;
        }
        int vStart = lVertices.Count;
        Vector3[] vertices = new Vector3[4];

        vertices[0] = from.GetCorner(dir) + fromT;
        vertices[1] = from.GetCorner(dir.Next()) + fromT;
        vertices[2] = to.GetCorner(dir.Opposite()) + toT;
        vertices[3] = to.GetCorner(dir.Next().Opposite()) + toT;

        if (!ExistsInList(vertices, lVertices))
        {
            lVertices.AddRange(vertices);
        }
        int[] triangles = {
                vStart + 1,
                vStart,
                vStart + 2,
                vStart + 2,
                vStart,
                vStart + 3
        };

        if (!ExistsInList(triangles, lTriangles))
        {

            lTriangles.AddRange(triangles);
        }
        int[] edgeTri = new int[3];
        switch (dir)
        {
            case HexDirection.NE:
                {
                    break;
                }
            case HexDirection.E:
                {
                    break;
                }
            case HexDirection.SE:
                {
                    break;
                }
            case HexDirection.SW:
                {
                    break;
                }
            case HexDirection.W:
                {
                    break;
                }
            case HexDirection.NW:
                {
                    break;
                }
        }
    }
    public void AddBridge(HexDirection direction, HexMesh hex)
    {
        HexMesh neighbor = GetNeighbor(direction, hex);
        if (neighbor == null)
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
        if (!ExistsInList(triangles, lTriangles)) //double check to not add duplicate triangles
        {
            lTriangles.AddRange(triangles);
        }
    }

    public void AddTriangle(HexDirection direction, HexMesh hex)
    {
        Vector3[] vertices = new Vector3[3];
        int[] triangles = new int[3];
        switch (direction)
        {
            case HexDirection.SW:
                {
                    HexMesh n1 = GetNeighbor(HexDirection.SE, hex);
                    HexMesh n2 = GetNeighbor(HexDirection.SW, hex);
                    if (n1 == null || n2 == null)
                    {
                        return;
                    }
                    vertices[0] = hex.GetCorner(HexDirection.SW);
                    vertices[1] = n1.GetCorner(HexDirection.NW);
                    vertices[2] = n2.GetCorner(HexDirection.E);
                    triangles[0] = lVertices.IndexOf(vertices[0]);
                    triangles[1] = lVertices.IndexOf(vertices[1]);
                    triangles[2] = lVertices.IndexOf(vertices[2]);
                    break;
                }
            case HexDirection.W:
                {
                    HexMesh n1 = GetNeighbor(HexDirection.SW, hex);
                    HexMesh n2 = GetNeighbor(HexDirection.W, hex);
                    if (n1 == null || n2 == null)
                    {
                        return;
                    }
                    vertices[0] = hex.GetCorner(HexDirection.W);
                    vertices[1] = n1.GetCorner(HexDirection.NE);
                    vertices[2] = n2.GetCorner(HexDirection.SE);
                    triangles[0] = lVertices.IndexOf(vertices[0]);
                    triangles[1] = lVertices.IndexOf(vertices[1]);
                    triangles[2] = lVertices.IndexOf(vertices[2]);
                    break;
                }
            case HexDirection.NW:
                {
                    HexMesh n1 = GetNeighbor(HexDirection.W, hex);
                    HexMesh n2 = GetNeighbor(HexDirection.NW, hex);
                    if (n1 == null || n2 == null)
                    {
                        return;
                    }
                    vertices[0] = hex.GetCorner(HexDirection.NW);
                    vertices[1] = n1.GetCorner(HexDirection.E);
                    vertices[2] = n2.GetCorner(HexDirection.SW);
                    triangles[0] = lVertices.IndexOf(vertices[0]);
                    triangles[1] = lVertices.IndexOf(vertices[1]);
                    triangles[2] = lVertices.IndexOf(vertices[2]);
                    break;
                }
        }
        if (!ExistsInList(triangles, lTriangles)) //double check to not add duplicate triangles
        {
            lTriangles.AddRange(triangles);
        }
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        AddHexMesh(mesh, HexMetrics.chunkSize, HexMetrics.chunkSize);
        return mesh;
    }
    public ChunkDataset GetChunkDataset(int xP, int zP)
    {
        ChunkDataset dataset = new ChunkDataset();
        dataset.xPos = xP;
        dataset.xPos = zP;
        for (int x = 0; x < MapMetrics.chunkSize; ++x)
        {
            for (int z = 0; z < MapMetrics.chunkSize; ++z)
            {
                HexMesh currentMesh = hexMeshes[x + xP, z + zP];
                dataset.verts.AddRange(currentMesh.GetVertices());
                int minTri = int.MaxValue;
                int[] newTris = currentMesh.GetTriangles();
                for (int i = 0; i < newTris.Length; ++i)
                {
                    if (newTris[i] < minTri)
                        minTri = newTris[i];
                }
                for (int i = 0; i < newTris.Length; ++i)
                {
                    newTris[i] -= minTri;
                }
                dataset.tris.AddRange(newTris);

            }
        }

        return dataset;
    }
    public void AddTriangleOutside(HexDirection dir, HexMesh from, HexMesh n1, HexMesh n2, Vector3 tFrom, Vector3 tTo)
    {
        int[] tris = new int[3];
        Vector3[] verts = new Vector3[3];
        int vStart = lVertices.Count;
        switch(dir)
        {
            case HexDirection.NE:
                {

                    break;
                }
            case HexDirection.E:
                {
                    break;
                }
            case HexDirection.SE:
                {
                    verts[0] = from.GetCorner(HexDirection.SE) + tFrom;
                    verts[1] = n1.GetCorner(HexDirection.W) + tFrom;
                    verts[2] = n2.GetCorner(HexDirection.NE) + tTo;
                    if (!ExistsInList(verts, lVertices))
                    {
                        lVertices.AddRange(verts);
                    }
                    tris[0] = vStart;
                    tris[1] = vStart + 1;
                    tris[2] = vStart + 2;
                    Debug.Log("Southeast triangle with vertices: " + tris[0] + ", " + tris[1] + ", " + tris[2]);
                    break;
                }
            case HexDirection.SW:
                {
                    verts[0] = from.GetCorner(HexDirection.SW) + tFrom;
                    verts[1] = n1.GetCorner(HexDirection.NW) + tTo;
                    verts[2] = n2.GetCorner(HexDirection.E) + tTo;
                    if (!ExistsInList(verts, lVertices))
                    {
                        lVertices.AddRange(verts);
                    }
                    tris[0] = vStart;
                    tris[1] = vStart + 1;
                    tris[2] = vStart + 2;
                    Debug.Log("Southeast triangle with vertices: " + tris[0] + ", " + tris[1] + ", " + tris[2]);
                    break;
                }
            case HexDirection.W:
                {
                    break;
                }
            case HexDirection.NW:
                {
                    break;
                }
        }
        if(!ExistsInList(tris, lTriangles))
        {
            lTriangles.AddRange(tris);
        }
    }
    public void AddNeighborChunk(bool isAbove, HexChunk neighbor)
    {
        if(this == neighbor)
        { //this never gets tripped apparently
            Debug.Log("Error: Chunk given self as neighbor");
            return;
        }
        if(isAbove)
        {
            
            for (int i = 0; i < HexMetrics.chunkSize; ++i)
            {
                HexMesh from = hexMeshes[i, 0];
                HexMesh to = neighbor.hexMeshes[i, 5];
                HexMesh n1;
                HexMesh n2;
                //Debug.Log("From hex: " + from.xOff + ", " + from.zOff);
                //Debug.Log("To hex: " + to.xOff + ", " + to.zOff);
                AddBridgeOutside(HexDirection.SE, from, to, translationVector, neighbor.translationVector);
                if(i > 0)
                {
                    HexMesh toSW = neighbor.hexMeshes[ i - 1, 5];
                    AddBridgeOutside(HexDirection.SW, from, toSW, translationVector, neighbor.translationVector);
                    n1 = to;
                    n2 = neighbor.hexMeshes[i - 1, 5];
                    if(n1 != null && n2 != null)
                    {
                        AddTriangleOutside(HexDirection.SW, from, n1, n2, translationVector, neighbor.translationVector);
                    }
                }
                if(i < HexMetrics.chunkSize - 1)
                {
                    n1 = hexMeshes[i + 1, 0];
                    n2 = to;
                    if(n1 != null && n2 != null)
                    {
                        AddTriangleOutside(HexDirection.SE, from, n1, n2, translationVector, neighbor.translationVector);
                    }

                }
            }
        }
        else
        {
            for (int i = 0; i < HexMetrics.chunkSize; ++i)
            {
                HexMesh from = hexMeshes[0, i];
                HexMesh to = neighbor.hexMeshes[neighbor.hexMeshes.GetLength(0) - 1, i];
                //Debug.Log("From hex: " + from.xOff + ", " + from.zOff);
                //Debug.Log("To hex: " + to.xOff + ", " + to.zOff);
                AddBridgeOutside(HexDirection.W, from, to, translationVector, neighbor.translationVector);
                if(i % 2 == 0)
                {
                    to = neighbor.hexMeshes[neighbor.hexMeshes.GetLength(0) - 1, i + 1];
                    AddBridgeOutside(HexDirection.NW, from, to, translationVector, neighbor.translationVector);
                    if (i != 0)
                    {
                        to = neighbor.hexMeshes[neighbor.hexMeshes.GetLength(0) - 1, i - 1];
                        AddBridgeOutside(HexDirection.SW, from, to, translationVector, neighbor.translationVector);
                    }
                    
                }
            }
        }

    }
    public void SetElevation(float[,] heightMap)
    {
        for (int x = 0; x < mWidth; ++x)
        {
            for(int z = 0; z < mHeight; ++z)
            {
                hexMeshes[x, z].SetElevation(heightMap[x, z]);
            }
        }
    }
}
