using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (SphereTransform))]
public class Character : MonoBehaviour
{	
	// ------------ Public, editable in the GUI, serialized
	public float									MaxSpeed = 20.0f;
	public float									Accelleration = 5.0f;
	public float									Inertia = 0.9f;
	public bool										PacmanMovement = false;

	// ------------ Public, serialized
	
	
	// ------------ Public, non-serialized
	// This will be useful on mobile
	//#if ((UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR)
	//[System.NonSerialized] public Joystick     m_LeftJoystick;
	//[System.NonSerialized] public Joystick     m_RightJoystick;
	//#endif
	
	// ------------ Private
	private Quaternion	 							mRotation 		= Quaternion.identity;
	private Quaternion						 		mPrevRotation	= Quaternion.identity;
	private Vector3									mPrevDirection  = Vector3.zero;
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
										
		if((mCollidable.CachedNearest != null) && (mCollidable.CachedNearest.mIsCorridor)) // Automatic "on-rails" navigation in corridors
		{
			MovementOnRails();
		}
		else
		{
			mMoveController.Move(mRotation);
		}
		
		//transform.localRotation = Quaternion.AngleAxis (mFacingAngle, Vector3.up);
	}	
	
	void MovementOnRails()
	{
		Vector3 prevPos = mMoveController.Up * Planet.Instance.Radius;		
		Vector3 newPos = mMoveController.MovedUp(mRotation) * Planet.Instance.Radius;
		
		mCollidable.CachedNearest = NavigationManager.FindClosestWaypoint(prevPos, mCollidable.CachedNearest, NavigationManager.instance.waypointList);
		
		Vector3 movementDirection = (newPos - prevPos);
		float movementLength = movementDirection.magnitude; // TODO: this is constant for a given WalkSpeed
		
		Vector3 currentWaypointPos = mCollidable.CachedNearest.Position;
		
		Vector3 chosenPos = Vector3.zero;
		float chosenDirectionDot = -1.0f;
		Vector3 chosenPosUsingPrev = Vector3.zero;
		float chosenDirectionUsingPrevDot = -1.0f;
		
		const float smoothing = 0.1f; // TODO: this should be probably proportional to the movement speed*deltatime
		
		foreach(WayPoint w in mCollidable.CachedNearest.connections)
		{
			Vector3 candidateDirection = (w.Position - currentWaypointPos).normalized;			
			float cosAngleTimesLenght = Vector3.Dot (candidateDirection, movementDirection);
			
			if(cosAngleTimesLenght > chosenDirectionDot)
			{
				chosenPos = prevPos + candidateDirection * movementLength;
				// smootly converge to the path
				chosenPos = Vector3.Lerp(chosenPos, NavigationManager.PointNearestSegment(chosenPos, w.Position, currentWaypointPos), smoothing);
				chosenDirectionDot = cosAngleTimesLenght;
			}
			
			cosAngleTimesLenght = Vector3.Dot (candidateDirection, mPrevDirection);
			if(cosAngleTimesLenght > chosenDirectionUsingPrevDot)
			{
				chosenPosUsingPrev = prevPos + candidateDirection * movementLength;
				// smootly converge to the path
				chosenPosUsingPrev = Vector3.Lerp(chosenPosUsingPrev, NavigationManager.PointNearestSegment(chosenPosUsingPrev, w.Position, currentWaypointPos), smoothing);
				chosenDirectionUsingPrevDot = cosAngleTimesLenght;
			}
		}
		
		if(chosenDirectionUsingPrevDot > chosenDirectionDot)
			chosenPos = chosenPosUsingPrev;
		
		mMoveController.Move (chosenPos);
		mPrevDirection = movementDirection;
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
		// NOTE: the * 2.0 in the angles are due to the lerp 0.5 -- HACK, not proper physics...
		
		float angularSpeed = (Accelleration * 2.0f) * Time.deltaTime;
		Vector3 perpDirection = new Vector3(Input.GetAxis("Vertical"), 0, -Input.GetAxis("Horizontal"));
		
		if(perpDirection != Vector3.zero)
		{
			perpDirection.Normalize();
			mRotation = Quaternion.AngleAxis (angularSpeed, perpDirection);
		}
		else
		{
			if(!PacmanMovement)
				mRotation = Quaternion.identity; // we could avoid the special case as both .Normalize and .AngleAxis work with 0,0,0
		}	
			
		if(!PacmanMovement)
		{
			float currAngle;
			Vector3 currAxis;
			mPrevRotation.ToAngleAxis(out currAngle, out currAxis);
			
			currAngle = Mathf.Min (MaxSpeed, currAngle) * Inertia * 2.0f;
			
			mRotation = Quaternion.Lerp (
				mRotation,
				Quaternion.AngleAxis(currAngle, currAxis),
				0.5f);
			
			/*mRotation = Quaternion.Lerp (
				mRotation,
				mCurrRotation,
				Inertia);*/
		}
		
		mPrevRotation = mRotation;	
			
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