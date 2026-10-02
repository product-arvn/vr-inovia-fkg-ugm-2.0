using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class ButtonSelect : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonText;
    
    public bool HasSelected { get; private set; }

    public void Selected(bool isSelect)
    {
        if (isSelect)
        {
            HasSelected = true;
            button.enabled = false;
        }
        else
        {
            HasSelected = false;
            button.enabled = true;
        }
    }

    public void AddOnclickButton(UnityAction callback)
    {
        button.onClick.AddListener(callback);
    }

    public void ResetOnclick()
    {
        button.onClick.RemoveAllListeners();
    }

    public void SetText(string text)
    {
        buttonText.text = text;
    }

    public void ActivateButton(bool activated)
    {
        button.interactable = activated;
    }
}
