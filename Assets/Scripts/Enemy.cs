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
		transform.position 	= transform.up * Planet.Instance.Radius;	
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
		Target = (FindObjectOfType(typeof(Character)) as Character).transform;
	}

	void Update () 
	{
		if (hasPlaymaker)
			return; // TODO: we really will always have playmaker attached, this is temporary
		
		if (mMoveController)
			FollowTargetOnNavMesh(Time.deltaTime, Speed);
	}
	
	public void FollowPoint(float dt, float speed, Vector3 chasePos)
	{
		float currentSpeed 	= speed * dt;
		mMoveController.Move (chasePos, currentSpeed);
	}
	
	public void FollowTarget(float dt, float speed)
	{
		FollowPoint(dt, speed, Target.position);
	}
	
	public void FollowTargetOnNavMesh(float dt, float speed)
	{
		float currentSpeed 	= speed * dt;
		Vector3 chasePos 	= Target.position;
		
		Vector3 currentPos 	= mMoveController.Rotation * Vector3.up;
		List<WayPoint> path = new List<WayPoint>();

		NavigationManager.instance.CalculatePath(currentPos, chasePos, path); // TODO: path caching, don't recompute every frame

Vector3 previosWPPosition = path[0].Position;
foreach (WayPoint wp in path) 
{
	Debug.DrawLine (wp.Position, previosWPPosition, Color.green);
	previosWPPosition = wp.Position;
}
		
		mMoveController.Move (path[path.Count > 2 ? 2 : 0].Position, currentSpeed); // TODO: proper path navigation
	}
	
	public void CollideWithNavMesh()
	{
		// TODO: move all this in the collision manager, after resolving sphere collisions - same also for the character!
		
		Vector3 currentPos = mMoveController.Rotation * Vector3.up * Planet.Instance.Radius;
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
					
				enemy.CollideWithNavMesh();
			}
		}
	}
	
	[ActionCategory("_PlanetGameplay")]
	[Tooltip("Follows the character, flying (no navmesh collision)")]
	public class FollowFlying : FsmStateAction
	{			
		// TODO: add a flying height, transition between flying and not
		
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
				enemy.FollowTarget(Time.deltaTime, enemy.Speed * SpeedMultiplier.Value);
		}
	}
	
	[ActionCategory("_PlanetGameplay")]
	[Tooltip("Wanders around")]
	public class Wander : FsmStateAction
	{					
		[Tooltip("The base speed is in the enemy component")]
		public FsmFloat 		SpeedMultiplier 	= 1.0f;
		
		[Tooltip("The probability of changing direction")]
		public FsmFloat 		DirectionChange 	= 0.01f;
		
		private int				currentNearest 		= 0;
		private List<WayPoint>	currentPath			= null;
		private Enemy 			enemy 				= null;
		
		public override void Reset()
		{
			enemy = Owner.GetComponent<Enemy> ();
		}
		
		public override void OnEnter()
		{
			currentNearest = 0;
			currentPath	= null;
			
			OnUpdate();
		}
		
		public override void OnUpdate()
		{
			if(enemy == null)
				return;
				
			bool changeDir = Random.value <= DirectionChange.Value;
			
			if(changeDir || currentPath == null)
			{
				WayPoint target = NavigationManager.instance.SelectRandomWaypoint();
				currentPath = new List<WayPoint>();
				
				// TODO: we already know the waypoint closest to target (and the one closest to current position) but this will recompute it
				NavigationManager.instance.CalculatePath(enemy.transform.position, target.Position, currentPath);
				
				currentNearest = 0;
			}

			WayPoint next = NavigationManager.NavigatePath(currentPath, enemy.transform.position, currentNearest, out currentNearest);

for(int i=0;i<currentPath.Count-1;i++)
	Debug.DrawLine(currentPath[i].Position, currentPath[i+1].Position, Color.cyan);
	
			if(next == null)
			{
				// Finished navigating a path, destroy it and create a new one
				currentPath = null;
				OnUpdate();
			}
			else
			{
				enemy.FollowPoint(Time.deltaTime, enemy.Speed * SpeedMultiplier.Value, next.Position);
			}
		}
	}
	
	// TODO: Escape action (get away from the player)
}
