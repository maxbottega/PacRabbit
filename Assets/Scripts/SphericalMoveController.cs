using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SphereTransform))]
public class SphericalMoveController : MonoBehaviour
{
	public SphereTransform m_SphereTransform = null;

	void Awake()
	{
		m_SphereTransform = GetComponent<SphereTransform> ();
		m_SphereTransform.Pivot = transform.parent;
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
		m_SphereTransform.Rotation *= deltaRotation;
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
		m_SphereTransform.Rotation = Quaternion.FromToRotation(Vector3.up, targetPosition.normalized);
	}

	public void Apply ()
	{
		//m_SphereTransform.Pivot.rotation = Quaternion.Slerp(m_SphereTransform.Pivot.rotation, m_SphereTransform.Rotation, 1);
		m_SphereTransform.Pivot.rotation = m_SphereTransform.Rotation;
	}

	//Wrapper Functions
	public Quaternion Rotation
	{
		get { return m_SphereTransform.Rotation; }
	}
	
	public Vector3 Up
	{
		get { return m_SphereTransform.Up; }
	}
	
	/*	
	public Vector3 Right
	{
		get { return m_SphereTransform.Right; }
	}
	
	public Vector3 Fwd
	{
		get { return m_SphereTransform.Fwd; }
	}*/
}




