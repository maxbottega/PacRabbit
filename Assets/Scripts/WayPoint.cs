using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IPathNode<T>
{
    List<T> Connections { get; }
    Vector3 Position { get; }
    bool Invalid {get;}
}


public class WayPoint : MonoBehaviour, IPathNode<WayPoint> 
{
	//[SerializeField, HideInInspector]
	public List<WayPoint> m_Neighboors = new List<WayPoint>();	
	
	public List<WayPoint> Connections 
	{ 
		get { return m_Neighboors; } 
	}
	
    public Vector3 Position 
	{ 
		get { return transform.position; }
	}
	
    public bool Invalid
    {
        get { return (this == null); }
    }
	
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;

		Gizmos.DrawSphere(transform.position, 0.5f);
		Gizmos.color = Color.red;
		
		foreach (WayPoint neighboor in m_Neighboors)
		{
			Gizmos.DrawLine(transform.position, neighboor.transform.position);
		}

	}
}
