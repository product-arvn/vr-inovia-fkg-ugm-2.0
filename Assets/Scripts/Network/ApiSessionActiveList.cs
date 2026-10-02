using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using NaughtyAttributes;

public class ApiSessionActiveList : MonoBehaviour
{

    [SerializeField] private List<SessionActiveData> sessionActiveDataList = new List<SessionActiveData>();
    private readonly string _url = GameEnv.BASE_URL + "/session/active/list?page=1&limit=10";
    
    // Get Api Controller
    [SerializeField] private ApiController _apiController;

    [Button]
    public void CallSessionActive()
    {
        _apiController.CallApi(_url, HandleSessionActiveList, _apiController.HandleError);
    }
    
    private void HandleSessionActiveList(JObject jsonResponse)
    {
        bool success = jsonResponse["success"].Value<bool>();

        if (success)
        {
            JArray list = (JArray)jsonResponse["data"]["list"];

            if (list == null) return;
            
            foreach (var item in list)
            {
                SessionActiveData sessionData = new SessionActiveData
                {
                    id = item["id"]?.ToString(),
                    status = item["status"]?.ToString(),
                    startTestAt = item["start_test_at"]?.ToString(),
                    endTestAt = item["end_test_at"]?.ToString(),
                    patientId = item["patient"]?["id"]?.ToString(),
                    patientName = item["patient"]?["name"]?.ToString(),
                    patientAddress = item["patient"]?["address"]?.ToString(),
                    patientDob = item["patient"]?["date_of_birth"]?.ToString(),
                    doctorName = item["doctor"]?["name"]?.ToString(),
                    doctorRole = item["doctor"]?["role"]?.ToString()
                };

                sessionActiveDataList.Add(sessionData);
            }
        }
    }

    public List<SessionActiveData> GetSessionActiveData()
    {
        return sessionActiveDataList;
    }
}

[System.Serializable]
public class SessionActiveData
{
    public string id;
    public string status;
    public string startTestAt;
    public string endTestAt;

    public string patientId;
    public string patientName;
    public string patientAddress;
    public string patientDob;

    public string doctorName;
    public string doctorRole;
}