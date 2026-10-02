using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EducationModulView : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject creatorPanel;
    [SerializeField] private TMP_Text creatorText;
    [SerializeField] private TMP_Text typeText;
    
    [Header("Content")]
    public EducationCoverView contentCoverPanel;
    public EducationContentView contentView;
    
    [Header("Done")]
    [SerializeField] private GameObject donePanel;
    [SerializeField] private TMP_Text descText;
    
    //Timer
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
    
    public void SetTypeText(string text)
    {
        typeText.text = text;
    }
    
    public void ShowDonePanel(string content)
    {
        contentCoverPanel.ActiveThis(false);
        contentView.ActivateThis(false);
        donePanel.SetActive(true);
        descText.text = $"Kamu sudah menyelesaikan {content}.\n Hebat sekali semoga kamu senang dan belajar hal baru.";
    }

    public void ShowCoverPanel()
    {
        contentCoverPanel.ActiveThis(true);
        contentView.ActivateThis(false);
        donePanel.SetActive(false);
    }

    public void ShowEducationContent()
    {
        contentCoverPanel.ActiveThis(false);
        contentView.ActivateThis(true);
        donePanel.SetActive(false);
    }
}
