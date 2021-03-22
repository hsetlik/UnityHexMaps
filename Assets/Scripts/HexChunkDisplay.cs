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
    public void CreateMap()
    {
        mFilter = GetComponent<MeshFilter>();
        mRenderer = GetComponent<MeshRenderer>();
        hexChunk = GetComponent<HexChunk>();
        Mesh mesh = new Mesh();
        hexChunk.ApplyToMesh(mesh);
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

}
