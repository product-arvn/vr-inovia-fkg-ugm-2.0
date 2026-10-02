using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VideoModuleView : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text creatorText;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TMP_Text timerText;

    [Header("Content")]
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private GameObject donePanel;
    
    private readonly TimerManager _timerManager = new TimerManager();
    

    public void SetTitle(string text)
    {
        titleText.text = text;
    }
    
    public void SetCreator(string text)
    {
        creatorText.text = text;
    }
    
    public void SetTimerText(float t)
    {
        timerText.text = _timerManager.FormatTime(t);
    }

    public void ShowDonePanel()
    {
        timerPanel.SetActive(false);
        timerPanel.SetActive(false);
        contentPanel.SetActive(false);
        donePanel.SetActive(true);
    }
}
