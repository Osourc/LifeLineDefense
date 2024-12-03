using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectileData
{
    public GameObject ProjectilePrefab;
    public int SpawnCount = 10;
}

public class ProjectilePoolScript : MonoBehaviour
{
    public List<ProjectileData> ProjectileDataList;
    private Dictionary<GameObject, Queue<GameObject>> ProjectilePool;

    private void Awake()
    {
        // Initialize the projectile pool
        ProjectilePool = new Dictionary<GameObject, Queue<GameObject>>();

        foreach (var data in ProjectileDataList)
        {
            Queue<GameObject> poolQueue = new Queue<GameObject>();

            // Preload projectiles into the pool based on SpawnCount
            for (int i = 0; i < data.SpawnCount; i++)
            {
                GameObject projectile = Instantiate(data.ProjectilePrefab, transform);
                projectile.SetActive(false);
                poolQueue.Enqueue(projectile);
            }

            ProjectilePool[data.ProjectilePrefab] = poolQueue;
        }
    }

    public GameObject GetProjectileFromPool(GameObject projectilePrefab)
    {
        if (ProjectilePool.ContainsKey(projectilePrefab) && ProjectilePool[projectilePrefab].Count > 0)
        {
            GameObject projectile = ProjectilePool[projectilePrefab].Dequeue();
            projectile.transform.SetParent(transform);
            projectile.SetActive(true);
            return projectile;
        }
        else
        {
            Debug.LogWarning("Projectile pool exhausted, instantiating new projectile.");
            GameObject projectile = Instantiate(projectilePrefab, transform);
            return projectile;
        }
    }

    public void ReturnProjectileToPool(GameObject projectile)
    {
        projectile.SetActive(false);

        // Return to the pool using the original prefab
        foreach (var entry in ProjectilePool)
        {
            if (entry.Key.name == projectile.name.Replace("(Clone)", "").Trim())
            {
                entry.Value.Enqueue(projectile);
                return;
            }
        }

        Debug.LogWarning("Projectile prefab not found in the pool.");
    }
}
