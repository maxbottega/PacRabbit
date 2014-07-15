using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour 
{
	// ------------ Public, editable in the GUI, serialized
	public GameObject 								EnemyPrefab;
	public int										NumEnemies = 1;
	public EnemyWave[]								Waves;
		
	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	//[System.NonSerialized] List<MeshFilter> 		mEnemyMeshes = new List<MeshFilter>();
	[System.NonSerialized] List<Enemy> 				mEnemies = new List<Enemy>();
	[System.NonSerialized] int 						m_CurrentWaveId = 0;
	
	void Start () 
	{

		GameObject folder = new GameObject ("ENEMIES");
		EnemyPrefab.SetActive(true);
	
		for (int i=0; i<NumEnemies; ++i)
		{
			GameObject instance = Instantiate(EnemyPrefab, EnemyPrefab.transform.position, EnemyPrefab.transform.rotation) as GameObject;

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
			
			instance.name = instance.name + i;
			instance.transform.up = Random.insideUnitSphere;
			instance.transform.parent = folder.transform;
			//mEnemyMeshes.Add (meshFilter);
			mEnemies.Add (instance.GetComponentInChildren<Enemy>());
			mEnemies[mEnemies.Count-1].gameObject.SetActive(true);
			
			//m_AnimationSlot.Acquire(meshFilter);
		}
		
		//m_EnemyPrefab.GetComponentInChildren<Zombie>().SetAnimatedDummy();
		
		//Waves
		foreach (EnemyWave wave in Waves)
		{
			wave.Init();
		}
	}
	
	
	// Update is called once per frame
	void Update () 
	{

		if (m_CurrentWaveId < Waves.Length) 
		{
			WaveState state = Waves [m_CurrentWaveId].UpdateWave (Time.deltaTime);
			if (state == WaveState.Done)
				++m_CurrentWaveId;
		}
		
		// Mesh instancing
		//m_AnimationSlot.DoUpdate();
	}
	
	public void NotifyEnemyDeath()
	{
		Waves[m_CurrentWaveId].EnemyKilled("digger");
	}
	
	public void SpawnEnemy()
	{
		foreach (Enemy enemy in mEnemies)
		{
			if (!enemy.gameObject.activeInHierarchy)
			{
				enemy.gameObject.SetActive(true);
				Collidable collidable = enemy.GetComponent<Collidable>();
				collidable.Rotation = Random.rotation;
				break;
			}
		}
	}
}
