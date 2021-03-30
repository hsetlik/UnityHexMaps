using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(HexChunkGroup))]
public class ChunkGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HexChunkGroup group = (HexChunkGroup)target;
        if (DrawDefaultInspector() && group.autoUpdate)
            group.Generate();
        if (GUILayout.Button("Generate"))
        {
            group.Generate();
        }
        ForestGenerator fGen = group.forestGen;
        if(GUILayout.Button("Create Forests"))
        {
            fGen.GenerateMapleForest(group.TreeLocations());
        }
        if(GUILayout.Button("Clear Forests"))
        {
            fGen.ClearTrees();
        }
    }
}
    
