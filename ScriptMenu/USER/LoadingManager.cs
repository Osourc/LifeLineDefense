using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingPanel; // Assign the loading panel in the Inspector
    public TMP_Text loadingText; // Assign this in the Inspector if you want to show loading progress
    public GameObject loginPanel; // Assign the login panel to return to it if no internet
    public float baseLoadingTime = 5f; // Base loading time in seconds

    public void LoadGame()
    {
        loadingPanel.SetActive(true); // Show the loading panel
        StartCoroutine(CheckInternetConnection());
    }
    
    private IEnumerator CheckInternetConnection()
    {
        // Use a simple ping to check for internet connection with HTTPS
        using (UnityWebRequest request = UnityWebRequest.Get("https://www.google.com"))
        {
            // Set a timeout (e.g., 2 seconds)
            request.timeout = 2;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("No Internet Connection: " + request.error);
                loadingText.text = "No Internet Connection. Please Retry.";
                yield return new WaitForSeconds(2); // Show message for a while
                loadingPanel.SetActive(false);
                loginPanel.SetActive(true); // Show the login panel again
                yield break;
            }
        }

        // If there is internet, start loading the main menu
        StartCoroutine(LoadGameAsync());
    }

    private IEnumerator LoadGameAsync()
    {
        // Set a random loading time based on the base loading time
        float loadingTime = baseLoadingTime;
        float elapsedTime = 0f;

        loadingText.text = "Loading...";

        while (elapsedTime < loadingTime)
        {
            elapsedTime += Time.deltaTime;

            // Simulate bumps in loading time
            if (Random.Range(0f, 1f) < 0.1f) // 10% chance of a bump
            {
                elapsedTime += Random.Range(0.1f, 0.5f); // Add a random delay
            }

            // Update loading text with progress
            float progress = Mathf.Clamp01(elapsedTime / loadingTime);
            loadingText.text = $"Loading... {progress * 100f:0}%";

            yield return null; // Wait until the next frame
        }

        // Once loading is done, hide the loading panel and load the main menu scene
        loadingPanel.SetActive(false);
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        // Load the "MainMenu" scene
        Debug.Log("Main menu scene loading...");
        SceneManager.LoadScene("MainMenu");
    }
}
