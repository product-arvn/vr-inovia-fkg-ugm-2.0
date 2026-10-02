using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuizUmumModuleView : MonoBehaviour
{
    [Header("header")]
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject creatorPanel;
    [SerializeField] private TMP_Text creatorText;
    [SerializeField] private TMP_Text themeText;
    
    [Header("Result")]
    [SerializeField] private GameObject correctAnswerPanel;
    [SerializeField] private TMP_Text correctAnswerExplainText;
    [SerializeField] private GameObject wrongAnswerPanel;
    [SerializeField] private TMP_Text wrongAnswerExplainText;
    
    [Header("Content")]
    public QuizUmumPanelView panelView;
    
    [Header("Done")]
    [SerializeField] private GameObject donePanel;
    [SerializeField] private TMP_Text scoreText;
    
    //Timer
    private readonly TimerManager _timerManager = new TimerManager();
    
    public void SetTimerText(float t)
    {
        timerText.text = _timerManager.FormatTime(t);
    }
    
    public void SetHeaderText(string creator, string theme)
    {
        creatorText.text = creator;
        themeText.text = theme;
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
