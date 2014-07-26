using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class WayPointGizmoDrawer : MonoBehaviour 
{
	public bool DrawGizmos = false;
	private bool mDrawGizmos = false;

	// Update is called once per frame
	void Update () 
	{
		if(DrawGizmos == mDrawGizmos)
			return;
	
		foreach (Transform child in transform)
		{
			WayPoint wp = child.GetComponent<WayPoint> ();
			wp.mDrawGizmo = DrawGizmos;
		}
		
		mDrawGizmos = DrawGizmos;
	}
}
