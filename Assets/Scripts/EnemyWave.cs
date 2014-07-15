using UnityEngine;
using System.Collections;

public enum WaveState
{
	None, Waiting, Started, Running, Done
}

public class EnemyWave : MonoBehaviour
{
	public EnemyManager m_EnemyManager;
	
	public int 		m_Round;
	public int 		m_Wave;
	public int 		m_Phase;
	public int 		m_StartDelay;
	public float 	m_StartTime;
	public float 	m_SpawnCircleRadius;
	public float 	m_SpawnTime;
	public float 	m_SpawnTimeVariance;
	
	public int 		m_NumMinions;
	public int	 	m_MaxActiveMinions;

	public float 	m_SpeedVariance;

	int m_SpawnedMinions;
	
	int	m_ActiveMinions;
	
	float m_Timer;
	float m_TimerLimit;
	float m_StartTimer = 0;
	
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
		m_SpawnedMinions = m_NumMinions;
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
		return (m_SpawnedMinions == m_NumMinions);
	}
	
	public WaveState UpdateWave(float dt)
	{
		m_StartTimer += dt;
		if (m_StartTimer>m_StartTime)
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
				if (m_ActiveMinions < m_MaxActiveMinions)
				{
					OnSpawnedEnemy("Enemy");
					m_EnemyManager.SpawnEnemy();
				}
				m_TimerLimit = m_SpawnTime + m_SpawnTimeVariance * Random.Range(-1, 1);
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