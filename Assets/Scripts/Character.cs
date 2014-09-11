using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (SphereTransform))]
public class Character : MonoBehaviour
{	
	// ------------ Public, editable in the GUI, serialized
	public float									MinSpeed = 0.5f;
	public float									MaxSpeed = 2.0f;
	public bool										InCorridorsNavmeshCollision = false;

	// ------------ Public, serialized
	
	// ------------ Private
	private Vector3									mInputDirection	= Vector3.right;
	private Vector3 								mTargetWPPos = Vector3.zero;
	private	WayPoint								mPrevClosestWP = null;
	
	private Collidable								mCollidable 	= null;
	private SphereTransform							mMoveController	= null;	
	private List<PlayMakerFSM>						mEnemyCollisionFSMs = new List<PlayMakerFSM>();
	private List<PlayMakerFSM>						mOtherCollisionFSMs = new List<PlayMakerFSM>();
	
	private float									mCurrentSpeed;

	void Awake () 
	{
		Application.targetFrameRate = 60;
		mMoveController = GetComponent<SphereTransform>();
		mCollidable 	= GetComponent<Collidable> ();
		
		PlayMakerFSM[] FSMs = GetComponents<PlayMakerFSM>();
		bool hasCollisionFSMs = false;
		
		foreach(PlayMakerFSM fsm in FSMs)
		{
			if(fsm.Fsm.HasEvent("EnemyCollision"))
			{
				mEnemyCollisionFSMs.Add(fsm);
				hasCollisionFSMs = true;
			}
			
			if(fsm.Fsm.HasEvent("OtherCollision"))
			{
				mOtherCollisionFSMs.Add(fsm);
				hasCollisionFSMs = true;
			}
		}
		
		if (mCollidable && hasCollisionFSMs) // register callback that sends events to playmaker
			mCollidable.OnCollision = new Collidable.CollisionCallback(OnCollision);
	}
	
	void Start ()
	{
		mCollidable.CachedNearest = NavigationManager.FindClosestWaypoint(transform.position, mCollidable.CachedNearest, NavigationManager.instance.waypointList);
		mTargetWPPos = mCollidable.CachedNearest.connections[0].Position;
		mMoveController.ImmediateSet (Quaternion.FromToRotation(Vector3.up, mCollidable.CachedNearest.Position.normalized));
		
	}
	
	
	void Update()
	{
		UpdateInput();
										
		if((mCollidable.CachedNearest != null) && (mCollidable.CachedNearest.mIsCorridor)) // Automatic "on-rails" navigation in corridors
		{
			MovementOnRails();
			mCollidable.SphereNavMeshCollision = InCorridorsNavmeshCollision;
		}
	}	
	
	void UpdateInput()
	{
		Vector3 inputVec = new Vector3 (Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		mInputDirection = (inputVec==Vector3.zero) ? mInputDirection : transform.TransformDirection (inputVec).normalized;
	}
	
	void MovementOnRails()
	{
		float planetRadius 		= Planet.Instance.Radius;
		Vector3 currPos 		= mMoveController.Up * planetRadius;	
		WayPoint closestWP 		= NavigationManager.FindClosestWaypoint (currPos, mCollidable.CachedNearest, NavigationManager.instance.waypointList);
		
		// We need to get a new segment to follow only if we are changing closest WP. 
		// This comes in handy to avoid changing directions when input changes right after a crossroad
		if (closestWP != mCollidable.CachedNearest)
		{
			mPrevClosestWP 				= mCollidable.CachedNearest;
			mCollidable.CachedNearest 	= closestWP;
			float maxDotInput 			= -1.0f;
			
			foreach(WayPoint w in mCollidable.CachedNearest.connections)
			{
				// The first condition is to prevent ever going backwards
				// The second condition is to cope for leaves in the graph, i.e. dead ends
				if (w == mPrevClosestWP || w.connections.Count==1)
					continue;
				
				Vector3 dir 	= (w.Position - mCollidable.CachedNearest.Position).normalized;
				float dotInput 	= Vector3.Dot (mInputDirection.normalized, dir);
				
				if (dotInput > maxDotInput)
				{
					mTargetWPPos 	= w.Position;
					maxDotInput 	= dotInput;
				}
			}
		}

		Vector3 rotAxis	= -Vector3.Cross (mTargetWPPos, currPos);
		Quaternion rot	= Quaternion.AngleAxis (MinSpeed, rotAxis);
		
		mMoveController.Move (rot, Space.World);
	}
	
	public void OnCollision(Collidable other)
	{		
		Enemy enemy = other.GetComponent<Enemy> ();	
		if(enemy!=null)
		{	
			foreach(PlayMakerFSM fsm in mEnemyCollisionFSMs)
			{
				if(fsm.Fsm.EventTarget != null)
				{
					Debug.LogError ("EventTarget set in Enemy FSM - this might cause issues so we reset it");
					// EventTarget might redirect SendEvent to another target, we check here to be safe as it seems
					// to be a possible cause of nasty bugs, but I haven't verified this directly yet, just reading
					// about in on some forums...
					fsm.Fsm.EventTarget = null;
				}
				
				//TODO: Cache event HutongGames.PlayMaker.FsmEvent ev = HutongGames.PlayMaker.FsmEvent.FindEvent("EnemyCollision");	
				fsm.Fsm.Event("EnemyCollision");
Debug.Log ("EnemyCollision sent: "+this.name+" fsm:"+fsm.FsmName);
			}
			
			return;
		}
		
		foreach(PlayMakerFSM fsm in mOtherCollisionFSMs)
		{
			if(fsm.Fsm.EventTarget != null)
			{
				Debug.LogError ("EventTarget set in Enemy FSM - this might cause issues so we reset it");
				// EventTarget might redirect SendEvent to another target, we check here to be safe as it seems
				// to be a possible cause of nasty bugs, but I haven't verified this directly yet, just reading
				// about in on some forums...
				fsm.Fsm.EventTarget = null;
			}
			
			//TODO: Cache event HutongGames.PlayMaker.FsmEvent ev = HutongGames.PlayMaker.FsmEvent.FindEvent("OtherCollision");	
			fsm.Fsm.Event("OtherCollision");
Debug.Log ("OtherCollision sent: "+this.name+" fsm:"+fsm.FsmName);
		}
	}
	
}

