using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Character2 : MonoBehaviour
{	
	float m_Radius = 0;

	void Start () 
	{
		m_Radius = 12.0f; // planet radius TODO: grab from planet
		Application.targetFrameRate = 60; // TODO: move somewhere else
		transform.position = transform.up * m_Radius;
	}
	
	void Update()
	{
		//Vector3 direction = CameraRelativeDirection(new Vector3(Input.GetAxis("MoveHorizontal"), 0, Input.GetAxis("MoveVertical")));
		Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		direction.Normalize();
		
		float speed = 2.0f * Time.deltaTime;
		Vector3 perpendicular = new Vector3(direction.z, 0, -direction.x);
		
		transform.rotation = Quaternion.AngleAxis (speed, perpendicular); // maybe?!?
		
		// Facing 1 frame late...because of camera LateUpdate
		Vector3 planePoint = transform.up * m_Radius;
		Plane charPlane = new Plane(transform.up, planePoint);
		float distance = 0;
		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		charPlane.Raycast(mouseRay, out distance);
		Vector3 mousePoint = mouseRay.GetPoint(distance);
		
		//mousePoint = Quaternion.Inverse(mCollidable.Rotation) * mousePoint;
		float facingAngle = Mathf.Atan2 (mousePoint.x, mousePoint.z) * Mathf.Rad2Deg;

		transform.localRotation = Quaternion.AngleAxis (facingAngle, Vector3.up);
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