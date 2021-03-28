using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerViewport : MonoBehaviour
{
    public HexChunkGroup map;
	void Update()
	{
		if (Input.GetMouseButton(0))
		{
			HandleInput();
		}
	}

	void HandleInput()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
		{
			HexMesh clickedMesh = map.GetMeshAt(hit.point);
			if(clickedMesh != null)
			Debug.Log("Mesh Clicked at: " + (clickedMesh.xOff + clickedMesh.CoordX) + ", " + (clickedMesh.zOff + clickedMesh.CoordZ));
		}
	}


}
