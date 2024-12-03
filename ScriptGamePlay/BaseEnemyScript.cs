using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemyScript : MonoBehaviour
{
    public float Speed = 5f;
    public float MaxHealth = 100f;
    public int Resource = 5;
    public int DNAPoints = 1;
    [HideInInspector] public float CurrentHealth;
    [HideInInspector] public Transform[] Waypoints;
    [HideInInspector] public int CurrentWaypointIndex = 0;
    private EnemyPoolScript EnemyPool;
    private MonoBehaviour activeMasterScript;
    private MonoBehaviour activeUIMasterScript;
    public string UniqueID { get; private set; }

    public Animator Animator;

    public void Awake()
    {
        // Generate a unique ID for this instance
        UniqueID = System.Guid.NewGuid().ToString();
        CurrentHealth = MaxHealth;
        if (Animator == null)
        {
            Animator = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(50);
        }
        if (Waypoints != null && Waypoints.Length > 0)
        {
            MoveTowardsWaypoint();
        }
    }

    // Set waypoints from the master script
    public void Initialize(Transform[] waypoints, EnemyPoolScript enemyPool, MonoBehaviour master, MonoBehaviour uiMaster)
    {
        Waypoints = waypoints;
        EnemyPool = enemyPool;
        activeMasterScript = master;
        activeUIMasterScript = uiMaster;
        CurrentWaypointIndex = 0;
    }

    // Move the enemy towards the next waypoint
    private void MoveTowardsWaypoint()
    {
        if (CurrentHealth <= 0 || CurrentWaypointIndex >= Waypoints.Length)
            return;

        Transform targetWaypoint = Waypoints[CurrentWaypointIndex];
        float step = Speed * Time.deltaTime;

        // Move the object towards the waypoint
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, step);

        // Check if we reached the waypoint
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            transform.position = targetWaypoint.position;
            CurrentWaypointIndex++;
        }

        Animator.SetBool("isWalking", CurrentWaypointIndex < Waypoints.Length);
    }


    // Take damage and return to pool if health is 0 or below
    public void TakeDamage(float damageAmount)
    {
        if (activeUIMasterScript == null)
        {
            Debug.LogError("uiMasterScript is not initialized!");
            return;
        }
        CurrentHealth = Mathf.Max(CurrentHealth - damageAmount, 0);

        if (CurrentHealth <= 0)
        {
            Debug.Log($"{UniqueID} is dying.");

            if (activeUIMasterScript is SoloUIMasterScript soloUIMaster)
            {
                soloUIMaster.AddO2(Resource);
            }
            else if (activeUIMasterScript is EndlessUIMasterScript endlessUIMaster)
            {
                endlessUIMaster.AddO2(Resource);
            }
            else if (activeUIMasterScript is MultiUIMasterScript multiUIMaster)
            {
                multiUIMaster.AddO2(Resource);
                multiUIMaster.AddPoints(Resource);
            }

            StartCoroutine(ReturnToPoolAfterDeath());
        }
    }


    public IEnumerator ReturnToPoolAfterDeath()
    {
        // Get the length of the currently playing "Die" animation
        Animator.SetBool("isDead", true);
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        CurrentHealth = MaxHealth;
        CurrentWaypointIndex = 0;
        string key = gameObject.name.Replace("(Clone)", "").Trim();

        if (activeMasterScript is SoloMasterScript soloMaster)
        {
            soloMaster.RemovePathogen(gameObject);
        }
        else if (activeMasterScript is EndlessMasterScript endlessMaster)
        {
            endlessMaster.RemovePathogen(gameObject);
        }
        else if (activeMasterScript is MultiMasterScript multiMaster)
        {
            multiMaster.RemovePathogen(gameObject);
        }

        EnemyPool.ReturnEnemyToPool(gameObject);
        Animator.SetBool("isDead", false);
    }
}
