using UnityEngine;
using UnityEngine.SceneManagement;
 

public class RoomUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private GameObject joinRoomPanel;



    private void Start()
    {
        // Ensure panels are closed initially
        createRoomPanel.SetActive(false);
        joinRoomPanel.SetActive(false);
    }

    // Open Create Room Panel
    public void OpenCreateRoomPanel()
    {
        createRoomPanel.SetActive(true);
    }

    // Close Create Room Panel
    public void CloseCreateRoomPanel()
    {
        createRoomPanel.SetActive(false);
    }

    // Open Join Room Panel
    public void OpenJoinRoomPanel()
    {
        joinRoomPanel.SetActive(true);
    }

    // Close Join Room Panel
    public void CloseJoinRoomPanel()
    {
        joinRoomPanel.SetActive(false);
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
