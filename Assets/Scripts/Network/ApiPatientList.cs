using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

public class ApiPatientList : MonoBehaviour
{
    private string _url;
    // Patient List
    public List<PatientData> patientDataList = new List<PatientData>();
    [SerializeField] private ApiController apiController;

    private JObject _savedResponse;
    
    public bool isFetching = false; // Flag untuk menandakan pengambilan data sedang berlangsung
    private int _limit = 15;
    private int _currentPage = 1;
    private int _totalData = 0;
    
    // Data Handler
    private DataHandler _dataHandler;

    private void Awake()
    {
        _dataHandler = DataHandler.Instance;
    }

    private void Start()
    {
        _url = GameEnv.BASE_URL + "/patient/list";
    }
    
    [Button]
    public void CallPatientList()
    {
        if (!isFetching) // Cegah pengambilan data ganda
        {
            _currentPage = 1;
            isFetching = true;
            patientDataList.Clear(); // Reset list pasien sebelum mengambil data baru
            FetchPatientData(_currentPage);
        }
    }

    private void FetchPatientData(int page)
    {
        string paginatedUrl = _url + "?page=" + page + "&limit=" + _limit;
        apiController.CallApi(paginatedUrl, HandlePatientDataList, apiController.HandleError);
    }

    private void HandlePatientDataList(JObject jsonResponse)
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
                    PatientData data = new PatientData
                    {
                        id = item["id"]?.ToString(),
                        name = item["name"]?.ToString()
                    };

                    patientDataList.Add(data);
                }

                Debug.Log($"Data Saved: {patientDataList.Count} records");

                if (patientDataList.Count < _totalData)
                {
                    _currentPage++;
                    FetchPatientData(_currentPage); // Ambil halaman berikutnya
                }
                else
                {
                    isFetching = false; // Pengambilan selesai
                    
                    //Sort Patient Data
                    patientDataList = patientDataList.OrderBy(patient => patient.name).ToList();
                    Debug.Log("All data has been fetched.");
                }
            }
        }
        else
        {
            isFetching = false; // Gagal mengambil data
            Debug.LogError("API Response Not Success");
        }
    }
}

[System.Serializable]
public class PatientData
{
    public string id;
    public string name;
}