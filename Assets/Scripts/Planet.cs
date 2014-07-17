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
	[System.NonSerialized] static public Planet Instance = null;
//	Character mPlayer = null;
//	Zombie[] mEnemies = null;
//	Collidable[] mCollidables = null;
//	CollisionManager mCollisionManager = null;
	
	void Awake() 
	{
		Instance = this;
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
/*
	public static float GetRadius()
	{
		if (mInstance == null)
			Debug.LogError ("Some object is querying the Planet for radius, but the Planet is not inititialized yet.");
			
		return mInstance.Radius * mInstance.transform.lossyScale.x;	
	}
*/	

#if UNITY_EDITOR 
	void OnDrawGizmos() 
	{
		// This is where the planet thinks to be
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(Vector3.zero, Radius);
		
		// This is the reference axis, all objects are expressed as rotations of this
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(Vector3.up * Radius, 1.0f);
	}
#endif
}
