using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public GameObject EnemyPrefab;
    public int SpawnCount = 10;
}

public class EnemyPoolScript : MonoBehaviour
{
    public List<EnemyData> EnemyDataList;
    private Dictionary<GameObject, Queue<GameObject>> EnemyPool;

    private void Awake()
    {
        // Initialize the enemy pool
        EnemyPool = new Dictionary<GameObject, Queue<GameObject>>();

        foreach (var data in EnemyDataList)
        {
            Queue<GameObject> poolQueue = new Queue<GameObject>();

            // Preload enemies into the pool based on SpawnCount
            for (int i = 0; i < data.SpawnCount; i++)
            {
                GameObject enemy = Instantiate(data.EnemyPrefab, transform);
                enemy.SetActive(false);
                poolQueue.Enqueue(enemy);
            }

            EnemyPool[data.EnemyPrefab] = poolQueue;
        }
    }

    public GameObject GetEnemyFromPool(GameObject EnemyPrefab)
    {
        if (EnemyPool.ContainsKey(EnemyPrefab) && EnemyPool[EnemyPrefab].Count > 0)
        {
            // Get an enemy from the pool and activate it
            GameObject enemy = EnemyPool[EnemyPrefab].Dequeue();
            enemy.transform.SetParent(transform);
            enemy.SetActive(true);
            return enemy;
        }
        else
        {
            // If no enemies are left in the pool, optionally instantiate a new one
            Debug.LogWarning("Pool exhausted, instantiating new enemy.");
            GameObject newEnemy = Instantiate(EnemyPrefab);
            // Generate a unique ID for the new enemy
            BaseEnemyScript enemyScript = newEnemy.GetComponent<BaseEnemyScript>();
            if (enemyScript != null)
            {
                enemyScript.Awake(); // Ensure Awake is called to generate a unique ID
            }
            return newEnemy;
        }
    }

    // Return an enemy to the pool
    public void ReturnEnemyToPool(GameObject enemy)
{
    enemy.SetActive(false);

    // Return to the pool using the original prefab
    foreach (var entry in EnemyPool)
    {
        if (entry.Key.name == enemy.name.Replace("(Clone)", "").Trim())
        {
            entry.Value.Enqueue(enemy);
            return;
        }
    }

    Debug.LogWarning("Enemy prefab not found in the pool.");
}

}
