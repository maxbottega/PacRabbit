using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
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
	
	void Update()
	{
	}

#if UNITY_EDITOR 
	void OnDrawGizmos() 
	{
		// This is where the planet thinks to be
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(Vector3.zero, Radius);
	}
#endif
}
