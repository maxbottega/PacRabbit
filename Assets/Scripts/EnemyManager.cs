//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
//public class EnemyManager : MonoBehaviour 
//{
//	public GameObject 	m_EnemyPrefab;
//	public int			m_NumEnemies;
//	public EnemyWave[]	m_Waves;
//	
//	public AnimationSlot m_AnimationSlot = null;
//	
//	List<MeshFilter> mEnemyMeshes = new List<MeshFilter>();
//	List<Zombie> mEnemies = new List<Zombie>();
//	
//	int m_CurrentWaveId = 0;
//
//	// Use this for initialization
//	void Start () 
//	{
//		Transform folder = m_EnemyPrefab.transform.parent;
//		
//		m_EnemyPrefab.SetActive(true);
//	
//		for (int i=0; i<m_NumEnemies; ++i)
//		{
//			GameObject instance = Instantiate(m_EnemyPrefab, m_EnemyPrefab.transform.position, m_EnemyPrefab.transform.rotation) as GameObject;
//			SkinnedMeshRenderer skinnedMesh = instance.GetComponentInChildren<SkinnedMeshRenderer>();
//			GameObject renderObj = skinnedMesh.gameObject;
//			SkinnedMeshRenderer smRenderer = renderObj.GetComponent<SkinnedMeshRenderer>();
//			Material[] materials = smRenderer.sharedMaterials;
//			Destroy (smRenderer);
//			
//			Animator animator = instance.GetComponentInChildren<Animator>();
//			Destroy (animator);
//			
//			MeshFilter meshFilter = renderObj.AddComponent<MeshFilter>();
//			MeshRenderer meshRenderer = renderObj.AddComponent<MeshRenderer>();
//			meshRenderer.sharedMaterials = materials;
//			meshRenderer.castShadows = true;
//			meshRenderer.receiveShadows = false;
//			
//			instance.name = instance.name + i;
//			instance.transform.up = Random.insideUnitSphere;
//			instance.transform.parent = folder;
//			mEnemyMeshes.Add (meshFilter);
//			mEnemies.Add (instance.GetComponentInChildren<Zombie>());
//			mEnemies[mEnemies.Count-1].gameObject.SetActive(false);
//			
//			m_AnimationSlot.Acquire(meshFilter);
//		}
//		
//		m_EnemyPrefab.GetComponentInChildren<Zombie>().SetAnimatedDummy();
//		
//		//Waves
//		foreach (EnemyWave wave in m_Waves)
//		{
//			wave.Init();
//		}
//	}
//	
//	
//	// Update is called once per frame
//	void Update () 
//	{
//		//Waves
//#if UNITY_EDITOR
//		if (m_Waves.Length==0) Debug.LogError("No enemy waves!");
//#endif
//		WaveState state = m_Waves[m_CurrentWaveId].UpdateWave (Time.deltaTime);
//		if (state==WaveState.Done) ++m_CurrentWaveId;
//		
//		// Mesh instancing
//		m_AnimationSlot.DoUpdate();
//	}
//	
//	public void NotifyEnemyDeath()
//	{
//		m_Waves[m_CurrentWaveId].EnemyKilled("digger");
//	}
//	
//	public void SpawnEnemy()
//	{
//		foreach (Zombie enemy in mEnemies)
//		{
//			if (!enemy.gameObject.activeInHierarchy)
//			{
//				enemy.gameObject.SetActive(true);
//				Collidable collidable = enemy.GetComponent<Collidable>();
//				collidable.Rotation = Random.rotation;
//				break;
//			}
//		}
//	}
//}
