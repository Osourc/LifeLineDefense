using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MultiPauseMenu : MonoBehaviourPun
{
    public GameObject Pause;
    // Start is called before the first frame update
    private void Awake()
    {
    }
    public GameObject Default, Forfeit, GameOver, Victory;
    public MultiMasterScript masterScript;
    public MultiUIMasterScript multiUIMaster;

    public void ShowDefault()
    {
        Default.SetActive(true);
        Forfeit.SetActive(false);
    }

    public void ShowForfeit()
    {
        Default.SetActive(false);
        Forfeit.SetActive(true);
    }

    public void ForfeitG()
    {
        Forfeit.SetActive(false);
        masterScript.Forfeit();
    }

    public void ShowPause()
    {
        Pause.SetActive(true);
    }

    public void HidePause()
    {
        Pause.SetActive(false);
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        ShowPause();
        Default.SetActive(false);
        GameOver.SetActive(true);
    }

    public void ShowVictory()
    {
        Time.timeScale = 0f;
        ShowPause();
        Default.SetActive(false);
        Victory.SetActive(true);
    }

    public void Next()
    {
        Time.timeScale = 1f;
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonConnection.Instance.ManuallyDisconnected = true;
            Debug.LogWarning("Leaving Current Room!");
            PhotonNetwork.LeaveRoom();
            // Start the coroutine to wait until the player leaves the room
            StartCoroutine(WaitForRoomToLeave());
        }
        else
        {
            LoadMultiplayerScene();  // Load level immediately if not in a room
        }
    }

    private IEnumerator WaitForRoomToLeave()
    {
        // Wait until PhotonNetwork is no longer in the room
        while (PhotonNetwork.InRoom)
        {
            yield return null;  // Wait for the next frame
        }
        yield return new WaitForSeconds(0.5f);
        LoadMultiplayerScene();
    }
    
    private void LoadMultiplayerScene()
    {
        PhotonNetwork.LoadLevel("Multiplayer");  // Load the Multiplayer scene directly
        PhotonNetwork.AutomaticallySyncScene = true;
    }
}