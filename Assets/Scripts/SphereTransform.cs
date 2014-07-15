using UnityEngine;
using System.Collections;

public class SphereTransform : MonoBehaviour
{
	public Transform Pivot = null;
	
	public Quaternion Rotation;
	public Vector3 Up;
	
	void Update()
	{
		Up = Rotation * Vector3.up; // y-axis
	}
	
	void Awake ()
	{
		Rotation = Pivot.transform.rotation;
	}
}
