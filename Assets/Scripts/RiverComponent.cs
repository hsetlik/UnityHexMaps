using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class RiverComponent : MonoBehaviour
{
    private List<Vector3> lVertices;
    private List<int> lTriangles;
    private MeshFilter mFilter;
    private MeshRenderer mRender;
    public RiverComponent()
    {
        mFilter = GetComponent<MeshFilter>();
        mRender = GetComponent<MeshRenderer>();
    }

}
