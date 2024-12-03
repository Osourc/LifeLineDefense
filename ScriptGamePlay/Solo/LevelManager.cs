using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // Singleton instance
    public Button[] levelButtons; // List of level buttons
    private int highestUnlockedLevel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadProgress()
    {
        // Load the highest unlocked level; default to 0 if not saved
        highestUnlockedLevel = PlayerPrefs.GetInt("HighestUnlockedLevel", 0);
        
        // Set interactable state for buttons up to the saved level
        for (int i = 0; i <= highestUnlockedLevel && i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = true;
        }
    }

    public void UnlockNextLevel()
    {
        // Unlock the next level if within range
        if (highestUnlockedLevel < levelButtons.Length - 1)
        {
            highestUnlockedLevel++;
            levelButtons[highestUnlockedLevel].interactable = true;

            // Save the new progress
            PlayerPrefs.SetInt("HighestUnlockedLevel", highestUnlockedLevel);
            PlayerPrefs.Save(); // Ensure data is saved to disk
        }
    }
}
