//using UnityEngine;
//using System.Collections;
//
//public class Ammo : MonoBehaviour 
//{
//	public bool m_InheritCharacterVelocity = true;
//	
//	float 		mSpeed = 0.0f;
//	float 		mLifeTime = 0.0f;
//	float 		mMaxLifeTime = 0.0f;
//	int			mDamage = 1;
//	Collidable  mCollidable = null;
//	
//	
//	protected virtual void Awake () 
//	{	
//		mCollidable = GetComponent<Collidable>();
//		mCollidable.OnCollision = new Collidable.CollisionCallback(OnCollision);
//	}
//	
//	void OnEnable()
//	{
//		mLifeTime = 0;
//	}
//	
//	public void OnCollision(Collidable other)
//	{
//		if (other.GetComponent<Character>()==null)
//		{
//			//transform.parent.gameObject.SetActive(false);
//			mLifeTime = mMaxLifeTime;
//		}
//	}
//	
//	public void Init(int damage, float speed, float lifetime)
//	{
//		mDamage = damage;
//		mSpeed = speed;
//		mMaxLifeTime = lifetime;	
//	}
//	
//	void Update () 
//	{			
//		Quaternion movRot = Quaternion.AngleAxis(mSpeed*Time.deltaTime, Vector3.right);
//		Quaternion compensationRot = Quaternion.identity;
////		if (m_InheritCharacterVelocity)
////		{
////			Vector3 mariachiDeltaRotation = GameManager.GetPlayer().GetComponent<Mariachi>().m_DeltaRotation;
////			compensationRot = Quaternion.Euler(mariachiDeltaRotation);
////		}
//		
//		mCollidable.Rotation = mCollidable.Rotation * movRot;
//		//transform.parent.rotation = compensationRot * transform.parent.rotation * movRot;
//		
//		mLifeTime += Time.deltaTime;
//		if (mLifeTime >= mMaxLifeTime)
//		{
//			transform.parent.gameObject.SetActive(false);
//		}
//	}
//	
//	public void Fire (Transform trans)
//	{	
//		transform.parent.gameObject.SetActive(true);
//		mLifeTime = 0;
//		transform.parent.position = Vector3.zero;
//		
//		transform.parent.rotation = trans.rotation;
//		mCollidable.Rotation = trans.rotation;
//		mCollidable.Up = trans.up;
//		transform.localPosition = Vector3.up * Planet.GetRadius() * 1.05f;
//		//transform.rotation = trans.rotation;
//	}
//	
//}
