﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (SphereTransform))]
public class Character : MonoBehaviour
{	
	// ------------ Public, editable in the GUI, serialized
	public float									WalkSpeed = 20.0f;
	public float									Drag = 0.5f;

	// ------------ Public, serialized
	
	
	// ------------ Public, non-serialized
	// This will be useful on mobile
	//#if ((UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR)
	//[System.NonSerialized] public Joystick     m_LeftJoystick;
	//[System.NonSerialized] public Joystick     m_RightJoystick;
	//#endif
	
	// ------------ Private
	private Quaternion	 							mRotation 		= Quaternion.identity;
	private Quaternion						 		mCurrRotation	= Quaternion.identity;
	private Collidable								mCollidable 	= null;
	//private float									mFacingAngle 	= 0;
	private SphereTransform							mMoveController	= null;	
	private PlayMakerFSM							mPlaymaker		= null;

	void Start () 
	{
		mMoveController = GetComponent<SphereTransform>();
		mCollidable 	= GetComponent<Collidable> ();
		mPlaymaker 		= GetComponent<PlayMakerFSM> ();
		
		if (mCollidable && mPlaymaker)
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
				
				if(cosAngleTimesLenght > chosenDirectionDot) // TODO: under a given angle we should make the choice based on previous direction, NOT current input!
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
/*
		Enemy enemy = other.GetComponent<Enemy> ();
		
		if (enemy != null)
			enemy.gameObject.SetActive (false);
*/		
			
		Enemy enemy = other.GetComponent<Enemy> ();
		
		if(mPlaymaker.Fsm.EventTarget != null)
		{
			Debug.LogError ("EventTarget set in Enemy FSM - this might cause issues so we reset it");
			// EventTarget might redirect SendEvent to another target, we check here to be safe as it seems
			// to be a possible cause of nasty bugs, but I haven't verified this directly yet, just reading
			// about in on some forums...
			mPlaymaker.Fsm.EventTarget = null;
		}
		
		if(enemy!=null)
		{
			mPlaymaker.SendEvent("EnemyCollision");
			return;
		}
		
		mPlaymaker.SendEvent("OtherCollision");
	}
	
	void UpdateInput()
	{
		float angularSpeed = WalkSpeed * Time.deltaTime;
		
		Vector3 perpDirection = new Vector3(Input.GetAxis("Vertical"), 0, -Input.GetAxis("Horizontal"));
		
		if(perpDirection != Vector3.zero)
		{
			perpDirection.Normalize();
			mRotation = Quaternion.AngleAxis (angularSpeed, perpDirection);
		}
		else
			mRotation = Quaternion.identity; // we could avoid the special case as both .Normalize and .AngleAxis work with 0,0,0
		
		mRotation = Quaternion.Lerp (
			mRotation,
			mCurrRotation,
			Drag);
		mCurrRotation = mRotation;	
			
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
}