using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class GameManagerApi : MonoBehaviour
{
    [SerializeField] private PatientListManager patientListManager;
    [SerializeField] private LoadingView loadingView;
    [SerializeField] private ContentPatientManager contentPatientManager;
    private DataHandler _dataHandler;

    private void Awake()
    {
        loadingView.ActiveLoadingPanel("Loading Patient List");
    }

    private void Start()
    {
        _dataHandler = DataHandler.Instance;
        
        // Get List Patient
        Invoke(nameof(InitPatientList), 1f);
        
        contentPatientManager.ContentChanged += () => EnableContentPanel(contentPatientManager.HasContent);

        contentPatientManager.SessionEnd += () => loadingView.DisableLoadingPanel();
    }

    [Button]
    public void StartContent()
    {
        loadingView.ActiveLoadingPanel($"Loading <color=blue>{_dataHandler.GetPatientSelectedName()}</color> Content ...");
        StartCoroutine(contentPatientManager.CheckContent());

        contentPatientManager.WhenEndContent += ContentEnd;
    }

    private void ContentEnd()
    {
        loadingView.ActiveLoadingPanel(
            $"Loading <color=blue>{_dataHandler.GetPatientSelectedName()}</color> Content ...");
        contentPatientManager.isContentDone = true;
    }

    private void EnableContentPanel(bool activate)
    {
        if (activate)
        {
            loadingView.DisableLoadingPanel();
        }
        else
        {
            loadingView.ActiveLoadingPanel($"Loading <color=blue>{_dataHandler.GetPatientSelectedName()}</color> Content ...");
        }
    }

    private void InitPatientList()
    {
        patientListManager.Init();
        patientListManager.GetPatientComplete += () => loadingView.DisableLoadingPanel();
    }

}
