using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine.UI;

public class PatientListManager : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject patientListPanel;
    [SerializeField] private GameObject buttonSelectPrefab;
    [SerializeField] private Transform parentButtonSelect;
    private List<ButtonSelect> _buttonSelects = new List<ButtonSelect>();
    [SerializeField] private Button nextButton;
    public UnityAction GetPatientComplete;

    [Header("API")]
    [SerializeField] private ApiPatientList patientList;
    
    //Data Handler
    private DataHandler _dataHandler;

    private void Start()
    {
        _dataHandler = DataHandler.Instance;
        nextButton.interactable = false;
        patientListPanel.SetActive(false);
    }
    
    [Button]
    public void Init()
    {
        StartCoroutine(CheckPatientListAndGenerate());
    }

    IEnumerator CheckPatientListAndGenerate()
    {
        while (patientList.patientDataList.Count == 0 || patientList.patientDataList == null || patientList.isFetching)
        {
            Debug.Log("Patient List Empty or Fetching");
            patientList.CallPatientList();
            yield return new WaitForSeconds(5f);
        }
        
        patientListPanel.SetActive(true);
        GenerateListPatient();
        GetPatientComplete?.Invoke();
    }

    private void GenerateListPatient()
    {
        for (int i = 0; i < patientList.patientDataList.Count; i++)
        {
            int index = i;
            GameObject gp = Instantiate(buttonSelectPrefab, parentButtonSelect);
            ButtonSelect buttonSelect = gp.GetComponent<ButtonSelect>();
            buttonSelect.SetText(patientList.patientDataList[index].name);
            buttonSelect.AddOnclickButton(() => SaveIdPatient(patientList.patientDataList[index].id, patientList.patientDataList[index].name));
            buttonSelect.AddOnclickButton(() => SelectPatient(index));
            _buttonSelects.Add(buttonSelect);   
        }
    }

    private void SaveIdPatient(string id, string patientName)
    {
        _dataHandler.SetPatientSelectedId(id);
        _dataHandler.SetPatientSelectedName(patientName);
        nextButton.interactable = true;
    }
    

    private void SelectPatient(int index)
    {
        for (int i = 0; i < _buttonSelects.Count; i++)
        {
            _buttonSelects[i].ActivateButton(i != index);
        }
    }
}
