using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent (typeof (NavigationManager))]
[RequireComponent (typeof (CollisionManager))]
[RequireComponent (typeof (EnemyManager))]
public class Planet : MonoBehaviour 
{
	// ------------ Public, editable in the GUI, serialized
	public float Radius = 10.0f;
	
	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	[System.NonSerialized] static Planet mInstance = null;
//	Character mPlayer = null;
//	Zombie[] mEnemies = null;
//	Collidable[] mCollidables = null;
//	CollisionManager mCollisionManager = null;
	
	void Awake() 
	{
		mInstance = this;
	}
	
	void Start()
	{
		Application.targetFrameRate = 60; 

//		mPlayer = FindObjectOfType(typeof(Character)) as Character;
//		mEnemies = FindObjectsOfType(typeof(Zombie)) as Zombie[];
//		mCollidables = FindObjectsOfType(typeof(Collidable)) as Collidable[];
//		mCollisionManager = FindObjectOfType(typeof(CollisionManager)) as CollisionManager;
	}
	
	void Update()
	{
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
	}
	
	public static float GetRadius()
	{
		if (mInstance == null)
			Debug.LogError ("Some object is querying the Planet for radius, but the Planet is not inititialized yet.");
			
		return mInstance.Radius * mInstance.transform.lossyScale.x;	
	}
}
