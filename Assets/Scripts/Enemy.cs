using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Enemy : MonoBehaviour 
{
	[System.NonSerialized]
	public Transform 		Target 				= null;

	[SerializeField]	
	float 					m_Speed 			= 5;
	float 					mSpeedMultiplier 	= 1;
	int 					mNumCollisions 		= 0;	
	ISphereMoveController 	mMoveController 	= null;	
	Collidable				mCollidable 		= null;
	
	//EnemyManager mEnemyManager = null;

	void Start () 
	{
		//mEnemyManager = FindObjectOfType(typeof(EnemyManager)) as EnemyManager;
		mMoveController = (ISphereMoveController) GetComponent(typeof(ISphereMoveController));
		mCollidable = (Collidable) GetComponent(typeof(Collidable));

		if (mCollidable)
			mCollidable.OnCollision = new Collidable.CollisionCallback(OnCollision);

		Target = (FindObjectOfType(typeof(Character)) as Character).transform; // TODO 

		transform.position = transform.up * Planet.GetRadius();
	}

	void Update () 
	{
		UpdateLocomotion(Time.deltaTime, m_Speed);
		
		mSpeedMultiplier = 1.0f;
		mNumCollisions = 0;	
	}

	public void OnCollision(Collidable other)
	{
	
		++mNumCollisions;
		if (mNumCollisions>3)
		{				
			mSpeedMultiplier = 0.3f;
		}
	}
	
	virtual protected void UpdateLocomotion(float dt, float speed)
	{
		float currentSpeed = speed*mSpeedMultiplier*dt;
		
		Vector3 chasePos = Target ? Target.position : transform.position;

		Vector3 direction = chasePos - Vector3.Dot (chasePos, mMoveController.GetUpVector())*mMoveController.GetUpVector();
		Vector3 localDirection = Quaternion.Inverse(mMoveController.GetCurrentRotation()) * direction;
		float angle = Mathf.Atan2 (localDirection.x, localDirection.z) * Mathf.Rad2Deg;
		
		Quaternion yRot = Quaternion.AngleAxis (angle, Vector3.up);
		Quaternion xRot = Quaternion.AngleAxis (currentSpeed, Vector3.right);

		mMoveController.Move (xRot * yRot);
		//mCollidable.Rotation *= xRot * yRot;	
	}
}
