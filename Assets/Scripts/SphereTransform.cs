﻿using UnityEngine;
using System.Collections;

public class SphereTransform : MonoBehaviour
{
	// ------------ Public, editable in the GUI, serialized
		
	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	[System.NonSerialized] public Quaternion 	Rotation;
	[System.NonSerialized] public Vector3 		Up;
	[System.NonSerialized] public Transform		Pivot = null;
	
	void Awake()
	{
		Pivot = transform.parent;
		Rotation = Pivot.transform.rotation;
	}
	
	void Start()
	{
	}
	
	void Update()
	{
		Up = Rotation * Vector3.up; // y-axis
	}
		
	void LateUpdate ()	
	{
		Apply ();
	}
	
	public void Move (Quaternion deltaRotation)
	{
		Rotation *= deltaRotation;
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
		Quaternion rotation = Quaternion.FromToRotation (Up, targetPosition.normalized);
		Rotation = rotation * Rotation;
	}
	
	public void Apply ()
	{
		Pivot.rotation = Rotation;
	}
}
