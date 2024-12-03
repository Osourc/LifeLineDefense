using UnityEngine;
using UnityEngine.SceneManagement;


public class ModeSelectorScript : MonoBehaviour
{
    public GameObject ModePanel, LevelSelect, Endless;

    public void ShowLevelSelect()
    {
        LevelSelect.SetActive(true);
        ModePanel.SetActive(false);
    }

    public void ShowEndless()
    {
        Endless.SetActive(true);
        ModePanel.SetActive(false);
    }

    public void BacktoModePanel()
    {
        ModePanel.SetActive(true);
        Endless.SetActive(false);
        LevelSelect.SetActive(false);
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    private SwitchScene switchScene;

    // Method to switch the scene or update UI
    public void Switcher()
    {
        // Find the SwitchScene component in the scene
        switchScene = FindObjectOfType<SwitchScene>();

        // Check if SwitchScene is found
        if (switchScene != null)
        {
            // Call the RememberScene method from SwitchScene
            switchScene.RememberScene();
        }
        else
        {
            Debug.LogError("SwitchScene not found in the scene.");
        }
    }
  
}
