using UnityEngine;
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
	//[System.NonSerialized] float					mFacingAngle 	= 0;
	[System.NonSerialized] SphereTransform			mMoveController	= null;	
	[System.NonSerialized] WayPoint					mCachedNearest = null;

	void Start () 
	{
		mMoveController = GetComponent<SphereTransform>();
	}
	
	void Update()
	{
		UpdateInput();
		//transform.localRotation = Quaternion.AngleAxis (mFacingAngle, Vector3.up);
		
		//mMoveController.Move(mRotation);
		
		// TODO: move all this in the collision manager, after resolving sphere collisions
		Vector3 currentPos = (mMoveController.Rotation * mRotation) * Vector3.up * Planet.GetRadius();
		Vector3 newPos = 
			NavigationManager.instance.PointNavMeshEdgesCollision(
				currentPos, 0.75f, mCachedNearest, out mCachedNearest);
				
		mMoveController.Move(newPos);
		
		//if( Vector3.Distance(currentPos, newPos) > 0.000001f ) // TODO: use dot instead
		//	mMoveController.Move(Quaternion.FromToRotation(mMoveController.Rotation * Vector3.up, newPos.normalized));
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