using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NaughtyAttributes;

public class ClinicListManager : MonoBehaviour
{
    [SerializeField] private GameObject clinicListPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject buttonSelectPrefabs;
    [SerializeField] private ApiClinicList apiClinicList;
    [SerializeField] private Button nextButton;
    public UnityAction OnGetClinicComplete;
    
    private List<ButtonSelect> _buttonSelects = new List<ButtonSelect>();

    private void Start()
    {
        nextButton.interactable = false;
        clinicListPanel.SetActive(false);
    }

    [Button]
    public void Init()
    {
        StartCoroutine(CheckClinicListAndGenerate());
    }

    IEnumerator CheckClinicListAndGenerate()
    {
        while (apiClinicList.clinicDataList.Count == 0 || apiClinicList.clinicDataList == null ||
               apiClinicList.isFetching)
        {
            Debug.Log("Clinic List Empty or Fetching");
            apiClinicList.CallClinicList();
            yield return new WaitForSeconds(5f);
        }

        clinicListPanel.SetActive(true);
        GenerateClinicList();
        OnGetClinicComplete?.Invoke();
    }

    private void GenerateClinicList()
    {
        for (int i = 0; i < apiClinicList.clinicDataList.Count; i++)
        {
            int index = i;
            GameObject clinic = Instantiate(buttonSelectPrefabs, contentParent);
            ButtonSelect buttonSelect = clinic.GetComponent<ButtonSelect>();
            _buttonSelects.Add(buttonSelect);
            buttonSelect.SetText(apiClinicList.clinicDataList[index].name);
            
            buttonSelect.AddOnclickButton(() =>
            {
                PlayerPrefs.SetString("accessKey", apiClinicList.clinicDataList[index].accessKey);
                nextButton.interactable = true;
                SelectClinic(index);
                Debug.Log("Selected Clinic: " + PlayerPrefs.GetString("accessKey"));
            });
        }
    }
    
    private void SelectClinic(int index)
    {
        for (int i = 0; i < _buttonSelects.Count; i++)
        {
            _buttonSelects[i].ActivateButton(i != index);
        }
    }
}
