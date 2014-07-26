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
	private SphereTransform							mMoveController 	= null;	
	private Collidable								mCollidable 		= null;
	private PlayMakerFSM							mPlaymaker			= null;
	
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
		mMoveController 	= GetComponent<SphereTransform>();
		mCollidable 		= GetComponent<Collidable> ();

		mPlaymaker = GetComponent<PlayMakerFSM> ();

		if (mCollidable && mPlaymaker) // register callback that sends events to playmaker
			mCollidable.OnCollision = new Collidable.CollisionCallback(OnCollision);
			
	}
	
	public void OnCollision(Collidable other)
	{
		Enemy enemy = other.GetComponent<Enemy> ();
		
		if(mPlaymaker.Fsm.EventTarget != null)
		{
			Debug.LogError ("EventTarget set in Enemy FSM - this might cause issues so we reset it");
			// EventTarget might redirect SendEvent to another target, we check here to be safe as it seems
			// to be a possible cause of nasty bugs, but I haven't verified this directly yet, just reading
			// about in on some forums...
			mPlaymaker.Fsm.EventTarget = null;
		}
		
		if(enemy!=null)
		{
			mPlaymaker.SendEvent("EnemyCollision");
			return;
		}
		
		Character character = other.GetComponent<Character> ();
		
		if(character!=null)
		{
			mPlaymaker.SendEvent("CharacterCollision");
			return;	
		}
		
		mPlaymaker.SendEvent("OtherCollision");
	}

	void Start () 
	{
		Target = (FindObjectOfType(typeof(Character)) as Character).transform;
	}

	void Update () 
	{
		if (mPlaymaker != null)
			return; // TODO: we really will always have playmaker attached, this is temporary
		
		if (mMoveController)
			FollowPointOnNavMesh(Time.deltaTime, Speed, Target.position);
	}
	
	public void FollowPoint(float dt, float speed, Vector3 chasePos)
	{
		float currentSpeed 	= speed * dt;
		mMoveController.Move (chasePos, currentSpeed);
	}
	
	public void FollowPointOnNavMesh(float dt, float speed, Vector3 chasePos)
	{
		float currentSpeed 	= speed * dt;
		
		Vector3 currentPos 	= mMoveController.Rotation * Vector3.up;
		List<WayPoint> path = new List<WayPoint>();

		NavigationManager.instance.CalculatePath(currentPos, chasePos, path); // TODO: path caching, don't recompute every frame

		Vector3 previousWPPosition = path[0].Position;
		foreach (WayPoint wp in path) 
		{
			Debug.DrawLine (wp.Position, previousWPPosition, Color.green);
			previousWPPosition = wp.Position;
		}
		
		mMoveController.Move (path[path.Count > 2 ? 2 : 0].Position, currentSpeed); // TODO: proper path navigation
	}
}

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("_PlanetGameplay")]
	[Tooltip("Follows the character")]
	public class Follow : FsmStateAction
	{					
		[Tooltip("The base speed is in the enemy component")]
		public FsmFloat 		SpeedMultiplier 	= 1.0f;
		[Tooltip("Tries to escape from the target instead of following")]
		public FsmBool			Escape				= false;
		[Tooltip("Optional Target, if not specified it will be the player")]
		public FsmGameObject	Target				= null;
		
		private Enemy 			enemy 				= null;
		
		public override void Reset()
		{
			enemy = Owner.GetComponent<Enemy> ();
		}
		
		public override void OnEnter()
		{
			OnUpdate();
		}
		
		public override void OnUpdate()
		{
			if(enemy != null)
			{
				Transform transf = Target.Value != null ? Target.Value.transform : enemy.Target;
				
				if(Escape.Value)
					enemy.FollowPointOnNavMesh(Time.deltaTime, enemy.Speed * SpeedMultiplier.Value, -transf.position);
				else
					enemy.FollowPointOnNavMesh(Time.deltaTime, enemy.Speed * SpeedMultiplier.Value, transf.position);
			}
		}
	}
	
	[ActionCategory("_PlanetGameplay")]
	[Tooltip("Follows the character, flying (no pathfinding)")]
	public class FollowFlying : FsmStateAction
	{			
		// TODO: add a flying height, transition between flying and not
		
		[Tooltip("The base speed is in the enemy component")]
		public FsmFloat 		SpeedMultiplier 	= 1.0f;
		[Tooltip("Tries to escape from the target instead of following")]
		public FsmBool			Escape				= false;
		[Tooltip("Optional Target, if not specified it will be the player")]
		public FsmGameObject	Target				= null;
		
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
				Transform transf = Target.Value != null ? Target.Value.transform : enemy.Target;
				
				if(Escape.Value)
					enemy.FollowPoint(Time.deltaTime, enemy.Speed * SpeedMultiplier.Value, -transf.position);
				else
					enemy.FollowPoint(Time.deltaTime, enemy.Speed * SpeedMultiplier.Value, transf.position);
			}
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

Debug.DrawLine(enemy.transform.position, target.Position, Color.yellow, 10.0f);
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

	// TODO: WanderFlying
}
