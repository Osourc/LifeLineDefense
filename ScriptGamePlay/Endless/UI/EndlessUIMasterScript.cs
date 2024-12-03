using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndlessUIMasterScript : MonoBehaviour
{
    public EndlessMasterScript EndlessManager;
    public PauseMenu PauseManager;
    public int Life, O2, WaveNum, WaveCountDown;
    private int Points;
    public TMP_Text LifeX, O2X, WaveNumX, WaveCountDownX, PointsX;
    public bool isPaused = false;

    //Countdown in Secs
    float CountDownTime = 60.0f;
    //Internal timer Var
    private float CurrentTime;
    // Flag to check if the countdown is active
    private bool isCountDownActive = false;
    //Start is called before the first frame update
    void Start()
    {
        WaveCountDownX.text = "";
        LifeX.text = Life.ToString("00");
        O2X.text = O2.ToString("00");
        PointsX.text = Points.ToString("00");
        CountDownTime = float.Parse("" + WaveCountDown);
        // Initialize Timer with CountDownTime
        CurrentTime = CountDownTime;
        WaveNumX.text = WaveNum.ToString("0");
        StartCountDown();
    }

    // Update is called once per frame
    void Update()
    {
        PrepCountDown();
        O2X.text = O2.ToString("00");
        LifeX.text = Life.ToString("00");
        PointsX.text = Points.ToString("00");
        Life = Mathf.Clamp(Life, 0, int.MaxValue);
        if(Life == 0)
        {
            PauseManager.ShowGameOver();
            EndlessManager.Lost(true);
        }
    }

    public void AddO2(int amount)
    {
        O2 += amount;
        O2X.text = O2.ToString("00"); // Update the UI text for O2
        Debug.Log($"O2 added: {amount}. Total O2: {O2}");
    }

    public void AddPoints(int amount)
    {
        Points += amount;
        PointsX.text = Points.ToString("00"); // Update the UI text for Points
        Debug.Log($"Points added: {amount}. Total Points: {Points}");
    }

    public void PrepCountDown()
    {
        if(isCountDownActive)
        {
            CurrentTime -= Time.deltaTime;
            CurrentTime = Mathf.Clamp(CurrentTime, 0.0f, CountDownTime);
            UpdateCountdownText();

            if(CurrentTime <= 0.0f)
            {
                StopCountDown();
                OnCountDownComplete();
            }
        }
    }

    public void SkipPrep()
    {
        CurrentTime = 0;
    }
    public void StartCountDown()
    {
        isCountDownActive = true;
    }
    public void StopCountDown()
    {
        isCountDownActive = false;
    }
    void ResetCountdown()
    {
        CurrentTime = CountDownTime;
    }

    void UpdateCountdownText()
    {
        int minutes = Mathf.FloorToInt(CurrentTime / 60);
        int seconds = Mathf.FloorToInt(CurrentTime % 60);
        WaveCountDownX.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    void OnCountDownComplete()
    {
        Debug.Log($"Countdown Complete!");
        // Plan and spawn the next wave using the current WaveNum
        EndlessManager.PlanSpawn(WaveNum);
        EndlessManager.SpawnPathogens(WaveNum);
        WaveNum++; // Move to the next wave
        WaveNumX.text = WaveNum.ToString("0");
        ResetCountdown();
        StartCountDown();
    }
}