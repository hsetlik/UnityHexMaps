using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(HexChunk))]
public class HexChunkGenerator : MonoBehaviour
{
    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        
        return mesh;
    }
    public HexChunk GetChunk() { return GetComponent<HexChunk>(); }
}

