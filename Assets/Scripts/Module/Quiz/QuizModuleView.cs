using System;
using System.Collections;
using System.Collections.Generic;
using Quiz;
using TMPro;
using UnityEngine;

public class QuizModuleView : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject creatorPanel;
    [SerializeField] private TMP_Text creatorText;
    
    [Header("Result Answer")]
    [SerializeField] private GameObject correctAnswerPanel;
    [SerializeField] private TMP_Text correctAnswerExplainText;
    [SerializeField] private GameObject wrongAnswerPanel;
    [SerializeField] private TMP_Text wrongAnswerExplainText;
    
    [Header("Done Panel")]
    [SerializeField] private GameObject donePanel;
    [SerializeField] private TMP_Text scoreText;

    public QuizPanelView panelView;
    
    private readonly TimerManager _timerManager = new TimerManager();

    private GlobalTimer _globalTimer;

    private void Start()
    {
        _globalTimer = FindObjectOfType<GlobalTimer>();
        SetTimerText(_globalTimer.Timer);
    }

    public void SetTimerText(float t)
    {
        timerText.text = _timerManager.FormatTime(t);
    }
    
    public void SetCreatorText(string text)
    {
        creatorText.text = text;
    }
    
    public void ShowResultAnswer(bool isCorrect)
    {
        correctAnswerPanel.SetActive(isCorrect);
        wrongAnswerPanel.SetActive(!isCorrect);
    }
    
    public void SetAnswerExplainText(string text)
    {
        correctAnswerExplainText.text = text;
        wrongAnswerExplainText.text = text;
    }
    
    public void ShowDonePanel(int score)
    {
        panelView.gameObject.SetActive(false);
        correctAnswerPanel.SetActive(false);
        wrongAnswerPanel.SetActive(false);
        donePanel.SetActive(true);
        scoreText.text = "SCORE = " + score;
    }
    
    
}
