using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ContentPatientManager : MonoBehaviour
{
    [Header("Content Controller")]
    [SerializeField] private QuizController quizController;
    [SerializeField] private QuizUmumController quizUmumController;
    [SerializeField] private PuzzleController puzzleController;
    [SerializeField] private EducationController educationController;
    [SerializeField] private VideoController videoController;

    [Header("Content Panel")] 
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject contentNotFoundPanel;

    [SerializeField] private GeneralScript generalScript;
    
    public bool HasContent { get; private set; }
    public bool isContentDone;
    
    private GameObject _activeContent;
    public UnityAction ContentChanged;
    public UnityAction WhenEndContent;
    public UnityAction SessionEnd;
    // true = sesi sudah di-assign ke headset tetapi belum ada konten berjalan, false = belum ada sesi
    public UnityAction<bool> WaitingContent;
    // true = sesi dijeda dari dashboard, false = dilanjutkan
    public UnityAction<bool> SessionPausedChanged;

    public bool HasActiveContent => _activeContent != null;
    public string CurrentSessionId => apiPatientDetail.sessionDetailData?.id;
    public string CurrentSessionTypeLabel => apiPatientDetail.HasSession ? apiPatientDetail.sessionDetailData?.sessionTypeLabel : null;

    private const float EducationCoverMaxSeconds = 5f;
    // null = belum diketahui: event berikutnya selalu dikirim (dipakai setelah kembali dari nonaktif)
    private bool? _isSessionPaused = false;

    [Header("Api Controller")]
    [SerializeField] private ApiPatientDetail apiPatientDetail;

    private void SessionEndedFromServer()
    {
        StartCoroutine(ReloadScene());
    }

    IEnumerator ReloadScene()
    {
        yield return new WaitForSecondsRealtime(2f);
        SessionEnd?.Invoke();
        contentNotFoundPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Loop sesi: GET /device/session tiap SESSION_POLL_INTERVAL detik (sekaligus heartbeat ke dashboard).
    // Pakai waktu realtime: saat sesi dijeda Time.timeScale = 0, tetapi polling harus tetap jalan.
    public IEnumerator CheckContent()
    {
        while (true)
        {
            while (!apiPatientDetail.HasContent)
            {
                apiPatientDetail.GetNecessaryDetail();
                HasContent = false;
                yield return new WaitForSecondsRealtime(GameEnv.SESSION_POLL_INTERVAL);
                UpdatePausedState();
                if (!apiPatientDetail.HasContent) WaitingContent?.Invoke(apiPatientDetail.HasSession);
            }

            apiPatientDetail.GetSessionDetail();
            yield return new WaitWhile(() => apiPatientDetail.IsFetchingDetail);
            UpdatePausedState();

            // Konten yang sedang tampil sudah tidak berjalan di server (sesi diakhiri/dihentikan dari dashboard)
            if (_activeContent != null && !apiPatientDetail.HasContent)
            {
                SessionEndedFromServer();
                yield break;
            }

            if (!apiPatientDetail.IsDataIdentical && apiPatientDetail.HasContent && apiPatientDetail.IsDoneLoadedContent)
            {
                yield return new WaitForSecondsRealtime(1f);
                HasContent = true;
                SetContent();
                ContentChanged?.Invoke();
                isContentDone = false;
            }

            yield return new WaitForSecondsRealtime(GameEnv.SESSION_POLL_INTERVAL);
        }
    }

    private void UpdatePausedState()
    {
        bool paused = apiPatientDetail.HasSession && apiPatientDetail.IsPaused;
        if (_isSessionPaused == paused) return;
        _isSessionPaused = paused;
        SessionPausedChanged?.Invoke(paused);
    }

    public void ResetPausedState()
    {
        _isSessionPaused = null;
    }

    // Bekukan konten tanpa menutupnya: timer modul memakai Time.deltaTime, video & audio dijeda terpisah
    public void SetFrozen(bool frozen)
    {
        Time.timeScale = frozen ? 0f : 1f;
        AudioListener.pause = frozen;

        if (_activeContent == null) return;
        foreach (var player in _activeContent.GetComponentsInChildren<UnityEngine.Video.VideoPlayer>(true))
        {
            if (frozen) player.Pause();
            else if (player.isPrepared) player.Play();
        }
    }
    
    
    
    private void SetContent()
    {
        if (contentParent.childCount != 0)
        {
            // // Destroy all child in content parent
            // foreach (Transform child in contentParent)
            // {
            //     Destroy(child.gameObject);
            // }
            return;
        }
        
        switch (apiPatientDetail.contentList.content.type)
        {
            case "quiz":
                HandleSetQuiz();
                break;
            case "puzzle":
                HandleSetPuzzle();
                break;
            case "education":
                HandleSetEducation();
                break;
            case "video":
                HandleSetVideo();
                break;
        }
    }

    private void HandleSetQuiz()
    {
        bool isQuizUmum = false;
        
        foreach (var choice in apiPatientDetail.contentList.content.questions)
        {
            if (choice.choices.Count > 2)
            {
                isQuizUmum = true;
                break;
            }
        }
        
        if (isQuizUmum)
        {
            HandleQuizUmum();
        }
        else
        {
            HandleQuizNormal();
        }
    }

    private void HandleQuizNormal()
    {
        _activeContent = Instantiate(quizController.gameObject, contentParent);
        QuizController quiz = _activeContent.GetComponent<QuizController>();
                
        foreach (var q in apiPatientDetail.contentList.content.questions)
        {
            quiz.AddQuestionData(question: q.questionText, score: q.score, trueAnswer: q.correctAnswer, explain: q.explanation, 
                optionA: q.choices[0].text, optionB: q.choices[1].text, 
                optionASprite: q.choices[0].image, optionBSprite: q.choices[1].image);
        }
        
        quiz.SetHeader(apiPatientDetail.sessionDetailData.doctorData.name);
        quiz.SetDuration(apiPatientDetail.contentList.duration);

        quiz.OnQuizEnd += apiPatientDetail.DoneContent;
        quiz.OnQuizEnd += () => Destroy(_activeContent);
        quiz.OnQuizEnd += () => WhenEndContent?.Invoke();
    }

    private void HandleQuizUmum()
    {
        _activeContent = Instantiate(quizUmumController.gameObject, contentParent);
        QuizUmumController quiz = _activeContent.GetComponent<QuizUmumController>();

        foreach (var q in apiPatientDetail.contentList.content.questions)
        {
            List<string> options = new List<string>();
            foreach (var c in q.choices)
            {
                options.Add(c.text);
            }
            
            quiz.AddQuestionData(q.questionText, q.correctAnswer, q.score, q.explanation, options);
        }
        
        quiz.SetHeader(apiPatientDetail.sessionDetailData.doctorData.name, apiPatientDetail.sessionDetailData.patientData.name);
        quiz.SetDuration(apiPatientDetail.contentList.duration);
        
        quiz.OnQuizEnd += apiPatientDetail.DoneContent;
        quiz.OnQuizEnd += () => Destroy(_activeContent);
        quiz.OnQuizEnd += () => WhenEndContent?.Invoke();
        quiz.OnQuizEnd += () => WhenEndContent?.Invoke();
    }

    private void HandleSetPuzzle()
    {
        _activeContent = Instantiate(puzzleController.gameObject, contentParent);
        PuzzleController puzzle = _activeContent.GetComponent<PuzzleController>();
        puzzle.SetPuzzleData(apiPatientDetail.contentList.content.title ,apiPatientDetail.contentList.content.puzzleImage, apiPatientDetail.contentList.duration, apiPatientDetail.contentList.content.puzzleType);
        puzzle.SetHeader(apiPatientDetail.sessionDetailData.doctorData.name, apiPatientDetail.contentList.content.title);
        
        puzzle.OnPuzzleEnd += () => Destroy(_activeContent);
        puzzle.OnPuzzleEnd += () => apiPatientDetail.DoneContent();
        puzzle.OnPuzzleEnd += () => WhenEndContent?.Invoke();
    }

    private void HandleSetEducation()
    {
        _activeContent = Instantiate(educationController.gameObject, contentParent);
        EducationController education = _activeContent.GetComponent<EducationController>();
        List<string> contents = new List<string>(apiPatientDetail.contentList.content.description.Split(new []{ "\r\n" }, StringSplitOptions.None));

        for (int i = 0; i < contents.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(contents[i]))
            {
                continue;
            }
            
            education.AddEducationData(contents[i],
                i < apiPatientDetail.contentList.content.images.Count
                    ? apiPatientDetail.contentList.content.images[i]
                    : null);
        }
        
        education.SetEducationCover(apiPatientDetail.contentList.content.title, apiPatientDetail.contentList.content.coverSprite);
        education.SetHeader(apiPatientDetail.contentList.content.title, apiPatientDetail.sessionDetailData.doctorData.name);
        education.SetDuration(apiPatientDetail.contentList.duration / contents.Count);
        education.OnEducationDone += () => Destroy(education.gameObject);
        education.OnEducationDone += () => apiPatientDetail.DoneContent();
        education.OnEducationDone += () => WhenEndContent?.Invoke();
    }
    
    private void HandleSetVideo()
    {
        _activeContent = Instantiate(videoController.gameObject, contentParent);
        VideoController video = _activeContent.GetComponent<VideoController>();
        video.SetContent(apiPatientDetail.contentList.content.videoUrl);
        video.SetHeader(apiPatientDetail.contentList.content.title, apiPatientDetail.sessionDetailData.doctorData.name);
        video.SetContentDuration(apiPatientDetail.contentList.duration);
        video.OnVideoDone += () => Destroy(video.gameObject);
        video.OnVideoDone += () => apiPatientDetail.DoneContent();
        video.OnVideoDone += () => WhenEndContent?.Invoke();
        video.OnVideoDone += () => generalScript.PlayBGM(true);
        
        //Stop Music
        generalScript.PlayBGM(false);
    }
}
