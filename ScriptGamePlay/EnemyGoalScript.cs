using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGoalScript : MonoBehaviour
{
    public MonoBehaviour uiManager;
    public GameObject PauseMenu, Gameover, Default;
    public bool isEndlessMode;

    private void Start()
    {
        FindActiveUIManager();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) // Enemy Unit Layer
        {
            BaseEnemyScript enemyScript = other.GetComponent<BaseEnemyScript>();
            if (enemyScript != null)
            {
                // Call method to return the enemy to the pool
                StartCoroutine(enemyScript.ReturnToPoolAfterDeath());
            }

            var soloUIManager = uiManager as SoloUIMasterScript;
            var endlessUIManager = uiManager as EndlessUIMasterScript;
            var multiUIManager = uiManager as MultiUIMasterScript;

            // Decrease life count based on the active game mode
            if (soloUIManager != null)
            {
                soloUIManager.Life--;
                soloUIManager.LifeX.text = soloUIManager.Life.ToString("00");

                if (soloUIManager.Life <= 0)
                {
                    TriggerGameOver();
                }
            }
            if (endlessUIManager != null)
            {
                endlessUIManager.Life--;
                endlessUIManager.LifeX.text = endlessUIManager.Life.ToString("00");

                if (endlessUIManager.Life <= 0)
                {
                    TriggerGameOver();
                }
            }
            if (multiUIManager != null)
            {
                multiUIManager.Life--;
                multiUIManager.LifeX.text = multiUIManager.Life.ToString("00");

                if (multiUIManager.Life <= 0)
                {
                    TriggerGameOver();
                }
            }
            else
            {
                Debug.LogWarning("No active UI script found in Enemy Goal");
            }
        }
    }

        private MonoBehaviour FindActiveUIManager()
    {
        // Try to find UI managers and return the active one
        SoloUIMasterScript soloUI = FindObjectOfType<SoloUIMasterScript>();
        EndlessUIMasterScript endlessUI = FindObjectOfType<EndlessUIMasterScript>();
        MultiUIMasterScript multiUI = FindObjectOfType<MultiUIMasterScript>();

        // Check which one is active and return it
        if (soloUI != null && soloUI.gameObject.activeInHierarchy)
        {
            return soloUI;
        }
        else if (endlessUI != null && endlessUI.gameObject.activeInHierarchy)
        {
            return endlessUI;
        }
        else if (multiUI != null && multiUI.gameObject.activeInHierarchy)
        {
            return multiUI;
        }
        else
        {
            Debug.LogWarning("No active UI manager found in the scene.");
            return null;
        }
    }

    private void TriggerGameOver()
    {
        PauseMenu.SetActive(true);
        Default.SetActive(false);
        Gameover.SetActive(true);
    }
}
