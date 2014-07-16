using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
[RequireComponent (typeof (SphereTransform))]
[RequireComponent (typeof (Collidable))]
public class Enemy : MonoBehaviour 
{
	// ------------ Public, editable in the GUI, serialized
	public float 									Speed 				= 5;
	
	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	[System.NonSerialized] public Transform 		Target 				= null;
	[System.NonSerialized] SphereTransform			mMoveController 	= null;	
	[System.NonSerialized] Collidable				mCollidable 		= null;
	[System.NonSerialized] WayPoint					mCachedNearest 		= null;	
	//[System.NonSerialized] EnemyManager 			mEnemyManager 		= null;
	
	// ------------ Private	
	private bool 									hasPlaymaker 		= false;

	void Awake ()
	{
		// Position the enemy on the planet surface
		transform.position 	= transform.up * Planet.GetRadius();	
		//mEnemyManager 	= FindObjectOfType(typeof(EnemyManager)) as EnemyManager;
		mMoveController 	= GetComponent<SphereTransform>();
		mCollidable 		= GetComponent<Collidable> ();

		if (mCollidable)
			mCollidable.OnCollision = new Collidable.CollisionCallback(OnCollision);
			
		if(GetComponent<PlayMakerFSM> () != null)
			hasPlaymaker = true;
	}

	void Start () 
	{
		Target = (FindObjectOfType(typeof(Character)) as Character).transform; // TODO remove this 
	}

	void Update () 
	{
		if (hasPlaymaker)
			return;
		
		if (mMoveController)
			FollowPlayerOnNavMesh(Time.deltaTime, Speed);
	}
	
	public void FollowPlayerOnNavMesh(float dt, float speed)
	{
		float currentSpeed 	= speed * dt;
		Vector3 chasePos 	= Target ? Target.position : transform.position;
		
		mMoveController.Move (chasePos, currentSpeed);
		
		Vector3 currentPos 	= mMoveController.Rotation * Vector3.up;
		List<WayPoint> path = new List<WayPoint>();

		NavigationManager.instance.CalculatePath(currentPos, chasePos, path);

		Vector3 previosWPPosition = path[0].transform.position;
		foreach (WayPoint wp in path) 
		{
			Debug.DrawLine (wp.transform.position, previosWPPosition, Color.cyan);
			previosWPPosition = wp.transform.position;
		}
		
		mMoveController.Move (path[path.Count > 2 ? 2 : 0].transform.position, currentSpeed);
		
		// TODO: move all this in the collision manager, after resolving sphere collisions
		currentPos = mMoveController.Rotation * Vector3.up * Planet.GetRadius();
		Vector3 newPos = 
			NavigationManager.instance.PointNavMeshEdgesCollision(
				currentPos, 0.75f, mCachedNearest, out mCachedNearest);
				
		mMoveController.Move(newPos);
	}

	public void OnCollision(Collidable other)
	{
		// Collision reaction
	}
}

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("PlanetGameplay")]
	[Tooltip("Follows the character")]
	public class Puppa : FsmStateAction
	{	
		private Enemy enemy = null;
		
		// TODO: add a bool option to follow not on the navmesh (flying)
		// TODO: add a speed multiplier
		
		public override void Reset()
		{
			enemy = Owner.GetComponent<Enemy> ();
		}
		
		public override void OnEnter()
		{
			if(enemy == null)
				enemy.FollowPlayerOnNavMesh(Time.deltaTime, enemy.Speed);
		}
		
		public override void OnUpdate()
		{
			if(enemy == null)	
				enemy.FollowPlayerOnNavMesh(Time.deltaTime, enemy.Speed);
		}	
	}
}
