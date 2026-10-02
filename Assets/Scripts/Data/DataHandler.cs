using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class DataHandler : MonoBehaviour
{
    public static DataHandler Instance;
    
    // Data
    [ShowNonSerializedField] private string _patientSelectedId = "9cc0e94a-dd72-4e70-a8cb-f2fd17ad9dc0";
    [ShowNonSerializedField] private string _patientSelectedName = "Ade Laksita";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPatientSelectedId(string id)
    {
        _patientSelectedId = id.Replace(" ", "");
    }

    public string GetPatientSelectedId()
    {
        return _patientSelectedId;
    }
    
    public void SetPatientSelectedName(string namePatient)
    {
        _patientSelectedName = namePatient;
    }

    public string GetPatientSelectedName()
    {
        return _patientSelectedName;
    }
}
