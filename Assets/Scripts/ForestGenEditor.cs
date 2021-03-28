using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ForestGenerator))]
public class ForestGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ForestGenerator gen = (ForestGenerator)target;
        if (GUILayout.Button("Generate"))
        {
            gen.GenerateForest();
        }
        if(GUILayout.Button("Clear"))
        {
            gen.ClearTrees();
        }
    }
}
