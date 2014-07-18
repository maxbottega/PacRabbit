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

// Waypoints are entities so we can select/move them in the scene... 
// TODO: Now it's quite a bad idea... even more as we access transform.position which is not fast
// Waypoints are just points with connections, we could be smarter (one day) and make them be
// elements with an area, so the path can be refined to be to any point inside a cell
public class WayPoint : MonoBehaviour, IPathNode
{
	// ------------ Public, serialized
	public List<WayPoint> connections = new List<WayPoint>();
	public List<CollisionEdge> collisionEdges = new List<CollisionEdge>();

	public Vector3 mPosition; // private copy as transform.position access is slow
	
	public Vector3 Position
	{
		get { return mPosition; }
	}
	
	void Awake()
	{
		mPosition = transform.position;
	}
	
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;

		Gizmos.DrawSphere(transform.position, 0.5f);
		
		foreach (WayPoint neighboor in connections)
		{
			Gizmos.DrawLine(transform.position, neighboor.transform.position);
		}

		Gizmos.color = Color.red;

		foreach (CollisionEdge edge in collisionEdges)
		{
			Gizmos.DrawLine(edge.v0, edge.v1);
		}
	}
}
