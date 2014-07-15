using UnityEngine;
using System.Collections;

public enum WaveState
{
	None, Waiting, Started, Running, Done
}

public class EnemyWave : MonoBehaviour
{
	// ------------ Public, editable in the GUI, serialized
	public int 		Round;
	public int 		Wave;
	public int 		Phase;
	public int 		StartDelay;
	public float 	StartTime;
	public float 	SpawnCircleRadius;
	public float 	SpawnTime;
	public float 	SpawnTimeVariance;
	public int 		NumMinions;
	public int	 	MaxActiveMinions;
	public float 	SpeedVariance;
		
	// ------------ Public, serialized
	
	// ------------ Public, non-serialized
	[System.NonSerialized] public EnemyManager 	m_EnemyManager = null;
	[System.NonSerialized] int 					m_SpawnedMinions = 0;
	[System.NonSerialized] int					m_ActiveMinions = 0;
	[System.NonSerialized] float 				m_Timer = 0.0f;
	[System.NonSerialized] float 				m_TimerLimit = 0.0f;
	[System.NonSerialized] float 				m_StartTimer = 0.0f;
	
	WaveState m_State = WaveState.None;
	
	public WaveState State
	{
		get { return m_State; }
	}
	
	public void Init()
	{
		m_EnemyManager = FindObjectOfType(typeof(EnemyManager)) as EnemyManager;
	}
	
	public void StartWave()
	{	
		m_StartTimer = 0;
	}
	
	public void EndWave()
	{	
		m_SpawnedMinions = NumMinions;
		m_ActiveMinions = 0;
	}
	
	public void EnemyRespawned(string tag)
	{
		--m_ActiveMinions;
		--m_SpawnedMinions;
	}
	
	public void EnemyKilled(string tag)
	{		
		--m_ActiveMinions;
		
		m_Timer = 0.0f;
	}
	
	bool IsWaveFinished()
	{
		return (m_SpawnedMinions == NumMinions);
	}
	
	public WaveState UpdateWave(float dt)
	{
		m_StartTimer += dt;
		if (m_StartTimer>StartTime)
		{
			if (m_State == WaveState.Waiting)
			{
				m_State = WaveState.Started;
				return WaveState.Started;
			}
			
			if (IsWaveFinished())
			{
				m_State = WaveState.Done;
				return WaveState.Done;
			}
				
			m_Timer += dt;
			if (m_Timer>m_TimerLimit)
			{
				m_Timer = 0;
				if (m_ActiveMinions < MaxActiveMinions)
				{
					OnSpawnedEnemy("Enemy");
					m_EnemyManager.SpawnEnemy();
				}
				m_TimerLimit = SpawnTime + SpawnTimeVariance * Random.Range(-1, 1);
			}
			
			m_State = WaveState.Running;
			return WaveState.Running;
		}
		
		m_State = WaveState.Waiting;
		return WaveState.Waiting;
	}
	
	public string GetEnemyTagToSpawn()
	{
		string enemyTag	= null;
		enemyTag = "minion";
		return enemyTag;
	}
	
	void OnSpawnedEnemy(string enemyTag)
	{
		++m_SpawnedMinions;
		++m_ActiveMinions;
	}
}