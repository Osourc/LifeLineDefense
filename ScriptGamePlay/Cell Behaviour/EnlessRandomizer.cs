using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnlessRandomizer : MonoBehaviour
{
    [SerializeField] private List<string> SceneNames;

    public void LoadRandomScene()
    {
        if (SceneNames.Count == 0)
        {
            Debug.LogWarning("No scenes found on the list!");
            return;
        }

        int randomIndex = Random.Range(0, SceneNames.Count);
        string selectedScene = SceneNames[randomIndex];
        Debug.Log("Loading " + selectedScene);
        SceneManager.LoadScene(selectedScene);
    }
}
