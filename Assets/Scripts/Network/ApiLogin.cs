using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class ApiLogin : MonoBehaviour
{
    private string _urlOption = GameEnv.BASE_URL + "/pin/options";
    private string _urlCheck = GameEnv.BASE_URL + "/pin/verify";
    [SerializeField] private ApiController apiController;

    public List<string> pinList = new List<string>();

    public bool HasPinList { get; private set; }
    
    public bool IsLogin { get; private set; }
    

    public void GetOptionPin()
    {
        apiController.CallApi(_urlOption, HandleOptionPin, apiController.HandleError);
    }

    public IEnumerator CheckPinCoroutine(string pin)
    {
        JObject postData = new JObject
        {
            ["pin"] = pin
        };

        // Membuat flag sementara untuk menunggu hasil
        bool isDone = false;

        // Mengirimkan permintaan API secara asynchronous
        apiController.PostApi(_urlCheck, postData, response =>
            {
                bool success = response["success"].Value<bool>();
                IsLogin = success;
                isDone = true;  // Menandakan bahwa proses selesai
            },
            error =>
            {
                IsLogin = false;
                isDone = true;  // Menandakan bahwa proses selesai meskipun gagal
            });

        // Tunggu hingga API memberikan respons
        while (!isDone)
        {
            yield return null;  // Menunggu frame berikutnya hingga respons diterima
        }
    }

    private void HandleOptionPin(JObject jsonResponse)
    {
        bool success = jsonResponse["success"].Value<bool>();
        
        if (success)
        {
            Debug.Log("Pin Gathering Valid");
            HasPinList = true;
            
            JArray options = (JArray)jsonResponse["data"]["options"];
            List<string> optionData = new List<string>();

            if (options != null)
            {
                foreach (var opsi in options)
                {
                    optionData.Add(opsi.ToString());
                }

                pinList = optionData;
                
                foreach (var list in pinList)
                {
                    Debug.Log(list);
                }
            }
        }
        else
        {
            HasPinList = false;
            Debug.Log("Pin List Nothing");
        }
    }
    
}
