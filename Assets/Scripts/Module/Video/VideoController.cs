using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using YoutubePlayer.Components;

public class VideoController : MonoBehaviour, IModuleController
{
    [SerializeField] private InvidiousVideoPlayer videoPlayer;
    [SerializeField] private VideoModuleView videoModuleView;
    
    //Timer Trigger
    [SerializeField] private TimerTrigger contentTimerTrigger;
    
    //GLobal Timer
    private GlobalTimer _globalTimer;

    public UnityAction OnVideoDone;

    public void StartContent()
    {
        PlayVideo(true);
    }

    public void EndContent()
    {
        OnVideoDone?.Invoke();   
    }
    
    public void ActivateGameObject(bool active)
    {
        gameObject.SetActive(active);
    }

    private void Start()
    {
        _globalTimer = FindObjectOfType<GlobalTimer>();
        
        contentTimerTrigger.AddAction(() => videoModuleView.ShowDonePanel());
        
        StartContent();
    }

    private void Update()
    {
        if (_globalTimer.IsPaused == false)
        {
            videoModuleView.SetTimerText(_globalTimer.Timer);
        }
    }

    public void SetContent(string id)
    {
        videoPlayer.VideoId = id;
        videoPlayer.gameObject.SetActive(true);
    }

    public void PlayVideo(bool activated)
    {
        videoPlayer.gameObject.SetActive(activated);
    }
    
    public void SetHeader(string title, string creator)
    {
        videoModuleView.SetTitle(title);
        videoModuleView.SetCreator(creator);
    }
    
    public void SetContentDuration(float time)
    {
        contentTimerTrigger.SetTimer(time);
    }
}
