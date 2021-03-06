using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMesh
{
    public int parentVertexIndex;
    public int parentTriangleIndex;
    public Dictionary<HexDirection, int> quadVertexIndeces;
    public Dictionary<HexDirection, int> quadTriangleIndeces;
    public Dictionary<HexDirection, int> triVertexIndeces;
    public Dictionary<HexDirection, int> triTriangleIndeces;
    public HexMesh(int xOffset, int zOffset, int vertexOffset, float elevation = 0f)
    {
        quadVertexIndeces = new Dictionary<HexDirection, int>();
        quadTriangleIndeces = new Dictionary<HexDirection, int>();
        triVertexIndeces = new Dictionary<HexDirection, int>();
        triTriangleIndeces = new Dictionary<HexDirection, int>();
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
    private Vector3 tCenter;
    public Vector3 CenterT
    {
        get { return tCenter; }
        set { tCenter = value; }
    }
    public void AddNeighbor(HexDirection dir, HexMesh n)
    {
        if(n != null)
        {
            this.neighbors[(int)dir] = n;
            n.neighbors[(int)dir.Opposite()] = this;
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
    public Vector3 GetCenter()
    {
        return vertices[0];
    }
    public Vector3 GetCorner(HexDirection dir)
    {
        return corners[(int)dir];
    }
    public Vector3[] GetVertices(int index)
    {
        parentVertexIndex = index;
        return vertices.ToArray();
    }
    public int[] GetTriangles(int index)
    {
        parentTriangleIndex = index;
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
    public List<Vector3> lVertices;
    public List<int> lTriangles;
    public List<Vector2> lUvs;
    public List<Color> lColors;
    public List<Color> lSecondColors;
    int mWidth;
    int mHeight;
    public Vector3 translationVector;
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
    public float GetMinHeight()
    {
        float height = float.MaxValue;
        for(int i = 0; i < lVertices.Count; ++i)
        {
            if(lVertices[i].y < height)
            {
                height = lVertices[i].y;
            }    
        }
        return height;
    }
    public float GetMaxHeight()
    {
        float height = float.MinValue;
        for (int i = 0; i < lVertices.Count; ++i)
        {
            if (lVertices[i].y > height)
            {
                height = lVertices[i].y;
            }
        }
        return height;
    }
    public HexMesh GetClosestMesh(Vector3 location)
    {
        float distance = float.MaxValue;
        HexMesh mesh = hexMeshes[0, 0];
        for(int x = 0; x < hexMeshes.GetLength(0); ++x)
        {
            for(int z = 0; z < hexMeshes.GetLength(1); ++z)
            {
                if(Vector3.Distance(location, hexMeshes[x, z].GetCenter() + translationVector) < distance)
                {
                    distance = Vector3.Distance(location, hexMeshes[x, z].GetCenter() + translationVector);
                    mesh = hexMeshes[x, z];
                }
            }
        }
        return mesh;
    }

    public void CreateGrid(int width, int height, float[,] noiseMap, float noiseScale)
    {
        mWidth = width;
        mHeight = height;
        
        lVertices = new List<Vector3>();
        lTriangles = new List<int>();
        lUvs = new List<Vector2>();
        lColors = new List<Color>();
        lSecondColors = new List<Color>();
        hexMeshes = new HexMesh[width, height];
        int hexIndex = 0;
        int vOffset = 0;
        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                hexMeshes[x, z] = new HexMesh(x, z, hexIndex * 7 + vOffset);
                hexMeshes[x, z].SetElevation(noiseMap[x, z] * noiseScale);
                lVertices.AddRange(hexMeshes[x, z].GetVertices(lVertices.Count));
                lTriangles.AddRange(hexMeshes[x, z].GetTriangles(lTriangles.Count));
                ++hexIndex;
                HexMesh currentHex = hexMeshes[x, z];
                for (HexDirection dir = HexDirection.SE; dir <= HexDirection.NW; ++dir)
                {
                    if (GetNeighbor(dir, currentHex) != null)
                    {
                        AddBridge(dir, currentHex);
                        AddTriangle(dir, currentHex);
                    }
                }
            }
        }
    }
    public void SetColors(Gradient gradient, float max, float min)
    {
        for(int i = 0; i < lVertices.Count; ++i)
        {
            float value = Mathf.InverseLerp(min, max, lVertices[i].y);
            Color color = gradient.Evaluate(value);
            lColors.Add(color);
            lSecondColors.Add(color);
        }
    }
    public void ColorHex(int x, int z, Color color)
    {
        var startIndex = hexMeshes[x, z].parentVertexIndex;
        for(int i = 0; i < 7; ++i)
        {
            lSecondColors[startIndex + i] = color;
        }
    }
    public void SetColors(Gradient gradient, float[,] noiseMap, float forestDensity, float minElev, float maxElev)
    {
        var width = noiseMap.GetLength(0);
        var height = noiseMap.GetLength(1);
        treeCounts = new int[width, height];
        for(int x = 0; x < width; ++x)
        {
            for(int z = 0; z < height; ++z)
            {
                var value = noiseMap[x, z];
                if (value > maxElev || value < minElev)
                    value = 0.0f;
                var color = gradient.Evaluate(value);
                treeCounts[x, z] = Mathf.FloorToInt(forestDensity * value);
                ColorHex(x, z, color);
            }
        }
    }
    public void SetColors(Gradient gradient, float[,] noiseMap)
    {
        var width = noiseMap.GetLength(0);
        var height = noiseMap.GetLength(1);
        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                var value = noiseMap[x, z];
                var color = gradient.Evaluate(value);
                ColorHex(x, z, color);
            }
        }
    }
    public int[,] treeCounts;
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
        for(int x = 0; x < hexMeshes.GetLength(0); ++x)
        {
            for(int z = 0; z < hexMeshes.GetLength(1); ++z)
            {
                hexMeshes[x, z].CenterT = hexMeshes[x, z].GetCenter() + delta;
            }
        }
    }
    public void ApplyToMesh(Mesh mesh)
    {
        CalculateUVs();
        mesh.vertices = lVertices.ToArray();
        mesh.triangles = lTriangles.ToArray();
        mesh.uv = lUvs.ToArray();
        mesh.colors = lSecondColors.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

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
            from.quadVertexIndeces.Add(dir, lVertices.Count);
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
            from.quadTriangleIndeces.Add(dir, lTriangles.Count);
            lTriangles.AddRange(triangles);
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
            hex.quadTriangleIndeces.Add(direction, lTriangles.Count);
            lTriangles.AddRange(triangles);
        }
    }

    public void CalculateUVs()
    {
        lUvs.Clear();
        float zMax = float.MinValue;
        float zMin = float.MaxValue;
        float xMin = float.MaxValue;
        float xMax = float.MinValue;
        int numPoints = lVertices.Count;
        for(int i = 0; i < numPoints; ++i)
        {
            if (lVertices[i].x > xMax)
                xMax = lVertices[i].x;
            if (lVertices[i].x < xMin)
                xMin = lVertices[i].x;
            if (lVertices[i].z > zMax)
                zMax = lVertices[i].z;
            if (lVertices[i].z < zMin)
                zMin = lVertices[i].z;
            Vector2 point = new Vector2();
            point.x = lVertices[i].x;
            point.y = lVertices[i].z;
            lUvs.Add(point);
        }
        float xRange = xMax - xMin;
        float zRange = zMax - zMin;
        Vector2 uvScale = new Vector2();
        uvScale.x = xRange;
        uvScale.y = zRange;
        for(int i = 0; i < numPoints; ++i)
        {
            lUvs[i].Scale(uvScale);
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
            hex.triTriangleIndeces.Add(direction, lTriangles.Count);
            lTriangles.AddRange(triangles);
        }
    }
    public void AddTriangleOutside(HexDirection dir, HexMesh from, HexMesh n1, HexMesh n2, Vector3 tFrom, Vector3 tTo, bool evenRow)
    {
        int[] tris = new int[3];
        Vector3[] verts = new Vector3[3];
        int vStart = lVertices.Count;
        if(evenRow)
        {
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
                        verts[0] = from.GetCorner(HexDirection.SE) + tFrom;
                        verts[1] = n1.GetCorner(HexDirection.W) + tFrom;
                        verts[2] = n2.GetCorner(HexDirection.NE) + tTo;
                        break;
                    }
                case HexDirection.SW:
                    {
                        verts[0] = from.GetCorner(HexDirection.SW) + tFrom;
                        verts[1] = n1.GetCorner(HexDirection.NW) + tTo;
                        verts[2] = n2.GetCorner(HexDirection.E) + tTo;
                        break;
                    }
                case HexDirection.W:
                    {
                        verts[0] = from.GetCorner(HexDirection.W) + tFrom;
                        verts[1] = n1.GetCorner(HexDirection.NE) + tTo;
                        verts[2] = n2.GetCorner(HexDirection.SE) + tTo;
                        break;
                    }
                case HexDirection.NW:
                    {
                        verts[0] = from.GetCorner(HexDirection.NW) + tFrom;
                        verts[1] = n1.GetCorner(HexDirection.E) + tTo;
                        verts[2] = n2.GetCorner(HexDirection.SW) + tTo;
                        break;
                    }
            }
        }
        else
        {
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
                        verts[0] = from.GetCorner(HexDirection.W) + tFrom;
                        verts[1] = n1.GetCorner(HexDirection.NE) + tFrom;
                        verts[2] = n2.GetCorner(HexDirection.SE) + tTo;
                        break;
                    }
                case HexDirection.NW:
                    {
                        verts[0] = from.GetCorner(HexDirection.NW) + tFrom;
                        verts[1] = n1.GetCorner(HexDirection.E) + tTo;
                        verts[2] = n2.GetCorner(HexDirection.SW) + tFrom;
                        break;
                    }
            }
        }
        
        if (!ExistsInList(verts, lVertices))
        {
            from.triVertexIndeces.Add(dir, lVertices.Count);
            lVertices.AddRange(verts);
        }
        tris[0] = vStart;
        tris[1] = vStart + 1;
        tris[2] = vStart + 2;
        if (!ExistsInList(tris, lTriangles))
        {
            from.triTriangleIndeces.Add(dir, lTriangles.Count);
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
                AddBridgeOutside(HexDirection.SE, from, to, translationVector, neighbor.translationVector);
                if(i > 0)
                {
                    HexMesh toSW = neighbor.hexMeshes[ i - 1, 5];
                    AddBridgeOutside(HexDirection.SW, from, toSW, translationVector, neighbor.translationVector);
                    n1 = to;
                    n2 = neighbor.hexMeshes[i - 1, 5];
                    if(n1 != null && n2 != null)
                    {
                        AddTriangleOutside(HexDirection.SW, from, n1, n2, translationVector, neighbor.translationVector, true);
                    }
                }
                if(i < HexMetrics.chunkSize - 1)
                {
                    n1 = hexMeshes[i + 1, 0];
                    n2 = to;
                    if(n1 != null && n2 != null)
                    {
                        AddTriangleOutside(HexDirection.SE, from, n1, n2, translationVector, neighbor.translationVector, true);
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
                HexMesh n1;
                HexMesh n2;
                AddBridgeOutside(HexDirection.W, from, to, translationVector, neighbor.translationVector);
                if(i % 2 == 0)
                {
                    to = neighbor.hexMeshes[neighbor.hexMeshes.GetLength(0) - 1, i + 1];
                    AddBridgeOutside(HexDirection.NW, from, to, translationVector, neighbor.translationVector);
                    n1 = neighbor.hexMeshes[neighbor.hexMeshes.GetLength(0) - 1, i];
                    n2 = to;
                    AddTriangleOutside(HexDirection.NW, from, n1, n2, translationVector, neighbor.translationVector, true);
                    if (i != 0)
                    {
                        to = neighbor.hexMeshes[neighbor.hexMeshes.GetLength(0) - 1, i - 1];
                        AddBridgeOutside(HexDirection.SW, from, to, translationVector, neighbor.translationVector);
                        n2 = n1;
                        n1 = to;
                        AddTriangleOutside(HexDirection.W, from, n1, n2, translationVector, neighbor.translationVector, true);
                    }
                    
                }
                else
                {
                    n2 = to;
                    n1 = hexMeshes[0, i - 1];
                    AddTriangleOutside(HexDirection.W, from, n1, n2, translationVector, neighbor.translationVector, false);
                    if(i < HexMetrics.chunkSize - 1)
                    {
                        n1 = n2;
                        n2 = hexMeshes[0, i + 1];
                        AddTriangleOutside(HexDirection.NW, from, n1, n2, translationVector, neighbor.translationVector, false);
                    }
                }
            }
        }
    }
    public void FillCorner(HexChunk left, HexChunk below, HexChunk corner)
    {
        //add quad
        AddBridgeOutside(HexDirection.SW, hexMeshes[0, 0], corner.hexMeshes[HexMetrics.chunkSize - 1, HexMetrics.chunkSize - 1], translationVector, corner.translationVector);
        int limit = HexMetrics.chunkSize - 1;
        //add lower triangle
        Vector3[] lVerts = new Vector3[3];
        int vStart = lVertices.Count;
        lVerts[0] = hexMeshes[0, 0].GetCorner(HexDirection.SW) + translationVector;
        lVerts[1] = below.hexMeshes[0, limit].GetCorner(HexDirection.NW) + below.translationVector;
        lVerts[2] = corner.hexMeshes[limit, limit].GetCorner(HexDirection.E) + corner.translationVector;
        hexMeshes[0, 0].triVertexIndeces.Add(HexDirection.SW, lVertices.Count);
        lVertices.AddRange(lVerts);
        int[] lTris = { vStart, vStart + 1, vStart + 2 };
        if (!ExistsInList(lTris, lTriangles))
        {
            hexMeshes[0, 0].triTriangleIndeces.Add(HexDirection.SW, lTriangles.Count);
            lTriangles.AddRange(lTris);
        }
        //add upper triangle
        vStart = lVertices.Count;
        Vector3[] uVerts = new Vector3[3];
        uVerts[0] = hexMeshes[0, 0].GetCorner(HexDirection.W) + translationVector;
        uVerts[1] = corner.hexMeshes[limit, limit].GetCorner(HexDirection.NE) + corner.translationVector;
        uVerts[2] = left.hexMeshes[limit, 0].GetCorner(HexDirection.SE) + left.translationVector;
        hexMeshes[0, 0].triVertexIndeces.Add(HexDirection.W, lVertices.Count);
        lVertices.AddRange(uVerts);
        int[] uTris = { vStart, vStart + 1, vStart + 2 };
        if (!ExistsInList(uTris, lTriangles))
        {
            hexMeshes[0, 0].triTriangleIndeces.Add(HexDirection.W, lTriangles.Count);
            lTriangles.AddRange(uTris);


        }
    }
    public void SetElevation(float[,] heightMap) //remember to call this before connecting bridges and triangles
    {
        for (int x = 0; x < mWidth; ++x)
        {
            for(int z = 0; z < mHeight; ++z)
            {
                hexMeshes[x, z].SetElevation(heightMap[x, z]);
            }
        }
    }
    public HexTileData[,] GetTileData(int xOff, int zOff)
    {
        HexTileData[,] tiles = new HexTileData[mWidth, mHeight];
        for(int x = 0; x < mWidth; ++x)
        {
            for(int z = 0; z < mHeight; ++z)
            {
                Vector3 center = hexMeshes[x, z].CenterT;
                int iX = xOff + x;
                int iZ = zOff + z;
                tiles[x, z] = new HexTileData(iX, iZ, center, hexMeshes[x, z]);
            }
        }
        return tiles;
    }

   
}
