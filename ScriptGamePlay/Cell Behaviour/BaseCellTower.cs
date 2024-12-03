using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCellTower : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    [SerializeField]
    protected float attackInterval = .3f;
    public float damage = 10f;
    public CircleCollider2D AttackRangeCollider;
    private float lastAttackTime;
    private bool isFacingLeft = false;
    public float positionOffset = 0.28f;

    private List<Collider2D> enemiesInRange = new List<Collider2D>();
    private Animator animator;
    private ProjectilePoolScript ProjectilePool;
    private Transform ProjectileSpawnPoint;

    protected virtual void Awake()
    {
        lastAttackTime = Time.time;
        ProjectilePool = FindObjectOfType<ProjectilePoolScript>();
        animator = GetComponentInChildren<Animator>();

        if(ProjectilePool == null)
        {
            Debug.LogError("ProjectilePoolScript is not found in " + gameObject.name);
        }
        else
        {
            Debug.Log("ProjectilePoolScript is found in " + gameObject.name);
        }

        AttackRangeCollider = GetComponentInChildren<CircleCollider2D>();
        if (AttackRangeCollider == null)
        {
            Debug.LogWarning("AttackRangeCollider is not found in " + gameObject.name);
        }
        else
        {
            AttackRangeCollider.isTrigger = true;
        }

        ProjectileSpawnPoint = transform.Find("ProjectileOrigin");
        if (ProjectileSpawnPoint == null)
        {
            Debug.LogError("Projectile origin point is not found in " + gameObject.name);
        }

        if (animator == null)
        {
            Debug.LogError("Animator not found in children of " + gameObject.name);
        }
        else
        {
            Debug.Log("Animator found in children of " + gameObject.name);
        }
        if(ProjectilePrefab == null)
        {
            Debug.LogWarning("Projectile Perfab not set");
        }
    }

    protected virtual void Update()
    {
        if (Time.time >= lastAttackTime + attackInterval)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    protected virtual void Attack()
    {
        // Target the closest enemy
        Collider2D closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D enemy in enemiesInRange)
        {
            // Ensure the enemy is on the correct layer
            if (enemy.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                // Access the enemy script to check health
                BaseEnemyScript enemyScript = enemy.GetComponent<BaseEnemyScript>();
                if (enemyScript != null && enemyScript.CurrentHealth > 0)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
            }
        }
        if (closestEnemy != null)
        {
            animator.SetBool("isAttacking", true);
            FlipTower(closestEnemy.transform.position);
            FireProjectile(closestEnemy.gameObject);
        }
        else
        {
            if (animator.GetBool("isAttacking"))
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    StartCoroutine(WaitForAttackAnimation(stateInfo.length));
                    animator.SetBool("isIdle", true);
                    animator.SetBool("isAttacking", false);
            }
        }
    }

    private IEnumerator WaitForAttackAnimation(float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isIdle", true);
    }

    protected virtual void FireProjectile(GameObject targetEnemy)
    {
        GameObject projectile = ProjectilePool.GetProjectileFromPool(ProjectilePrefab);

        if (projectile != null)
        {
            projectile.transform.position = ProjectileSpawnPoint.position;
            projectile.SetActive(true);
            projectile.transform.localScale = isFacingLeft ? new Vector3(-1f, 1f, 1f) : new Vector3(1f, 1f, 1f);

            var projScript = projectile.GetComponent<ProjectileScript>();
            if (projScript != null)
            {
                projScript.SetTarget(targetEnemy);
                projScript.SetFacingDirection(isFacingLeft);
                projScript.Damage = damage;
            }
        }
        else
        {
            Debug.LogError("Projectile Prefab not found!");
        }
    }

    protected virtual void FlipTower(Vector3 enemyPosition)
    {
        Vector3 direction = enemyPosition - transform.position; 


        if (direction.x < 0)
        {
            if (!isFacingLeft)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
                isFacingLeft = true;
                Vector3 currentPosition = transform.position;
                currentPosition.x -= positionOffset;
                transform.position = currentPosition;
            }
        }
        else
        {
            if (isFacingLeft)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
                isFacingLeft = false;

                Vector3 currentPosition = transform.position;
                currentPosition.x += positionOffset;
                transform.position = currentPosition;
            }
        }
    }

        protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (!enemiesInRange.Contains(other))
            {
                enemiesInRange.Add(other);
            }
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (enemiesInRange.Contains(other))
            {
                enemiesInRange.Remove(other);
            }
        }
    }




    public virtual void AdjustAttackSpeed(float multiplier)
    {
        attackInterval *= multiplier;
    }
}