
//#define FULL_DEBUG

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
	
	static public Vector3 PointSegmentCollision(Vector3 point, CollisionEdge edge, float radius)
	{
Debug.DrawLine(edge.v0, edge.v1, Color.red, 1.0f);
	
		Vector3 v0p = point - edge.v0;
		Vector3 v0v1 = edge.v1 - edge.v0;
		float len2 = v0v1.sqrMagnitude;
		float dot = Vector3.Dot(v0p, v0v1);
		float t = dot / len2;
		
		t = Mathf.Clamp01(t);
		
		Vector3 closest = edge.v0 + v0v1 * t;
		Vector3 toClosest = point - closest;
		
		if(toClosest.sqrMagnitude < radius * radius)
		{
			toClosest.Normalize();
			toClosest *= radius;
			
Debug.DrawLine(point, closest, Color.magenta, 1.0f);
			return toClosest + closest;
		}
		else
		{			
Debug.DrawLine(point, closest, Color.grey, 0.25f);
			return point;
		}
	}

	// Note-TODO: assumes point is more or less on the plane of the navmesh!!!
	public Vector3 PointNavMeshEdgesCollision(Vector3 point, float radius, WayPoint previousNearest, out WayPoint newNearest)
	{
		// find nearest waypoint
		List<WayPoint> searchList;
		float nearestdist2 = float.MaxValue;
		
		if (previousNearest == null)
		{
			searchList = waypointList;
			newNearest = null;
		}
		else
		{
			searchList = previousNearest.connections;
			newNearest = previousNearest;
			nearestdist2 = Vector3.SqrMagnitude(newNearest.transform.position - point);
		}
		
		foreach(WayPoint w in searchList)
		{
			float dist2 = Vector3.SqrMagnitude(w.transform.position - point);
			if( dist2 < nearestdist2 )
			{
				newNearest = w;
				nearestdist2 = dist2;
			}
		}
		
Debug.DrawLine(newNearest.transform.position, newNearest.connections[0].transform.position, Color.green, 1.0f);
		
		// resolve collisions (TODO: could do multiple iterations...)
		// TODO: not the smartest way as we don't check in which side we are
		// -- A smart and easy way is to force point-in-triangle
		// taking the result that have the least movement among the triangles we check
		foreach(CollisionEdge e in newNearest.collisionEdges)
			point = PointSegmentCollision(point, e, radius);
			
		foreach(WayPoint w in newNearest.connections)
		{
Debug.DrawLine(w.transform.position, w.connections[0].transform.position, Color.yellow, 1.0f);
			foreach(CollisionEdge e in w.collisionEdges)
				point = PointSegmentCollision(point, e, radius);
		}		
		return point;
	}

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
	
	void Start ()
	{

	}
	
	public WayPoint SelectRandomWaypoint()
	{
		return waypointList[Random.Range(0, waypointList.Count-1)];
	}

	public WayPoint FindClosestWaypoint(Vector3 position)
	{
		//float minAngle = 100000;
		float minDistance = 100000;
		WayPoint result = null;
		
		foreach (WayPoint obj in waypointList)
		{			
			//float angle = Vector3.Angle(position, obj.transform.position);
			float distance = (position - obj.transform.position).sqrMagnitude;
			//if (angle<minAngle)
			if (distance<minDistance)
			{
				//minAngle = angle;	
				minDistance = distance;
				result = obj;
			}
		}
		
		return result;
	}

	public void CalculatePath(Vector3 position, Vector3 targetPosition,  List<WayPoint> path)
	{
		WayPoint start = FindClosestWaypoint (position);
		WayPoint end = FindClosestWaypoint (targetPosition);
		
		Calculate(start, end, ref path);

//		path.RemoveAt(0);
//		if (path.Count<3)
//		{
//			path.Clear();
//		}

//		WayPoint playerWp = m_Player.GetComponent<WayPoint>();
//		path.Add(playerWp);
		
#if UNITY_EDITOR && FULL_DEBUG
		for(int i = 0; i < path.Count-1; ++i)
			Debug.DrawLine(path[i].transform.position, path[i+1].transform.position, Color.red);
#endif
		return;
	}


	List<WayPoint> closedset = new List<WayPoint>(); 
	List<WayPoint> openset = new List<WayPoint>();    
	Dictionary<WayPoint, WayPoint> came_from = new Dictionary<WayPoint, WayPoint>();  
	Dictionary<WayPoint, float> g_score = new Dictionary<WayPoint, float>();
	Dictionary<WayPoint, float> h_score = new Dictionary<WayPoint, float>();
	Dictionary<WayPoint, float> f_score = new Dictionary<WayPoint, float>();
		
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
		return Vector3.Distance(start.position, goal.position);
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
