using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
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

    // videoUrl: link YouTube -> diputar lewat Invidious; selain itu (file upload / URL MP4) -> VideoPlayer Unity langsung
    public void SetContent(string videoUrl)
    {
        string youtubeId = YoutubeParse.GetYouTubeVideoID(videoUrl);

        if (!string.IsNullOrEmpty(youtubeId))
        {
            videoPlayer.enabled = true;
            videoPlayer.VideoId = youtubeId;
        }
        else if (!string.IsNullOrWhiteSpace(videoUrl))
        {
            // Matikan Invidious sebelum GameObject aktif agar OnEnable-nya tidak meminta API YouTube
            videoPlayer.enabled = false;
            VideoPlayer player = videoPlayer.GetComponent<VideoPlayer>();
            player.source = VideoSource.Url;
            // Backend bisa mengirim path relatif (/storage/...) untuk video upload
            player.url = GameEnv.ToAbsoluteUrl(videoUrl).Replace(" ", "%20");
        }
        else
        {
            Debug.LogError("Konten video tanpa video_url");
        }

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
