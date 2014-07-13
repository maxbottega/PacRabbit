using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class Enemy : MonoBehaviour 
{
	[System.NonSerialized]
	public Transform 		Target 				= null;
	public float 			m_Speed 			= 5;
	SphericalMoveController	mMoveController 	= null;	
	Collidable				mCollidable 		= null;
	
	//EnemyManager mEnemyManager = null;

	void Awake ()
	{
		// Position the enemy on the planet surface
		transform.position 	= transform.up * Planet.GetRadius();	
		//mEnemyManager 	= FindObjectOfType(typeof(EnemyManager)) as EnemyManager;
		mMoveController 	= GetComponent<SphericalMoveController>();
		mCollidable 		= GetComponent<Collidable> ();

		if (mCollidable)
			mCollidable.OnCollision = new Collidable.CollisionCallback(OnCollision);
	}

	void Start () 
	{
		Target = (FindObjectOfType(typeof(Character)) as Character).transform; // TODO remove this 
	}

	void Update () 
	{
		if (mMoveController)
			UpdateLocomotion(Time.deltaTime, m_Speed);
	}
	
	virtual protected void UpdateLocomotion(float dt, float speed)
	{
		float currentSpeed 	= speed * dt;
		Vector3 chasePos 	= Target ? Target.position : transform.position;

		mMoveController.Move (chasePos, currentSpeed);
	}

	public void OnCollision(Collidable other)
	{
		// Collision reaction
	}
}
