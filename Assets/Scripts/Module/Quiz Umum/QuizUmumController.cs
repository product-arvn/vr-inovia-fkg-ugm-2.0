using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuizUmumController : MonoBehaviour, IModuleController
{
    private float _duration;
    public List<QuizUmumData> questionData = new List<QuizUmumData>();
    [SerializeField] private QuizUmumModuleView quizUmumModuleView;
    private int _indexQuestion = 0;
    private GlobalTimer _globalTimer;
    private int _score;
    [SerializeField] private TimerTrigger timerTrigger;

    public UnityAction OnQuizEnd;

    public void StartContent()
    {
        SetQuestion(0);
    }

    public void EndContent()
    {
        OnQuizEnd?.Invoke();
    }
    
    public void ActivateGameObject(bool activate)
    {
        this.gameObject.SetActive(activate);
    }

    private void Start()
    {
        _globalTimer = FindObjectOfType<GlobalTimer>();
        string[] answers = { "a", "b", "c", "d" };

        for (int i = 0; i < quizUmumModuleView.panelView.GetOptionCount(); i++)
        {
            int index = i;
            quizUmumModuleView.panelView.GetOptionView(i).AddButtonListener(() => NextQuestion(answers[index]));
        }

        StartContent();
        
        timerTrigger.SetTimer(_duration);
        timerTrigger.AddAction(() => quizUmumModuleView.ShowDonePanel(_score));
    }

    private void Update()
    {
        quizUmumModuleView.SetTimerText(_globalTimer.Timer);
    }

    public void AddQuestionData(string question, string answer, int score, string explain, List<string> options)
    {
        QuizUmumData data = new QuizUmumData
        {
            question = question,
            answer = answer,
            score = score,
            explain = explain,
            options = options
        };
        
        questionData.Add(data);
    }

    public void SetHeader(string creator, string theme)
    {
        quizUmumModuleView.SetHeaderText(creator, theme);
    }
    
    public void SetDuration(float d)
    {
        _duration = d;
    }

    private void SetQuestion(int index)
    {
        quizUmumModuleView.panelView.SetQuestionText(questionData[index].question);
        quizUmumModuleView.panelView.SetCountText(GetCount());
        quizUmumModuleView.SetAnswerExplainText(questionData[index].explain);
        
        for (int i = 0; i < questionData[index].options.Count; i++)
        {
            quizUmumModuleView.panelView.GetOptionView(i).ActiveThis(true);
            quizUmumModuleView.panelView.GetOptionView(i).SetText(questionData[index].options[i]);
        }
    }

    private string GetCount()
    {
        return $"{_indexQuestion}/{questionData.Count}";
    }

    public void NextQuestion(string answer)
    {
        quizUmumModuleView.panelView.gameObject.SetActive(false);

        if (answer == questionData[_indexQuestion].answer)
        {
            _score += questionData[_indexQuestion].score;
            Debug.Log($"{gameObject.name} index {_indexQuestion} is Correct");
            quizUmumModuleView.ShowResultAnswer(true);
        }
        else
        {
            quizUmumModuleView.ShowResultAnswer(false);
            Debug.Log($"{gameObject.name} index {_indexQuestion} is Wrong");
        }

        if (_indexQuestion == questionData.Count - 1)
        {
            quizUmumModuleView.ShowDonePanel(_score);
        }
        else
        {
            _indexQuestion++;
            quizUmumModuleView.panelView.NonActiveAllOption();
            SetQuestion(_indexQuestion);
        }
    }
}

[System.Serializable]
public class QuizUmumData
{
    public string question;
    public string answer;
    public string explain;
    public int score;
    public List<string> options = new List<string>();
}