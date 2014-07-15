using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SphereTransform))]
public class Collidable : MonoBehaviour
{
	public float 				Radius 	= 1;
	public float 				Mass 	= 1;
	public bool 				Static 	= false;

	public SphereTransform 		m_SphereTransform = null;

	[System.NonSerialized]
	public Vector3 				Center 		= Vector3.zero;
	[System.NonSerialized]
	public float 				AngleRadius = 1;
	[System.NonSerialized]
	public bool 				Colliding 	= false;
	[System.NonSerialized]
	public bool 				Active 		= true;

	public Vector3 				Min = Vector3.zero;
	public Vector3 				Max = Vector3.zero;
	public float 				MinValue = 0.0f;
	public float 				MaxValue = 0.0f;
	
	float 						PlanetRadius 		= 1;
	CollisionManager 			mCollisionManager 	= null;	
	
	public delegate void 		CollisionCallback(Collidable other);
	public CollisionCallback 	OnCollision;

	void Awake()
	{
		m_SphereTransform = GetComponent<SphereTransform> ();
		m_SphereTransform.Pivot = transform.parent;
		mCollisionManager 		= FindObjectOfType(typeof(CollisionManager)) as CollisionManager;
	}
	
	void Start ()
	{	
		mCollisionManager.AddCollider(this);

		PlanetRadius 	= Planet.GetRadius();
		Center 			= Up * PlanetRadius;
		AngleRadius 	= 2*Mathf.Asin(Radius/(2*PlanetRadius))*Mathf.Rad2Deg; 

		// These are used by the CollisionManager for SAP
		Min.x 	= Up.x * PlanetRadius - Radius;
		Min.y 	= Up.y * PlanetRadius - Radius;
		Min.z 	= Up.z * PlanetRadius - Radius;
		
		Max.x 	= Up.x * PlanetRadius + Radius;
		Max.y 	= Up.y * PlanetRadius + Radius;
		Max.z 	= Up.z * PlanetRadius + Radius;
	}
	
	public Vector3 GetCurrentPosition()
	{
		return Rotation * Vector3.up * PlanetRadius; // y-axis
	}

	public void MoveFromPoint(Vector3 p)
	{
		Rotation = Quaternion.FromToRotation(Vector3.up, p);
	}

	void Update ()
	{
		if (!Static)
		{			
			Min.x = Up.x * PlanetRadius - Radius;
			Min.y = Up.y * PlanetRadius - Radius;
			Min.z = Up.z * PlanetRadius - Radius;
			
			Max.x = Up.x * PlanetRadius + Radius;
			Max.y = Up.y * PlanetRadius + Radius;
			Max.z = Up.z * PlanetRadius + Radius;
			
			Center = Up * PlanetRadius;
		}	
	}

	// Wrapper functions	
	public Quaternion Rotation
	{
		get { return m_SphereTransform.Rotation; }
		set { m_SphereTransform.Rotation = value; }
	}
	
	public Vector3 Up
	{
		get { return m_SphereTransform.Up; }
		set { m_SphereTransform.Up = value; }
	}
	
/*	
	public Vector3 Right
	{
		get { return m_SphereTransform.Right; }
		set { m_SphereTransform.Right = value; }
	}
	
	public Vector3 Fwd
	{
		get { return m_SphereTransform.Fwd; }
		set { m_SphereTransform.Fwd = value; }
	}
*/
	// Debug
#if UNITY_EDITOR 
	void OnDrawGizmos() 
	{
		Gizmos.color = Colliding ? Color.red : Color.white;
	    Gizmos.DrawSphere(Center, Radius);
	}
#endif


}
