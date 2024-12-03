using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelectScript : MonoBehaviour
{
    public int Level;
    public TMP_Text LevelText;
    private string ChapterName;
    private Button levelButton;
    // Start is called before the first frame update
    void Start()
    {
        LevelText = GetComponentInChildren<TMP_Text>();
        levelButton = GetComponent<Button>();
        if (LevelText == null)
        {
            Debug.LogError("TMP_Text component not found on the button's children.");
            return;
        }

        ChapterName = transform.parent.gameObject.name;
        LevelText.text = Level.ToString();
        Debug.Log("Chapter Name: " + ChapterName + "Level Name: " + Level);

        if (PlayerPrefs.GetInt("Level1", 0) != 1)
        {
            PlayerPrefs.SetInt("Level1", 1);
            PlayerPrefs.Save();
        }
        //LockLevelButton();***************************************************************
    }

    /*private void LockLevelButton()
    {
        bool isLevelUnlocked = PlayerPrefs.GetInt("Level" + Level, 0) == 1;  // Check if this level is unlocked**************************************************************
        levelButton.interactable = isLevelUnlocked;
        if (!isLevelUnlocked)
        {
            levelButton.GetComponentInChildren<TMP_Text>().text = "Locked";
            levelButton.GetComponentInChildren<TMP_Text>().fontSize = 24;
        }
    }*/

    public void OpenScene()
    {
        string sceneName = ChapterName + "Level" + Level.ToString();
        Debug.Log("Constructed scene name: " + sceneName);

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log("Scene found, loading: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene not found in build settings: " + sceneName);
        }
    }
}