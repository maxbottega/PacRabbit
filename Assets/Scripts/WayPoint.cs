using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// We make the player be a waypoint as well, so we need an interface
public interface IPathNode
{
    Vector3 Position { get; }
}

[System.Serializable]
public class CollisionEdge
{
	public Vector3 v0, v1;

	public CollisionEdge(Vector3 vv0, Vector3 vv1)
	{
		v0 = vv0;
		v1 = vv1;
	}
}

[ExecuteInEditMode]
// Waypoints are entities so we can select/move them in the scene... 
// TODO: Now it's quite a bad idea... even more as we access transform.position which is not fast
// Waypoints are just points with connections, we could be smarter (one day) and make them be
// elements with an area, so the path can be refined to be to any point inside a cell
public class WayPoint : MonoBehaviour, IPathNode
{
	// ------------ Public, serialized
	public List<WayPoint> connections = new List<WayPoint>();
	public List<CollisionEdge> collisionEdges = new List<CollisionEdge>();
	
	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	[System.NonSerialized] public bool mDrawGizmo = false; // use WayPointGizmoDrawer script - TODO: we can just disable the specific gizmo draw via the dropdown menu...	
	[System.NonSerialized] public bool mIsCorridor = false; // this could be serialized but I didn't want to change the levels re-generating waypoints
	
	// ------------ Private
	[SerializeField] private Vector3 mPosition; // private copy as transform.position access is slow

	public Vector3 Position
	{
		get { return mPosition; }
	}
	
	void Awake()
	{
		mPosition = transform.position;
		
		// This is a heuristic...
		mIsCorridor = collisionEdges.Count != 0;
		foreach (WayPoint neighbor in connections)
			if(neighbor.collisionEdges.Count == 0)
			{
				mIsCorridor = false;
				break;
			}
			
		// ...if we still think it's an open-space then do a more refined test
		if(!mIsCorridor)
		{
			mIsCorridor = true;
			
			foreach (WayPoint neighbor in connections)
			{
				foreach(WayPoint neighbor2 in neighbor.connections)
					if(connections.Contains(neighbor2))
					{
						mIsCorridor = false;
						break;
					}
			}
		}
	}
	
#if UNITY_EDITOR 
	void OnDrawGizmos()
	{
		if (!mDrawGizmo) return;
		
		if (mIsCorridor)
			Gizmos.color = Color.yellow;
		else	
			Gizmos.color = Color.green;
		
		Gizmos.DrawSphere(transform.position, 0.25f);
		
		foreach (WayPoint neighbor in connections)
		{
			Gizmos.DrawLine(transform.position, neighbor.transform.position);
		}

		Gizmos.color = Color.red;

		foreach (CollisionEdge edge in collisionEdges)
		{
			Gizmos.DrawLine(edge.v0, edge.v1);
		}
	}
#endif
}
