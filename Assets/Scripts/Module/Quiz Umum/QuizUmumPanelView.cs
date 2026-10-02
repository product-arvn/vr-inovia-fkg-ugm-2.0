using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class QuizUmumPanelView : MonoBehaviour
{
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private OptionQuizUmumView[] optionViews;
    
    public void SetQuestionText(string text)
    {
        questionText.text = text;
    }
    
    public void SetCountText(string count)
    {
        countText.text = count;
    }
    
    public int GetOptionCount()
    {
        return optionViews.Length;
    }
    
    public OptionQuizUmumView GetOptionView(int index)
    {
        return optionViews[index];
    }
    
    public void NonActiveAllOption()
    {
        foreach (var t in optionViews)
        {
            t.ActiveThis(false);
        }
    }
}
