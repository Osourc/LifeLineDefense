using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MultiEnemyGoalScript : MonoBehaviourPun
{
    public MultiMasterScript masterScript;
    public MultiUIMasterScript multiUIMaster;
    public MultiPauseMenu multiPauseMenu;
    public GameObject PauseMenu, Gameover, Default;
    public bool isEndlessMode;
    public bool currentPlayerLost = false;

    private void Start()
    {
        if(multiUIMaster == null)
        {
            multiUIMaster = FindAnyObjectByType<MultiUIMasterScript>();
            Debug.Log("Multiplayer UI found!");
        }
    }

    private void Update()
    {
        if (multiUIMaster.Life <= 0)
        {
            photonView.RPC("TriggerGameOver", RpcTarget.AllBuffered);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            BaseEnemyScript enemyScript = other.GetComponent<BaseEnemyScript>();
            if (enemyScript != null)
            {
                StartCoroutine(enemyScript.ReturnToPoolAfterDeath());
            }

            if (multiUIMaster != null)
            {
                multiUIMaster.Life--;
                multiUIMaster.LifeX.text = multiUIMaster.Life.ToString("00");
            }
        }
    }

    [PunRPC]
    void TriggerGameOver()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentPlayerLost = multiUIMaster.Life <= 0;
            masterScript.isLose = currentPlayerLost;
            masterScript.isWon = !currentPlayerLost;

            // Set the flags for both MasterClient and Others
            photonView.RPC("SetFlags", RpcTarget.MasterClient, masterScript.isLose, masterScript.isWon);
            photonView.RPC("SetFlags", RpcTarget.Others, masterScript.isLose, masterScript.isWon);

            // Handle Game Over or Victory
            photonView.RPC(currentPlayerLost ? "HandleGameOver" : "HandleVictory", RpcTarget.MasterClient);
            photonView.RPC(currentPlayerLost ? "HandleVictory" : "HandleGameOver", RpcTarget.Others);
        }
    }

    [PunRPC]
    void SetFlags(bool lose, bool won)
    {
        masterScript.isLose = lose;
        masterScript.isWon = won;
    }

    private bool victoryHandled = false;
    [PunRPC]
    void HandleVictory()
    {
        if (victoryHandled) return;
        victoryHandled = true;
        multiPauseMenu.ShowVictory();
        multiUIMaster.ConvertPointsToDNA(10);
    }

    [PunRPC]
    void HandleGameOver()
    {
        multiPauseMenu.ShowGameOver();
    }
}
