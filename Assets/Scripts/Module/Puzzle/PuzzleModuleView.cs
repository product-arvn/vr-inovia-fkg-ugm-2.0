using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleModuleView : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private GameObject creatorPanel;
    [SerializeField] private TMP_Text creatorText;
    [SerializeField] private TMP_Text themeText;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TMP_Text timerText;
    
    [Header("Content")]
    [SerializeField] private GameObject contentPanel;

    
    [Header("Done")]
    [SerializeField] private GameObject donePanel;
    [SerializeField] private TMP_Text scoreText;
    
    //Timer
    private readonly TimerManager _timerManager = new TimerManager();
    
    public void SetTimerText(float t)
    {
        timerText.text = _timerManager.FormatTime(t);
    }
    
    public void SetCreatorText(string text)
    {
        creatorText.text = text;
    }
    
    public void SetThemeText(string text)
    {
        themeText.text = text;
    }
    
    public void ShowDonePanel(int score)
    {
        timerPanel.SetActive(false);
        creatorPanel.SetActive(false);
        contentPanel.SetActive(false);
        donePanel.SetActive(true);
        scoreText.text = "SCORE = " + score;
    }
    
}
