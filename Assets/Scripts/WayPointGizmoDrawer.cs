using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GizmoDrawer : MonoBehaviour 
{
	public bool DrawGizmos = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		foreach (Transform child in transform)
		{
			WayPoint wp = child.GetComponent<WayPoint> ();
			wp.DrawGizmo = DrawGizmos;
		}
	}
}
