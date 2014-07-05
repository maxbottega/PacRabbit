using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Character2 : MonoBehaviour
{
	#region Public Variables
	public float		m_WalkSpeed = 1;
	
	[System.NonSerialized]
	public Vector3		mRotationAxis = Vector3.right;
	[System.NonSerialized]
	public Vector3		mLightDirection = Vector3.right;
	[System.NonSerialized]
	public Quaternion 	mRotation = Quaternion.identity;
	#endregion
	
	#region Private/Protected Variables
	float				mFacingAngle = 0;
	float 				m_Radius = 0;
	#endregion
	
	void Start () 
	{
		m_Radius = 12.0f; // planet radius TODO: grab from planet
		Application.targetFrameRate = 60; // TODO: move somewhere else
		transform.position = transform.up * m_Radius;
	}
	
	void Update()
	{
		UpdateInput();
		
		//mCollidable.Rotation *= mRotation;
		transform.localRotation = Quaternion.AngleAxis (mFacingAngle, Vector3.up);
	}	
	
	void UpdateInput()
	{
		//Vector3 direction = CameraRelativeDirection(new Vector3(Input.GetAxis("MoveHorizontal"), 0, Input.GetAxis("MoveVertical")));
		Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		direction.Normalize();
		
		//mAnimator.SetFloat("WalkFwdSpeed", direction.magnitude);
		
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
			
			//mousePoint = Quaternion.Inverse(mCollidable.Rotation) * mousePoint;
			mFacingAngle = Mathf.Atan2 (mousePoint.x, mousePoint.z) * Mathf.Rad2Deg;
		}
	}
	
	Vector3 CameraRelativeDirection (Vector3 dir)
	{
		dir = Camera.main.transform.TransformDirection(dir);
		dir = Quaternion.Inverse(transform.parent.rotation) * dir;
		dir.y = 0;
		dir.Normalize();
		
		return dir;
	}	
}