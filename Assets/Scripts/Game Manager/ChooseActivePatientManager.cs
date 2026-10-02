using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseActivePatientManager : MonoBehaviour
{
    [Header("Panel Setting")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Transform scrollViewParent;
    [SerializeField] private GameObject buttonPrefabs;

    [Header("Other Script")]
    [SerializeField] private ApiSessionActiveList apiSessionActiveList;
    
    // Local  Varible
    private List<ButtonSelect> _buttonSelects = new List<ButtonSelect>();
    
    //Data Handler
    private DataHandler _dataHandler;

    private void Awake()
    {
        apiSessionActiveList.CallSessionActive();
    }

    private void Start()
    {
        _dataHandler = DataHandler.Instance;
        nextButton.interactable = false;
    }

    public void ShowListSessionActive()
    {
        var data = apiSessionActiveList.GetSessionActiveData();
        
        for (int i = 0; i < data.Count; i++)
        {
            var session = data[i];
            var index = i;
            
            GameObject button = Instantiate(buttonPrefabs, scrollViewParent);
            ButtonSelect buttonSelect = button.GetComponent<ButtonSelect>();
            _buttonSelects.Add(buttonSelect);

            buttonSelect.SetText(session.patientName);
            buttonSelect.AddOnclickButton(() => _dataHandler.SetPatientSelectedId(session.patientId));
            buttonSelect.AddOnclickButton(() => _dataHandler.SetPatientSelectedName(session.patientName));
            buttonSelect.AddOnclickButton(() => nextButton.interactable = true);
            buttonSelect.AddOnclickButton(() =>  SelectPatient(index));
        }
    }

    private void SelectPatient(int index)
    {
        for (int i = 0; i < _buttonSelects.Count; i++)
        {
            _buttonSelects[i].ActivateButton(i != index);
        }
    }
}
