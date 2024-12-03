using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessMasterScript : MonoBehaviour
{
    public EndlessUIMasterScript endlessUIMasterScript;
    public PauseMenu pauseMenu;

    public GameObject[] SpawnPoints;
    public List<Transform> WayPoints;
    public EnemyPoolScript EnemyPool;

    public List<int> PathogensPerWave;
    private int CurrentWaveIndex = 0;

    public List<GameObject> SpawnedPathogens = new List<GameObject>();
    private Dictionary<string, int> plannedPathogensCount = new Dictionary<string, int>();

    private int waveCount = 1; // Track the number of waves
    private bool isLose = false;

    void Start()
    {
        if (endlessUIMasterScript == null)
        {
            endlessUIMasterScript = FindObjectOfType<EndlessUIMasterScript>();
            Debug.Log("Solo UI Master Script found!");
        }
        if (pauseMenu == null)
        {
            pauseMenu = FindObjectOfType<PauseMenu>();
            Debug.Log("Pause Menu Script found!");
        }

        PlanSpawn(CurrentWaveIndex); // Plan the first waveS
    }

    public void PlanSpawn(int waveIndex)
    {
        if(isLose) return;
        plannedPathogensCount.Clear();

        int pathogenCount = Mathf.CeilToInt(waveCount * 1.5f); // Increase count based on wave number
        for (int i = 0; i < pathogenCount; i++)
        {
            var enemyData = EnemyPool.EnemyDataList[Random.Range(0, EnemyPool.EnemyDataList.Count)];
            string pathogenName = enemyData.EnemyPrefab.name;

            if (plannedPathogensCount.ContainsKey(pathogenName))
            {
                plannedPathogensCount[pathogenName]++;
            }
            else
            {
                plannedPathogensCount[pathogenName] = 1;
            }
        }

        foreach (var entry in plannedPathogensCount)
        {
            Debug.Log($"Planned Pathogen: {entry.Key}, Count: {entry.Value}");
        }
    }

    public void SpawnPathogens(int waveIndex)
    {
        if(isLose) return;
        int pathogenCount = Mathf.CeilToInt(waveCount * 1.5f);

        for (int i = 0; i < pathogenCount; i++)
        {
            GameObject pathogenPrefab = EnemyPool.GetEnemyFromPool(EnemyPool.EnemyDataList[Random.Range(0, EnemyPool.EnemyDataList.Count)].EnemyPrefab);

            if (pathogenPrefab != null)
            {
                Vector3 spawnPosition = SpawnPoints[Random.Range(0, SpawnPoints.Length)].transform.position;
                pathogenPrefab.transform.position = spawnPosition;
                pathogenPrefab.SetActive(true);
                SpawnedPathogens.Add(pathogenPrefab);

                BaseEnemyScript enemyScript = pathogenPrefab.GetComponent<BaseEnemyScript>();
                if (enemyScript != null)
                {
                    enemyScript.Initialize(WayPoints.ToArray(), EnemyPool, this, endlessUIMasterScript);
                }
            }
        }
    }

    public void TriggerNextWave()
    {
        if(isLose) return;
        PlanSpawn(CurrentWaveIndex);
        SpawnPathogens(CurrentWaveIndex);
    }

    public void RemovePathogen(GameObject pathogen)
    {
        if (SpawnedPathogens.Contains(pathogen))
        {
            SpawnedPathogens.Remove(pathogen);
            Debug.Log($"{pathogen.name} removed from SpawnedPathogens.");
        }
        else
        {
            Debug.LogWarning($"{pathogen.name} was not found in SpawnedPathogens.");
        }
    }

    IEnumerator CheckAllPathogensDestroyed()
    {
        yield return new WaitForSeconds(1f);

        bool allDestroyed = true;
        foreach (GameObject pathogen in SpawnedPathogens)
        {
            if (pathogen != null)
            {
                allDestroyed = false;
                break;
            }
        }

        if (allDestroyed && !isLose)
        {
            Debug.Log("Wave cleared!");
            PlanSpawn(CurrentWaveIndex);
            SpawnPathogens(CurrentWaveIndex);
        }
    }

    public void Lost(bool lost)
    {
        if(lost)
        {
        isLose = true;
        }
    }
}
