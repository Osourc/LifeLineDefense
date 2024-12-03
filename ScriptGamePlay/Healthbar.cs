using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    private BaseEnemyScript parentEnemy;
    private SpriteRenderer spriteRenderer;

    private float fadeDuration = 2f;
    private float fadeDelay = 2f;
    private float fadeTimer;

    void Start()
    {
        parentEnemy = GetComponentInParent<BaseEnemyScript>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (parentEnemy == null) Debug.LogError("Parent Enemy Script not found!");
    }

    void Update()
    {
        if (parentEnemy == null) return;

        float healthPercentage = Mathf.Clamp((float)parentEnemy.CurrentHealth / parentEnemy.MaxHealth, 0f, 1f);
        transform.localScale = new Vector3(healthPercentage, 0.1f, 1f);
        
        if (parentEnemy.CurrentHealth < parentEnemy.MaxHealth)
        {
            fadeTimer = 0f; // Reset fade timer when taking damage
            SetHealthbarVisibility(1f); // Fully visible
        }
        else
        {
            fadeTimer += Time.deltaTime;
            if (fadeTimer > fadeDelay)
            {
                float fadeAmount = (fadeTimer - fadeDelay) / fadeDuration;
                SetHealthbarVisibility(Mathf.Clamp(1f - fadeAmount, 0f, 1f));
            }
        }
    }

    private void SetHealthbarVisibility(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
