using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacrophageProjectileScript : ProjectileScript
{
    private CapsuleCollider2D capsuleCollider;
    private Animator animator;
    private bool isTargetPositionSet = false;

    protected float speed = 40f;
    private float damageInterval = 1f; // How often damage is applied (in seconds)
    private float timeSinceLastDamage = 0f; // Timer to track time since last damage
    private float damageDurationElapsed = 0f;
    private float damageDuration = 3f;

    protected override void Awake()
    {
        base.Awake();
        if(animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if(capsuleCollider == null)
        {
            capsuleCollider = GetComponent<CapsuleCollider2D>();
        }
    }

    protected override void MoveTowardsTarget()
    {
        if(!isTargetPositionSet && target != null)
        {
            targetPosition = target.position;
            isTargetPositionSet = true;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
        Vector3 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (isTowerFacingLeft)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Adjust for left-facing
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // Check if the projectile has reached the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            OnReachTargetPosition();
        }
    }

    private void OnReachTargetPosition()
    {
        Debug.Log("Projectile reached the target position!");
        animator.SetBool("onLand", true);
        capsuleCollider.enabled = true;

        damageDurationElapsed += Time.deltaTime;
        if (damageDurationElapsed >= damageDuration)
        {
                
            ProjectilePool.ReturnProjectileToPool(gameObject);
            damageDurationElapsed = 0f;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other){}

    private void OnTriggerStay2D(Collider2D other) 
    {
        BaseEnemyScript enemyScript = other.GetComponent<BaseEnemyScript>();
        if (enemyScript != null)
        {
            // Apply damage over time while the projectile is in contact with the enemy
            timeSinceLastDamage += Time.deltaTime;

            // If enough time has passed, apply damage
            if (timeSinceLastDamage >= damageInterval)
            {
                enemyScript.TakeDamage(Damage); // Apply the damage
                timeSinceLastDamage = 0f; // Reset the damage timer
            }
        }
    }

}