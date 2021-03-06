﻿using UnityEngine;
using System.Collections;

// To avoid setting transform.rotation every time we manipulate the position, we cache the rotation of an object on the sphere
// in this class and update it only once in the LateUpdate(). All objects on the sphere are supposed to be parented with a null in zero
// and aligned on the Y axis, the null rotates to move them...
public class SphereTransform : MonoBehaviour
{
	// ------------ Public, editable in the GUI, serialized
		
	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	
	// ------------ Private
	private Quaternion 		mRotation;
	public 	Vector3 		mUp;
	private Vector3			mUpPrevious;
	private Transform		mPivot = null;
	
	public Quaternion Rotation
	{
		get { return mRotation; }
	}

	public Vector3 UpPreviousUpdate
	{
		get { return mUpPrevious; }
	}
			
	public Vector3 Up
	{
		get { return mUp; }
	}
	
	public Transform Pivot
	{
		get { return mPivot; }
	}
	
	void Awake()
	{
		Setup();
	}
	
	public void Setup()
	{
		mPivot = transform.parent;
		
		if(mPivot)
		{
			mRotation = mPivot.transform.rotation;
			if(mPivot.transform.position != Vector3.zero)
			{
				Debug.LogError("Wrong pivot, has to be in zero:" + name);
				mPivot.transform.position = Vector3.zero;
			}	
		}
		else
			Debug.LogWarning("No parent pivot for an object that has SphereTransform:" + name);
	}
	
	void Update()
	{
		mUpPrevious = mUp;
		mUp = mRotation * Vector3.up;
	}
		
	void LateUpdate ()	
	{
		ApplyCachedTransform ();
	}
	
	public void ApplyCachedTransform ()
	{
		if(mPivot)
			mPivot.rotation = mRotation;
	}
	
	// Adding the Space parameter is useful when the rotation specified is in local space.
	public void Move (Quaternion deltaRotation, Space space)
	{
		mRotation = space==Space.Self ? mRotation * deltaRotation : deltaRotation * mRotation;
	}
	
	public Vector3 MovedUp(Quaternion deltaRotation)
	{
		return (mRotation * deltaRotation) * Vector3.up;
	}
	
	public void Move (Vector3 targetPosition, float speed)
	{
		Vector3 direction = targetPosition - Vector3.Dot (targetPosition, mUp) * mUp;
		Vector3 localDirection = Quaternion.Inverse(mRotation) * direction;
		float angle = Mathf.Atan2 (localDirection.x, localDirection.z) * Mathf.Rad2Deg;
		
		Quaternion yRot = Quaternion.AngleAxis (angle, Vector3.up);
		Quaternion xRot = Quaternion.AngleAxis (speed, Vector3.right);
		
		Move (xRot*yRot, Space.World);
	}
	
	public void Move (Vector3 targetPosition)
	{
		Quaternion rotation = Quaternion.FromToRotation (mUp, targetPosition.normalized);
		mRotation = rotation * mRotation;
	}
	
	public void ImmediateSet (Quaternion rotation)
	{
		mRotation = rotation;
		mUp = mRotation * Vector3.up; 
	}
}
