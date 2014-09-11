using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class WayPointGizmoDrawer : MonoBehaviour 
{
	public bool DrawGizmos = false;

	// Update is called once per frame
	void Update () 
	{
		foreach (Transform child in transform)
		{
			WayPoint wp = child.GetComponent<WayPoint> ();
			if (wp!=null)
				wp.mDrawGizmo = DrawGizmos;
		}
	}
}
