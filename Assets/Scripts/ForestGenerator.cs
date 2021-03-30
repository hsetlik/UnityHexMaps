using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ForestGenerator : MonoBehaviour
{
    public GameObject maplePrefab1;
    public List<GameObject> maplePrefabs;
    public GameObject pinePrefab;
    private void PlaceMaple(Vector3 position)
    {
        GameObject newTree = Instantiate(maplePrefab1);
        newTree.tag = "Tree";
        newTree.transform.position = position;
    }
    private void PlaceRandomMaple(Vector3 position)
    {
        int idx = Mathf.FloorToInt(UnityEngine.Random.value * maplePrefabs.Count);
        var prefab = maplePrefabs[idx];
        GameObject newTree = Instantiate(prefab);
        newTree.tag = "Tree";
        newTree.transform.position = position;
    }
    public void ClearTrees()
    {
        GameObject[] allTrees = GameObject.FindGameObjectsWithTag("Tree");
        for (int i = 0; i < allTrees.Length; ++i) //every time the forests are regenerated, start by destroying all existing trees
        {
            DestroyImmediate(allTrees[i]);
        }
    }
    public void GenerateMapleForest(List<Vector3> positions)
    {
        ClearTrees();
        for(int i = 0; i < positions.Count; ++i)
        {
            if (maplePrefabs.Count < 2)
                PlaceMaple(positions[i]);
            else
                PlaceRandomMaple(positions[i]);
        }
    }
}
