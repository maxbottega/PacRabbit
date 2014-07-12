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
	public int 		m_NumZombies;
	public int 		m_NumZombiePukers;
	public int 		m_NumZombieCrawlers;
	public int 		m_NumZombieDiggers;
	public int 		m_NumZombieRunners;
	public int 		m_NumBoomers;
	public int 		m_NumTanks;
	public int 		m_NumZombieFast;
	public int 		m_NumCactus;
	
	public int	 	m_MaxActiveMinions;
	public int	 	m_MaxActiveZombies;
	public int	 	m_MaxActiveZombiePukers;
	public int	 	m_MaxActiveZombieCrawlers;
	public int	 	m_MaxActiveZombieDiggers;
	public int	 	m_MaxActiveZombieRunners;
	public int	 	m_MaxActiveBoomers;
	public int	 	m_MaxActiveTanks;
	public int	 	m_MaxActiveZombieFast;
	public int	 	m_MaxActiveCactus;
	
	public float 	m_MinionsSpawnProbability = 1;
	public float 	m_ZombiesSpawnProbability = 1;
	public float 	m_ZombieCrawlersSpawnProbability = 1;
	public float 	m_ZombiePukersSpawnProbability = 1;
	public float 	m_ZombieDiggersSpawnProbability = 1;
	public float 	m_ZombieRunnersSpawnProbability = 1;
	public float 	m_BoomersSpawnProbability = 1;
	public float 	m_TanksSpawnProbability = 1;
	public float 	m_ZombieFastSpawnProbability = 1;
	public float 	m_CactusSpawnProbability = 1;

	public float 	m_SpeedVariance;

	int m_SpawnedMinions;
	int m_SpawnedZombies;
	int m_SpawnedZombiePukers;
	int m_SpawnedZombieCrawlers;
	int m_SpawnedZombieDiggers;
	int m_SpawnedZombieRunners;
	int m_SpawnedBoomers;
	int m_SpawnedTanks;
	int m_SpawnedZombieFast;
	int m_SpawnedCactus;
	
	int	m_ActiveMinions;
	int	m_ActiveZombies;
	int	m_ActiveZombiePukers;
	int	m_ActiveZombieCrawlers;
	int	m_ActiveZombieDiggers;
	int	m_ActiveZombieRunners;
	int	m_ActiveBoomers;
	int	m_ActiveTanks;
	int m_ActiveZombieFast;
	int m_ActiveCactus;
	
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

		float totalMaxEnemies = m_MaxActiveMinions + m_MaxActiveZombies + m_MaxActiveZombiePukers + m_MaxActiveZombieCrawlers + m_MaxActiveZombieDiggers + m_MaxActiveZombieRunners + m_MaxActiveBoomers + m_MaxActiveTanks + m_MaxActiveZombieFast + m_MaxActiveCactus;
		
		m_MinionsSpawnProbability 			= m_MaxActiveMinions/totalMaxEnemies;
		m_ZombiesSpawnProbability 			= m_MaxActiveZombies/totalMaxEnemies 		+ m_MinionsSpawnProbability;
		m_ZombieCrawlersSpawnProbability 	= m_MaxActiveZombieCrawlers/totalMaxEnemies + m_ZombiesSpawnProbability;
		m_ZombiePukersSpawnProbability 		= m_MaxActiveZombiePukers/totalMaxEnemies 	+ m_ZombieCrawlersSpawnProbability;
		m_ZombieDiggersSpawnProbability 	= m_MaxActiveZombieDiggers/totalMaxEnemies 	+ m_ZombiePukersSpawnProbability;
		m_ZombieRunnersSpawnProbability 	= m_MaxActiveZombieRunners/totalMaxEnemies 	+ m_ZombieDiggersSpawnProbability;
		m_BoomersSpawnProbability			= m_MaxActiveBoomers/totalMaxEnemies 		+ m_ZombieRunnersSpawnProbability;
		m_TanksSpawnProbability	 			= m_MaxActiveTanks/totalMaxEnemies 			+ m_BoomersSpawnProbability;
		m_ZombieFastSpawnProbability	 	= m_MaxActiveZombieFast/totalMaxEnemies 	+ m_TanksSpawnProbability;
		m_CactusSpawnProbability	 		= m_MaxActiveCactus/totalMaxEnemies 		+ m_ZombieFastSpawnProbability;
	}
	
	public void StartWave()
	{	
		m_StartTimer = 0;
	}
	
	public void EndWave()
	{	
		m_SpawnedMinions = m_NumMinions;
		m_SpawnedZombies = m_NumZombies; 
		m_SpawnedZombiePukers = m_NumZombiePukers; 
		m_SpawnedZombieCrawlers = m_NumZombieCrawlers; 
		m_SpawnedZombieDiggers = m_NumZombieDiggers; 
		m_SpawnedZombieRunners = m_NumZombieRunners; 
		m_SpawnedBoomers = m_NumBoomers;
		m_SpawnedTanks = m_NumTanks;
		m_SpawnedZombieFast = m_NumZombieFast;
		m_SpawnedCactus = m_NumCactus;
		
		m_ActiveMinions = 0;
		m_ActiveZombies = 0;	
		m_ActiveZombiePukers = 0;	
		m_ActiveZombieCrawlers = 0;	
		m_ActiveZombieDiggers = 0;	
		m_ActiveZombieRunners = 0;	
		m_ActiveBoomers = 0;
		m_ActiveTanks = 0;
		m_ActiveZombieFast = 0;
		m_ActiveCactus = 0;
	}
	
	public void EnemyRespawned(string tag)
	{
		switch (tag)
		{	
		case "minion":
			{
				--m_ActiveMinions;
				--m_SpawnedMinions;
			}
			break;
		case "zombie":
			{Debug.Log("DECREMENTO");
				--m_ActiveZombies;
				--m_SpawnedZombies;
			}
			break;
		case "puker":
			{
				--m_ActiveZombiePukers;
				--m_SpawnedZombiePukers;
			}
			break;
		case "crawler":
			{
				--m_ActiveZombieCrawlers;
				--m_SpawnedZombieCrawlers;
			}
			break;
		case "digger":
			{
				--m_ActiveZombieDiggers;
				--m_SpawnedZombieDiggers;
			}
			break;
		case "runner":
			{
				--m_ActiveZombieRunners;
				--m_SpawnedZombieRunners;
			}
			break;
		case "boomer":
			{
				--m_ActiveBoomers;
				--m_SpawnedBoomers;
			}
			break;
		case "tanks":
			{
				--m_ActiveTanks;
				--m_SpawnedTanks;
			}
			break;
		case "fast":
			{
				--m_ActiveZombieFast;
				--m_SpawnedZombieFast;
			}
			break;
		case "cactus":
			{
				--m_ActiveCactus;
				--m_SpawnedCactus;
			}
			break;
		}
	}
	
	public void EnemyKilled(string tag)
	{		
		switch (tag)
		{	
		case "minion":
			{
				--m_ActiveMinions;
			}
			break;
		case "zombie":
			{
				--m_ActiveZombies;
			}
			break;
		case "puker":
			{
				--m_ActiveZombiePukers;
			}
			break;
		case "crawler":
			{
				--m_ActiveZombieCrawlers;
			}
			break;
		case "digger":
			{
				--m_ActiveZombieDiggers;
			}
			break;
		case "runner":
			{
				--m_ActiveZombieRunners;
			}
			break;
		case "boomer":
			{
				--m_ActiveBoomers;
			}
			break;
		case "tank":
			{
				--m_ActiveTanks;
			}
			break;
		case "fast":
			{
				--m_ActiveZombieFast;
			}
			break;
		case "cactus":
			{
				--m_ActiveCactus;
			}
			break;
		}
		
		m_Timer = 0.0f;
	}
	
	bool IsWaveFinished()
	{
		return (m_SpawnedMinions==m_NumMinions &&
			    m_SpawnedZombies==m_NumZombies && 
			    m_SpawnedZombiePukers==m_NumZombiePukers && 
			    m_SpawnedZombieCrawlers==m_NumZombieCrawlers && 
			    m_SpawnedZombieDiggers==m_NumZombieDiggers && 
			    m_SpawnedZombieRunners==m_NumZombieRunners && 
			    m_SpawnedBoomers==m_NumBoomers && 
			    m_SpawnedTanks==m_NumTanks &&
				m_SpawnedCactus==m_NumCactus &&
				m_SpawnedZombieFast==m_NumZombieFast &&
			    m_ActiveMinions==0 &&
			    m_ActiveZombies==0 &&
			    m_ActiveZombiePukers==0 &&
			    m_ActiveZombieCrawlers==0 &&
			    m_ActiveZombieDiggers==0 &&
			    m_ActiveZombieRunners==0 &&
			    m_ActiveBoomers==0 &&
			    m_ActiveTanks==0 &&
				m_ActiveZombieFast==0 &&
				m_ActiveCactus==0);
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
				if (m_ActiveZombieDiggers < m_MaxActiveZombieDiggers)
				{
					OnSpawnedEnemy("digger");
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
		
		float random = Random.value;
		
		if ((random < m_MinionsSpawnProbability) && (m_SpawnedMinions < m_NumMinions) && (m_ActiveMinions < m_MaxActiveMinions))
		{
			enemyTag = "minion";
		}
		else if ((random < m_ZombiesSpawnProbability) && (m_SpawnedZombies < m_NumZombies) && (m_ActiveZombies < m_MaxActiveZombies))
		{
			enemyTag = "zombie";
		}
		else if ((random < m_ZombieCrawlersSpawnProbability) && (m_SpawnedZombieCrawlers < m_NumZombieCrawlers) && (m_ActiveZombieCrawlers < m_MaxActiveZombieCrawlers))
		{
			enemyTag = "crawler";
		}
		else if ((random < m_ZombiePukersSpawnProbability) && (m_SpawnedZombiePukers < m_NumZombiePukers) && (m_ActiveZombiePukers < m_MaxActiveZombiePukers))
		{
			enemyTag = "puker";
		}
		else if ((random < m_ZombieDiggersSpawnProbability) && (m_SpawnedZombieDiggers < m_NumZombieDiggers) && (m_ActiveZombieDiggers < m_MaxActiveZombieDiggers))
		{
			enemyTag = "digger";	
		}
		else if ((random < m_ZombieRunnersSpawnProbability) && (m_SpawnedZombieRunners < m_NumZombieRunners) && (m_ActiveZombieRunners < m_MaxActiveZombieRunners))
		{
			enemyTag = "runner";
		}
		else if ((random < m_BoomersSpawnProbability) && (m_SpawnedBoomers < m_NumBoomers) && (m_ActiveBoomers < m_MaxActiveBoomers))
		{
			enemyTag = "boomer";
		}
		else if ((random < m_TanksSpawnProbability) && (m_SpawnedTanks < m_NumTanks) && (m_ActiveTanks < m_MaxActiveTanks))
		{
			enemyTag = "tank";
		}
		else if ((random < m_ZombieFastSpawnProbability) && (m_SpawnedZombieFast < m_NumZombieFast) && (m_ActiveZombieFast < m_MaxActiveZombieFast))
		{
			enemyTag = "fast";
		}
		else if ((random < m_CactusSpawnProbability) && (m_SpawnedCactus < m_NumCactus) && (m_ActiveCactus < m_MaxActiveCactus))
		{
			enemyTag = "cactus";
		}		
		else
		{
			enemyTag = "undefined";
		}
		return enemyTag;
	}
	
	void OnSpawnedEnemy(string enemyTag)
	{
		switch (enemyTag)
		{
		case "minion":
			{
				++m_SpawnedMinions;
				++m_ActiveMinions;
			}
			break;
		case "zombie":
			{
				++m_SpawnedZombies;
				++m_ActiveZombies;
			}
			break;
		case "puker":
			{
				++m_SpawnedZombiePukers;
				++m_ActiveZombiePukers;
			}
			break;
		case "crawler":
			{
				++m_SpawnedZombieCrawlers;
				++m_ActiveZombieCrawlers;
			}
			break;
		case "digger":
			{
				++m_SpawnedZombieDiggers;
				++m_ActiveZombieDiggers;
			}
			break;
		case "runner":
			{
				++m_SpawnedZombieRunners;
				++m_ActiveZombieRunners;
			}
			break;
		case "boomer":
			{
				++m_SpawnedBoomers;
				++m_ActiveBoomers;
			}
			break;
		case "tank":
			{
				++m_SpawnedTanks;
				++m_ActiveTanks;
			}
			break;
		case "fast":
			{
				++m_SpawnedZombieFast;
				++m_ActiveZombieFast;
			}
			break;
		case "cactus":
			{
				++m_SpawnedCactus;
				++m_ActiveCactus;
			}
			break;			
		}
	}
}