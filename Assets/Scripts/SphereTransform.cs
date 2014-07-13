using UnityEngine;
using System.Collections;

public class SphereTransform : MonoBehaviour
{
	public Transform Pivot = null;
	
	public Quaternion Rotation;
	public Vector3 Up;
	public Vector3 Right;
	public Vector3 Fwd;	

	void Awake ()
	{
		Rotation = Pivot.transform.rotation;
	}
}
