using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Planet : MonoBehaviour 
{
	[SerializeField]
	private float m_Radius = 0;
	
	static Planet mInstance = null;
	
	Character mPlayer = null;
	Zombie[] mEnemies = null;
	Collidable[] mCollidables = null;
	CollisionManager mCollisionManager = null;
	
	void Awake() 
	{
		mInstance = this;
		
	}
	
	void Start()
	{
		Application.targetFrameRate = 60; // TODO: move somewhere else

		mPlayer = FindObjectOfType(typeof(Character)) as Character;
		mEnemies = FindObjectsOfType(typeof(Zombie)) as Zombie[];
		mCollidables = FindObjectsOfType(typeof(Collidable)) as Collidable[];
		mCollisionManager = FindObjectOfType(typeof(CollisionManager)) as CollisionManager;
	}
	
//	void Update()
//	{
//		mPlayer.DoUpdate();
//		
//		foreach(Zombie enemy in mEnemies)
//		{
//			enemy.DoUpdate();
//		}
//		
//		foreach(Collidable collidable in mCollidables)
//		{
//			collidable.DoUpdate();
//		}
//		
//		mCollisionManager.DoUpdate();
//		
//	}
	
	public static float GetRadius()
	{
		return mInstance.m_Radius * mInstance.transform.lossyScale.x;	
	}
	
}
