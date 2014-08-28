using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SphereTransform))]
public class Collidable : MonoBehaviour
{
	// ------------ Public, editable in the GUI, serialized
	public float 							Radius 	= 1.0f;
	public float 							Mass 	= 1.0f;
	public float							CapsuleLength = 0.0f; // note: only for static colliders
	public float							RadiusForNavMesh = 0.75f;
	public bool 							Static = false;
	public bool								SphereNavMeshCollision = true;
	public int								Layer = 0;
	public bool								CollideWithOtherLayersOnly = false;

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

	[System.NonSerialized] public Vector3	CapsuleArm = Vector3.zero;
			
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
		if(mCollisionManager)
			mCollisionManager.AddCollider(this);

		PlanetRadius 	= Planet.Instance.Radius;
		Center 			= Up * PlanetRadius;
		CapsuleArm 		= (transform.rotation * Vector3.right) * CapsuleLength;
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
	}
	
	public Vector3 Up
	{
		get { return SphereTransf.Up; }
	}

#if UNITY_EDITOR 
	void OnDrawGizmos() 
	{	
		if(!SphereTransf) // In edit mode we didn't run Awake and Start...
		{
			SphereTransf = GetComponent<SphereTransform> ();			
			SphereTransf.Setup();
			PlanetRadius = (FindObjectOfType(typeof(Planet)) as Planet).Radius;
		}
		
		Center = (Rotation * Vector3.up) * PlanetRadius;
		
		if( CapsuleLength == 0.0f || (!Static) )
		{
			Gizmos.color = Colliding ? Color.red : Color.white;
			
	    	Gizmos.DrawWireSphere(Center, Radius);
		}
		else
		{
			Gizmos.color = Colliding ? Color.red : Color.yellow;
			
			CapsuleArm = (transform.rotation * Vector3.right) * CapsuleLength;
			
			Gizmos.DrawLine(Center - CapsuleArm, Center + CapsuleArm);
			Gizmos.DrawWireSphere(Center, Radius);
			Gizmos.DrawWireSphere(Center - CapsuleArm, Radius);
			Gizmos.DrawWireSphere(Center + CapsuleArm, Radius);			
		}
	}
#endif


}
