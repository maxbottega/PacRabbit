using UnityEngine;
using System.Collections;

// As manipulating transforms is slow, we "cache" them via the Move then the controller will apply
// them once at the end of the frame... All movements are done by manipulating a pivot at the origin of the world
// TODO: might be a bit overthinking it, we could just do it manually in each script
public interface ISphereMoveController
{
	void 		Move(Quaternion rotation);
	void		MoveFromPoint(Vector3 p);
	Vector3		GetCurrentPosition();
	Quaternion 	GetCurrentRotation();
	Vector3 	GetUpVector();
}

