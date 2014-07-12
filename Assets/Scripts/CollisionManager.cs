using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionManager : MonoBehaviour 
{
	public int 			NumColliders = 0;
	public bool 		m_SAP = false;
	public bool 		m_CompareTo = true;
	public bool	 		m_BreakSAPCycle = false;
	public bool 		m_AdaptiveSweepAxis = true;
	public int 			m_IterationCount = 10;
	public float 		m_Relaxation = 0.8f;
	public bool    	 	m_CheckSort = false;
	public float		m_CollisionErrorInterpolation = 1.0f;
	public bool    		m_LinearTest = true;
	
	List<Collidable> 	mColliders = new List<Collidable>();

	delegate void 		SweepFuncion();
	SweepFuncion 		mSweepFunction;
	
	Quaternion _quaternionIdentity = Quaternion.identity;
	
	void Start () 
	{
		mSweepFunction = new SweepFuncion(this.SweepXAxis);
	}
	
	public float CollisionErrorInterpolation
	{
		get { return m_CollisionErrorInterpolation; }
	}
	
	public void AddCollider(Collidable collider)
	{
		++NumColliders;
		mColliders.Add (collider);
	}
	
	public void RemoveCollider(Collidable collider)
	{
		--NumColliders;
		mColliders.Remove (collider);
	}
	
	void Update () 
	{
		if (m_SAP==true) UpdateDynamicCollisionsSingleSAP();
		else UpdateDynamicBrute();
	}
	
	public bool ResolveCoupleCollision(Collidable collider, Collidable other)
	{
		Quaternion colliderError = _quaternionIdentity;
		Quaternion otherError = _quaternionIdentity;

		float radiusSumAngle = collider.AngleRadius + other.AngleRadius;
		float dot = Vector3.Dot(collider.Up, other.Up);
		float angle = Mathf.Acos (dot) * Mathf.Rad2Deg;
		
		if (angle<radiusSumAngle)
		{
			float angleError = (radiusSumAngle - angle) * m_Relaxation;
			float colliderMassRatio = collider.Mass / (collider.Mass+other.Mass);
			
			Vector3 rotationAxis = Vector3.Cross(other.Up, collider.Up);
			colliderError = Quaternion.AngleAxis(angleError*(1.0f-colliderMassRatio), rotationAxis);	
			otherError = Quaternion.AngleAxis(-angleError*(colliderMassRatio), rotationAxis);
			
			collider.Rotation = colliderError * collider.Rotation;	
			collider.Up = collider.Rotation * Vector3.up;
			other.Rotation = otherError * other.Rotation;	
			other.Up = other.Rotation * Vector3.up;
			
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
				
				if (current==active) continue;
				if (!active.gameObject.activeInHierarchy) continue;
				if (active.Static && current.Static) continue;
				if (Physics.GetIgnoreLayerCollision(current.gameObject.layer, active.gameObject.layer)) continue; 
				
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
				float minX = -100000000000000000000000.0f;
				foreach (Collidable coll in mColliders)
				{
					if (coll.Min.x < minX) Debug.LogError("Sort is wrong");
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
			
				//positions summ for variance
				s += current.Center;
				s2 += Vector3.Scale (current.Center, current.Center);
				
				for (int activeIndex = colliderIndex+1; activeIndex<endIndex; activeIndex++)
				{
					Collidable active = mColliders[activeIndex];
					
					if (!active.gameObject.activeInHierarchy) continue;
					if (active.Static && current.Static) continue;
					if (Physics.GetIgnoreLayerCollision(current.gameObject.layer, active.gameObject.layer)) continue; 
					
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