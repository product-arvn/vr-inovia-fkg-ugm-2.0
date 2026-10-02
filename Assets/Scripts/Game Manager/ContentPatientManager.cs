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

    [Header("Api Controller")]
    [SerializeField] private ApiPatientDetail apiPatientDetail;

    private void Start()
    {
        apiPatientDetail.OnSessionNotFound += SceneNotFound;
    }

    private void SceneNotFound()
    {
        StartCoroutine(ReloadScene());
    }

    IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(2f);
        SessionEnd?.Invoke();
        contentNotFoundPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public IEnumerator CheckContent()
    {
        while (true)
        {
            while (!apiPatientDetail.HasContent)
            {
                apiPatientDetail.GetNecessaryDetail();
                HasContent = false;
                yield return new WaitForSeconds(5f);
            }

            apiPatientDetail.GetSessionDetail();
            HasContent = true;
            
            while (!apiPatientDetail.IsDoneLoadedContent)
            {
                yield return new WaitForSeconds(1f);
            }

            if (!apiPatientDetail.IsDataIdentical && apiPatientDetail.HasContent)
            {
                yield return new WaitForSeconds(1f);
                SetContent();
                ContentChanged?.Invoke();
                isContentDone = false;
            }

            yield return new WaitForSeconds(5f);
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
