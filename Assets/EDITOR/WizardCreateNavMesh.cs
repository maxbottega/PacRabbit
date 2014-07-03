using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class WizardCreateNavMesh : ScriptableWizard
{
   	public GameObject 	m_NavMesh = null;
	public GameObject	m_WayPointPrefab = null;
    
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
	
	class WayPointData
	{
		public Vector3 position;
		public int index;
		public string name;
		public List<WayPointData> Connections = new List<WayPointData>();
		public WayPoint waypoint;
	}
	
	void CreateWayPoints()
	{
		// delete previously stored data if any
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
		Mesh navMesh = m_NavMesh.GetComponent<MeshFilter>().mesh;
	
		int numTriangles = navMesh.triangles.Length / 3;
		List<WayPoint> waypoints = new List<WayPoint>();
		for (int triIdx=0; triIdx<numTriangles; ++triIdx)
		{

			WayPoint wp = (Instantiate(m_WayPointPrefab) as GameObject).GetComponent<WayPoint>();
			wp.name = "WP-index:" + triIdx;
			wp.transform.parent = containerFolder.transform;
		
			waypoints.Add(wp);
		}

		for (int triIdx=0; triIdx<numTriangles; ++triIdx)
		{
			Vector3 triCenter = Vector3.zero;

			for(int i=0;i<3;i++)
				triCenter += navMesh.vertices[navMesh.triangles[triIdx * 3 + i]];

			waypoints[triIdx].transform.position = triCenter/3;

			for (int triIdx2=0; triIdx2<numTriangles; ++triIdx2)
			{
				if(triIdx == triIdx2)
					continue;

				int connected = 0;
				for(int i=0;i<3;i++)
					for(int j=0;j<3;j++)
						if(navMesh.vertices[navMesh.triangles[triIdx * 3 + i]].AlmostEquals(navMesh.vertices[navMesh.triangles[triIdx2 * 3 + j]], 0.000001f))
							connected++;
	
				if(connected >= 2)
					waypoints[triIdx].Connections.Add(waypoints[triIdx2]);
			}
		}

	}
}