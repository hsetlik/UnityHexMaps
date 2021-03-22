using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HexChunkDisplay))]
public class HexMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HexChunkDisplay display = (HexChunkDisplay)target;
        if (DrawDefaultInspector() && display.AutoUpdate)
            display.CreateMap();
        if (GUILayout.Button("Generate"))
        {
            display.CreateMap();
        }
    }
}

