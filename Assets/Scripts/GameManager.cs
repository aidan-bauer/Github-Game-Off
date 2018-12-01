using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public GameObject finishedCanvas;
    public GameObject playerCanvas, minimap;
    public Text timerText, finalText;
    

    float startTime, stopTime, totalRunTime;
    float timer = 0f;
    bool isTiming;

    private void Start()
    {
        StartTimer();
    }

    private void FixedUpdate()
    {
        if (isTiming && !PauseManager.IsPaused)
        {
            timer += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    public void StartTimer()
    {
        isTiming = true;
    }

    public void StopTimer()
    {
        isTiming = false;
    }

    public void ResetTimer()
    {
        isTiming = false;
        timer = 0;
    }

    public void UpdateTimerUI()
    {
        //update the timer ui every fixedupdate
        timerText.text = finalText.text = FormatTime();
    }

    public void Finish()
    {
        StopTimer();
        finishedCanvas.SetActive(true);
        playerCanvas.SetActive(false);
        minimap.SetActive(false);
    }

    string FormatTime()
    {
        return (Mathf.Floor(timer / 60f).ToString("N0") + ":" + (timer % 60f).ToString("N2"));
    }
}
