using UnityEngine;
using System.Collections;

public class SphereTransform : MonoBehaviour
{
	// ------------ Public, editable in the GUI, serialized
	public Transform 							Pivot = null;
		
	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	[System.NonSerialized] public Quaternion 	Rotation;
	[System.NonSerialized] public Vector3 		Up;
	
	void Update()
	{
		Up = Rotation * Vector3.up; // y-axis
	}
	
	void Awake ()
	{
		Rotation = Pivot.transform.rotation;
	}
}
