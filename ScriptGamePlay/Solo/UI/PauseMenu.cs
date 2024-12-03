using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject Pause;
    // Start is called before the first frame update
    void Start()
    {
    }

    public GameObject Default,Quit,Restart, GameOver, StageClear;
    public static bool isPaused;
    public MonoBehaviour MasterScript;
    public MonoBehaviour UIMaster;

    public void ShowDefault()
    {
        Default.SetActive(true);
        Restart.SetActive(false);
        Quit.SetActive(false);
    }

    public void ShowRestart()
    {
        Default.SetActive(false);
        Restart.SetActive(true);
    }
    public void RestartG()
    { 
        Time.timeScale = 1f;
        Pause.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Load the current scene
    }

    public void ShowQuit()
    {
        Default.SetActive(false);
        Quit.SetActive(true);
    }
    public void QuitG()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelectScene");
        //Replace "LobbyScene" with the name of the target scene you want eg:ChapterSelect
    }

    public void ShowPause()
    {
        isPaused = (true);
        Pause.SetActive(true);
        Time.timeScale = 0f;
    }
    public void HidePause()
    {
        isPaused = (false);
        Pause.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ShowStageComplete()
    {
        ShowPause();
        Default.SetActive(false);
        StageClear.SetActive(true);
    }

    public void EndGame(bool hasWon)
    {
        if(hasWon)
        {
            ShowStageComplete();
        }
        else
        {
            Debug.Log("Hasn't won yet");
        }
    }

    public void ShowGameOver()
    {
        ShowPause();
        Default.SetActive(false);
        GameOver.SetActive(true);
    }
}
