using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class WizardCreateNavMesh : ScriptableWizard
{
	// ------------ Public, editable in the GUI
   	public GameObject 	NavMesh = null;
	public GameObject	WayPointPrefab = null;
    
    [MenuItem ("GameObject/Create Nav Mesh Wizard")]
    static void CreateWizard () 
	{
        ScriptableWizard.DisplayWizard<WizardCreateNavMesh>("Create Nav Mesh", "Create");
    }
	
    void OnWizardCreate () 
	{
       CreateWayPoints();
    }  
	
    void OnWizardUpdate () 
	{
        helpString = "Please select the mesh to use";
    }

	void CreateWayPoints()
	{
		// Delete previously stored data if any...
		GameObject wayPoints = GameObject.Find("WAYPOINTS");
		List<GameObject> toDestroy = new List<GameObject>();
		if (wayPoints)
		{
			foreach (Transform obj in wayPoints.transform)
			{
				toDestroy.Add (obj.gameObject);
			}
			
			toDestroy.ForEach(delegate(GameObject obj) { DestroyImmediate(obj); });
		}
		DestroyImmediate(wayPoints);

		GameObject containerFolder = new GameObject("WAYPOINTS");
		Mesh navMesh = NavMesh.GetComponent<MeshFilter>().mesh;
	
		int numTriangles = navMesh.triangles.Length / 3;
		WayPoint[] waypoints = new WayPoint[numTriangles];

		// 3 edges per triangle. Edge order is: 0 is 0-1 = 1, 1 is 0-2 = 2, 2 is 1-2 = 3
		bool[] connectionEdges = new bool[numTriangles * 3];

		for (int i=0; i<connectionEdges.Length; i++)
						connectionEdges[i] = false;

		for (int triIdx=0; triIdx<numTriangles; ++triIdx)
		{

			WayPoint wp = (Instantiate(WayPointPrefab) as GameObject).GetComponent<WayPoint>();
			wp.name = "WP-index:" + triIdx;
			wp.transform.parent = containerFolder.transform;
		
			waypoints[triIdx] = wp;
		}

		int[] connectedIndex = new int[2];
		for (int triIdx=0; triIdx<numTriangles; ++triIdx)
		{
			Vector3 triCenter = Vector3.zero;

			for(int i=0;i<3;i++)
				triCenter += navMesh.vertices[navMesh.triangles[triIdx * 3 + i]];

			waypoints[triIdx].transform.position = triCenter/3;

			for (int triIdx2=0; triIdx2<numTriangles; ++triIdx2) // Super-duper dumb!
			{
				if(triIdx == triIdx2)
					continue;

				int connected = 0;
				for(int i=0;i<3;i++)
					for(int j=0;j<3;j++)
						if(navMesh.vertices[navMesh.triangles[triIdx * 3 + i]] == 
							navMesh.vertices[navMesh.triangles[triIdx2 * 3 + j]]
						)
						{
							connectedIndex[connected] = i;
							connected++;
						}

				if(connected == 2) // single vertex isn't a connection edge, 3 vertices should never happen
				{
					// store the connection edge in the helper structure:
					int connectingEdge = connectedIndex[0] + connectedIndex[1] - 1;
					connectionEdges[triIdx * 3 + connectingEdge] = true;

					waypoints[triIdx].connections.Add(waypoints[triIdx2]);
				}
			}
		}

		// "invert" connection edges to find open edges
		for (int triIdx=0; triIdx<numTriangles; ++triIdx)
		{
			for(int edge=0; edge<3; edge++)
			{
				if(connectionEdges[triIdx*3+edge] == false)
				{
					// Edge order is: 0 is 0-1 = 1, 1 is 0-2 = 2, 2 is 1-2 = 3
					int v0idx = edge == 2 ? 1:0;
					int v1idx = edge == 0 ? 1:2;

					waypoints[triIdx].collisionEdges.Add(
						new CollisionEdge(	navMesh.vertices[navMesh.triangles[triIdx * 3 + v0idx]],
					                  		navMesh.vertices[navMesh.triangles[triIdx * 3 + v1idx]]
					    )
					);
				}
			}
		}

		// Do some Laplacian smoothing
		Vector3[] newPositions = new Vector3[numTriangles];
		for (int pass=0; pass<3; pass++) 
		{
			for (int triIdx=0; triIdx<numTriangles; ++triIdx)
			{
				newPositions[triIdx] = Vector3.zero;

				for (int i=0; i<waypoints[triIdx].connections.Count; i++)
					newPositions[triIdx] += waypoints[triIdx].connections[i].transform.position;

				newPositions[triIdx] *= 1.0f / waypoints[triIdx].connections.Count;
				newPositions[triIdx] = Vector3.Lerp(newPositions[triIdx], waypoints[triIdx].transform.position, 0.5f);
			}
			
			for (int triIdx=0; triIdx<numTriangles; ++triIdx)
				waypoints[triIdx].transform.position = newPositions[triIdx];
		}
	}
}