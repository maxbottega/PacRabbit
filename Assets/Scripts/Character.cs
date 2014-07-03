using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Character : MonoBehaviour
{
	#region Public Variables
	public float		m_WalkSpeed;
	
	public Joystick     m_LeftJoystick;
	public Joystick     m_RightJoystick;
	
	[System.NonSerialized]
	public Vector3		mRotationAxis = Vector3.right;
	[System.NonSerialized]
	public Vector3		mLightDirection = Vector3.right;
	[System.NonSerialized]
	public Quaternion 	mRotation = Quaternion.identity;
	#endregion
	
	#region Private/Protected Variables
	float				mFacingAngle = 0;
	Vector2 			mLocoDirection = Vector2.zero;
	Collidable          mCollidable = null;
	Animator			mAnimator = null;
	float 				m_Radius = 1;
	#endregion

	void Start () 
	{
		mCollidable = GetComponent<Collidable>();
		mAnimator = GetComponent<Animator>();
		Application.targetFrameRate = 60;
	
		m_Radius = Planet.GetRadius();
		transform.position = transform.up * m_Radius;
	}

	#region Update
	void Update()
	{
		UpdateInput();
				
		mCollidable.Rotation *= mRotation;
		transform.localRotation = Quaternion.AngleAxis (mFacingAngle, Vector3.up);
	}	
	#endregion

	#region Input
	
	void UpdateInputMOBILE()
	{			
		
		Vector3 direction = CameraRelativeDirection(new Vector3(m_LeftJoystick.position.x, 0, m_LeftJoystick.position.y));
		float speed = m_WalkSpeed * Time.deltaTime * direction.magnitude;
		Vector3 perpendicular = new Vector3(direction.z, 0, -direction.x);
		mRotationAxis = perpendicular;
		mRotation = Quaternion.AngleAxis (speed, perpendicular);
		
		mAnimator.SetFloat("WalkFwdSpeed", speed);
		
		// Facing 
		{
			if (m_RightJoystick.position.SqrMagnitude() > 0.1f)
			{
				Vector3 facingDir = CameraRelativeDirection(new Vector3(m_RightJoystick.position.x, 0, m_RightJoystick.position.y));
				mLightDirection = transform.parent.TransformDirection(facingDir);
				Debug.DrawRay(transform.position, mLightDirection*10);
				mFacingAngle = Mathf.Atan2 (facingDir.x, facingDir.z) * Mathf.Rad2Deg;
			}
		}
	}
	
	void UpdateInputEDITOR()
	{			
		Vector3 direction = CameraRelativeDirection(new Vector3(Input.GetAxis("MoveHorizontal"), 0, Input.GetAxis("MoveVertical")));
		//Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		direction.Normalize();
		
		mAnimator.SetFloat("WalkFwdSpeed", direction.magnitude);
		
		float speed = m_WalkSpeed * Time.deltaTime * direction.magnitude;
		Vector3 perpendicular = new Vector3(direction.z, 0, -direction.x);
		mRotationAxis = perpendicular;
		mRotation = Quaternion.AngleAxis (speed, perpendicular);
		
		// Facing 1 frame late...because of camera LateUpdate
		{
			Vector3 planePoint = transform.up * m_Radius;
			Plane charPlane = new Plane(transform.up, planePoint);
			float distance = 0;
			Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			charPlane.Raycast(mouseRay, out distance);
			Vector3 mousePoint = mouseRay.GetPoint(distance);
			_DebugMouse = mousePoint;
			
			mousePoint = Quaternion.Inverse(mCollidable.Rotation) * mousePoint;
			mFacingAngle = Mathf.Atan2 (mousePoint.x, mousePoint.z) * Mathf.Rad2Deg;
		}
	}
	
	Vector3 _DebugMouse = Vector3.zero;
//	void OnDrawGizmos() 
//	{
//        Gizmos.DrawSphere(_DebugMouse, 0.5f);
//    }
	
	Vector3 CameraRelativeDirection (Vector3 dir)
	{
//		dir = Camera.main.transform.TransformDirection(dir);
//		dir = Quaternion.Inverse(transform.parent.rotation) * dir;
//		dir.y = 0;
		dir.Normalize();
		
		return dir;
	}
	
	void UpdateInput()
	{
#if ((UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR)
		UpdateInputMOBILE();
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
		UpdateInputEDITOR();
#endif
	}	
	
	#endregion
}