using UnityEngine;
using System.Collections;

public class Collidable : MonoBehaviour, ISphereMoveController
{
	public Transform Pivot = null;
	public float Mass = 1;
	public bool Static = false;

	[System.NonSerialized]
	public Quaternion Rotation;
	[System.NonSerialized]
	public Vector3 Up;
	[System.NonSerialized]
	public Vector3 Fwd;
	[System.NonSerialized]
	public Vector3 Right;
	[System.NonSerialized]
	public Vector3 Center;
	[System.NonSerialized]
	public float AngleRadius = 1;
	[System.NonSerialized]
	public float Radius = 1;
	[System.NonSerialized]
	public Quaternion CollisionError = Quaternion.identity;

	public Vector3 GetUpVector()
	{
		return Up;
	}
	
	[System.NonSerialized]
	public bool Colliding = false;
	
	[System.NonSerialized]
	public bool Active = true;
	
	[System.NonSerialized]
	public Vector3 Min = Vector3.zero;
	
	[System.NonSerialized]
	public Vector3 Max = Vector3.zero;
	
	[System.NonSerialized]
	public float MinValue = 0.0f;
	[System.NonSerialized]
	public float MaxValue = 0.0f;
	
	public delegate void CollisionCallback(Collidable other);
	
	public CollisionCallback OnCollision;

	CollisionManager mCollisionManager;	

	float PlanetRadius = 1; // TODO get from planet
	
	void Awake()
	{
		mCollisionManager = FindObjectOfType(typeof(CollisionManager)) as CollisionManager;
		mCollisionManager.AddCollider(this);
	}
	
	void Start ()
	{	
		PlanetRadius = Planet.GetRadius();

		Pivot = transform.parent;
		
		SphereCollider sphereCollider = GetComponent<SphereCollider>();
		
#if UNITY_EDITOR 
		if (sphereCollider==null)
			Debug.LogError("This object needs a sphere collider in order to collide");
#endif
		Radius = sphereCollider.radius * Pivot.lossyScale.x;
		
		AngleRadius = 2*Mathf.Asin(Radius/(2*PlanetRadius))*Mathf.Rad2Deg; 
		
		Init ();
		
		Destroy (collider);
		Destroy(rigidbody);
		
		Min.x = Up.x * PlanetRadius - Radius;
		Min.y = Up.y * PlanetRadius - Radius;
		Min.z = Up.z * PlanetRadius - Radius;
		
		Max.x = Up.x * PlanetRadius + Radius;
		Max.y = Up.y * PlanetRadius + Radius;
		Max.z = Up.z * PlanetRadius + Radius;
		
		Center = Up * PlanetRadius;
	}
	
	void OnEnable()
	{
		//mCollisionManager.AddCollider(this);
	}
	
	void OnDisable()
	{
		//mCollisionManager.RemoveCollider(this);
	}
	
	public void DisableCollisions()
	{
		mCollisionManager.RemoveCollider(this);
	}

	public void Move(Quaternion rotation)
	{
		Rotation *= rotation;
	}

	public Quaternion GetCurrentRotation()
	{
		return Rotation;
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
			Up = Rotation * Vector3.up; // y-axis
			
			Min.x = Up.x * PlanetRadius - Radius;
			Min.y = Up.y * PlanetRadius - Radius;
			Min.z = Up.z * PlanetRadius - Radius;
			
			Max.x = Up.x * PlanetRadius + Radius;
			Max.y = Up.y * PlanetRadius + Radius;
			Max.z = Up.z * PlanetRadius + Radius;
			
			Center = Up * PlanetRadius;
		}	
	}
	
	void LateUpdate ()	
	{
		if (!Static)
			Apply ();
	}
	
	public void Apply ()
	{
		Pivot.rotation = Quaternion.Slerp(Pivot.rotation, Rotation, mCollisionManager.CollisionErrorInterpolation);
	}
	
	public void Init (Quaternion rotation)
	{
		Rotation	= rotation;
		Up = Rotation * Vector3.up;
		Right = Rotation * Vector3.right;
		Fwd = Rotation * Vector3.forward;
		Apply();
	}
	
	public void Init ()
	{
		Rotation = Pivot.rotation;
		Up = Rotation * Vector3.up; // TODO: ?!? rotation changes but these vectors won't
		Right = Rotation * Vector3.right;
		Fwd = Rotation * Vector3.forward;
		Apply ();
	}
	
#if UNITY_EDITOR 
	void OnDrawGizmos() 
	{
		Gizmos.color = Colliding ? Color.red : Color.white;
	    Gizmos.DrawSphere(transform.position, Radius);
	}
#endif
}
