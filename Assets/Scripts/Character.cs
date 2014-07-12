using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Character : MonoBehaviour
{	
	public float		m_WalkSpeed;
	
	#if ((UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR)	
	public Joystick     m_LeftJoystick;
	public Joystick     m_RightJoystick;
	#endif
	
	[System.NonSerialized]
	public Vector3		mLightDirection = Vector3.right;
	[System.NonSerialized]
	public Quaternion 	mRotation 		= Quaternion.identity;
	
	float					mFacingAngle 	= 0;
	ISphereMoveController	mMoveController	= null;
	WayPoint				mCachedNearest = null;

	void Start () 
	{
		mMoveController = GetComponent (typeof(ISphereMoveController)) as ISphereMoveController;
		transform.position = transform.up * Planet.GetRadius();
	}
	
	void Update()
	{
		UpdateInput();
		
		mMoveController.Move(mRotation);
		
		Vector3 newPos = 
			NavigationManager.instance.PointNavMeshEdgesCollision(
				mMoveController.GetCurrentPosition(), 0.75f, mCachedNearest, out mCachedNearest);

		mMoveController.MoveFromPoint(newPos);
		
		transform.localRotation = Quaternion.AngleAxis (mFacingAngle, Vector3.up);
	}	
	
	void UpdateInput()
	{
		Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		direction.Normalize();

		float speed = m_WalkSpeed * Time.deltaTime /* * direction.magnitude */;
		Vector3 perpendicular = new Vector3(direction.z, 0, -direction.x);
		mRotation = Quaternion.AngleAxis (speed, perpendicular);
		
		// TODO: facing from direction, not from mouse
		// Facing 1 frame late...because of camera LateUpdate
		{
			Vector3 planePoint = transform.up * Planet.GetRadius();
			Plane charPlane = new Plane(transform.up, planePoint);
			float distance = 0;
			Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

			charPlane.Raycast(mouseRay, out distance);
			Vector3 mousePoint = mouseRay.GetPoint(distance);
			
			mousePoint = Quaternion.Inverse(mMoveController.GetCurrentRotation()) * mousePoint;
			mFacingAngle = Mathf.Atan2 (mousePoint.x, mousePoint.z) * Mathf.Rad2Deg;
		}
	}
	
	#if UNITY_EDITOR 
	void OnDrawGizmos() 
	{
		//Debug Draws
	}
	#endif

}