using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
[RequireComponent (typeof (SphereTransform))]
public class Enemy : MonoBehaviour 
{
	// ------------ Public, editable in the GUI, serialized
	public float 									Speed 				= 5;
	
	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	[System.NonSerialized] public Transform 		Target 				= null;
	[System.NonSerialized] SphereTransform			mMoveController 	= null;	
	[System.NonSerialized] Collidable				mCollidable 		= null;
	//[System.NonSerialized] EnemyManager 			mEnemyManager 		= null;

	void Awake ()
	{
		// Position the enemy on the planet surface
		transform.position 	= transform.up * Planet.GetRadius();	
		//mEnemyManager 	= FindObjectOfType(typeof(EnemyManager)) as EnemyManager;
		mMoveController 	= GetComponent<SphereTransform>();
		mCollidable 		= GetComponent<Collidable> ();

		if (mCollidable)
			mCollidable.OnCollision = new Collidable.CollisionCallback(OnCollision);
	}

	void Start () 
	{
		Target = (FindObjectOfType(typeof(Character)) as Character).transform; // TODO remove this 
	}

	void Update () 
	{
		if (mMoveController)
			UpdateLocomotion(Time.deltaTime, Speed);
	}
	
	virtual protected void UpdateLocomotion(float dt, float speed)
	{
		float currentSpeed 	= speed * dt;
		Vector3 chasePos 	= Target ? Target.position : transform.position;

		mMoveController.Move (chasePos, currentSpeed);
	}

	public void OnCollision(Collidable other)
	{
		// Collision reaction
	}
}

/*
namespace HutongGames.PlayMaker.Actions
{
	//[CheckForComponent(typeof())]
	[ActionCategory("CustomActions")]
	[Tooltip("puppa")]
	public class Puppa : FsmStateAction
	{
		[UIHint(UIHint.Variable)]
		[Tooltip("")]
		public FsmString test;
		
		
		[HasFloatSlider(0,100)]
		public FsmFloat teeest;
		
		//[RequiredField]
		
		public override void Reset()
		{
		}
		
		public override void OnEnter()
		{
			DoMyAction();
			
			//if(!everyFrame) Finish();
			
			//myComponent = (MyComponent)Owner;
		}
		
		public override void OnUpdate()
		{
			DoMyAction();
		}
		
		void DoMyAction()
		{	
		}		
	}
}*/
