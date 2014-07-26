﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (SphereTransform))]
public class Character : MonoBehaviour
{	
	// ------------ Public, editable in the GUI, serialized
	public float									WalkSpeed = 20.0f;

	// ------------ Public, serialized
	
	
	// ------------ Public, non-serialized
	
	// This will be useful on mobile
	//#if ((UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR)
	//[System.NonSerialized] public Joystick     m_LeftJoystick;
	//[System.NonSerialized] public Joystick     m_RightJoystick;
	//#endif
	
	[System.NonSerialized] public Vector3			mLightDirection = Vector3.right;
	[System.NonSerialized] public Quaternion 		mRotation 		= Quaternion.identity;

	private Collidable								mCollidable 	= null;
	//[System.NonSerialized] float					mFacingAngle 	= 0;
	[System.NonSerialized] SphereTransform			mMoveController	= null;	

	void Start () 
	{
		mMoveController = GetComponent<SphereTransform>();
		mCollidable 	= GetComponent<Collidable> ();
		
		if (mCollidable)
			mCollidable.OnCollision = new Collidable.CollisionCallback(OnCollision);
	}
	
	void Update()
	{
		UpdateInput();	
		
		if((mCollidable.CachedNearest != null) && (mCollidable.CachedNearest.mIsCorridor)) // automatic on-rails navigation in corridors
		{
			Vector3 prevPos = mMoveController.Up * Planet.Instance.Radius;		
			Vector3 newPos = mMoveController.MovedUp(mRotation) * Planet.Instance.Radius;
			
			mCollidable.CachedNearest = NavigationManager.FindClosestWaypoint(prevPos, mCollidable.CachedNearest, NavigationManager.instance.waypointList);
			
			Vector3 movementDirection = (newPos - prevPos);
			float movementLength = movementDirection.magnitude; // TODO: this is constant for a given WalkSpeed
			
			Vector3 currentWaypointPos = mCollidable.CachedNearest.Position;
			Vector3 chosenPos = Vector3.zero;
			float chosenDirectionDot = -1.0f;
			
			foreach(WayPoint w in mCollidable.CachedNearest.connections)
			{
				Vector3 candidateDirection = (w.Position - currentWaypointPos).normalized;			
				float cosAngleTimesLenght = Vector3.Dot (candidateDirection, movementDirection);
				
				if(cosAngleTimesLenght > chosenDirectionDot)
				{
					chosenPos = prevPos + candidateDirection * movementLength;
					
					// smootly converge to the path
					chosenPos = Vector3.Lerp(chosenPos, NavigationManager.PointNearestSegment(chosenPos, w.Position, currentWaypointPos), 0.1f);
					
					chosenDirectionDot = cosAngleTimesLenght;
				}
			}

			mMoveController.Move (chosenPos);
		}
		else
		{
			mMoveController.Move(mRotation);
		}
		
		//transform.localRotation = Quaternion.AngleAxis (mFacingAngle, Vector3.up);
	}	
	
	public void OnCollision(Collidable other)
	{
		// Collision reaction
		Enemy enemy = other.GetComponent<Enemy> ();
		
		if (enemy != null)
			enemy.gameObject.SetActive (false);
	}
	
	void UpdateInput()
	{
		Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		direction.Normalize();

		float speed = WalkSpeed * Time.deltaTime /* * direction.magnitude */;
		Vector3 perpendicular = new Vector3(direction.z, 0, -direction.x);
		mRotation = Quaternion.AngleAxis (speed, perpendicular);
		
		// TODO: facing from direction, not from mouse
		
		/* Facing 1 frame late...because of camera LateUpdate
		{
			Vector3 planePoint = transform.up * Planet.GetRadius();
			Plane charPlane = new Plane(transform.up, planePoint);
			float distance = 0;
			Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

			charPlane.Raycast(mouseRay, out distance);
			Vector3 mousePoint = mouseRay.GetPoint(distance);
			
			mousePoint = Quaternion.Inverse(mMoveController.Rotation) * mousePoint;
			mFacingAngle = Mathf.Atan2 (mousePoint.x, mousePoint.z) * Mathf.Rad2Deg;
		}*/
	}

	#if UNITY_EDITOR 
	void OnDrawGizmos() 
	{
	}
	#endif
}