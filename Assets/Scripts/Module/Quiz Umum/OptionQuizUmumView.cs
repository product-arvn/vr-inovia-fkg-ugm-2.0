using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionQuizUmumView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text optionText;
    
    public void SetText(string text)
    {
        optionText.text = text;
    }

    public void ActiveThis(bool activate)
    {
        gameObject.SetActive(activate);
    }
    
    public void AddButtonListener(UnityAction action)
    {
        button.onClick.AddListener(action);
    }
    
    public void RemoveAllButtonListener()
    {
        button.onClick.RemoveAllListeners();
    }

    public void SetOption(string nameText, UnityAction action)
    {
        SetText(nameText);
        AddButtonListener(action);
    }
}
