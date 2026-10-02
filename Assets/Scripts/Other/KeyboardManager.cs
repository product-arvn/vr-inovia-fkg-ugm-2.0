using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KeyboardManager : MonoBehaviour
{
    public TMP_InputField inputField;
    //public TMP_InputField activPinField;

    //public TMP_InputField[] pinFields;

    public string tempText;
    public int caretPos;
    //public string pinText;

    [Header("Keyboard Components")]
    public GameObject keyboardObject;
    public GameObject lowerCasePack;
    public GameObject upperCasePack;
    public GameObject numbersPack;
    public GameObject alphaButton;
    public GameObject numberButton;
    public bool isUpperCase;

    // Start is called before the first frame update
    void Start()
    {
        tempText = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (inputField != null)
        {
            caretPos = inputField.caretPosition;
        }
    }

    public void HideKeyboard()
    {
        isUpperCase = false;
        ShowCharactersPack();
        keyboardObject.SetActive(false);
    }

    public void SelectInput(TMP_InputField selected)
    {
        if (inputField != null)
        {
            inputField.text = tempText;
        }
        inputField = selected;
        tempText = inputField.text;
        caretPos = inputField.caretPosition;
        //inputField.caretPosition = tempText.Length - 1;
    }

    //Input the letters on the keyboard into the input field
    public void InputText(string text)
    {
        tempText = tempText.Insert(caretPos, text);
        //tempText.Insert(0, text);
        caretPos += text.Length;
        UpdateText();
        //StartCoroutine(Haptics());
    }
    //Backspace button function
    public void DeleteText()
    {
        //StartCoroutine(Haptics());
        if (tempText.Length > 0 && caretPos > 0)
        {
            caretPos--;
            string text;
            text = tempText.Remove(caretPos, 1);
            tempText = text;
            UpdateText();
        }
    }

    //Function to reset keyboard to the default state
    public void ClearText()
    {
        //StartCoroutine(Haptics());
        tempText = "";
        caretPos = 0;
        UpdateText();
    }

    //Update the input field
    public void UpdateText()
    {
        //inputField.GetComponentInChildren<TMP_InputField>().text = tempText + "_";
        inputField.text = tempText;
        inputField.caretPosition = caretPos;
    }

    public void ShowKeyboard()
    {
        
    }

    public void SwitchCase()
    {
        isUpperCase = !isUpperCase;
        lowerCasePack.SetActive(!isUpperCase);
        upperCasePack.SetActive(isUpperCase);
    }

    public void ShowNumbersPack()
    {
        lowerCasePack.SetActive(false);
        upperCasePack.SetActive(false);
        numbersPack.SetActive(true);
        numberButton.SetActive(false);
        alphaButton.SetActive(true);
    }

    public void ShowCharactersPack()
    {
        lowerCasePack.SetActive(!isUpperCase);
        upperCasePack.SetActive(isUpperCase);
        numbersPack.SetActive(false);
        numberButton.SetActive(true);
        alphaButton.SetActive(false);
    }
}
