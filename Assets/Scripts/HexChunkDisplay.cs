using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(HexChunk))]
public class HexChunkDisplay : MonoBehaviour
{
    private HexChunk hexChunk;
    private MeshFilter mFilter;
    private MeshRenderer mRenderer;
    public bool AutoUpdate = true;
    private Mesh mesh;
    public Mesh Mesh
    {
        get { return mesh; }
    }
    public MeshFilter Filter
    {
        get { return mFilter; }
    }
    public void CreateMap()
    {
        mFilter = GetComponent<MeshFilter>();
        mRenderer = GetComponent<MeshRenderer>();
        hexChunk = GetComponent<HexChunk>();
        mesh = new Mesh();
        hexChunk.ApplyToMesh(mesh);
        mesh.RecalculateNormals();
        mFilter.sharedMesh = mesh;
    }
    private void Start()
    {
        CreateMap();
    }
    private int xOffset;
    public int OffsetX
    {
        get { return xOffset; }
        set { xOffset = value;  }
    }
    private int zOffset;
    public int OffsetZ
    {
        get { return zOffset; }
        set { zOffset = value; }
    }
    public HexChunk Chunk
    {
        get
        {
            hexChunk = GetComponent<HexChunk>();
            return hexChunk;
        }
        set { hexChunk = value; }
    }

    private void Update()
    {
        /*
        if (mesh != null)
        {
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mFilter.sharedMesh = mesh;
        }
        */
    }


}
