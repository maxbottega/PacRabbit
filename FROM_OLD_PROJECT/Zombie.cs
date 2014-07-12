//using UnityEngine;
//using System.Collections;
//
//public class Zombie : MonoBehaviour 
//{
//	public float mSpeed = 5;
//	public bool m_UpdateLoco = true;
//	
//	Transform mTarget = null;
//	
//	Collidable mCollidable = null;
//	
//	float mSpeedMultiplier = 1.0f;
//	
//	int mNumCollisions = 0;
//	
//	//EnemyManager mEnemyManager = null;
//	
//	// Use this for initialization
//	void Start () 
//	{
//		//mEnemyManager = FindObjectOfType(typeof(EnemyManager)) as EnemyManager;
//		mCollidable = GetComponent<Collidable>();
//		mCollidable.OnCollision = new Collidable.CollisionCallback(OnCollision);
//		mTarget = (FindObjectOfType(typeof(Character)) as Character).transform;
//	}
//	
//	// Update is called once per frame
//	void Update () 
//	{
//		if (m_UpdateLoco)
//		{
//			UpdateLocomotion(Time.deltaTime, mSpeed);
//		}
//		
//		mSpeedMultiplier = 1.0f;
//		mNumCollisions = 0;
//		
//	}
//	
//	public void SetAnimatedDummy()
//	{
//		mCollidable = GetComponent<Collidable>();
//		transform.position = Vector3.zero;
//		mCollidable.DisableCollisions();
//		m_UpdateLoco = false;
//	}
//	
//	void Die ()
//	{
//		//mCollidable.DisableCollisions();
//		gameObject.SetActive(false);
//		//mEnemyManager.NotifyEnemyDeath();
//	}
//	
//	public void OnCollision(Collidable other)
//	{
//		if (other.GetComponent<Ammo>()!=null)
//		{
//			Die ();	
//		}
//		else
//		{
//			++mNumCollisions;
//			if (mNumCollisions>3)
//			{
//				mSpeedMultiplier = 0.3f;
//			}
//		}
//	}
//	
//	virtual protected void UpdateLocomotion(float dt, float speed)
//	{
//		float currentSpeed = speed*mSpeedMultiplier*dt;
//		
//		Vector3 chasePos = mTarget.position;
//
//		Vector3 direction = chasePos - Vector3.Dot (chasePos, mCollidable.Up)*mCollidable.Up;
//		Vector3 localDirection = Quaternion.Inverse(mCollidable.Rotation) * direction;
//		float angle = Mathf.Atan2 (localDirection.x, localDirection.z) * Mathf.Rad2Deg;
//		
//		Quaternion yRot = Quaternion.AngleAxis (angle, Vector3.up);
//		Quaternion xRot = Quaternion.AngleAxis (currentSpeed, Vector3.right);
//		
//		mCollidable.Rotation *= xRot * yRot;	
//	}
//}
