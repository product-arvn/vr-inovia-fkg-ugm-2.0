using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class DataHandler : MonoBehaviour
{
    public static DataHandler Instance;
    
    // Data
    [ShowNonSerializedField] private string _patientSelectedId;
    [ShowNonSerializedField] private string _patientSelectedName;

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

    // Pasien kini ditentukan dari sesi yang di-assign admin ke headset (GET /device/session)
    public void SetPatientSelected(string id, string namePatient)
    {
        if (id != null) SetPatientSelectedId(id);
        _patientSelectedName = namePatient;
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
