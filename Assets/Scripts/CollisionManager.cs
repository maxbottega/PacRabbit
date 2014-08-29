using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionManager : MonoBehaviour 
{
	// ------------ Public, editable in the GUI, serialized

	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	[System.NonSerialized] public int 			mNumColliders = 0;
	[System.NonSerialized] List<Collidable> 	mColliders = new List<Collidable>();

	// ------------ Private
	private bool 								m_SAP = false;
	private bool 								m_CompareTo = true;
	private bool	 							m_BreakSAPCycle = false;
	private bool 								m_AdaptiveSweepAxis = true;
	private int 								m_IterationCount = 10;
	private float 								m_Relaxation = 0.8f;
	private bool    						 	m_CheckSort = false;
	private float								m_CollisionErrorInterpolation = 1.0f;
	private bool    							m_LinearTest = true;
	private delegate void 						SweepFuncion();
	private SweepFuncion 						mSweepFunction;
	private Quaternion 							_quaternionIdentity = Quaternion.identity; // was this an optimization?
	
	void Start () 
	{
		mSweepFunction = new SweepFuncion(this.SweepXAxis);
	}
	
/*	public float CollisionErrorInterpolation
	{
		get { return m_CollisionErrorInterpolation; }
	}*/
	
	public void AddCollider(Collidable collider)
	{
		++mNumColliders;
		mColliders.Add (collider);
	}
	
	public void RemoveCollider(Collidable collider)
	{
		--mNumColliders;
		mColliders.Remove (collider);
	}
	
	// NOTE: Due to ExecutionOrderManager we know this Update is after SphereTransform Update which is after all other components Updates
	void Update () 
	{		
		if (m_SAP==true) 
			UpdateDynamicCollisionsSingleSAP();
		else 
			UpdateDynamicBrute();
			
		ResolveNavMeshCollisions();
	}
	
	void ResolveNavMeshCollisions()
	{
		int startIndex = 0;
		int endIndex = mColliders.Count;
		float navmeshRadius = Planet.Instance.Radius;
		
		for (int colliderIndex = startIndex; colliderIndex<endIndex; ++colliderIndex)
		{	
			Collidable coll = mColliders[colliderIndex];
			
			if(coll.SphereNavMeshCollision == false)
				continue;
			
			Vector3 currentPos = coll.SphereTransf.Up * navmeshRadius;
			
			// TODO: as we have prevPos we can do continous collision detection instead, which would also
			// solve issues with the navmesh collision currently not being "oriented"
#if true
			Vector3 newPos = 
				NavigationManager.instance.PointNavMeshEdgesCollision(
					currentPos, coll.RadiusForNavMesh, coll.CachedNearest, out coll.CachedNearest);
#else		
			Vector3 prevPos = coll.SphereTransf.UpPreviousUpdate * navmeshRadius;
			
			Vector3 newPos = 
				NavigationManager.instance.SegmentNavMeshEdgesCollision(
					prevPos, currentPos, coll.RadiusForNavMesh, coll.CachedNearest, out coll.CachedNearest);			
#endif		
			//if( Vector3.Distance(currentPos, newPos) > 0.000001f ) // TODO: use dot instead
			coll.SphereTransf.Move(newPos);
		}
	}
	
	static public Vector3 PointNearestSegment(Vector3 point, Vector3 edgeA, Vector3 edgeB)
	{
		Vector3 v0p = point - edgeA;
		Vector3 v0v1 = edgeB - edgeA;
		float len2 = v0v1.sqrMagnitude;
		float dot = Vector3.Dot(v0p, v0v1);
		float t = dot / len2;
		
		t = Mathf.Clamp01(t);
		
		Vector3 closest = edgeA + v0v1 * t;
		
		return closest;
	}
	
	bool ResolveCoupleCollision(Collidable collider, Collidable other)
	{
		Quaternion colliderError = _quaternionIdentity;
		Quaternion otherError = _quaternionIdentity;
		
		Vector3 colliderUp = collider.Up;
		Vector3 otherUp = other.Up;
		
		if(collider.Static && (collider.CapsuleLength != 0))
			colliderUp = PointNearestSegment(otherUp, colliderUp - collider.CapsuleArm, colliderUp + collider.CapsuleArm);
		
		if(other.Static && (other.CapsuleLength != 0))
			otherUp = PointNearestSegment(colliderUp, otherUp - other.CapsuleArm, otherUp + other.CapsuleArm);
		
		float radiusSumAngle = collider.AngleRadius + other.AngleRadius;
		float dot = Vector3.Dot(colliderUp, otherUp);
		float angle = Mathf.Acos (dot) * Mathf.Rad2Deg;
		
		if (angle<radiusSumAngle)
		{
			float angleError = (radiusSumAngle - angle) * m_Relaxation;
			
			float colliderMassRatio = collider.Mass / (collider.Mass + other.Mass);
			if(collider.Static)
				colliderMassRatio = 1.0f;
			if(other.Static)
				colliderMassRatio = 0.0f;
			
			Vector3 rotationAxis = Vector3.Cross(otherUp, colliderUp);
			colliderError = Quaternion.AngleAxis(angleError * (1.0f-colliderMassRatio), rotationAxis);	
			otherError = Quaternion.AngleAxis(-angleError * (colliderMassRatio), rotationAxis);
			
			if (other.Static)
			{
				collider.SphereTransf.ImmediateSet(Quaternion.FromToRotation(Vector3.up, collider.SphereTransf.UpPreviousUpdate));
			}
			else
			{
				collider.SphereTransf.ImmediateSet(colliderError * collider.Rotation);
			}
			
			if (collider.Static)
			{
				other.SphereTransf.ImmediateSet(Quaternion.FromToRotation(Vector3.up, other.SphereTransf.UpPreviousUpdate));
			}
			else
			{
				other.SphereTransf.ImmediateSet(otherError * other.Rotation);
			}
				
			if (collider.OnCollision!=null)
				collider.OnCollision(other);
				
			if (other.OnCollision!=null)
				other.OnCollision(collider);
			
			return true;
		}
		return false;
	}
	
	void UpdateDynamicBrute()
	{
		int startIndex = 0;
		int endIndex = mColliders.Count;
		
		for (int colliderIndex = startIndex; colliderIndex<endIndex; ++colliderIndex)
		{	
			Collidable current = mColliders[colliderIndex];
			
			if (!current.gameObject.activeInHierarchy) continue;
			
			for (int activeIndex = colliderIndex+1; activeIndex<endIndex; activeIndex++)
			{
				Collidable active = mColliders[activeIndex];
				
				if (!active.gameObject.activeInHierarchy) continue;
				
				if (active.CollideWithOtherLayersOnly || current.CollideWithOtherLayersOnly)
				{
					if (active.Layer == current.Layer) continue;
				}
				else
					if (active.Layer != current.Layer) continue;
					
				if (active.Static && current.Static) continue;
				//if (Physics.GetIgnoreLayerCollision(current.gameObject.layer, active.gameObject.layer)) continue; 
				
				ResolveCoupleCollision(current, active);
			}
		}
	}

	void UpdateDynamicCollisionsSingleSAP()
	{	
		int startIndex = 0;
		int endIndex = mColliders.Count;

		for (int i=0; i<m_IterationCount; ++i)
		{
			mSweepFunction();
			
			if (m_CheckSort)
			{
				float minX = float.MinValue;
				foreach (Collidable coll in mColliders)
				{
					if (coll.Min.x < minX) 
						Debug.LogError("Sort is wrong");
					minX = coll.Min.x;
				}
			}
		
			Vector3 s = Vector3.zero;
			Vector3 s2 = Vector3.zero;
			Vector3 v = Vector3.zero;
			
			for (int colliderIndex = startIndex; colliderIndex<endIndex; ++colliderIndex)
			{	
				Collidable current = mColliders[colliderIndex];
				
				if (!current.gameObject.activeInHierarchy) continue;
			
				//positions sum for variance
				s += current.Center;
				s2 += Vector3.Scale (current.Center, current.Center);
				
				for (int activeIndex = colliderIndex+1; activeIndex<endIndex; activeIndex++)
				{
					Collidable active = mColliders[activeIndex];
					
					if (!active.gameObject.activeInHierarchy) continue;
					
					if (active.CollideWithOtherLayersOnly || current.CollideWithOtherLayersOnly)
					{
						if (active.Layer == current.Layer) continue;
					}
					else
						if (active.Layer != current.Layer) continue;
					
					if (active.Static && current.Static) continue;
					//if (Physics.GetIgnoreLayerCollision(current.gameObject.layer, active.gameObject.layer)) continue; 
					
					float currentEnd = current.MaxValue;
					float activeStart = active.MinValue;
					
					if (m_BreakSAPCycle)
					{
						if (currentEnd<activeStart) break;
					}
					else
					{
						if (currentEnd<activeStart) continue;
					}
				
					float radiusSum = current.Radius+active.Radius;
					float distanceSquared = (current.Center - active.Center).sqrMagnitude;
				
					if (m_LinearTest)
					{
						if (distanceSquared<(radiusSum*radiusSum)*1.2f)
						{
							ResolveCoupleCollision(current, active);
						}
					}
					else
						ResolveCoupleCollision(current, active);
				}
			}
			
			//Variance 
			Vector3 sSquared = Vector3.Scale (s,s);
			v = (s2 - (sSquared/endIndex))/endIndex;
			
			if (m_AdaptiveSweepAxis)
			{
				if (v.y>v.x && v.y>v.z)
					mSweepFunction = SweepYAxis;
				
				if (v.x>v.y && v.x>v.z)
					mSweepFunction = SweepXAxis;
				
				if (v.z>v.x && v.z>v.y)
					mSweepFunction = SweepZAxis;
			}
		}
	}
	
	void SweepXAxis()
	{
		mColliders.Sort( (a, b) => {
					a.MinValue = a.Min.x;
					b.MinValue = b.Min.x;
					a.MaxValue = a.Max.x;
					b.MaxValue = b.Max.x;
					
					if (m_CompareTo == true)
					{
						return a.Min.x.CompareTo(b.Min.x);
					}
					else
					{	
						if (a.Min.x >= b.Min.x) return 1;
						else return -1;
					}
				});
	}
	
	void SweepYAxis()
	{
		mColliders.Sort( (a, b) => {
					a.MinValue = a.Min.y;
					b.MinValue = b.Min.y;
					a.MaxValue = a.Max.y;
					b.MaxValue = b.Max.y;
					
					if (a.Min.y >= b.Min.y) return 1;
					else return -1;
				});
	}
	
	void SweepZAxis()
	{
		mColliders.Sort( (a, b) => {
					a.MinValue = a.Min.z;
					b.MinValue = b.Min.z;
					a.MaxValue = a.Max.z;
					b.MaxValue = b.Max.z;
					
					if (a.Min.z >= b.Min.z) return 1;
					else return -1;
				});
	}
}
