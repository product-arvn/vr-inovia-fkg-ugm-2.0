using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerText : MonoBehaviour
{
    public GlobalTimer globalTimer;
    public TMP_Text timerText;
    
    private readonly TimerManager _timerManager = new TimerManager();
    
    private void Update()
    {
        timerText.text = _timerManager.FormatTime(globalTimer.Timer);
    }
}

public class TimerManager
{
    public string FormatTime(float t)
    {
        int minutes = (int)t / 60;
        int seconds = (int)t - 60 * minutes;
        return $"{minutes:00}:{seconds:00}";
    }
}
