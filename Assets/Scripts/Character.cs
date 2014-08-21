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

	private Collidable								mCollidable 	= null;
	[System.NonSerialized] float					mFacingAngle 	= 0;
	[System.NonSerialized] public Vector3			mLocoDir 		= Vector3.right;
	[System.NonSerialized] SphereTransform			mMoveController	= null;	
	
	// Movement Physics
	[System.NonSerialized] public Vector3			mVelocityVec 		= Vector3.zero;
	[System.NonSerialized] public Vector3			mAccelerationVec	= Vector3.zero;
	
	[System.NonSerialized] public Vector3			mGravity			= Vector3.zero;
	

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
		mMoveController.Move(mRotation);
		
		transform.localRotation = Quaternion.AngleAxis (mFacingAngle, Vector3.up);
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
		Vector3 inputVec = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		bool input = inputVec.sqrMagnitude > Mathf.Epsilon;
		
		mAccelerationVec = inputVec -  mVelocityVec;
		mVelocityVec = mVelocityVec + mAccelerationVec * Time.deltaTime -  mVelocityVec * 0.05f;
		
		Vector3 perpendicular = new Vector3(mVelocityVec.normalized.z, 0, -mVelocityVec.normalized.x);
		mRotation = Quaternion.AngleAxis (mVelocityVec.magnitude, perpendicular);
		Debug.Log (mVelocityVec.magnitude + mAccelerationVec.magnitude);
		
		if (!Mathf.Approximately(mVelocityVec.magnitude, 0.0f))
			mFacingAngle = Mathf.Atan2 (mVelocityVec.normalized.x, mVelocityVec.normalized.z) * Mathf.Rad2Deg;
			
		bool jump = Input.GetKey(KeyCode.Space);
	}

	#if UNITY_EDITOR 
	void OnDrawGizmos() 
	{
	}
	#endif
}