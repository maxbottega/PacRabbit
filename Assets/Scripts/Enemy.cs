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
	[System.NonSerialized] public Transform 		Target				= null;
	
	
	// ------------ Private	
	private bool 									hasPlaymaker 		= false;
	private SphereTransform							mMoveController 	= null;	
	private Collidable								mCollidable 		= null;
	private WayPoint								mCachedNearest 		= null;	
	//private EnemyManager 							mEnemyManager 		= null;
	
	public float DistanceToTarget
	{
		get
		{ 
			if(Target!=null)
				return Vector3.Distance(Target.position, transform.position);
			else
				return float.MaxValue;
		}
	}

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
			return; // TODO: we really will always have playmaker attached, this is temporary
		
		if (mMoveController)
			FollowTargetOnNavMesh(Time.deltaTime, Speed);
	}
	
	public void FollowTarget(float dt, float speed)
	{
		float currentSpeed 	= speed * dt;
		Vector3 chasePos 	= Target ? Target.position : transform.position;

		mMoveController.Move (chasePos, currentSpeed);
	}
	
	public void FollowTargetOnNavMesh(float dt, float speed)
	{
		float currentSpeed 	= speed * dt;
		Vector3 chasePos 	= Target ? Target.position : transform.position;
		
		Vector3 currentPos 	= mMoveController.Rotation * Vector3.up;
		List<WayPoint> path = new List<WayPoint>();

		NavigationManager.instance.CalculatePath(currentPos, chasePos, path); // TODO: path caching, don't recompute every frame

		Vector3 previosWPPosition = path[0].transform.position;
		foreach (WayPoint wp in path) 
		{
			Debug.DrawLine (wp.transform.position, previosWPPosition, Color.cyan);
			previosWPPosition = wp.transform.position;
		}
		
		mMoveController.Move (path[path.Count > 2 ? 2 : 0].transform.position, currentSpeed); // TODO: proper path navigation
	}
	
	public void CollideWithNavMesh()
	{
		// TODO: move all this in the collision manager, after resolving sphere collisions - same also for the character!
		
		Vector3 currentPos = mMoveController.Rotation * Vector3.up * Planet.GetRadius();
		Vector3 newPos = 
			NavigationManager.instance.PointNavMeshEdgesCollision(
				currentPos, 0.75f /* radius - TODO: configurable */, mCachedNearest, out mCachedNearest);
		
		mMoveController.Move(newPos);
	}

	public void OnCollision(Collidable other)
	{
		// Collision reaction
	}
}

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("_PlanetGameplay")]
	[Tooltip("Follows the character")]
	public class Follow : FsmStateAction
	{			
		[Tooltip("If not flying it will follow the navmesh")]
		public FsmBool 			Flying 				= false; // TODO: add a flying height, transition between flying and not
		
		[Tooltip("Dumb doesn't use pathfinding")]
		public FsmBool 			Dumb 				= false; 
		
		[Tooltip("The base speed is in the enemy component")]
		public FsmFloat 		SpeedMultiplier 	= 1.0f;
		
		// TODO: follow any point, not only the character
		
		private Enemy 			enemy 				= null;
		
		public override void Reset()
		{
			enemy = Owner.GetComponent<Enemy> ();
		}
		
		public override void OnEnter()
		{
			OnUpdate();
		}
		
		public override  void OnUpdate()
		{
			if(enemy != null)
			{
				if(Dumb.Value)
					enemy.FollowTarget(Time.deltaTime, enemy.Speed * SpeedMultiplier.Value);
				else
					enemy.FollowTargetOnNavMesh(Time.deltaTime, enemy.Speed * SpeedMultiplier.Value);
					
				if(!Flying.Value)
					enemy.CollideWithNavMesh();
			}
		}
	}
	
	// TODO: Wander action
}
