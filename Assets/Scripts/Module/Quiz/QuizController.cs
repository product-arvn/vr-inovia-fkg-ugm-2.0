using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class QuizController : MonoBehaviour, IModuleController
{
    private float _duration;
    public List<QuizDataQuestion> questionsData = new List<QuizDataQuestion>();
    [SerializeField] private QuizModuleView quizModuleView;
    private int _indexQuestion = 0;
    private GlobalTimer _globalTimer;
    private int _score = 0;
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
        StartContent();
        
        quizModuleView.panelView.GetOptionA().SetOnClick(() => NextQuestion("a"));
        quizModuleView.panelView.GetOptionB().SetOnClick((() => NextQuestion("b")));

        _globalTimer = FindObjectOfType<GlobalTimer>();
        
        timerTrigger.SetTimer(_duration);
        timerTrigger.AddAction(() => quizModuleView.ShowDonePanel(_score));
    }

    private void Update()
    {
        quizModuleView.SetTimerText(_globalTimer.Timer);
    }

    #region Setup Data
    public void AddQuestionData(string question,int score, string trueAnswer, string explain, 
        string optionA, Sprite optionASprite, string optionB, Sprite optionBSprite)
    {
        QuizDataQuestion data = new QuizDataQuestion
        {
            question = question,
            score = score,
            trueAnswer = trueAnswer,
            explainAnswer = explain,
            optionAText = optionA,
            optionBText = optionB,
            optionASprite = optionASprite,
            optionBSprite = optionBSprite
        };
        
        questionsData.Add(data);
    }

    public void SetDuration(float d)
    {
        _duration = d;
    }

    public void SetHeader(string creator)
    {
        quizModuleView.SetCreatorText(creator);
    }

    private void SetQuestion(int indexQ)
    {
        quizModuleView.panelView.SetQuestionText(questionsData[indexQ].question);
        quizModuleView.panelView.SetCountQuizText(GetCount());
        quizModuleView.panelView.GetOptionA().SetOption(questionsData[indexQ].optionAText, questionsData[indexQ].optionASprite);
        quizModuleView.panelView.GetOptionB().SetOption(questionsData[indexQ].optionBText, questionsData[indexQ].optionBSprite);
        quizModuleView.SetAnswerExplainText(questionsData[indexQ].explainAnswer);
    }

    private string GetCount()
    {
        return $"{_indexQuestion}/{questionsData.Count}";
    }
    #endregion

    #region Logic Question

    private void NextQuestion(string answer)
    {
        quizModuleView.panelView.gameObject.SetActive(false);
        
        if (answer == questionsData[_indexQuestion].trueAnswer)
        {
            _score += questionsData[_indexQuestion].score;
            Debug.Log($"{gameObject.name} index {_indexQuestion} is Correct");
            quizModuleView.ShowResultAnswer(true);
        }
        else
        {
            quizModuleView.ShowResultAnswer(false);
            Debug.Log($"{gameObject.name} index {_indexQuestion} is Wrong");
        }
        
        if (_indexQuestion == questionsData.Count - 1)
        {
            quizModuleView.ShowDonePanel(_score);
        }
        else
        {
            _indexQuestion++;
            SetQuestion(_indexQuestion);
        }
    }
    #endregion
}

[Serializable]
public class QuizDataQuestion
{
    public string question;
    public int score;
    public string trueAnswer;
    public string explainAnswer;
    public string optionAText;
    public Sprite optionASprite;
    public string optionBText;
    public Sprite optionBSprite;
}
