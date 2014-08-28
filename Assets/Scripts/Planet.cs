using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class Planet : MonoBehaviour 
{
	// ------------ Public, editable in the GUI, serialized
	public float Radius = 10.0f;
	
	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	[System.NonSerialized] static public Planet Instance = null;
	
	void Awake()
	{
		Instance = this;
	}
	
	void Start()
	{
		Application.targetFrameRate = 60; 
	}
	
	//void Update()
	//{
	//}

#if UNITY_EDITOR 
	void OnDrawGizmosSelected() 
	{
		// This is where the planet thinks to be
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(Vector3.zero, Radius);
		
		// This is the reference axis, all objects are expressed as rotations of this
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(Vector3.up * Radius, 1.0f);
	}
#endif
}
