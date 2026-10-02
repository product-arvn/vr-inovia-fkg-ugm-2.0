using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ApiClinicList : MonoBehaviour
{
    private string _url = GameEnv.BASE_URL + "/clinic/list";
    public List<ClinicData> clinicDataList = new List<ClinicData>();
    private JObject _savedResponse;
    
    public bool isFetching = false;
    private int _limit = 10;
    private int _currentPage = 1;
    private int _totalData = 0;
    
    [SerializeField] private ApiController apiController;

    public void CallClinicList()
    {
        if (!isFetching)
        {
            _currentPage = 1;
            isFetching = true;
            clinicDataList.Clear();
            FetchClinicData(_currentPage);
        }
    }

    private void FetchClinicData(int page)
    {
        string paginationUrl = _url + "?page=" + page + "&limit=" + _limit;
        apiController.CallApi(paginationUrl, HandleClinicDataList, apiController.HandleError);
    }

    private void HandleClinicDataList(JObject jsonResponse)
    {
        bool success = jsonResponse["success"].Value<bool>();

        if (success)
        {
            _totalData = jsonResponse["data"]["total"]!.Value<int>();
            JArray list = (JArray)jsonResponse["data"]["list"];

            if (list != null)
            {
                foreach (var item in list)
                {
                    ClinicData clinic = new ClinicData
                    {
                        id = item["id"]?.ToString(),
                        name = item["title"]?.ToString(),
                        accessKey = item["access_key"]?.ToString()
                    };
                    
                    clinicDataList.Add(clinic);
                }
                
                Debug.Log($"Data Saved: {clinicDataList.Count} Records.");

                if (clinicDataList.Count < _totalData)
                {
                    _currentPage++;
                    
                }
                else
                {
                    isFetching = false;
                    
                    // Sort Clinic Result
                    clinicDataList = clinicDataList.OrderBy(clinic => clinic.name).ToList();
                    Debug.Log("All Data Loaded");
                }
            }
        }
        else
        {
            isFetching = false;
            Debug.Log("API Response Not Success");
        }
    }
}

[System.Serializable]
public class ClinicData
{
    public string id;
    public string name;
    public string accessKey;
}