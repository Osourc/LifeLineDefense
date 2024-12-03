using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileScript : MonoBehaviour
{
    protected Transform target;
    protected float Speed = 25f;
    [HideInInspector]
    private float damage = 0f;
    public float Damage
    {
        get => damage;
        set => damage = value;
    }

    protected bool isTowerFacingLeft;
    protected Vector3 targetPosition;
    protected ProjectilePoolScript ProjectilePool;
    //public string UniqueID {get; private set;}

    protected virtual void Awake()
    {
        //UniqueID = System.Guid.NewGuid().ToString();
        ProjectilePool = FindObjectOfType<ProjectilePoolScript>();
        if (ProjectilePool == null)
        {
            Debug.LogWarning("Projectile Pool Script not found on " + gameObject.name);
        }
        
    }

    // Set target using GameObject
    public void SetTarget(GameObject targetObject)
    {
        if (targetObject != null)
        {
            target = targetObject.transform;
            targetPosition = target.position;
        }
        else
        {
            Debug.LogWarning("Target GameObject is null!");
            target = null;
        }
    }

    protected virtual void Update()
    {
        if (target != null)
        {
            MoveTowardsTarget();
        }


    }

    public void SetFacingDirection(bool towerFacingLeft)
    {
        isTowerFacingLeft = towerFacingLeft;
    }

    protected virtual void MoveTowardsTarget()
{
    if (target != null)
    {
        // Move the projectile towards the target position and set its rotaiton to face
        transform.position = Vector3.MoveTowards(transform.position, target.position, Speed * Time.deltaTime);
        Vector3 direction = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Adjust the rotation based on the facing direction
        if (isTowerFacingLeft)
        {
            angle += 180; // Adjust for left-facing
        }
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
    else
    {
        Debug.LogError("No Target Detected!");
    }
}


    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            BaseEnemyScript enemyScript = other.GetComponent<BaseEnemyScript>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(Damage);
            }
            else
            {
                Debug.LogWarning("Enemy Script not found on collider!");
            }

            ProjectilePool.ReturnProjectileToPool(gameObject);
        }
    }
}
