using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class UserLayer : MonoBehaviour
{
    [Header("Panels to Manage")]
    public GameObject[] panels; // Array to store panel GameObjects

    public GameObject quitpanel;   // Reference to the quit confirmation panel


    // Open a panel by index with animation
    public void OpenPanel(int index)
    {
        if (IsValidPanelIndex(index))
        {
            panels[index].SetActive(true);  // Ensure the panel is active before animating
            panels[index].transform.localScale = Vector2.zero;  // Set initial scale to zero for scaling animation
            panels[index].transform.LeanScale(Vector2.one, 0.8f).setEaseOutBack();  // Animate to full size
        }
        else
        {
            Debug.LogError("Invalid panel index: " + index);
        }
    }

    // Close a panel by index with animation
    public void ClosePanel(int index)
    {
        if (IsValidPanelIndex(index))
        {
            panels[index].transform.LeanScale(Vector2.zero, 1f).setEaseInBack().setOnComplete(() =>
            {
                panels[index].SetActive(false);  // Only deactivate panel after the animation is complete
            });
        }
        else
        {
            Debug.LogError("Invalid panel index: " + index);
        }
    }

    // Enhanced Logout function with animation
    public async void OnLogout()
    {
        // Wait until PlayerId is available
        while (string.IsNullOrEmpty(AuthenticationManager.Instance.PlayerId))
        {
            await Task.Yield();  // Yield to allow Unity to update while waiting
        }

        // Show and animate quit confirmation panel
        quitpanel.SetActive(true);
        quitpanel.transform.localScale = Vector2.zero;  // Start with scale 0
        quitpanel.transform.LeanScale(Vector2.one, 0.8f).setEaseOutBack();  // Animate to full size
    }

    // Confirm logout and proceed to save player data, close all panels, and show login panel or quit
    public void OnYes()
    {
        Debug.Log("Logout confirmed");

        // Clear PlayerId after logout confirmation
        // AuthenticationManager.Instance.SetPlayerId(null);  // Set to null to clear PlayerId


        // Close all active panels with no animation
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);  // Ensure all panels are hidden
        }

        // Hide the quit panel
        quitpanel.SetActive(false);

      
        // Uncomment to quit the application or stop the Unity editor
        Debug.Log("Application Quit initiated");
        Application.Quit();  // This will work in a built version of the game

        // For testing in the Unity Editor
    #if UNITY_EDITOR
            Debug.Log("Unity Editor: Stopping play mode");
            UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }

    // Close the quit confirmation panel if the player cancels logout
    public void OnNo()
    {
        Debug.Log("Logout cancelled");

        // Animate and close the quit confirmation panel
        quitpanel.transform.LeanScale(Vector2.zero, 0.8f).setEaseInBack().setOnComplete(() =>
        {
            quitpanel.SetActive(false);  // Deactivate after animation completes
        });
    }

    // Helper function to check if the panel index is valid
    private bool IsValidPanelIndex(int index)
    {
        return index >= 0 && index < panels.Length;
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    
}
