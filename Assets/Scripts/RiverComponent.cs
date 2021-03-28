using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO: create prefab for this w/ appropriate water shader
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
