using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EducationController : MonoBehaviour, IModuleController
{
    private string _title;
    private float _duration;
    private List<EducationData> _educationData = new List<EducationData>();
    private EducationData _educationDataCover = new EducationData();
    [SerializeField] private EducationModulView educationModulView;
    private int _indexEducation = -1;
    
    // Timer Trigger
    [SerializeField] private TimerTrigger coverTimerTrigger;
    [SerializeField] private TimerTrigger contentTimerTrigger;

    private GlobalTimer _globalTimer;
    public UnityAction OnEducationDone;
    
    public void StartContent()
    {
        educationModulView.ShowCoverPanel();
        SetCover();
    }

    public void EndContent()
    {
        OnEducationDone?.Invoke();
    }

    private IEnumerator Start()
    {
        _globalTimer = FindObjectOfType<GlobalTimer>();
        coverTimerTrigger.SetTimer(_duration);
        contentTimerTrigger.SetTimer(_duration);
        
        // Add Event
        coverTimerTrigger.AddAction(NextContent);
        contentTimerTrigger.AddAction(NextContent);

        yield return new WaitForSeconds(2f);
        StartContent();
    }

    private void Update()
    {
        educationModulView.SetTimerText(_globalTimer.Timer);
    }

    public void ActivateGameObject(bool activate)
    {
        this.gameObject.SetActive(activate);
    }

    #region Setup Data

    public void AddEducationData(string content, Sprite image)
    {
        EducationData data = new EducationData
        {
            contentText = content,
            image = image
        };
        
        _educationData.Add(data);
    }

    public void SetEducationCover(string title, Sprite image)
    {
        _educationDataCover.contentText = title;
        _educationDataCover.image = image;
    }

    public void SetDuration(int time)
    {
        _duration = time;
    }

    public void SetHeader(string title, string creator)
    {
        _title = title;
        educationModulView.SetTypeText(title);
        educationModulView.SetCreatorText(creator);
    }

    private void SetEducationContent(int indexE)
    {
        educationModulView.contentView.SetContent(_educationData[indexE].contentText);
        educationModulView.contentView.SetCount(_indexEducation, _educationData.Count);
        educationModulView.contentView.SetThumbnail(_educationData[indexE].image);
        educationModulView.contentView.SetCount(indexE + 1, _educationData.Count);
        educationModulView.contentView.SetType(_title);
    }

    private void SetCover()
    {
        educationModulView.contentCoverPanel.SetTitle(_educationDataCover.contentText);
        educationModulView.contentCoverPanel.SetCover(_educationDataCover.image);
    }

    #endregion
    private void NextContent()
    {
        if (_indexEducation < _educationData.Count - 1)
        {
            educationModulView.ShowEducationContent();
            _indexEducation++;
            SetEducationContent(_indexEducation);
            contentTimerTrigger.ResetTimer();
        }
        else
        {
            educationModulView.ShowDonePanel(_title);
        }
    }
}

[Serializable]
public class EducationData
{
    public string contentText;
    public Sprite image;
}