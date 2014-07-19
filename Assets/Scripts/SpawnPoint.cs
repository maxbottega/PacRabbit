using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnPoint : MonoBehaviour 
{
	[System.Serializable]
	public class EnemyData
	{
		public GameObject 	EnemyPrefab = null;
		public int 			Count = 1;
	}
	
	public EnemyData[] EnemiesList = null;
	List<Enemy> mEnemies = new List<Enemy>();

	public void SpawnEnemy ()
	{
		foreach (Enemy enemy in mEnemies)
		{
			if (!enemy.gameObject.activeInHierarchy)
			{
				enemy.gameObject.SetActive (true);
				SphereTransform moveController = enemy.GetComponent<SphereTransform> ();
				moveController.ImmediateSet (Quaternion.FromToRotation (Vector3.up, transform.position.normalized));
				
				break;
			}
		}
	}
	
	void Start () 
	{
		foreach (EnemyData ed in EnemiesList)
		{
			GameObject folder = GameObject.Find ("ENEMIES");
			if (folder==null)
				folder = new GameObject ("ENEMIES");
				
			for (int i=0; i<ed.Count; ++i)
			{
				GameObject instance = Instantiate(ed.EnemyPrefab, Vector3.zero, Quaternion.FromToRotation(Vector3.up, transform.position.normalized)) as GameObject;
				
				instance.name = instance.name + i;
				instance.transform.parent = folder.transform;
				instance.gameObject.SetActive (true); // you need to activate the game object, or GetComponentInChildred will fail to find it
				Enemy enemyComponent = instance.GetComponentInChildren<Enemy> ();
				enemyComponent.gameObject.SetActive (false);
				
				mEnemies.Add (enemyComponent);
			}		
		}
	}
	
	void Update () 
	{
		SpawnEnemy ();	
	}
}
