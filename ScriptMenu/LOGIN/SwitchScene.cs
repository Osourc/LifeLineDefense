using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Add this for scene management

public class SwitchScene : MonoBehaviour
{
    public static SwitchScene Instance; // Singleton reference

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject loginPanel;
    private string _playerId;
    public string PlayerId => _playerId;

    // Awake is called before Start
    void Awake()
    {
        // Ensure only one instance of SwitchScene exists
        if (Instance == null)
        {
            Instance = this; // Assign this as the instance
            DontDestroyOnLoad(gameObject); // Keep this object between scene loads
            Debug.Log("SwitchScene initialized and will not be destroyed.");
        }
        else
        {
            Debug.Log("Duplicate SwitchScene instance destroyed.");
            Destroy(gameObject); // Destroy any additional instances to enforce singleton pattern
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Start the coroutine to wait for the PlayerId and then remember the scene
        StartCoroutine(WaitForIdAndFetchData());
    }

    // Coroutine to wait for PlayerId to be retrieved
    private IEnumerator WaitForIdAndFetchData()
    {
        // Wait until PlayerId is available
        while (string.IsNullOrEmpty(AuthenticationManager.Instance.PlayerId))
        {
            yield return null;
        }

        _playerId = AuthenticationManager.Instance.PlayerId;
        Debug.Log("Player ID retrieved: " + _playerId);

        // After fetching Player ID, update the scene accordingly
        RememberScene();
    }

    // Remember the scene and show the appropriate UI panels based on the login status
    public void RememberScene()
    {
        // Debugging message
        Debug.Log("RememberScene called. Checking panels.");

        // Check if the panels are not destroyed before accessing them
        if (mainMenuPanel == null || loginPanel == null)
        {
            Debug.LogWarning("Main menu or login panel is null.");
            // Try re-assigning panels after scene load
            ReassignPanels();
        }
        else
        {
            Debug.Log("Main menu and login panel are available.");

            // Check if the player is already logged in
            if (!string.IsNullOrEmpty(PlayerId))
            {
                // Show the main menu if logged in
                loginPanel.SetActive(false);
                mainMenuPanel.SetActive(true);
            }
            else
            {
                // Show the login panel if not logged in
                loginPanel.SetActive(true);
                mainMenuPanel.SetActive(false);
            }
        }
    }

    // Method to attempt re-assigning the panels
    private void ReassignPanels()
    {
        // Reassign panels by looking for them in the scene
        mainMenuPanel = GameObject.Find("MainMenuPanel");
        loginPanel = GameObject.Find("LoginPanel");

        Debug.Log("Re-assigned panels: MainMenuPanel = " + mainMenuPanel + ", LoginPanel = " + loginPanel);

        // If they aren't found, instantiate them (optional, depending on your needs)
        if (mainMenuPanel == null)
        {
            mainMenuPanel = Instantiate(Resources.Load("MainMenuPanelPrefab") as GameObject);
        }

        if (loginPanel == null)
        {
            loginPanel = Instantiate(Resources.Load("LoginPanelPrefab") as GameObject);
        }
    }

    // Optional: Ensure the panels are re-assigned correctly after scene load
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reassign the panels after scene load
        ReassignPanels();
        RememberScene();
    }

    private void OnEnable()
    {
        // Register to scene load event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unregister from scene load event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
