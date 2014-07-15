using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SphereTransform))]
public class SphericalMoveController : MonoBehaviour
{
	// ------------ Public, editable in the GUI, serialized
	
	// ------------ Public, serialized
		
	// ------------ Public, non-serialized
	[System.NonSerialized] public SphereTransform 		mSphereTransform = null;

	void Awake()
	{
		mSphereTransform = GetComponent<SphereTransform> ();
		mSphereTransform.Pivot = transform.parent;
	}

	void Start()
	{
	}

	void Update ()
	{
	}

	void LateUpdate ()	
	{
		Apply ();
	}

	public void Move (Quaternion deltaRotation)
	{
		mSphereTransform.Rotation *= deltaRotation;
	}

	public void Move (Vector3 targetPosition, float speed)
	{
		Vector3 direction = targetPosition - Vector3.Dot (targetPosition, Up) * Up;
		Vector3 localDirection = Quaternion.Inverse(Rotation) * direction;
		float angle = Mathf.Atan2 (localDirection.x, localDirection.z) * Mathf.Rad2Deg;
		
		Quaternion yRot = Quaternion.AngleAxis (angle, Vector3.up);
		Quaternion xRot = Quaternion.AngleAxis (speed, Vector3.right);
		
		Move (xRot*yRot);
	}
	
	public void Move (Vector3 targetPosition)
	{
		mSphereTransform.Rotation = Quaternion.FromToRotation(Vector3.up, targetPosition.normalized);
	}

	public void Apply ()
	{
		//m_SphereTransform.Pivot.rotation = Quaternion.Slerp(m_SphereTransform.Pivot.rotation, m_SphereTransform.Rotation, 1);
		mSphereTransform.Pivot.rotation = mSphereTransform.Rotation;
	}

	public Quaternion Rotation
	{
		get { return mSphereTransform.Rotation; }
	}
	
	public Vector3 Up
	{
		get { return mSphereTransform.Up; }
	}
}




