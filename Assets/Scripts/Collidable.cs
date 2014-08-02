using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SphereTransform))]
public class Collidable : MonoBehaviour
{
	// ------------ Public, editable in the GUI, serialized
	public float 							Radius 	= 1.0f;
	public float 							Mass 	= 1.0f;
	public float							RadiusForNavMesh = 0.75f;
	public bool 							Static 	= false;
	public bool								SphereNavMeshCollision = true;
	public int								Layer = 0;

	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	[System.NonSerialized] public SphereTransform SphereTransf = null;
	[System.NonSerialized] public Vector3 	Center 		= Vector3.zero;
	[System.NonSerialized] public float 	AngleRadius = 1;
	[System.NonSerialized] public bool 		Colliding 	= false;
	[System.NonSerialized] public bool 		Active 		= true;
	[System.NonSerialized] public Vector3 	Min = Vector3.zero;
	[System.NonSerialized] public Vector3 	Max = Vector3.zero;
	[System.NonSerialized] public float 	MinValue = 0.0f;
	[System.NonSerialized] public float 	MaxValue = 0.0f;
	[System.NonSerialized] public WayPoint	CachedNearest = null;

	public delegate void 					CollisionCallback(Collidable other);
	[System.NonSerialized] public CollisionCallback OnCollision;
	
	// ------------ Private	
	private float 							PlanetRadius 		= 1;
	private CollisionManager 				mCollisionManager 	= null;	
	
	void Awake()
	{
		SphereTransf = GetComponent<SphereTransform> ();
		mCollisionManager = FindObjectOfType(typeof(CollisionManager)) as CollisionManager;
	}
	
	void Start ()
	{	
		mCollisionManager.AddCollider(this);

		PlanetRadius 	= Planet.Instance.Radius;
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
		get { return SphereTransf.Rotation; }
		//set { m_SphereTransform.Rotation = value; }
	}
	
	public Vector3 Up
	{
		get { return SphereTransf.Up; }
		//set { m_SphereTransform.Up = value; }
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
