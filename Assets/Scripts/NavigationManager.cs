using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NavigationManager : MonoBehaviour 
{
	// ------------ Public, editable in the GUI, serialized
	
	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	[System.NonSerialized] public List<WayPoint> 			waypointList = new List<WayPoint>();
	[System.NonSerialized] public static NavigationManager 	instance = null;
	
	// ------------ Private
	private List<WayPoint> closedset = new List<WayPoint>(); 
	private List<WayPoint> openset = new List<WayPoint>();    
	private Dictionary<WayPoint, WayPoint> came_from = new Dictionary<WayPoint, WayPoint>();  
	private Dictionary<WayPoint, float> g_score = new Dictionary<WayPoint, float>();
	private Dictionary<WayPoint, float> h_score = new Dictionary<WayPoint, float>();
	private Dictionary<WayPoint, float> f_score = new Dictionary<WayPoint, float>();
	
	void Awake()
	{
		if(instance == null)
			instance = this;
		else
			Debug.LogError("There should be only one navigation manager!");
		
		foreach(GameObject wp in GameObject.FindGameObjectsWithTag("way_point"))
		{
			waypointList.Add(wp.GetComponent<WayPoint>());
		}
		
		if(waypointList.Count == 0)
			Debug.LogError("No waypoints!");
	}
	
	static public Vector3 PointNearestSegment(Vector3 point, Vector3 edgeA, Vector3 edgeB)
	{
		//Debug.DrawLine(edge.v0, edge.v1, Color.red, 1.0f);
		
		Vector3 v0p = point - edgeA;
		Vector3 v0v1 = edgeB - edgeA;
		float len2 = v0v1.sqrMagnitude;
		float dot = Vector3.Dot(v0p, v0v1);
		float t = dot / len2;
		t = Mathf.Clamp01(t);
		Vector3 closest = edgeA + v0v1 * t;
		
		return closest;
	}
	
	static public Vector3 PointLineProjection (Vector3 point, Vector3 edgeA, Vector3 edgeB)
	{
		Vector3 v0p = point - edgeA;
		Vector3 v0v1Dir = (edgeB - edgeA).normalized;
		
		float t = Vector3.Dot (v0p, v0v1Dir);
		return (edgeA + v0v1Dir * t);
	}

	static public Vector3 PointSegmentCollision(Vector3 point, CollisionEdge edge, float radius, out Vector3 displacementNormal)
	{
//Debug.DrawLine(edge.v0, edge.v1, Color.red, 1.0f);
		Vector3 closest = PointNearestSegment(point, edge.v0, edge.v1);
		Vector3 toClosest = point - closest;
		
		if(toClosest.sqrMagnitude < radius * radius)
		{
			toClosest.Normalize();
			displacementNormal = toClosest;
			
			toClosest *= radius;
			
Debug.DrawLine(point, closest, Color.magenta, 1.0f);
			return toClosest + closest;
		}
		else
		{			
Debug.DrawLine(point, closest, Color.grey, 0.25f);
			displacementNormal = Vector3.zero;
			return point;
		}
	}
	
	// Note-TODO: assumes point is more or less on the plane of the navmesh!
	public Vector3 PointNavMeshEdgesCollision(Vector3 point, float radius, WayPoint previousNearest, out WayPoint newNearest)
	{
		Vector3 unused;
		newNearest = FindClosestWaypoint(point, previousNearest, waypointList);
		
//Debug.DrawLine(newNearest.Position, newNearest.connections[0].Position, Color.green, 1.0f);
		
		// resolve collisions (TODO: could do multiple iterations...)
		// TODO: not the smartest way as we don't check in which side we are
		// -- A smart and easy way is to force point-in-triangle
		// taking the result that have the least movement among the triangles we check
		foreach(CollisionEdge e in newNearest.collisionEdges)
			point = PointSegmentCollision(point, e, radius, out unused);
			
		foreach(WayPoint w in newNearest.connections)
		{
//Debug.DrawLine(w.Position, w.connections[0].Position, Color.yellow, 1.0f);

			foreach(CollisionEdge e in w.collisionEdges)
				point = PointSegmentCollision(point, e, radius, out unused);
		}		
		return point;
	}
	
/*
	// Note-TODO: assumes segment is more or less on the plane of the navmesh!
	// TODO: continous collision
	// TODO...
	public Vector3 SegmentNavMeshEdgesCollision(Vector3 prevPoint, Vector3 point, float radius, WayPoint previousNearest, out WayPoint newNearest)
	{
		Vector3 displacementNormal;
		newNearest = FindClosestWaypoint(point, previousNearest, waypointList);
		
		Vector3 deltaPoint = point - prevPoint;
		float deltaLength = deltaPoint.magnitude;
			
Debug.DrawLine(point, prevPoint, Color.green);
	
		bool keepResolving = true;
		
		//while(keepResolving)
		{
			keepResolving = false;
					
			foreach(CollisionEdge e in newNearest.collisionEdges)
			{
				Vector3 newPoint = PointSegmentCollision(point, e, radius, out displacementNormal);
Debug.DrawLine(point, newPoint, Color.magenta);
Debug.DrawLine(prevPoint, newPoint, Color.yellow);
				
				if( newPoint != point )
				{
					float len = (newPoint - prevPoint).magnitude;
					float lenDiff = Mathf.Max ( 0.0f, deltaLength - len );
					
					Vector3 displacementTangent = (e.v0 - e.v1).normalized;
					
					if(Vector3.Dot(displacementTangent, deltaPoint) < 0.0f)
						lenDiff = -lenDiff;
					
					point = newPoint + displacementTangent * lenDiff;
					
Debug.Log("move len:"+deltaLength.ToString()+" clamped len:"+len.ToString()+" diff:"+lenDiff.ToString()+" new len:"+(point-prevPoint).magnitude.ToString());
Debug.DrawLine(point, newPoint, Color.blue);					
					keepResolving = true;		
				}
				else
					point = newPoint;
			}
			
			foreach(WayPoint w in newNearest.connections)
			{
				foreach(CollisionEdge e in w.collisionEdges)
				{
					Vector3 newPoint = PointSegmentCollision(point, e, radius, out displacementNormal);
Debug.DrawLine(point, newPoint, Color.magenta);
Debug.DrawLine(prevPoint, newPoint, Color.yellow);
					
					if( newPoint != point )
					{
						float len = (newPoint - prevPoint).magnitude;
						float lenDiff = Mathf.Max ( 0.0f, deltaLength - len );
						
						Vector3 displacementTangent = (e.v0 - e.v1).normalized;
						
						if(Vector3.Dot(displacementTangent, deltaPoint) < 0.0f)
							lenDiff = -lenDiff;
						
						point = newPoint + displacementTangent * lenDiff;
						
Debug.Log("move len:"+deltaLength.ToString()+" clamped len:"+len.ToString()+" diff:"+lenDiff.ToString()+" new len:"+(point-prevPoint).magnitude.ToString());
Debug.DrawLine(point, newPoint, Color.blue);					
						keepResolving = true;		
					}
					else
						point = newPoint;
				}
			}		
				
		}
		return point;
	}
*/
			
	static public WayPoint NavigatePath(List<WayPoint> path, Vector3 point, int pathIndex, out int newPathIndex)
	{
		float distance = Vector3.Distance(path[pathIndex].Position, point);
		
		if(pathIndex < path.Count - 1)
		{
			float nextDistance = Vector3.Distance(path[pathIndex+1].Position, point);
			if(distance >= nextDistance)
				pathIndex++;
		}
		
		newPathIndex = pathIndex;
		
		int targetIndex = Mathf.Min(pathIndex + 2, path.Count - 1); // the target waypoint is a bit "forward"
			
		if(newPathIndex != targetIndex)
			return path[targetIndex];
		else
			return null; // we reached the end of the path
	}

	public WayPoint SelectRandomWaypoint()
	{
		return waypointList[Random.Range(0, waypointList.Count-1)];
	}

	static public WayPoint FindClosestWaypoint(Vector3 point, WayPoint previousNearest, List<WayPoint> allWaypoints)
	{
		// find nearest waypoint
		List<WayPoint> searchList;
		WayPoint newNearest;
		float nearestdist2 = float.MaxValue;
		
		if (previousNearest == null)
		{
			searchList = allWaypoints;
			newNearest = null;
		}
		else
		{
			searchList = previousNearest.connections;
			newNearest = previousNearest;
			nearestdist2 = Vector3.SqrMagnitude(newNearest.Position - point);
		}
		
		foreach(WayPoint w in searchList)
		{
			float dist2 = Vector3.SqrMagnitude(w.Position - point);
			if( dist2 < nearestdist2 )
			{
				newNearest = w;
				nearestdist2 = dist2;
			}
		}
		
		return newNearest;
	}
	
	public void CalculatePath(Vector3 position, Vector3 targetPosition,  List<WayPoint> path)
	{
		WayPoint start = FindClosestWaypoint (position, null, waypointList);
		WayPoint end = FindClosestWaypoint (targetPosition, null, waypointList);
		
		Calculate(start, end, ref path);
	}
		
	void Calculate(WayPoint start, WayPoint goal, ref List<WayPoint> path) 
    {
		// The set of nodes already evaluated.
        closedset.Clear(); 
        // The set of tentative nodes to be evaluated.
		openset.Clear();
		// The map of navigated nodes.
		came_from.Clear();
		g_score.Clear();
		h_score.Clear();
		f_score.Clear();
		
        openset.Add(start);
        g_score[start] = 0.0f; // Cost from start along best known path.
        h_score[start] = HeuristicCostEstimate(start, goal); 
        f_score[start] = h_score[start]; // Estimated total cost from start to goal through y.
		
		int iterations = 0;
        while(openset.Count != 0)
        {
			++iterations;

            WayPoint x = LowestScore(openset, f_score);

            if(x.Equals(goal))
            {
				path.Clear();
                ReconstructPath(came_from, x, ref path);
                return;
            }
            openset.Remove(x);
            closedset.Add(x);
            foreach(WayPoint y in x.connections)
            {
                if(Invalid(y) || closedset.Contains(y))
				{
                    continue;
				}
                float tentative_g_score = g_score[x] + Distance(x, y);
                
                bool tentative_is_better = false;
                if(!openset.Contains(y))
                {
                    openset.Add(y);
                    tentative_is_better = true;
                }
                else if (tentative_g_score < g_score[y])
                    tentative_is_better = true;
                
                if(tentative_is_better)
                {
                    came_from[y] = x;
                    g_score[y] = tentative_g_score;
                    h_score[y] = HeuristicCostEstimate(y, goal);
                    f_score[y] = g_score[y] + h_score[y];
                }
            }
        }
 
		return;
    }
	
	bool Invalid(WayPoint inNode) // TODO: kill
    {
        if(inNode == null) // /*|| inNode.Invalid )
            return true;
        return false;
    }  
    
    float Distance(WayPoint start, WayPoint goal)
    {
        if(Invalid(start) || Invalid(goal))
            return float.MaxValue;
		return Vector3.Distance(start.Position, goal.Position);
    }
   
    float HeuristicCostEstimate(WayPoint start, WayPoint goal)
    {
        return Distance(start, goal);
    }
    
   	WayPoint LowestScore(List<WayPoint> openset, Dictionary<WayPoint, float> scores)
    {
        int index = 0;
        float lowScore = float.MaxValue;
        
        for(int i = 0; i < openset.Count; i++)
        {
            if(scores[openset[i]] > lowScore)
                continue;
            index = i;
            lowScore = scores[openset[i]];
        }
        
        return openset[index];
    }
	
    void ReconstructPath(Dictionary<WayPoint, WayPoint> came_from, WayPoint current_node, ref List<WayPoint> result)
    {
        if(came_from.ContainsKey(current_node))
        {
            ReconstructPath(came_from, came_from[current_node], ref result);
            result.Add(current_node);
            return;
        }
        result.Add(current_node);
    }

}
