using UnityEngine;
using System.Collections;

public interface ISphereMoveController
{
	void 		Move(Quaternion rotation);
	Quaternion 	GetCurrentRotation();
	Vector3 	GetUpVector();
}

