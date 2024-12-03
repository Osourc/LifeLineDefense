using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoloMasterScript : MonoBehaviour
{
    public SoloUIMasterScript soloUIMasterScript;
    public PauseMenu pauseMenu;
    public CutsceneManagerScript cutScene;

    public int TotalWaves = 3;
    public GameObject[] SpawnPoints;
    public List<Transform> WayPoints;
    public EnemyPoolScript EnemyPool;
    
    public List<int> PathogensPerWave;
    private int CurrentWaveIndex = 0;
    private bool hasWon = false;

    public List<GameObject> SpawnedPathogens = new List<GameObject>(); // Track spawned pathogens
    private Dictionary<string, int> plannedPathogensCount = new Dictionary<string, int>(); // To plan and count pathogens

    void Start()
    {
        //this.enabled = false;******************************************************************
        if(soloUIMasterScript == null)
        {
        soloUIMasterScript = FindObjectOfType<SoloUIMasterScript>();
        Debug.Log("Solo UI Master Script found!");
        }
        if(pauseMenu == null)
        {
        pauseMenu = FindObjectOfType<PauseMenu>();
        Debug.Log("Pause Menu Script found!");
        }
        if(cutScene == null)
        {
        cutScene = FindObjectOfType<CutsceneManagerScript>();
        Debug.Log("Solo UI Master Script found!");
        }

        if (PathogensPerWave == null || PathogensPerWave.Count == 0)
        {
            InitializePathogensPerWave();
        }
        PlanSpawn(CurrentWaveIndex); // Plan the first wave
    }

    void InitializePathogensPerWave()
    {
        if (TotalWaves <= 0)
        {
            Debug.LogError("TotalWaves is not set or is set to a non-positive value.");
            return;
        }

        PathogensPerWave = new List<int>();
        int firstWaveCount = Random.Range(1, 5);
        PathogensPerWave.Add(firstWaveCount); // Add the first wave count

        for (int i = 1; i < TotalWaves; i++)
        {
            int previousWaveCount = PathogensPerWave[i - 1];
            int minSpawn = previousWaveCount + 1;
            int maxSpawn = minSpawn + Random.Range(1, 5);
            PathogensPerWave.Add(Random.Range(minSpawn, maxSpawn));
        }
    }

    public void PlanSpawn(int waveIndex)
    {
        if (waveIndex >= TotalWaves || hasWon) return;

        // Clear previous planned pathogens
        plannedPathogensCount.Clear();

        // Get the number of pathogens for this wave
        int pathogenCount = PathogensPerWave[waveIndex];

        for (int i = 0; i < pathogenCount; i++)
        {
            // Get a random enemy from the pool
            var enemyData = EnemyPool.EnemyDataList[Random.Range(0, EnemyPool.EnemyDataList.Count)];
            string pathogenName = enemyData.EnemyPrefab.name;

            // Increment the count of this pathogen type
            if (plannedPathogensCount.ContainsKey(pathogenName))
            {
                plannedPathogensCount[pathogenName]++;
            }
            else
            {
                plannedPathogensCount[pathogenName] = 1;
            }
        }

        // Log planned pathogens for debugging
        foreach (var entry in plannedPathogensCount)
        {
            Debug.Log($"Planned Pathogen: {entry.Key}, Count: {entry.Value}");
        }
    }

    public void SpawnPathogens(int waveIndex)
    {
        if (waveIndex >= TotalWaves || hasWon) return;
        int pathogenCount = PathogensPerWave[waveIndex];

        for (int i = 0; i < pathogenCount; i++)
        {
            // Get a random enemy prefab from the pool
            GameObject pathogenPrefab = EnemyPool.GetEnemyFromPool(EnemyPool.EnemyDataList[Random.Range(0, EnemyPool.EnemyDataList.Count)].EnemyPrefab);

            if (pathogenPrefab != null)
            {
                Vector3 spawnPosition = SpawnPoints[Random.Range(0, SpawnPoints.Length)].transform.position;
                pathogenPrefab.transform.position = spawnPosition;
                pathogenPrefab.SetActive(true);
                SpawnedPathogens.Add(pathogenPrefab);

                // Initialize the enemy with waypoints and pool reference
                BaseEnemyScript enemyScript = pathogenPrefab.GetComponent<BaseEnemyScript>();
                if (enemyScript != null)
                {
                    enemyScript.Initialize(WayPoints.ToArray(), EnemyPool, this, soloUIMasterScript);
                }
            }
        }

        // Check if all waves are complete
        if (waveIndex >= TotalWaves - 1)
        {
            StartCoroutine(CheckAllPathogensDestroyed());
        }
        else
        {
            CurrentWaveIndex++; // Move to the next wave
        }
    }

    public void TriggerNextWave()
    {
        if (CurrentWaveIndex < TotalWaves && !hasWon)
        {
            PlanSpawn(CurrentWaveIndex); // Plan the next wave
            SpawnPathogens(CurrentWaveIndex); // Spawn the next wave
        }
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

        if (allDestroyed)
        {
            Debug.Log("You win! All pathogens destroyed.");
            hasWon = true;
            StartCoroutine(HandleEndgameAfterDialogue());
        }
        else
        {
            Debug.Log("Not all pathogens destroyed yet...");
            StartCoroutine(CheckAllPathogensDestroyed());
        }
    }

IEnumerator HandleEndgameAfterDialogue()
{
    GameObject cutsceneObject = cutScene.gameObject;
    cutsceneObject.SetActive(true);
    // Wait for the ending dialogue to finish
    yield return StartCoroutine(cutScene.TriggerEndingDialogue());

    // After the dialogue finishes, proceed with endgame
    pauseMenu.EndGame(hasWon);
    //LevelManager.Instance.UnlockNextLevel();****************************************************
}
    

    public void EnableScript()
    {
        this.enabled = true;
        Debug.Log("SoloMasterScript enabled!");
    }
}